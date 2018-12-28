using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

namespace Lecs.Memory
{
    /// <summary>
    /// Partial containing Avx intrinsics logic.
    /// </summary>
    public unsafe partial struct Mask256
    {
        /* NOTE: Query for support before calling this! */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool HasAllAvx(long* dataPointerA, long* dataPointerB)
        {
            /* Basic logic is: A & B == B */

            /*
            TestC:
                IF ((NOT a[255:0]) AND b[255:0] == 0)
                    CF := 1
                ELSE
                    CF := 0
                FI
                RETURN CF
            https://software.intel.com/sites/landingpage/IntrinsicsGuide/#text=testc_si256&expand=5875
            */

            var vectorA = Avx.LoadVector256(dataPointerA);
            var vectorB = Avx.LoadVector256(dataPointerB);
            return Avx.TestC(vectorA, vectorB);
        }

        /* NOTE: Query for support before calling this! */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool HasAnyAvx(long* dataPointerA, long* dataPointerB)
        {
            /* Basic logic is: B & A != 0 */

            /*
            TestNZC:
                IF (a[255:0] AND b[255:0] == 0)
                    ZF := 1
                ELSE
                    ZF := 0
                FI
                IF ((NOT a[255:0]) AND b[255:0] == 0)
                    CF := 1
                ELSE
                    CF := 0
                FI
                IF (ZF == 0 && CF == 0)
                    RETURN 1
                ELSE
                    RETURN 0
                FI
            https://software.intel.com/sites/landingpage/IntrinsicsGuide/#text=testnzc_si256&expand=5905
            */

            var vectorA = Avx.LoadVector256(dataPointerA);
            var vectorB = Avx.LoadVector256(dataPointerB);
            return Avx.TestNotZAndNotC(vectorA, vectorB);
        }

        /* NOTE: Query for support before calling this! */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool NotHasAnyAvx(long* dataPointerA, long* dataPointerB)
        {
            /* Basic logic is: B & A == 0 */

            /*
            TestZ:
                IF (a[255:0] AND b[255:0] == 0)
                    ZF := 1
                ELSE
                    ZF := 0
                FI
                RETURN ZF
            https://software.intel.com/sites/landingpage/IntrinsicsGuide/#text=testz_si256&expand=5911&techs=AVX,AVX2
            */

            var vectorA = Avx.LoadVector256(dataPointerA);
            var vectorB = Avx.LoadVector256(dataPointerB);
            return Avx.TestZ(vectorA, vectorB);
        }

        /* NOTE: Query for support before calling this! */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AddAvx(long* dataPointerA, long* dataPointerB)
        {
            /* Basic logic is: A |= B */

            /*
            Or:
                FOR j := 0 to 3
                    i := j*32
                    dst[i+31:i] := a[i+31:i] BITWISE OR b[i+31:i]
                ENDFOR
            https://software.intel.com/sites/landingpage/IntrinsicsGuide/#expand=4030&techs=SSE&text=_mm_or_ps
            */

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

        /* NOTE: Query for support before calling this! */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void RemoveAvx(long* dataPointerA, long* dataPointerB)
        {
            /* Basic logic is: A &= ~B */

            /*
            AndNot:
                FOR j := 0 to 3
                    i := j*32
                    dst[i+31:i] := ((NOT a[i+31:i]) AND b[i+31:i])
                ENDFOR
            https://software.intel.com/sites/landingpage/IntrinsicsGuide/#text=mm_andnot_ps&expand=323
            */

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

        /* NOTE: Query for support before calling this! */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void InvertAvx(long* dataPointer)
        {
            /* Basic logic is: A = ~A */

            /*
            Weirdly enough there is no 'Not' instruction but according to stack-overflow the
            recommended way of doing it is a Xor with a 'constant' of all ones
            https://stackoverflow.com/questions/42613821/is-not-missing-from-sse-avx

            /*
            Xor:
                FOR j := 0 to 3
                    i := j*32
                    dst[i+31:i] := a[i+31:i] XOR b[i+31:i]
                ENDFOR
            https://software.intel.com/sites/landingpage/IntrinsicsGuide/#text=_mm_xor_ps&expand=6117
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

        /* NOTE: Query for support before calling this! */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool EqualsAvx(long* dataPointerA, long* dataPointerB)
        {
            /* Basic logic is: A == B */

            /*
            With Avx we compare all bytes together (bytes because there is no 32 bit MoveMask?)
            Then we aggregate the results using 'MoveMask' and check if all bits are 1

            MoveMask:
                FOR j := 0 to 31
                    i := j*8
                    dst[j] := a[i+7]
                ENDFOR
            https://software.intel.com/sites/landingpage/IntrinsicsGuide/#text=mm_movemask_epi8&expand=3831
            */

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

        /* NOTE: Query for support before calling this! */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool HasAllAvx(in Mask256 other)
        {
            fixed (long* dataPointerA = this.Data, dataPointerB = other.Data)
            {
                return HasAllAvx(dataPointerA, dataPointerB);
            }
        }

        /* NOTE: Query for support before calling this! */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool HasAnyAvx(in Mask256 other)
        {
            fixed (long* dataPointerA = this.Data, dataPointerB = other.Data)
            {
                return HasAnyAvx(dataPointerA, dataPointerB);
            }
        }

        /* NOTE: Query for support before calling this! */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool NotHasAnyAvx(in Mask256 other)
        {
            fixed (long* dataPointerA = this.Data, dataPointerB = other.Data)
            {
                return NotHasAnyAvx(dataPointerA, dataPointerB);
            }
        }

        /* NOTE: Query for support before calling this! */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddAvx(in Mask256 other)
        {
            fixed (long* dataPointerA = this.Data, dataPointerB = other.Data)
            {
                AddAvx(dataPointerA, dataPointerB);
            }
        }

        /* NOTE: Query for support before calling this! */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RemoveAvx(in Mask256 other)
        {
            fixed (long* dataPointerA = this.Data, dataPointerB = other.Data)
            {
                RemoveAvx(dataPointerA, dataPointerB);
            }
        }

        /* NOTE: Query for support before calling this! */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void InvertAvx()
        {
            fixed (long* dataPointer = this.Data)
            {
                InvertAvx(dataPointer);
            }
        }

        /* NOTE: Query for support before calling this! */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool EqualsAvx(in Mask256 other)
        {
            fixed (long* dataPointerA = this.Data, dataPointerB = other.Data)
            {
                return EqualsAvx(dataPointerA, dataPointerB);
            }
        }
    }
}
