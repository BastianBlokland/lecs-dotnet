using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

using Lecs.Memory;

namespace Lecs.Benchmark.Memory
{
    public struct TestStruct
    {
        public TestStruct(double data1, double data2, double data3, double data4)
        {
            this.Data1 = data1;
            this.Data2 = data2;
            this.Data3 = data3;
            this.Data4 = data4;
        }

        public double Data1 { get; set; }

        public double Data2 { get; set; }

        public double Data3 { get; set; }

        public double Data4 { get; set; }

        public static TestStruct CreateRandom(Random rand) =>
            new TestStruct(rand.NextDouble(), rand.NextDouble(), rand.NextDouble(), rand.NextDouble());
    }

    public class IntMap_RandomInsert_Benchmark
    {
        private const int ElementCount = 100_000;

        private readonly Dictionary<int, TestStruct> dictionary = new Dictionary<int, TestStruct>();
        private readonly IntMap<TestStruct> intMap = new IntMap<TestStruct>();

        private readonly int[] testKeys = new int[ElementCount];
        private readonly TestStruct[] testValues = new TestStruct[ElementCount];

        public IntMap_RandomInsert_Benchmark()
        {
            // Initialize test data
            var rand = new Random(Seed: 1);
            for (int i = 0; i < ElementCount; i++)
            {
                this.testKeys[i] = (int)((long)int.MinValue + ((long)int.MaxValue * 2 * rand.NextDouble()));
                this.testValues[i] = TestStruct.CreateRandom(rand);

                // Prewarm the maps
                this.dictionary[this.testKeys[i]] = this.testValues[i];
                this.intMap.Add(this.testKeys[i], this.testValues[i]);
            }
        }

        [Benchmark(Baseline = true)]
        public void RandomInsert_Dictionary()
        {
            this.dictionary.Clear();
            for (int i = 0; i < ElementCount; i++)
                this.dictionary[this.testKeys[i]] = this.testValues[i];
        }

        [Benchmark]
        public void RandomInsert_IntMap()
        {
            this.intMap.Clear();
            for (int i = 0; i < ElementCount; i++)
            {
                this.intMap.GetSlot(this.testKeys[i], out var slot, addIfMissing: true);
                this.intMap.GetValueRef(slot) = this.testValues[i];
            }
        }
    }

    public class IntMap_RandomAccess_Benchmark
    {
        private const int ElementCount = 100_000;

        private readonly Dictionary<int, TestStruct> dictionary = new Dictionary<int, TestStruct>();
        private readonly IntMap<TestStruct> intMap = new IntMap<TestStruct>();

        private readonly int[] testKeys = new int[ElementCount];

        public IntMap_RandomAccess_Benchmark()
        {
            // Initialize test data
            var rand = new Random(Seed: 1);
            for (int i = 0; i < ElementCount; i++)
            {
                this.testKeys[i] = rand.Next(maxValue: 250_000);// (int)((long)int.MinValue + ((long)int.MaxValue * 2 * rand.NextDouble()));
                var testValue = TestStruct.CreateRandom(rand);

                this.dictionary[this.testKeys[i]] = testValue;
                this.intMap.Add(this.testKeys[i], testValue);
            }
        }

        [Benchmark(Baseline = true)]
        public double RandomAccess_Dictionary()
        {
            double result = 0d;
            for (int i = 0; i < ElementCount; i++)
                result += this.dictionary[this.testKeys[i]].Data1;
            return result;
        }

        [Benchmark]
        public double RandomAccess_IntMap()
        {
            double result = 0d;
            for (int i = 0; i < ElementCount; i++)
            {
                this.intMap.GetSlot(this.testKeys[i], out var slot);
                result += this.intMap.GetValueRef(slot).Data1;
            }

            return result;
        }
    }

    public class IntMap_RandomUpdate_Benchmark
    {
        private const int ElementCount = 100_000;

        private readonly Dictionary<int, TestStruct> dictionary = new Dictionary<int, TestStruct>();
        private readonly IntMap<TestStruct> intMap = new IntMap<TestStruct>();

        private readonly int[] testKeys = new int[ElementCount];
        private readonly TestStruct[] testValues = new TestStruct[ElementCount];
        private int offset;

        public IntMap_RandomUpdate_Benchmark()
        {
            // Initialize test data
            var rand = new Random(Seed: 1);
            for (int i = 0; i < ElementCount; i++)
            {
                this.testKeys[i] = (int)((long)int.MinValue + ((long)int.MaxValue * 2 * rand.NextDouble()));
                this.testValues[i] = TestStruct.CreateRandom(rand);

                this.dictionary[this.testKeys[i]] = this.testValues[i];
                this.intMap.Add(this.testKeys[i], this.testValues[i]);
            }
        }

        [Benchmark(Baseline = true)]
        public void RandomUpdate_Dictionary()
        {
            this.offset = unchecked(this.offset + 1);
            for (int i = 0; i < ElementCount; i++)
            {
                ref TestStruct value = ref this.testValues[(i + this.offset) % ElementCount];
                this.dictionary[this.testKeys[i]] = value;
            }
        }

        [Benchmark]
        public void RandomUpdate_IntMap()
        {
            this.offset = unchecked(this.offset + 1);
            for (int i = 0; i < ElementCount; i++)
            {
                ref TestStruct value = ref this.testValues[(i + this.offset) % ElementCount];
                this.intMap.GetSlot(this.testKeys[i], out var slot);
                this.intMap.GetValueRef(slot) = value;
            }
        }
    }

    public class IntMap_Enumerate_Benchmark
    {
        private const int ElementCount = 100_000;

        private readonly Dictionary<int, TestStruct> dictionary = new Dictionary<int, TestStruct>();
        private readonly IntMap<TestStruct> intMap = new IntMap<TestStruct>();

        public IntMap_Enumerate_Benchmark()
        {
            // Initialize test data
            var rand = new Random(Seed: 1);
            for (int i = 0; i < ElementCount; i++)
            {
                int testKey = (int)((long)int.MinValue + ((long)int.MaxValue * 2 * rand.NextDouble()));
                var testValue = TestStruct.CreateRandom(rand);

                this.dictionary[testKey] = testValue;
                this.intMap.Add(testKey, testValue);
            }
        }

        [Benchmark(Baseline = true)]
        public double Enumerate_Dictionary()
        {
            double result = 0;
            foreach (var kvp in this.dictionary)
                result += kvp.Value.Data1;
            return result;
        }

        [Benchmark]
        public double Enumerate_IntMap()
        {
            double result = 0;
            foreach (var slot in this.intMap)
                result += this.intMap.GetValueRef(slot).Data1;
            return result;
        }
    }
}
