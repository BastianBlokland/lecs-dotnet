#pragma warning disable CA1710 // Identifiers should have correct suffix (Collection)

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

using static Lecs.Memory.ValueStore;

namespace Lecs.Memory
{
    public sealed partial class ValueStore<T>
        where T : unmanaged
    {
        /* NOTE: Query for support before calling this! */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe bool FindAvx2(int key, out SlotToken slotToken)
        {
            int hash = HashHelpers.Mix(key);
            int index = HashHelpers.ModuloPowerOfTwoMinusOne(hash, this.capacityMinusOne);

            unchecked
            {
                fixed (int* keysPointer = this.keys)
                {
                    // Create a vector that contains the target 'key' 8 times
                    var targetReferenceScalar = Vector128.CreateScalarUnsafe(key);
                    var targetReference = Avx2.BroadcastScalarToVector256(targetReferenceScalar);

                    while (true)
                    {
                        /*
                        Logic here is that we perform a 256 wide equality check and that gives us
                        8 values of 'FFFF' or '0000' (32 bits of either 1 or 1) in a 256 bit register.
                        Then with MoveMask we can combine that to a single 32 bit value by taking
                        4 bits from each 32 bit value (bit 7, 15, 23, 31). Because we know that only
                        a single element can be equal to our value we can construct a jump table
                        out of the integer.

                        eq 32:       0        0        1        0        0        0        0        0        0
                        MoveMask: 0 0 0 0  0 0 0 0  1 1 1 1  0 0 0 0  0 0 0 0  0 0 0 0  0 0 0 0  0 0 0 0  0 0 0 0
                        Hex:         0        0        F        0        0        0        0        0        0
                        */

                        // NOTE: We do not check for bounds here because we allocate our keys array
                        // 7 elements bigger then the capacity so its always safe to load 8 keys
                        var elements = Avx2.LoadAlignedVector256(keysPointer + index);

                        var elementEquals = Avx2.CompareEqual(elements, targetReference);
                        switch (Avx2.MoveMask(elementEquals.AsByte()))
                        {
                            case (int)0xF0000000: // At element 0
                                slotToken = Unsafe.As<int, SlotToken>(ref index);
                                return true;

                            case (int)0x0F000000: // At element 1
                                index += 1;
                                slotToken = Unsafe.As<int, SlotToken>(ref index);
                                return true;

                            case (int)0x00F00000: // At element 2
                                index += 2;
                                slotToken = Unsafe.As<int, SlotToken>(ref index);
                                return true;

                            case (int)0x000F0000: // At element 3
                                index += 3;
                                slotToken = Unsafe.As<int, SlotToken>(ref index);
                                return true;

                            case (int)0x0000F000: // At element 4
                                index += 4;
                                slotToken = Unsafe.As<int, SlotToken>(ref index);
                                return true;

                            case (int)0x00000F00: // At element 5
                                index += 5;
                                slotToken = Unsafe.As<int, SlotToken>(ref index);
                                return true;

                            case (int)0x000000F0: // At element 6
                                index += 6;
                                slotToken = Unsafe.As<int, SlotToken>(ref index);
                                return true;

                            case (int)0x0000000F: // At element 7
                                index += 7;
                                slotToken = Unsafe.As<int, SlotToken>(ref index);
                                return true;

                            case (int)0x00000000: // Not found
                                /*
                                When we did not find the target key then we check if the next 8 elements
                                contains a 'free' key, if so that means the target key doesn not exist
                                in this valuestore.
                                */

                                // Create a vector that contains the 'FreeKey' 8 times
                                var freeReferenceScalar = Vector128.CreateScalarUnsafe(FreeKey);
                                var freeReference = Avx2.BroadcastScalarToVector256(freeReferenceScalar);

                                var elementFree = Avx2.CompareEqual(elements, freeReference);
                                if (Avx2.MoveMask(elementFree.AsByte()) != 0)
                                {
                                    /* One of the items was a free-key so we can stop looking for
                                    the target key. */

                                    slotToken = default(SlotToken);
                                    return false;
                                }

                                // Increment the index to look at the next 8 elements.
                                index += 8;

                                // Once we reach the end of the data then we wrap
                                if (index > this.capacity)
                                    index = 0;
                                break;

                            default:
                                Debug.Fail("Key is found in dictionary multiple times");
                                break;
                        }
                    }
                }
            }
        }
    }
}
