using System;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace Lecs.Memory
{
    public unsafe partial struct Mask256 : IEquatable<Mask256>
    {
        public const int MaxEntries = 256;

        private fixed long data[4]; // 4 * 64 bit = 256 bit

        public static Mask256 Default { get; } = default(Mask256);

        public static bool operator ==(in Mask256 a, in Mask256 b) => a.Equals(in b);

        public static bool operator !=(in Mask256 a, in Mask256 b) => !a.Equals(in b);

        public static Mask256 Create(byte bit)
        {
            Span<byte> bits = stackalloc byte[] { bit };
            return Create(bits);
        }

        public static Mask256 Create(Span<byte> bits)
        {
            var result = default(Mask256);
            foreach (byte bit in bits)
            {
                for (int i = 0; i < 4; i++)
                {
                    var lowerLimit = i * 64; // 64 bit per long
                    var upperLimit = (i + 1) * 64; // 64 bit per long
                    if (bit < upperLimit)
                    {
                        result.data[i] |= 1L << (bit - lowerLimit);
                        break;
                    }
                }
            }

            return result;
        }

        public bool HasAll(in Mask256 other)
        {
            if (Avx2.IsSupported)
                return HasAllAvx2(in other);
            else
            if (Avx.IsSupported)
                return HasAllAvx(in other);
            else
                return HasAllSoftware(in other);
        }

        public bool HasAny(in Mask256 other)
        {
            if (Avx.IsSupported)
                return HasAnyAvx(in other);
            else
                return HasAnySoftware(in other);
        }

        public void Add(in Mask256 other)
        {
            if (Avx2.IsSupported)
                AddAvx2(in other);
            else
            if (Avx.IsSupported)
                AddAvx(in other);
            else
                AddSoftware(in other);
        }

        public void Remove(in Mask256 other)
        {
            if (Avx2.IsSupported)
                RemoveAvx2(in other);
            else
            if (Avx.IsSupported)
                RemoveAvx(in other);
            else
                RemoveSoftware(in other);
        }

        public void Invert()
        {
            if (Avx2.IsSupported)
                InvertAvx2();
            else
            if (Avx.IsSupported)
                InvertAvx();
            else
                InvertSoftware();
        }

        public void Clear()
        {
            if (Avx2.IsSupported)
                ClearAvx2();
            else
                ClearSoftware();
        }

        public override int GetHashCode()
        {
            var hashcode = default(HashCode);
            fixed (long* dataPointer = this.data)
            {
                for (int i = 0; i < 4; i++)
                    hashcode.Add(dataPointer[i]);
                return hashcode.ToHashCode();
            }
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder(capacity: 256);
            fixed (long* dataPointer = this.data)
            {
                for (int i = 0; i < 4; i++)
                {
                    long mask = 1L;
                    for (int j = 0; j < 64; j++)
                    {
                        stringBuilder.Append((dataPointer[i] & mask) != 0 ? "1" : "0");
                        mask <<= 1;
                    }
                }
            }

            return stringBuilder.ToString();
        }

        public override bool Equals(object obj) => (obj is Mask256) && this.Equals((Mask256)obj);

        public bool Equals(in Mask256 other)
        {
            if (Avx2.IsSupported)
                return EqualsAvx2(in other);
            else
            if (Avx.IsSupported)
                return EqualsAvx(in other);
            else
                return EqualsSoftware(in other);
        }

        // Prefer to use the Equals method with 'in' semantics to avoid copying this relatively big
        // struct around.
        bool IEquatable<Mask256>.Equals(Mask256 other) => this.Equals(other);
    }
}
