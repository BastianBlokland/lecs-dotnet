using System;
using System.Runtime.Intrinsics.X86;

namespace Lecs.Memory
{
    public unsafe struct Mask256 : IEquatable<Mask256>
    {
        public const int MaxEntries = byte.MaxValue;

        private fixed int data[8]; // 8 * 32 bit = 256 bit

        public static Mask256 Default { get; } = default(Mask256);

        public static bool operator ==(in Mask256 a, in Mask256 b) => a.Equals(b);

        public static bool operator !=(in Mask256 a, in Mask256 b) => !a.Equals(b);

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
            {
                /*
                With Avx we can get the inverted result in a single 'testnzc' instruction so we only
                need do one invert at the end to get the result
                */

                fixed (int* dataPointer1 = this.data)
                fixed (int* dataPointer2 = other.data)
                {
                    var vectorA = Avx.LoadVector256(dataPointer1);
                    var vectorB = Avx.LoadVector256(dataPointer2);
                    return !Avx.TestNotZAndNotC(vectorA, vectorB);
                }
            }
            else
            {
                /* Software fallback */

                fixed (int* dataPointer1 = this.data)
                fixed (int* dataPointer2 = other.data)
                {
                    // Todo: Benchmark if branching would be faster here
                    bool result = true;
                    for (int i = 0; i < 8; i++)
                        result &= (dataPointer1[i] & dataPointer2[i]) == dataPointer2[i];
                    return result;
                }
            }
        }

        public bool NotHas(in Mask256 other)
        {
            if (Avx.IsSupported)
            {
                /* With Avx we can get the result with a single 'testc' instruction */

                fixed (int* dataPointer1 = this.data)
                fixed (int* dataPointer2 = other.data)
                {
                    var vectorA = Avx.LoadVector256(dataPointer1);
                    var vectorB = Avx.LoadVector256(dataPointer2);
                    return Avx.TestC(vectorA, vectorB);
                }
            }
            else
            {
                /* Software fallback */

                fixed (int* dataPointer1 = this.data)
                fixed (int* dataPointer2 = other.data)
                {
                    // Todo: Benchmark if branching would be faster here
                    bool result = true;
                    for (int i = 0; i < 8; i++)
                        result &= (dataPointer1[i] & dataPointer2[i]) == 0;
                    return result;
                }
            }
        }

        public void Add(in Mask256 other)
        {
            if (Avx2.IsSupported)
            {
                /* With Avx2 we can do a 256 bit or in a single instruction */

                fixed (int* dataPointer1 = this.data)
                fixed (int* dataPointer2 = other.data)
                {
                    var vectorA = Avx2.LoadVector256(dataPointer1);
                    var vectorB = Avx2.LoadVector256(dataPointer2);
                    var result = Avx2.Or(vectorA, vectorB);
                    Avx2.Store(dataPointer1, result);
                }
            }
            else
            if (Avx.IsSupported)
            {
                /* With Avx we need two 128 bit or instructions */

                fixed (int* dataPointer1 = this.data)
                fixed (int* dataPointer2 = other.data)
                {
                    // First 4 ints
                    var vectorA = Avx.LoadVector128(dataPointer1);
                    var vectorB = Avx.LoadVector128(dataPointer2);
                    var result = Avx.Or(vectorA, vectorB);
                    Avx.Store(dataPointer1, result);

                    // Second 4 ints
                    int* dataPointer1Plus4 = dataPointer1 + 4;
                    int* dataPointer2Plus4 = dataPointer2 + 4;
                    vectorA = Avx.LoadVector128(dataPointer1Plus4);
                    vectorB = Avx.LoadVector128(dataPointer2Plus4);
                    result = Avx.Or(vectorA, vectorB);
                    Avx.Store(dataPointer1Plus4, result);
                }
            }
            else
            {
                /* Software fallback */

                fixed (int* dataPointer1 = this.data)
                fixed (int* dataPointer2 = other.data)
                {
                    for (int i = 0; i < 8; i++)
                        dataPointer1[i] |= dataPointer2[i];
                }
            }
        }

        public void Remove(in Mask256 other)
        {
            if (Avx2.IsSupported)
            {
                /* With Avx2 we can do a single 256 bit AndNot instruction */

                fixed (int* dataPointer1 = this.data)
                fixed (int* dataPointer2 = other.data)
                {
                    var vectorA = Avx2.LoadVector256(dataPointer1);
                    var vectorB = Avx2.LoadVector256(dataPointer2);
                    var result = Avx2.AndNot(vectorB, vectorA);
                    Avx2.Store(dataPointer1, result);
                }
            }
            else
            if (Avx.IsSupported)
            {
                /* With Avx we need two 128 bit AndNot instructions */

                fixed (int* dataPointer1 = this.data)
                fixed (int* dataPointer2 = other.data)
                {
                    // First 4 ints
                    var vectorA = Avx.LoadVector128(dataPointer1);
                    var vectorB = Avx.LoadVector128(dataPointer2);
                    var result = Avx.AndNot(vectorB, vectorA);
                    Avx.Store(dataPointer1, result);

                    // Second 4 ints
                    int* dataPointer1Plus4 = dataPointer1 + 4;
                    int* dataPointer2Plus4 = dataPointer2 + 4;
                    vectorA = Avx.LoadVector128(dataPointer1Plus4);
                    vectorB = Avx.LoadVector128(dataPointer2Plus4);
                    result = Avx.AndNot(vectorB, vectorA);
                    Avx.Store(dataPointer1Plus4, result);
                }
            }
            else
            {
                /* Software fallback */

                fixed (int* dataPointer1 = this.data)
                fixed (int* dataPointer2 = other.data)
                {
                    for (int i = 0; i < 8; i++)
                        dataPointer1[i] &= ~dataPointer2[i];
                }
            }
        }

        public void Invert()
        {
            if (Avx2.IsSupported)
            {
                /*
                Weirdly enough there is no 'Not' instruction but according to stack-overflow the
                recommended way of doing it is a Xor with a 'constant' of all ones
                https://stackoverflow.com/questions/42613821/is-not-missing-from-sse-avx
                */

                fixed (int* dataPointer = this.data)
                {
                    var vector = Avx2.LoadVector256(dataPointer);
                    var allOne = Avx2.CompareEqual(vector, vector);
                    var result = Avx2.Xor(vector, allOne);
                    Avx2.Store(dataPointer, result);
                }
            }
            else
            if (Avx.IsSupported)
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
            else
            {
                /* Software fallback */

                fixed (int* dataPointer = this.data)
                {
                    for (int i = 0; i < 8; i++)
                        dataPointer[i] = ~dataPointer[i];
                }
            }
        }

        public void Clear()
        {
            if (Avx.IsSupported)
            {
                fixed (int* dataPointer = this.data)
                {
                    var zeroVector = Avx.SetZeroVector256<int>();
                    Avx.Store(dataPointer, zeroVector);
                }
            }
            else
            {
                /* Software fallback */

                fixed (int* dataPointer = this.data)
                {
                    for (int i = 0; i < 8; i++)
                        dataPointer[i] = 0;
                }
            }
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
            {
                /*
                With Avx2 we compare all bytes together (bytes because there is no 32 bit MoveMask?)
                Then we aggregate the results using 'MoveMask' and check if all bits are 1
                */

                fixed (int* dataPointer1 = this.data)
                fixed (int* dataPointer2 = other.data)
                {
                    var vectorA = Avx2.LoadVector256(dataPointer1).AsByte();
                    var vectorB = Avx2.LoadVector256(dataPointer2).AsByte();
                    var elementWiseResult = Avx2.CompareEqual(vectorA, vectorB);
                    return Avx2.MoveMask(elementWiseResult) == 0xfffffff;
                }
            }
            else
            if (Avx.IsSupported)
            {
                /* With Avx we do the same but in two steps of 128 bits */

                fixed (int* dataPointer1 = this.data)
                fixed (int* dataPointer2 = other.data)
                {
                    // First 4 ints
                    var vectorA = Avx.LoadVector128(dataPointer1).AsByte();
                    var vectorB = Avx.LoadVector128(dataPointer2).AsByte();
                    var elementWiseResult = Avx.CompareEqual(vectorA, vectorB);
                    if (Avx.MoveMask(elementWiseResult) != 0xfffffff)
                        return false;

                    // Second 4 ints
                    int* dataPointer1Plus4 = dataPointer1 + 4;
                    int* dataPointer2Plus4 = dataPointer2 + 4;
                    vectorA = Avx.LoadVector128(dataPointer1).AsByte();
                    vectorB = Avx.LoadVector128(dataPointer2).AsByte();
                    elementWiseResult = Avx.CompareEqual(vectorA, vectorB);
                    return Avx.MoveMask(elementWiseResult) == 0xfffffff;
                }
            }
            else
            {
                /* Software fallback */

                fixed (int* dataPointer1 = this.data)
                fixed (int* dataPointer2 = other.data)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if (dataPointer1[i] != dataPointer2[i])
                            return false;
                    }

                    return true;
                }
            }
        }

        // Prefer to use the Equals method with 'in' semantics to avoid copying this relatively big
        // struct around.
        bool IEquatable<Mask256>.Equals(Mask256 other) => this.Equals(other);
    }
}
