using System;
using System.Runtime.Intrinsics.X86;

namespace Lecs.Memory
{
    public unsafe partial struct Mask256 : IEquatable<Mask256>
    {
        public const int MaxEntries = byte.MaxValue;

        private fixed int data[8]; // 8 * 32 bit = 256 bit

        public static Mask256 Default { get; } = default(Mask256);

        public static bool operator ==(in Mask256 a, in Mask256 b) => a.Equals(in b);

        public static bool operator !=(in Mask256 a, in Mask256 b) => !a.Equals(in b);

        public static Mask256 Create(byte bit)
        {
            var result = default(Mask256);
            for (int i = 0; i < 8; i++)
            {
                var lowerLimit = i * 32; // 32 bit per int
                var upperLimit = (i + 1) * 32; // 32 bit per int
                if (bit < upperLimit)
                    result.data[i] = 1 << (bit - lowerLimit);
            }

            return result;
        }

        public bool Has(in Mask256 other)
        {
            if (Avx.IsSupported)
                return HasAvx(in other);
            else
                return HasSoftware(in other);
        }

        public bool NotHas(in Mask256 other)
        {
            if (Avx.IsSupported)
                return NotHasAvx(in other);
            else
                return NotHasSoftware(in other);
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
            if (Avx.IsSupported)
                ClearAvx();
            else
                ClearSoftware();
        }

        public override int GetHashCode()
        {
            var hashcode = default(HashCode);
            fixed (int* dataPointer = this.data)
            {
                for (int i = 0; i < 8; i++)
                    hashcode.Add(dataPointer[i]);
                return hashcode.ToHashCode();
            }
        }

        public override bool Equals(object obj) =>
            (obj is Mask256) && this.Equals((Mask256)obj);

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
