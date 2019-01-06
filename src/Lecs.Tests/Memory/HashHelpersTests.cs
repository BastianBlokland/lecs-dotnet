using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

using Lecs.Memory;

namespace Lecs.Tests.Memory
{
    public sealed class HashHelpersTests
    {
        [Fact]
        public static void Mix_SequentialNumbersCauseNonSequentialHashes()
        {
            /* For exhaustive test use 'int.MinValue' through 'int.MaxValue', but it makes the test
            take a long time, so thats why we only test -10.000 through 10.000 right now */

            const int MinTestVal = -10_000;
            const int MaxTestVal = 10_000;

            // Calculate the hashes for all the test integers
            int prev = HashHelpers.Mix(MinTestVal);
            for (int i = MinTestVal + 1; i < MaxTestVal; i++)
            {
                int mix = HashHelpers.Mix(i);
                Assert.True(Distance(prev, mix) > 1);
                prev = mix;
            }

            int Distance(int a, int b) => Math.Abs(a - b);
        }

        [Fact]
        public static void Mix_NoIntegerCausesACollision()
        {
            /* For exhaustive test use 'int.MinValue' through 'int.MaxValue', but it makes the test
            take a long time, so thats why we only test -10.000 through 10.000 right now */

            var hashes = new HashSet<int>();
            for (int i = -10_000; i < 10_000; i++)
            {
                int hash = HashHelpers.Mix(i);
                Assert.True(hashes.Add(hash));
            }
        }

        [Theory]
        [MemberData(nameof(GetPowersOfTwoData))]
        public static void PowerOfTwo_ModuloPowerOfTwoWorksLikeNormalModulo(int pot)
        {
            /* For exhaustive test use 'int.MinValue' through 'int.MaxValue', but it makes the test
            take a long time, so thats why we only test -1.000 through 1.000 right now */

            for (int i = -1_000; i < 1_000; i++)
            {
                /* For positive integers we expect it to work like normal modulo but for negative
                integers it will also make them positive */

                Assert.Equal(Math.Abs(i % pot), HashHelpers.ModuloPowerOfTwo(i, pot));
            }
        }

        [Theory]
        [MemberData(nameof(GetPowersOfTwoData))]
        public static void PowerOfTwo_AllPositivePowersOfTwoAreRecognized(int pot)
        {
            Assert.True(HashHelpers.IsPowerOfTwo(pot));
        }

        [Fact]
        public static void PowerOfTwo_AllNonZeroPositiveIntegersAreRecognized()
        {
            int[] powersOfTwo = GetPowersOfTwo().ToArray();

            /* For exhaustive test use 'int.MaxValue', but it makes the test take a long time, so
            thats why we only test the first 10.000 entries right now */

            // NOTE: Starting from 1 because we only accept non-zero positive integers
            for (int i = 1; i < 10_000; i++)
            {
                bool isPot = Array.IndexOf(powersOfTwo, i) >= 0;
                Assert.Equal(isPot, HashHelpers.IsPowerOfTwo(i));
            }
        }

        [Fact]
        public static void PowerOfTwo_IntegersAreRoundedUpToAPowerOfTwoCorrectly()
        {
            /* For exhaustive test take all elements, but it makes the test take a long time, so
            thats why we only test the first 10 entries right now */

            int[] powersOfTwo = GetPowersOfTwo().Take(10).ToArray();

            for (int i = 0; i < powersOfTwo.Length - 1; i++)
            {
                var potA = powersOfTwo[i];
                var potB = powersOfTwo[i + 1];

                Assert.Equal(potA, HashHelpers.RoundUpToPowerOfTwo(potA));
                Assert.Equal(potB, HashHelpers.NextPowerOfTwo(potA));

                for (int j = potA + 1; j < potB; j++)
                {
                    Assert.Equal(potB, HashHelpers.RoundUpToPowerOfTwo(j));
                    Assert.Equal(potB, HashHelpers.NextPowerOfTwo(j));
                }
            }
        }

        public static IEnumerable<object[]> GetPowersOfTwoData()
        {
            foreach (var pot in GetPowersOfTwo())
                yield return new object[] { pot };
        }

        private static IEnumerable<int> GetPowersOfTwo()
        {
            yield return 1;
            yield return 2;
            yield return 4;
            yield return 8;
            yield return 16;
            yield return 32;
            yield return 64;
            yield return 128;
            yield return 256;
            yield return 512;
            yield return 1024;
            yield return 2048;
            yield return 4096;
            yield return 8192;
            yield return 16384;
            yield return 32768;
            yield return 65536;
            yield return 131072;
            yield return 262144;
            yield return 524288;
            yield return 1048576;
            yield return 2097152;
            yield return 4194304;
            yield return 8388608;
            yield return 16777216;
            yield return 33554432;
            yield return 67108864;
            yield return 134217728;
            yield return 268435456;
            yield return 536870912;
            yield return 1073741824;
        }
    }
}
