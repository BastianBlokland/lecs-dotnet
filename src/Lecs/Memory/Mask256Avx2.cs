using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

namespace Lecs.Memory
{
    /// <summary>
    /// Partial containing Avx2 intrinsics logic.
    /// </summary>
    public unsafe partial struct Mask256
    {
        /* NOTE: Query for support before calling this! */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AddAvx2(long* dataPointerA, long* dataPointerB)
        {
            /* Basic logic is: A |= B */

            /*
            Or:
                dst[255:0] := (a[255:0] OR b[255:0])
            https://software.intel.com/sites/landingpage/IntrinsicsGuide/#expand=4038&text=or_si256
            */

            var vectorA = Avx2.LoadVector256(dataPointerA);
            var vectorB = Avx2.LoadVector256(dataPointerB);
            var result = Avx2.Or(vectorA, vectorB);
            Avx2.Store(dataPointerA, result);
        }

        /* NOTE: Query for support before calling this! */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void RemoveAvx2(long* dataPointerA, long* dataPointerB)
        {
            /* Basic logic is: A &= ~B */

            /*
            AndNot:
                dst[255:0] := ((NOT a[255:0]) AND b[255:0])
            https://software.intel.com/sites/landingpage/IntrinsicsGuide/#expand=4038,333&text=andnot_si256
            */

            var vectorA = Avx2.LoadVector256(dataPointerA);
            var vectorB = Avx2.LoadVector256(dataPointerB);
            var result = Avx2.AndNot(vectorB, vectorA);
            Avx2.Store(dataPointerA, result);
        }

        /* NOTE: Query for support before calling this! */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void InvertAvx2(long* dataPointer)
        {
            /* Basic logic is: A = ~A */

            /*
            Weirdly enough there is no 'Not' instruction but according to stack-overflow the
            recommended way of doing it is a Xor with a 'constant' of all ones
            https://stackoverflow.com/questions/42613821/is-not-missing-from-sse-avx

            Xor:
                dst[255:0] := (a[255:0] XOR b[255:0])
            https://software.intel.com/sites/landingpage/IntrinsicsGuide/#expand=6125&text=xor_si256
            */

            var vector = Avx2.LoadVector256(dataPointer);
            var allOne = Avx2.CompareEqual(vector, vector);
            var result = Avx2.Xor(vector, allOne);
            Avx2.Store(dataPointer, result);
        }

        /* NOTE: Query for support before calling this! */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ClearAvx2(long* dataPointer)
        {
            /* Basic logic is: A = 0 */

            /*
            BroadcastScaler:
                FOR j := 0 to 31
                    i := j*8
                    dst[i+7:i] := a[7:0]
                ENDFOR
            https://software.intel.com/sites/landingpage/IntrinsicsGuide/#expand=527&techs=AVX2&text=broadcastb_epi8
            */

            byte zero = 0;
            var zeroVector = Avx2.BroadcastScalarToVector256(&zero);
            Avx.Store(dataPointer, zeroVector.AsInt64());
        }

        /* NOTE: Query for support before calling this! */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool EqualsAvx2(long* dataPointerA, long* dataPointerB)
        {
            /* Basic logic is: A == B */

            /*
            With Avx2 we compare all bytes together (bytes because there is no 32 bit MoveMask?)
            Then we aggregate the results using 'MoveMask' and check if all bits are 1

            MoveMask:
                FOR j := 0 to 31
                    i := j*8
                    dst[j] := a[i+7]
                ENDFOR
            https://software.intel.com/sites/landingpage/IntrinsicsGuide/#expand=3832&techs=AVX2&text=movemask_epi8
            */

            var vectorA = Avx2.LoadVector256(dataPointerA).AsByte();
            var vectorB = Avx2.LoadVector256(dataPointerB).AsByte();
            var elementWiseResult = Avx2.CompareEqual(vectorA, vectorB);
            return Avx2.MoveMask(elementWiseResult) == -1; // -1 is 32 bits set to 1
        }

        /* NOTE: Query for support before calling this! */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddAvx2(in Mask256 other)
        {
            fixed (long* dataPointerA = this.Data, dataPointerB = other.Data)
            {
                AddAvx2(dataPointerA, dataPointerB);
            }
        }

        /* NOTE: Query for support before calling this! */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RemoveAvx2(in Mask256 other)
        {
            fixed (long* dataPointerA = this.Data, dataPointerB = other.Data)
            {
                RemoveAvx2(dataPointerA, dataPointerB);
            }
        }

        /* NOTE: Query for support before calling this! */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void InvertAvx2()
        {
            fixed (long* dataPointer = this.Data)
            {
                InvertAvx2(dataPointer);
            }
        }

        /* NOTE: Query for support before calling this! */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ClearAvx2()
        {
            fixed (long* dataPointer = this.Data)
            {
                ClearAvx2(dataPointer);
            }
        }

        /* NOTE: Query for support before calling this! */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool EqualsAvx2(in Mask256 other)
        {
            fixed (long* dataPointerA = this.Data, dataPointerB = other.Data)
            {
                return EqualsAvx2(dataPointerA, dataPointerB);
            }
        }
    }
}
