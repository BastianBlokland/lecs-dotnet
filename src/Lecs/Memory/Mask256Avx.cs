using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

namespace Lecs.Memory
{
    public unsafe partial struct Mask256 : IEquatable<Mask256>
    {
        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool HasAvx(in Mask256 other)
        {
            /* With Avx we can get the result with a single inverted 'TestZ' instruction */

            fixed (int* dataPointer = this.data)
            fixed (int* otherDataPointer = other.data)
            {
                var vectorA = Avx.LoadVector256(dataPointer);
                var vectorB = Avx.LoadVector256(otherDataPointer);
                return !Avx.TestZ(vectorA, vectorB);
            }
        }

        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool NotHasAvx(in Mask256 other)
        {
            /* With Avx we can get the result with a single 'TestZ' instruction */

            fixed (int* dataPointer = this.data)
            fixed (int* otherDataPointer = other.data)
            {
                var vectorA = Avx.LoadVector256(dataPointer);
                var vectorB = Avx.LoadVector256(otherDataPointer);
                return Avx.TestZ(vectorA, vectorB);
            }
        }

        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddAvx(in Mask256 other)
        {
            /* With Avx we need two 128 bit OR instructions */

            fixed (int* dataPointer = this.data)
            fixed (int* otherDataPointer = other.data)
            {
                // First 4 ints
                var vectorA = Avx.LoadVector128(dataPointer);
                var vectorB = Avx.LoadVector128(otherDataPointer);
                var result = Avx.Or(vectorA, vectorB);
                Avx.Store(dataPointer, result);

                // Second 4 ints
                int* dataPointer1Plus4 = dataPointer + 4;
                int* dataPointer2Plus4 = otherDataPointer + 4;
                vectorA = Avx.LoadVector128(dataPointer1Plus4);
                vectorB = Avx.LoadVector128(dataPointer2Plus4);
                result = Avx.Or(vectorA, vectorB);
                Avx.Store(dataPointer1Plus4, result);
            }
        }

        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RemoveAvx(in Mask256 other)
        {
            /* With Avx we need two 128 bit AndNot instructions */

            fixed (int* dataPointer = this.data)
            fixed (int* otherDataPointer = other.data)
            {
                // First 4 ints
                var vectorA = Avx.LoadVector128(dataPointer);
                var vectorB = Avx.LoadVector128(otherDataPointer);
                var result = Avx.AndNot(vectorB, vectorA);
                Avx.Store(dataPointer, result);

                // Second 4 ints
                int* dataPointer1Plus4 = dataPointer + 4;
                int* dataPointer2Plus4 = otherDataPointer + 4;
                vectorA = Avx.LoadVector128(dataPointer1Plus4);
                vectorB = Avx.LoadVector128(dataPointer2Plus4);
                result = Avx.AndNot(vectorB, vectorA);
                Avx.Store(dataPointer1Plus4, result);
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

            fixed (int* dataPointer = this.data)
            {
                // First 4 ints
                var vector = Avx.LoadVector128(dataPointer);
                var allOne = Avx.CompareEqual(vector, vector);
                var result = Avx.Xor(vector, allOne);
                Avx.Store(dataPointer, result);

                // Second 4 bits
                int* dataPointerPlus4 = dataPointer + 4;
                vector = Avx.LoadVector128(dataPointerPlus4);
                result = Avx.Xor(vector, allOne);
                Avx.Store(dataPointerPlus4, result);
            }
        }

        /// <summary> NOTE: Query for support before calling this! </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool EqualsAvx(in Mask256 other)
        {
            /* With Avx we do the same but in two steps of 128 bits */

            fixed (int* dataPointer = this.data)
            fixed (int* otherDataPointer = other.data)
            {
                // First 4 ints
                var vectorA = Avx.LoadVector128(dataPointer).AsByte();
                var vectorB = Avx.LoadVector128(otherDataPointer).AsByte();
                var elementWiseResult = Avx.CompareEqual(vectorA, vectorB);
                if (Avx.MoveMask(elementWiseResult) != 0b_1111_1111_1111_1111)
                    return false;

                // Second 4 ints
                int* dataPointer1Plus4 = dataPointer + 4;
                int* dataPointer2Plus4 = otherDataPointer + 4;
                vectorA = Avx.LoadVector128(dataPointer1Plus4).AsByte();
                vectorB = Avx.LoadVector128(dataPointer2Plus4).AsByte();
                elementWiseResult = Avx.CompareEqual(vectorA, vectorB);
                return Avx.MoveMask(elementWiseResult) == 0b_1111_1111_1111_1111;
            }
        }
    }
}
