using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace Lecs.Benchmark
{
    [MemoryDiagnoser]
    public class DummyBenchmark
    {
        private const int DataSize = 10000;
        private readonly byte[] data;

        public DummyBenchmark()
        {
            this.data = new byte[DataSize];
            new Random(42).NextBytes(this.data);
        }

        [Params(1000)]
        public int ElementCount { get; set; }

        [Benchmark(Baseline = true)]
        public void ClearAndFillList()
        {
            var list = new List<byte>();
            for (int i = 0; i < this.ElementCount; i++)
                list.Add(this.data[i]);
        }

        [Benchmark]
        public void ClearAndFillDictionary()
        {
            var dictionary = new Dictionary<int, byte>();
            for (int i = 0; i < this.ElementCount; i++)
                dictionary.Add(i, this.data[i]);
        }
    }
}
