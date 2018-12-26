using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Lecs.Memory
{
    public unsafe partial struct Mask256 : IEquatable<Mask256>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SetBitSoftware(long* dataPointer, byte bit)
        {
            Span<byte> bits = stackalloc byte[] { bit };
            SetBitsSoftware(dataPointer, bits);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SetBitsSoftware(long* dataPointer, ReadOnlySpan<byte> bits)
        {
            foreach (byte bit in bits)
            {
                for (int i = 0; i < 4; i++)
                {
                    var lowerLimit = i * 64; // 64 bit per long
                    var upperLimit = (i + 1) * 64; // 64 bit per long
                    if (bit < upperLimit)
                    {
                        dataPointer[i] |= 1L << (bit - lowerLimit);
                        break;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool HasAllSoftware(long* dataPointerA, long* dataPointerB)
        {
            for (int i = 0; i < 4; i++)
            {
                if ((dataPointerA[i] & dataPointerB[i]) != dataPointerB[i])
                    return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool HasAnySoftware(long* dataPointerA, long* dataPointerB)
        {
            for (int i = 0; i < 4; i++)
            {
                if ((dataPointerA[i] & dataPointerB[i]) != 0)
                    return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AddSoftware(long* dataPointerA, long* dataPointerB)
        {
            for (int i = 0; i < 4; i++)
                dataPointerA[i] |= dataPointerB[i];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void RemoveSoftware(long* dataPointerA, long* dataPointerB)
        {
            for (int i = 0; i < 4; i++)
                dataPointerA[i] &= ~dataPointerB[i];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void InvertSoftware(long* dataPointer)
        {
            for (int i = 0; i < 4; i++)
                dataPointer[i] = ~dataPointer[i];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ClearSoftware(long* dataPointer)
        {
            for (int i = 0; i < 4; i++)
                dataPointer[i] = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetHashCodeSoftware(long* dataPointer)
        {
            var hashcode = default(HashCode);
            for (int i = 0; i < 4; i++)
                hashcode.Add(dataPointer[i]);
            return hashcode.ToHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string ToStringSoftware(long* dataPointer)
        {
            var stringBuilder = new StringBuilder(capacity: 256);
            for (int i = 0; i < 4; i++)
            {
                long mask = 1L;
                for (int j = 0; j < 64; j++)
                {
                    stringBuilder.Append((dataPointer[i] & mask) != 0 ? "1" : "0");
                    mask <<= 1;
                }
            }

            return stringBuilder.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ClearSoftware()
        {
            fixed (long* dataPointer = this.data)
            {
                ClearSoftware(dataPointer);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool EqualsSoftware(long* dataPointerA, long* dataPointerB)
        {
            for (int i = 0; i < 4; i++)
            {
                if (dataPointerA[i] != dataPointerB[i])
                    return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool HasAllSoftware(in Mask256 other)
        {
            fixed (long* dataPointerA = this.data, dataPointerB = other.data)
            {
                return HasAllSoftware(dataPointerA, dataPointerB);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool HasAnySoftware(in Mask256 other)
        {
            fixed (long* dataPointerA = this.data, dataPointerB = other.data)
            {
                return HasAnySoftware(dataPointerA, dataPointerB);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddSoftware(in Mask256 other)
        {
            fixed (long* dataPointerA = this.data, dataPointerB = other.data)
            {
                AddSoftware(dataPointerA, dataPointerB);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RemoveSoftware(in Mask256 other)
        {
            fixed (long* dataPointerA = this.data, dataPointerB = other.data)
            {
                RemoveSoftware(dataPointerA, dataPointerB);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void InvertSoftware()
        {
            fixed (long* dataPointer = this.data)
            {
                InvertSoftware(dataPointer);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool EqualsSoftware(in Mask256 other)
        {
            fixed (long* dataPointerA = this.data, dataPointerB = other.data)
            {
                return EqualsSoftware(dataPointerA, dataPointerB);
            }
        }
    }
}
