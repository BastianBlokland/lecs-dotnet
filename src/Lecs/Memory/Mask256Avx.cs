using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

namespace Lecs.Memory
{
    public unsafe partial struct Mask256 : IEquatable<Mask256>
    {
        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool HasAllAvx(long* dataPointerA, long* dataPointerB)
        {
            /* Basic logic is: A & B == B */

            var vectorA = Avx.LoadVector256(dataPointerA);
            var vectorB = Avx.LoadVector256(dataPointerB);
            return Avx.TestC(vectorA, vectorB);
        }

        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool HasAnyAvx(long* dataPointerA, long* dataPointerB)
        {
            var vectorA = Avx.LoadVector256(dataPointerA);
            var vectorB = Avx.LoadVector256(dataPointerB);
            return Avx.TestNotZAndNotC(vectorA, vectorB);
        }

        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AddAvx(long* dataPointerA, long* dataPointerB)
        {
            /* With Avx we need two 128 bit OR instructions */

            // First 128 bit
            var vectorA = Avx.LoadVector128(dataPointerA);
            var vectorB = Avx.LoadVector128(dataPointerB);
            var result = Avx.Or(vectorA, vectorB);
            Avx.Store(dataPointerA, result);

            // Second 128 bit
            long* dataPointer1Plus2 = dataPointerA + 2;
            long* dataPointer2Plus2 = dataPointerB + 2;
            vectorA = Avx.LoadVector128(dataPointer1Plus2);
            vectorB = Avx.LoadVector128(dataPointer2Plus2);
            result = Avx.Or(vectorA, vectorB);
            Avx.Store(dataPointer1Plus2, result);
        }

        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void RemoveAvx(long* dataPointerA, long* dataPointerB)
        {
            /* With Avx we need two 128 bit AndNot instructions */

            // First 128 bit
            var vectorA = Avx.LoadVector128(dataPointerA);
            var vectorB = Avx.LoadVector128(dataPointerB);
            var result = Avx.AndNot(vectorB, vectorA);
            Avx.Store(dataPointerA, result);

            // Second 128 bit
            long* dataPointer1Plus2 = dataPointerA + 2;
            long* dataPointer2Plus2 = dataPointerB + 2;
            vectorA = Avx.LoadVector128(dataPointer1Plus2);
            vectorB = Avx.LoadVector128(dataPointer2Plus2);
            result = Avx.AndNot(vectorB, vectorA);
            Avx.Store(dataPointer1Plus2, result);
        }

        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void InvertAvx(long* dataPointer)
        {
            /*
            Weirdly enough there is no 'Not' instruction but according to stack-overflow the
            recommended way of doing it is a Xor with a 'constant' of all ones
            https://stackoverflow.com/questions/42613821/is-not-missing-from-sse-avx
            */

            // First 128 bit
            var vector = Avx.LoadVector128(dataPointer);
            var allOne = Avx.CompareEqual(vector, vector);
            var result = Avx.Xor(vector, allOne);
            Avx.Store(dataPointer, result);

            // Second 128 bit
            long* dataPointerPlus2 = dataPointer + 2;
            vector = Avx.LoadVector128(dataPointerPlus2);
            result = Avx.Xor(vector, allOne);
            Avx.Store(dataPointerPlus2, result);
        }

        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool EqualsAvx(long* dataPointerA, long* dataPointerB)
        {
            /* With Avx we do the same but in two steps of 128 bits */

            // First 128 bit
            var vectorA = Avx.LoadVector128(dataPointerA).AsByte();
            var vectorB = Avx.LoadVector128(dataPointerB).AsByte();
            var elementWiseResult = Avx.CompareEqual(vectorA, vectorB);
            if (Avx.MoveMask(elementWiseResult) != 0b_1111_1111_1111_1111)
                return false;

            // Second 128 bit
            long* dataPointer1Plus4 = dataPointerA + 2;
            long* dataPointer2Plus4 = dataPointerB + 2;
            vectorA = Avx.LoadVector128(dataPointer1Plus4).AsByte();
            vectorB = Avx.LoadVector128(dataPointer2Plus4).AsByte();
            elementWiseResult = Avx.CompareEqual(vectorA, vectorB);
            return Avx.MoveMask(elementWiseResult) == 0b_1111_1111_1111_1111;
        }

        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool HasAllAvx(in Mask256 other)
        {
            fixed (long* dataPointerA = this.data, dataPointerB = other.data)
            {
                return HasAllAvx(dataPointerA, dataPointerB);
            }
        }

        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool HasAnyAvx(in Mask256 other)
        {
            fixed (long* dataPointerA = this.data, dataPointerB = other.data)
            {
                return HasAnyAvx(dataPointerA, dataPointerB);
            }
        }

        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddAvx(in Mask256 other)
        {
            fixed (long* dataPointerA = this.data, dataPointerB = other.data)
            {
                AddAvx(dataPointerA, dataPointerB);
            }
        }

        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RemoveAvx(in Mask256 other)
        {
            fixed (long* dataPointerA = this.data, dataPointerB = other.data)
            {
                RemoveAvx(dataPointerA, dataPointerB);
            }
        }

        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void InvertAvx()
        {
            fixed (long* dataPointer = this.data)
            {
                InvertAvx(dataPointer);
            }
        }

        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool EqualsAvx(in Mask256 other)
        {
            fixed (long* dataPointerA = this.data, dataPointerB = other.data)
            {
                return EqualsAvx(dataPointerA, dataPointerB);
            }
        }
    }
}
