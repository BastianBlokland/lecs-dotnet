using System;
using BenchmarkDotNet.Attributes;

using Lecs.Memory;

namespace Lecs.Benchmark.Memory
{
    public class Mask256_HasAll_Benchmark : Mask256_BaseBenchmark
    {
        [Benchmark(Baseline = true)]
        public bool HasAll_ReferenceMask64()
        {
            var toCheck = referenceMasks[0];
            var result = false;
            for (int i = 1; i < referenceMasks.Length; i++)
                result |= referenceMasks[i].HasAll(toCheck);
            return result;
        }

        [Benchmark]
        public bool HasAll_SoftwareMask256()
        {
            var toCheck = masks[0];
            var result = false;
            for (int i = 1; i < masks.Length; i++)
                result |= masks[i].HasAllSoftware(in toCheck);
            return result;
        }

        [Benchmark]
        public bool HasAll_AvxMask256()
        {
            var toCheck = masks[0];
            var result = false;
            for (int i = 1; i < masks.Length; i++)
                result |= masks[i].HasAllAvx(in toCheck);
            return result;
        }
    }

    public class Mask256_HasAny_Benchmark : Mask256_BaseBenchmark
    {
        [Benchmark(Baseline = true)]
        public bool HasAny_ReferenceMask64()
        {
            var toCheck = referenceMasks[0];
            var result = false;
            for (int i = 1; i < referenceMasks.Length; i++)
                result |= referenceMasks[i].HasAny(toCheck);
            return result;
        }

        [Benchmark]
        public bool HasAny_SoftwareMask256()
        {
            var toCheck = masks[0];
            var result = false;
            for (int i = 1; i < masks.Length; i++)
                result |= masks[i].HasAnySoftware(in toCheck);
            return result;
        }

        [Benchmark]
        public bool HasAny_AvxMask256()
        {
            var toCheck = masks[0];
            var result = false;
            for (int i = 1; i < masks.Length; i++)
                result |= masks[i].HasAnyAvx(in toCheck);
            return result;
        }
    }

    public class Mask256_NotHasAny_Benchmark : Mask256_BaseBenchmark
    {
        [Benchmark(Baseline = true)]
        public bool NotHasAny_ReferenceMask64()
        {
            var toCheck = referenceMasks[0];
            var result = false;
            for (int i = 1; i < referenceMasks.Length; i++)
                result |= referenceMasks[i].NotHasAny(toCheck);
            return result;
        }

        [Benchmark]
        public bool NotHasAny_SoftwareMask256()
        {
            var toCheck = masks[0];
            var result = false;
            for (int i = 1; i < masks.Length; i++)
                result |= masks[i].NotHasAnySoftware(in toCheck);
            return result;
        }

        [Benchmark]
        public bool NotHasAny_AvxMask256()
        {
            var toCheck = masks[0];
            var result = false;
            for (int i = 1; i < masks.Length; i++)
                result |= masks[i].NotHasAnyAvx(in toCheck);
            return result;
        }
    }

    public class Mask256_Add_Benchmark : Mask256_BaseBenchmark
    {
        /*
        Note: These benchmarks have the side-effect of actually changing the data, which is bad but
        in this case i know that none of the implementations benefit from specific input-data and
        making copies affects the results too much.
        */

        [Benchmark(Baseline = true)]
        public void Add_ReferenceMask64()
        {
            var toAdd = referenceMasks[0];
            for (int i = 1; i < referenceMasks.Length; i++)
                referenceMasks[i].Add(toAdd);
        }

        [Benchmark]
        public void Add_SoftwareMask256()
        {
            var toAdd = masks[0];
            for (int i = 1; i < masks.Length; i++)
                masks[i].AddSoftware(in toAdd);
        }

        [Benchmark]
        public void Add_AvxMask256()
        {
            var toAdd = masks[0];
            for (int i = 1; i < masks.Length; i++)
                masks[i].AddAvx(in toAdd);
        }

        [Benchmark]
        public void Add_Avx2Mask256()
        {
            var toAdd = masks[0];
            for (int i = 1; i < masks.Length; i++)
                masks[i].AddAvx2(in toAdd);
        }
    }

    public class Mask256_Remove_Benchmark : Mask256_BaseBenchmark
    {
        /*
        Note: These benchmarks have the side-effect of actually changing the data, which is bad but
        in this case i know that none of the implementations benefit from specific input-data and
        making copies affects the results too much.
        */

        [Benchmark(Baseline = true)]
        public void Remove_ReferenceMask64()
        {
            var toAdd = referenceMasks[0];
            for (int i = 1; i < referenceMasks.Length; i++)
                referenceMasks[i].Remove(toAdd);
        }

        [Benchmark]
        public void Remove_SoftwareMask256()
        {
            var toAdd = masks[0];
            for (int i = 1; i < masks.Length; i++)
                masks[i].RemoveSoftware(in toAdd);
        }

        [Benchmark]
        public void Remove_AvxMask256()
        {
            var toAdd = masks[0];
            for (int i = 1; i < masks.Length; i++)
                masks[i].RemoveAvx(in toAdd);
        }

        [Benchmark]
        public void Remove_Avx2Mask256()
        {
            var toAdd = masks[0];
            for (int i = 1; i < masks.Length; i++)
                masks[i].RemoveAvx2(in toAdd);
        }
    }

    public class Mask256_Invert_Benchmark : Mask256_BaseBenchmark
    {
        /*
        Note: These benchmarks have the side-effect of actually changing the data, which is bad but
        in this case i know that none of the implementations benefit from specific input-data and
        making copies affects the results too much.
        */

        [Benchmark(Baseline = true)]
        public void Invert_ReferenceMask64()
        {
            var toAdd = referenceMasks[0];
            for (int i = 1; i < referenceMasks.Length; i++)
                masks[i].Invert();
        }

        [Benchmark]
        public void Invert_SoftwareMask256()
        {
            var toAdd = masks[0];
            for (int i = 1; i < masks.Length; i++)
                masks[i].InvertSoftware();
        }

        [Benchmark]
        public void Invert_AvxMask256()
        {
            var toAdd = masks[0];
            for (int i = 1; i < masks.Length; i++)
                masks[i].InvertAvx();
        }

        [Benchmark]
        public void Invert_Avx2Mask256()
        {
            var toAdd = masks[0];
            for (int i = 1; i < masks.Length; i++)
                masks[i].InvertAvx2();
        }
    }

    public class Mask256_Clear_Benchmark : Mask256_BaseBenchmark
    {
        /*
        Note: These benchmarks have the side-effect of actually changing the data, which is bad but
        in this case i know that none of the implementations benefit from specific input-data and
        making copies affects the results too much.
        */

        [Benchmark(Baseline = true)]
        public void Clear_ReferenceMask64()
        {
            var toAdd = referenceMasks[0];
            for (int i = 1; i < referenceMasks.Length; i++)
                masks[i].Clear();
        }

        [Benchmark]
        public void Clear_SoftwareMask256()
        {
            var toAdd = masks[0];
            for (int i = 1; i < masks.Length; i++)
                masks[i].ClearSoftware();
        }

        [Benchmark]
        public void Clear_Avx2Mask256()
        {
            var toAdd = masks[0];
            for (int i = 1; i < masks.Length; i++)
                masks[i].ClearAvx2();
        }
    }

    public class Mask256_Equals_Benchmark : Mask256_BaseBenchmark
    {
        [Benchmark(Baseline = true)]
        public bool Equals_ReferenceMask64()
        {
            var toCheck = referenceMasks[0];
            var result = true;
            for (int i = 1; i < referenceMasks.Length; i++)
                result &= referenceMasks[i].Equals(toCheck);
            return result;
        }

        [Benchmark]
        public bool Equals_SoftwareMask256()
        {
            var toCheck = masks[0];
            var result = true;
            for (int i = 1; i < masks.Length; i++)
                result &= masks[i].EqualsSoftware(in toCheck);
            return result;
        }

        [Benchmark]
        public bool Equals_AvxMask256()
        {
            var toCheck = masks[0];
            var result = true;
            for (int i = 1; i < masks.Length; i++)
                result &= masks[i].EqualsAvx(in toCheck);
            return result;
        }

        [Benchmark]
        public bool Equals_Avx2Mask256()
        {
            var toCheck = masks[0];
            var result = true;
            for (int i = 1; i < masks.Length; i++)
                result &= masks[i].EqualsAvx2(in toCheck);
            return result;
        }
    }

    public abstract class Mask256_BaseBenchmark
    {
        protected const int ElementCount = 10_000;

        protected readonly ReferenceMask64[] referenceMasks = new ReferenceMask64[ElementCount];
        protected readonly Mask256[] masks = new Mask256[ElementCount];

        public Mask256_BaseBenchmark()
        {
            var testData = new byte[ElementCount * 2];
            new Random(Seed: 1).NextBytes(testData);

            // Initialize referenceMasks with a random bit
            for (int i = 0; i < ElementCount; i++)
            {
                referenceMasks[i] = ReferenceMask64.Create((byte)(testData[i] % 64));

                masks[i] = Mask256.Create(testData[i]);
            }

            // Combine masks to create more realistic masks
            for (int i = 0; i < ElementCount; i++)
            {
                // Take a random byte of testdata for how many masks to combine
                for (int j = 0; j < testData[i]; j++)
                {
                    var indexToAdd = (i + (j * 10)) % ElementCount;
                    referenceMasks[i].Add(referenceMasks[indexToAdd]);

                    masks[i].Add(in masks[indexToAdd]);
                }
            }
        }
    }

    /// <summary> Simplest mask implementation for benchmarking against </summary>
    public struct ReferenceMask64
    {
        private long data;

        private ReferenceMask64(byte bit) => data = 1L << bit;

        public static ReferenceMask64 Create(byte bit)
        {
            if (bit >= 64)
                throw new ArgumentException("Bit has to be below 64", nameof(bit));
            return new ReferenceMask64(bit);
        }

        public bool HasAll(ReferenceMask64 other) => (other.data & data) == other.data;

        public bool HasAny(ReferenceMask64 other) => (other.data & data) != 0;

        public bool NotHasAny(ReferenceMask64 other) => (other.data & data) == 0;

        public void Add(ReferenceMask64 other) => data |= other.data;

        public void Remove(ReferenceMask64 other) => data &= ~other.data;

        public void Invert() => data = ~data;

        public void Clear() => data = 0;

        public bool Equals(ReferenceMask64 other) => data == other.data;
    }
}
