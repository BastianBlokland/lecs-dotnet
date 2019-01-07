using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

using Lecs.Memory;
using Lecs.Tests.Attributes;

namespace Lecs.Tests.Memory
{
    public sealed class IntMapTests
    {
        [Fact]
        public static void Construct_WithOutOfBounds_InitialCapacity_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                testCode: () => new IntMap<float>(initialCapacity: -1));
            Assert.Throws<ArgumentOutOfRangeException>(
                testCode: () => new IntMap<float>(initialCapacity: int.MaxValue));
        }

        [Fact]
        public static void Construct_WithOutOfBounds_MaxLoadFactor_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                testCode: () => new IntMap<float>(initialCapacity: 2, maxLoadFactor: 0f));
            Assert.Throws<ArgumentOutOfRangeException>(
                testCode: () => new IntMap<float>(initialCapacity: 2, maxLoadFactor: 1f));
        }

        [Fact]
        public static void Set_AddingNewIncrementsCount()
        {
            const int AddCount = 25;

            var map = new IntMap<float>(initialCapacity: 2);

            for (int i = 0; i < AddCount; i++)
            {
                Assert.Equal(i, map.Count);
                map.Set(key: i, value: 1f);
            }
            Assert.Equal(AddCount, map.Count);
        }

        [Fact]
        public static void Set_UpdatingSameKeyDoesNotIncrementCount()
        {
            const int TestKey = -234928;

            var map = new IntMap<float>(initialCapacity: 2);
            map[TestKey] = 23423;
            map[TestKey] = 836;
            map[TestKey] = 93836;
            map[TestKey] = 283467;

            Assert.Equal(1, map.Count);
        }

        [Fact]
        public static void Remove_RemovedItemsNoLongerAppear()
        {
            // Force a low initial capacity so we test growing a few times
            var map = new IntMap<double>(initialCapacity: 2);

            // Insert test data in the map
            var rand = new Random(Seed: 1);
            for (int i = 0; i < 10_000; i++)
            {
                int key = rand.Next(maxValue: 1000);
                double value = rand.NextDouble();
                map[key] = value;
            }

            // Take note of all keys above 500
            var keysAbove500 = map.
                Select(slot => map.GetKey(slot)).
                Where(key => key > 500).
                ToArray();

            // Assert that the map contains more then just the keys above 500
            Assert.NotEqual(keysAbove500.Length, map.Count);

            // Remove all keys below or equal to 500
            map.RemoveAll(map.
                Select(slot => map.GetKey(slot)).
                Where(key => key <= 500).
                ToArray());

            // Assert that the map now only contains the keys above 500
            Assert.Equal(keysAbove500.Length, map.Count);

            var remainingKeys = map.Select(slot => map.GetKey(slot)).ToArray();
            keysAbove500.SequenceEqual(remainingKeys);
        }

        [Fact]
        public static void Remove_RandomRemoveWorksAsExpected()
        {
            /* This test tries to simulate 'normal' usage where keys are inserted and removed
            at random, behaviour is verified against a corefx dictionary */

            var map = new IntMap<double>();
            var referenceDict = new Dictionary<int, double>();

            var rand = new Random(Seed: 1);
            for (int i = 0; i < 1000; i++)
            {
                // Insert x items
                for (int j = 0; j < rand.Next(maxValue: 1000); j++)
                {
                    int key = rand.Next(maxValue: 10_000);
                    double value = rand.NextDouble();

                    map[key] = value;
                    referenceDict[key] = value;
                }

                // Remove x items
                for (int j = 0; j < rand.Next(maxValue: 1000); j++)
                {
                    int key = rand.Next(maxValue: 10_000);
                    map.Remove(key);
                    referenceDict.Remove(key);
                }
            }

            // Assert that both maps contain the same data
            Assert.Equal(referenceDict.Count, map.Count);
            foreach (var kvp in referenceDict)
            {
                IntMap.SlotToken slot;
                Assert.True(map.Find(kvp.Key, out slot));
                Assert.Equal(kvp.Key, map.GetKey(slot));
                Assert.Equal(kvp.Value, map.GetValue(slot));
            }
        }

        [Fact]
        public static void Get_ItemCanBeRetrieved()
        {
            // Force a low initial capacity so we test growing a few times
            var map = new IntMap<double>(initialCapacity: 2);
            var referenceDict = new Dictionary<int, double>();

            // Insert test data in both maps
            var rand = new Random(Seed: 1);
            for (int i = 0; i < 10_000; i++)
            {
                // Note: Might be duplicate keys but thats fine
                int key = rand.Next();
                double value = rand.NextDouble();

                map[key] = value;
                referenceDict[key] = value;
            }

            // Assert that both maps contain the same amount of entries
            Assert.Equal(referenceDict.Count, map.Count);

            // Assert that all keys from the reference dict can be found in our map
            foreach (var kvp in referenceDict)
                Assert.Equal(kvp.Value, map.GetValue(kvp.Key));

            // Get all the keys that are returned from the map enumerator
            var keys = map.Select(slot => map.GetKey(slot)).ToArray();

            // Assert that the map enumerator lists all entries
            Assert.Equal(referenceDict.Count, keys.Length);
            foreach (var referenceKey in referenceDict.Keys)
                Assert.Contains(referenceKey, keys);
        }

        [Fact]
        public static void Get_ItemCanBeUpdatedThroughRef()
        {
            const int TestKey = -234928;

            var map = new IntMap<int>(initialCapacity: 2);

            var slot = map.Set(TestKey, value: 10);
            ref var valRef = ref map.GetValue(slot);

            Assert.Equal(10, valRef);

            valRef = 20;

            Assert.Equal(20, map.GetValue(TestKey));
        }

        [Fact]
        public static void Clear_ResultsInZeroCount()
        {
            var map = new IntMap<float>
            {
                { 10, 20f },
                { 20, 30f }
            };

            Assert.Equal(2, map.Count);
            Assert.Equal(2, map.ToArray().Length);

            map.Clear();

            Assert.Equal(0, map.Count);
            Assert.Empty(map.ToArray());
        }
    }
}
