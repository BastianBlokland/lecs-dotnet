using System;
using BenchmarkDotNet.Attributes;

using Lecs.Memory;

namespace Lecs.Benchmark.Memory
{
    [MemoryDiagnoser]
    public class Mask256_AddRemove_Benchmark : Mask256_BaseBenchmark
    {
        [Benchmark(Baseline = true)]
        public void AddRemove_ReferenceMask64()
        {
            var toAdd = referenceMasks[0];
            for (int i = 1; i < referenceMasks.Length; i++)
            {
                referenceMasks[i].Add(toAdd);
                referenceMasks[i].Remove(toAdd);
            }
        }

        [Benchmark]
        public void AddRemove_SoftwareMask256()
        {
            var toAdd = masks[0];
            for (int i = 1; i < masks.Length; i++)
            {
                masks[i].AddSoftware(in toAdd);
                masks[i].RemoveSoftware(in toAdd);
            }
        }

        [Benchmark]
        public void AddRemove_AvxMask256()
        {
            var toAdd = masks[0];
            for (int i = 1; i < masks.Length; i++)
            {
                masks[i].AddAvx(in toAdd);
                masks[i].RemoveAvx(in toAdd);
            }
        }

        [Benchmark]
        public void AddRemove_Avx2Mask256()
        {
            var toAdd = masks[0];
            for (int i = 1; i < masks.Length; i++)
            {
                masks[i].AddAvx2(in toAdd);
                masks[i].RemoveAvx2(in toAdd);
            }
        }
    }

    [MemoryDiagnoser]
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

    [MemoryDiagnoser]
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

    [MemoryDiagnoser]
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

    public abstract class Mask256_BaseBenchmark
    {
        protected const int ElementCount = 1000;

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

                masks[i] = Mask256.Create(testData[i], isMutable: false);
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
    }
}
