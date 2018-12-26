using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

namespace Lecs.Memory
{
    public unsafe partial struct Mask256 : IEquatable<Mask256>
    {
        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool HasAllAvx(in Mask256 other)
        {
            /* Basic logic is: A & B == B */

            fixed (long* dataPointer = this.data)
            fixed (long* otherDataPointer = other.data)
            {
                var vectorA = Avx.LoadVector256(dataPointer);
                var vectorB = Avx.LoadVector256(otherDataPointer);
                return Avx.TestC(vectorA, vectorB);
            }
        }

        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool HasAnyAvx(in Mask256 other)
        {
            /* With Avx we can get the result with a single 'TestZ' instruction */

            fixed (long* dataPointer = this.data)
            fixed (long* otherDataPointer = other.data)
            {
                var vectorA = Avx.LoadVector256(dataPointer);
                var vectorB = Avx.LoadVector256(otherDataPointer);
                return !Avx.TestZ(vectorA, vectorB);
            }
        }

        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddAvx(in Mask256 other)
        {
            /* With Avx we need two 128 bit OR instructions */

            fixed (long* dataPointer = this.data)
            fixed (long* otherDataPointer = other.data)
            {
                // First 128 bit
                var vectorA = Avx.LoadVector128(dataPointer);
                var vectorB = Avx.LoadVector128(otherDataPointer);
                var result = Avx.Or(vectorA, vectorB);
                Avx.Store(dataPointer, result);

                // Second 128 bit
                long* dataPointer1Plus2 = dataPointer + 2;
                long* dataPointer2Plus2 = otherDataPointer + 2;
                vectorA = Avx.LoadVector128(dataPointer1Plus2);
                vectorB = Avx.LoadVector128(dataPointer2Plus2);
                result = Avx.Or(vectorA, vectorB);
                Avx.Store(dataPointer1Plus2, result);
            }
        }

        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RemoveAvx(in Mask256 other)
        {
            /* With Avx we need two 128 bit AndNot instructions */

            fixed (long* dataPointer = this.data)
            fixed (long* otherDataPointer = other.data)
            {
                // First 128 bit
                var vectorA = Avx.LoadVector128(dataPointer);
                var vectorB = Avx.LoadVector128(otherDataPointer);
                var result = Avx.AndNot(vectorB, vectorA);
                Avx.Store(dataPointer, result);

                // Second 128 bit
                long* dataPointer1Plus2 = dataPointer + 2;
                long* dataPointer2Plus2 = otherDataPointer + 2;
                vectorA = Avx.LoadVector128(dataPointer1Plus2);
                vectorB = Avx.LoadVector128(dataPointer2Plus2);
                result = Avx.AndNot(vectorB, vectorA);
                Avx.Store(dataPointer1Plus2, result);
            }
        }

        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void InvertAvx()
        {
            /*
            Weirdly enough there is no 'Not' instruction but according to stack-overflow the
            recommended way of doing it is a Xor with a 'constant' of all ones
            https://stackoverflow.com/questions/42613821/is-not-missing-from-sse-avx
            */

            fixed (long* dataPointer = this.data)
            {
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
        }

        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool EqualsAvx(in Mask256 other)
        {
            /* With Avx we do the same but in two steps of 128 bits */

            fixed (long* dataPointer = this.data)
            fixed (long* otherDataPointer = other.data)
            {
                // First 128 bit
                var vectorA = Avx.LoadVector128(dataPointer).AsByte();
                var vectorB = Avx.LoadVector128(otherDataPointer).AsByte();
                var elementWiseResult = Avx.CompareEqual(vectorA, vectorB);
                if (Avx.MoveMask(elementWiseResult) != 0b_1111_1111_1111_1111)
                    return false;

                // Second 128 bit
                long* dataPointer1Plus4 = dataPointer + 2;
                long* dataPointer2Plus4 = otherDataPointer + 2;
                vectorA = Avx.LoadVector128(dataPointer1Plus4).AsByte();
                vectorB = Avx.LoadVector128(dataPointer2Plus4).AsByte();
                elementWiseResult = Avx.CompareEqual(vectorA, vectorB);
                return Avx.MoveMask(elementWiseResult) == 0b_1111_1111_1111_1111;
            }
        }
    }
}
