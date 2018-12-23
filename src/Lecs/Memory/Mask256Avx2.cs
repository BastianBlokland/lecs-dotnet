using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

namespace Lecs.Memory
{
    public unsafe partial struct Mask256 : IEquatable<Mask256>
    {
        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddAvx2(in Mask256 other)
        {
            /* With Avx2 we can do a 256 bit OR in a single instruction */

            fixed (int* dataPointer = this.data)
            fixed (int* otherDataPointer = other.data)
            {
                var vectorA = Avx2.LoadVector256(dataPointer);
                var vectorB = Avx2.LoadVector256(otherDataPointer);
                var result = Avx2.Or(vectorA, vectorB);
                Avx2.Store(dataPointer, result);
            }
        }

        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RemoveAvx2(in Mask256 other)
        {
            /* With Avx2 we can do a single 256 bit AndNot instruction */

            fixed (int* dataPointer = this.data)
            fixed (int* otherDataPointer = other.data)
            {
                var vectorA = Avx2.LoadVector256(dataPointer);
                var vectorB = Avx2.LoadVector256(otherDataPointer);
                var result = Avx2.AndNot(vectorB, vectorA);
                Avx2.Store(dataPointer, result);
            }
        }

        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void InvertAvx2()
        {
            /*
            Weirdly enough there is no 'Not' instruction but according to stack-overflow the
            recommended way of doing it is a Xor with a 'constant' of all ones
            https://stackoverflow.com/questions/42613821/is-not-missing-from-sse-avx
            */

            fixed (int* dataPointer = this.data)
            {
                var vector = Avx2.LoadVector256(dataPointer);
                var allOne = Avx2.CompareEqual(vector, vector);
                var result = Avx2.Xor(vector, allOne);
                Avx2.Store(dataPointer, result);
            }
        }

        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ClearAvx2()
        {
            byte zero = 0;
            fixed (int* dataPointer = this.data)
            {
                var zeroVector = Avx2.BroadcastScalarToVector256(&zero);
                Avx.Store(dataPointer, zeroVector.AsInt32());
            }
        }

        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool EqualsAvx2(in Mask256 other)
        {
            /*
            With Avx2 we compare all bytes together (bytes because there is no 32 bit MoveMask?)
            Then we aggregate the results using 'MoveMask' and check if all bits are 1
            */

            fixed (int* dataPointer = this.data)
            fixed (int* otherDataPointer = other.data)
            {
                var vectorA = Avx2.LoadVector256(dataPointer).AsByte();
                var vectorB = Avx2.LoadVector256(otherDataPointer).AsByte();
                var elementWiseResult = Avx2.CompareEqual(vectorA, vectorB);
                return Avx2.MoveMask(elementWiseResult) == -1; // -1 is 32 bits set to 1
            }
        }
    }
}
