using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;

namespace Lecs.Memory
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 32)] // 4 * 8 byte = 32 byte
    public unsafe partial struct Mask256 : IEquatable<Mask256>
    {
        internal fixed long data[4]; // 4 * 64 bit = 256 bit

        public static bool operator ==(in Mask256 a, in Mask256 b) => a.Equals(in b);

        public static bool operator !=(in Mask256 a, in Mask256 b) => !a.Equals(in b);

        public static Mask256 Create() => default(Mask256);

        public static Mask256 Create(byte bit)
        {
            var result = default(Mask256);
            SetBitSoftware(result.data, bit);
            return result;
        }

        public static Mask256 Create(ReadOnlySpan<byte> bits)
        {
            var result = default(Mask256);
            SetBitsSoftware(result.data, bits);
            return result;
        }

        public ReadOnlyMask256 AsReadOnly() =>
            // Note: Its safe to cast these around like this because they have identical memory layout
            // and this returns a COPY so you won't be able to change the original
            Unsafe.As<Mask256, ReadOnlyMask256>(ref Unsafe.AsRef(in this));

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

        public void Add(in Mask256 other)
        {
            fixed (long* dataPointerA = this.data, dataPointerB = other.data)
            {
                if (Avx2.IsSupported)
                    AddAvx2(dataPointerA, dataPointerB);
                else
                if (Avx.IsSupported)
                    AddAvx(dataPointerA, dataPointerB);
                else
                    AddSoftware(dataPointerA, dataPointerB);
            }
        }

        public void Add(in ReadOnlyMask256 other)
        {
            fixed (long* dataPointerA = this.data, dataPointerB = other.data)
            {
                if (Avx2.IsSupported)
                    AddAvx2(dataPointerA, dataPointerB);
                else
                if (Avx.IsSupported)
                    AddAvx(dataPointerA, dataPointerB);
                else
                    AddSoftware(dataPointerA, dataPointerB);
            }
        }

        public void Remove(in Mask256 other)
        {
            fixed (long* dataPointerA = this.data, dataPointerB = other.data)
            {
                if (Avx2.IsSupported)
                    RemoveAvx2(dataPointerA, dataPointerB);
                else
                if (Avx.IsSupported)
                    RemoveAvx(dataPointerA, dataPointerB);
                else
                    RemoveSoftware(dataPointerA, dataPointerB);
            }
        }

        public void Remove(in ReadOnlyMask256 other)
        {
            fixed (long* dataPointerA = this.data, dataPointerB = other.data)
            {
                if (Avx2.IsSupported)
                    RemoveAvx2(dataPointerA, dataPointerB);
                else
                if (Avx.IsSupported)
                    RemoveAvx(dataPointerA, dataPointerB);
                else
                    RemoveSoftware(dataPointerA, dataPointerB);
            }
        }

        public void Invert()
        {
            fixed (long* dataPointer = this.data)
            {
                if (Avx2.IsSupported)
                    InvertAvx2(dataPointer);
                else
                if (Avx.IsSupported)
                    InvertAvx(dataPointer);
                else
                    InvertSoftware(dataPointer);
            }
        }

        public void Clear()
        {
            fixed (long* dataPointer = this.data)
            {
                if (Avx2.IsSupported)
                    ClearAvx2(dataPointer);
                else
                    ClearSoftware(dataPointer);
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

        public override bool Equals(object obj) => (obj is Mask256) && this.Equals((Mask256)obj);

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

        // Prefer to use the Equals method with 'in' semantics to avoid copying this relatively big
        // struct around.
        bool IEquatable<Mask256>.Equals(Mask256 other) => this.Equals(other);
    }
}
