#pragma warning disable CA1710 // Identifiers should have correct suffix (Collection)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

using static Lecs.Memory.ValueStore;

namespace Lecs.Memory
{
    public static partial class ValueStore
    {
        /* Use this non-generic class for putting static data that does not need to be 'instantiated'
        per generic type */

        internal const int FreeKey = -1;
        internal const int UnavailableKey = -2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Hash(int key)
        {
            // TODO: Add some form of bit mixing to avoid sequential keys (common case) having
            // sequential hashes (which is bad for our distribution)
            return key;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Unhash(int key)
        {
            // TODO: Add some form of bit mixing to avoid sequential keys (common case) having
            // sequential hashes (which is bad for our distribution)
            return key;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsPowerOfTwo(int num) => num != 0 && (num & (num - 1)) == 0;
    }

    public sealed partial class ValueStore<T> : IEnumerable<ValueStore.SlotToken>
        where T : unmanaged
    {
        private int[] keys; // Allocated 7 elements bigger for safe 256 bit jumps without range checking
        private T[] values;
        private int capacity; // Only power-of-two for fast modulo
        private int capacityMinusOne;

        public bool Find(int key, out SlotToken slotToken)
        {
            if (Avx2.IsSupported)
                return this.FindAvx2(key, out slotToken);
            else
                throw new NotImplementedException();
        }

        public int GetKey(SlotToken token)
        {
            // Its safe to interpret 'SlotToken' as an integer because they have the same layout
            ref int index = ref Unsafe.As<SlotToken, int>(ref token);

            // Assert that we are not indexing out of bounds
            Debug.Assert(index >= 0 && index < this.keys.Length, "'SlotToken' out of bounds");

            // Using explicit memory offsets to avoid the range checks that the array indexer does
            // normally. As long as we never shrink the arrays this should be safe because users cannot
            // create a 'SlotToken' themselves (without using reflection or Unsafe api's).
            return Unsafe.Add(ref this.keys[0], index);
        }

        public ref T GetValue(SlotToken token)
        {
            // Its safe to interpret 'SlotToken' as an integer because they have the same layout
            ref int index = ref Unsafe.As<SlotToken, int>(ref token);

            // Assert that we are not indexing out of bounds
            Debug.Assert(index >= 0 && index < this.values.Length, "'SlotToken' out of bounds");

            // Using explicit memory offsets to avoid the range checks that the array indexer does
            // normally. As long as we never shrink the arrays this should be safe because users cannot
            // create a 'SlotToken' themselves (without using reflection or Unsafe api's).
            return ref Unsafe.Add(ref this.values[0], index);
        }

        public SlotEnumerator GetEnumerator() => new SlotEnumerator(this.keys);

        IEnumerator<SlotToken> IEnumerable<SlotToken>.GetEnumerator() => new SlotEnumerator(this.keys);

        IEnumerator IEnumerable.GetEnumerator() => new SlotEnumerator(this.keys);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int ModuloCapacity(int value)
        {
            Debug.Assert(IsPowerOfTwo(this.capacity), "'capacity' is not a power-of-two");
            Debug.Assert((this.capacity - 1) == this.capacityMinusOne, "'capacityMinusOne' is not equal to 'capacity - 1'");

            // Because capacity is a power-of-two we can perform cheap modulo by just masking
            return value & this.capacityMinusOne;
        }
    }
}
