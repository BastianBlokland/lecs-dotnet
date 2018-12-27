using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;

using static Lecs.Memory.Mask256;

namespace Lecs.Memory
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 32)] // 4 * 8 byte = 32 byte
    public unsafe partial struct ReadOnlyMask256 : IEquatable<ReadOnlyMask256>, IEquatable<Mask256>
    {
        private static ReadOnlyMask256 empty = default(ReadOnlyMask256);

        internal fixed long data[4]; // 4 * 64 bit = 256 bit

        public static ref ReadOnlyMask256 Empty => ref empty;

        public static bool operator ==(in ReadOnlyMask256 a, in ReadOnlyMask256 b) => a.Equals(in b);

        public static bool operator ==(in ReadOnlyMask256 a, in Mask256 b) => a.Equals(in b);

        public static bool operator !=(in ReadOnlyMask256 a, in ReadOnlyMask256 b) => !a.Equals(in b);

        public static bool operator !=(in ReadOnlyMask256 a, in Mask256 b) => !a.Equals(in b);

        public static ReadOnlyMask256 Create(byte bit)
        {
            var result = default(ReadOnlyMask256);
            SetBitSoftware(result.data, bit);
            return result;
        }

        public static ReadOnlyMask256 Create(ReadOnlySpan<byte> bits)
        {
            var result = default(ReadOnlyMask256);
            SetBitsSoftware(result.data, bits);
            return result;
        }

        public Mask256 AsMutable() =>
            // Note: Its safe to cast these around like this because they have identical memory layout
            // and this returns a COPY so you won't be able to change the original
            Unsafe.As<ReadOnlyMask256, Mask256>(ref this);

        public bool HasAll(in ReadOnlyMask256 other)
        {
            fixed (long* dataPointerA = this.data, dataPointerB = other.data)
            {
                if (Avx.IsSupported)
                    return HasAllAvx(dataPointerA, dataPointerB);
                else
                    return HasAllSoftware(dataPointerA, dataPointerB);
            }
        }

        public bool HasAll(in Mask256 other)
        {
            fixed (long* dataPointerA = this.data, dataPointerB = other.data)
            {
                if (Avx.IsSupported)
                    return HasAllAvx(dataPointerA, dataPointerB);
                else
                    return HasAllSoftware(dataPointerA, dataPointerB);
            }
        }

        public bool HasAny(in ReadOnlyMask256 other)
        {
            fixed (long* dataPointerA = this.data, dataPointerB = other.data)
            {
                if (Avx.IsSupported)
                    return HasAnyAvx(dataPointerA, dataPointerB);
                else
                    return HasAnySoftware(dataPointerA, dataPointerB);
            }
        }

        public bool HasAny(in Mask256 other)
        {
            fixed (long* dataPointerA = this.data, dataPointerB = other.data)
            {
                if (Avx.IsSupported)
                    return HasAnyAvx(dataPointerA, dataPointerB);
                else
                    return HasAnySoftware(dataPointerA, dataPointerB);
            }
        }

        public bool NotHasAny(in ReadOnlyMask256 other)
        {
            fixed (long* dataPointerA = this.data, dataPointerB = other.data)
            {
                if (Avx.IsSupported)
                    return NotHasAnyAvx(dataPointerA, dataPointerB);
                else
                    return NotHasAnySoftware(dataPointerA, dataPointerB);
            }
        }

        public bool NotHasAny(in Mask256 other)
        {
            fixed (long* dataPointerA = this.data, dataPointerB = other.data)
            {
                if (Avx.IsSupported)
                    return NotHasAnyAvx(dataPointerA, dataPointerB);
                else
                    return NotHasAnySoftware(dataPointerA, dataPointerB);
            }
        }

        public override int GetHashCode()
        {
            fixed (long* dataPointer = this.data)
            {
                return GetHashCodeSoftware(dataPointer);
            }
        }

        public override string ToString()
        {
            fixed (long* dataPointer = this.data)
            {
                return ToStringSoftware(dataPointer);
            }
        }

        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case ReadOnlyMask256 readOnlyMask:
                    return this.Equals(readOnlyMask);
                case Mask256 mask:
                    return this.Equals(mask);
                default:
                    return false;
            }
        }

        public bool Equals(in ReadOnlyMask256 other)
        {
            fixed (long* dataPointerA = this.data, dataPointerB = other.data)
            {
                if (Avx2.IsSupported)
                    return EqualsAvx2(dataPointerA, dataPointerB);
                else
                if (Avx.IsSupported)
                    return EqualsAvx(dataPointerA, dataPointerB);
                else
                    return EqualsSoftware(dataPointerA, dataPointerB);
            }
        }

        public bool Equals(in Mask256 other)
        {
            fixed (long* dataPointerA = this.data, dataPointerB = other.data)
            {
                if (Avx2.IsSupported)
                    return EqualsAvx2(dataPointerA, dataPointerB);
                else
                if (Avx.IsSupported)
                    return EqualsAvx(dataPointerA, dataPointerB);
                else
                    return EqualsSoftware(dataPointerA, dataPointerB);
            }
        }

        // Prefer to use the Equals method with 'in' semantics to avoid copying this struct around.
        bool IEquatable<ReadOnlyMask256>.Equals(ReadOnlyMask256 other) => this.Equals(other);

        // Prefer to use the Equals method with 'in' semantics to avoid copying this struct around.
        bool IEquatable<Mask256>.Equals(Mask256 other) => this.Equals(other);
    }
}
