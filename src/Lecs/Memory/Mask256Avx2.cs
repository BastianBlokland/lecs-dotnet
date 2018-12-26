using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

namespace Lecs.Memory
{
    public unsafe partial struct Mask256 : IEquatable<Mask256>
    {
        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AddAvx2(long* dataPointerA, long* dataPointerB)
        {
            /* With Avx2 we can do a 256 bit OR in a single instruction */

            var vectorA = Avx2.LoadVector256(dataPointerA);
            var vectorB = Avx2.LoadVector256(dataPointerB);
            var result = Avx2.Or(vectorA, vectorB);
            Avx2.Store(dataPointerA, result);
        }

        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void RemoveAvx2(long* dataPointerA, long* dataPointerB)
        {
            /* With Avx2 we can do a single 256 bit AndNot instruction */

            var vectorA = Avx2.LoadVector256(dataPointerA);
            var vectorB = Avx2.LoadVector256(dataPointerB);
            var result = Avx2.AndNot(vectorB, vectorA);
            Avx2.Store(dataPointerA, result);
        }

        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void InvertAvx2(long* dataPointer)
        {
            /*
            Weirdly enough there is no 'Not' instruction but according to stack-overflow the
            recommended way of doing it is a Xor with a 'constant' of all ones
            https://stackoverflow.com/questions/42613821/is-not-missing-from-sse-avx
            */

            var vector = Avx2.LoadVector256(dataPointer);
            var allOne = Avx2.CompareEqual(vector, vector);
            var result = Avx2.Xor(vector, allOne);
            Avx2.Store(dataPointer, result);
        }

        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ClearAvx2(long* dataPointer)
        {
            byte zero = 0;
            var zeroVector = Avx2.BroadcastScalarToVector256(&zero);
            Avx.Store(dataPointer, zeroVector.AsInt64());
        }

        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool EqualsAvx2(long* dataPointerA, long* dataPointerB)
        {
            /*
            With Avx2 we compare all bytes together (bytes because there is no 32 bit MoveMask?)
            Then we aggregate the results using 'MoveMask' and check if all bits are 1
            */

            var vectorA = Avx2.LoadVector256(dataPointerA).AsByte();
            var vectorB = Avx2.LoadVector256(dataPointerB).AsByte();
            var elementWiseResult = Avx2.CompareEqual(vectorA, vectorB);
            return Avx2.MoveMask(elementWiseResult) == -1; // -1 is 32 bits set to 1
        }

        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddAvx2(in Mask256 other)
        {
            fixed (long* dataPointerA = this.data, dataPointerB = other.data)
            {
                AddAvx2(dataPointerA, dataPointerB);
            }
        }

        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RemoveAvx2(in Mask256 other)
        {
            fixed (long* dataPointerA = this.data, dataPointerB = other.data)
            {
                RemoveAvx2(dataPointerA, dataPointerB);
            }
        }

        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void InvertAvx2()
        {
            fixed (long* dataPointer = this.data)
            {
                InvertAvx2(dataPointer);
            }
        }

        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ClearAvx2()
        {
            fixed (long* dataPointer = this.data)
            {
                ClearAvx2(dataPointer);
            }
        }

        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool EqualsAvx2(in Mask256 other)
        {
            fixed (long* dataPointerA = this.data, dataPointerB = other.data)
            {
                return EqualsAvx2(dataPointerA, dataPointerB);
            }
        }
    }
}
