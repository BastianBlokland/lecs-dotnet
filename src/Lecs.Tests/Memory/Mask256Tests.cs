using System;
using System.Collections.Generic;
using Xunit;

using Lecs.Memory;
using Lecs.Tests.Attributes;

namespace Lecs.Tests.Memory
{
    public sealed class Mask256Tests
    {
        [AvxFact]
        public static void Mask256_AsMutable_WorksAsExpected()
        {
            var readOnlyMask = ReadOnlyMask256.Create(new byte[] { 31, 63, 95, 127, 159, 191, 223, 255 });
            var mutableMask = readOnlyMask.AsMutable();
            Assert.True(readOnlyMask.Equals(mutableMask));
        }

        [AvxFact]
        public static void Mask256_AsReadOnly_WorksAsExpected()
        {
            var mutableMask = Mask256.Create(new byte[] { 31, 63, 95, 127, 159, 191, 223, 255 });
            var readOnlyMask = mutableMask.AsReadOnly();
            Assert.True(mutableMask.Equals(readOnlyMask));
        }

        [AvxFact]
        public static void Mask256_NotHasAny_FindsAny_Avx()
        {
            var maskA = Mask256.Create(bit: 100);
            var maskB = Mask256.Create(new byte[] { 50, 75, 100, 125 });
            Assert.False(maskA.NotHasAnyAvx(in maskB));
        }

        [AvxFact]
        public static void Mask256_NotHasAny_DoesntFindsAny_Avx()
        {
            var maskA = Mask256.Create(bit: 100);
            var maskB = Mask256.Create(new byte[] { 50, 75, 125 });
            Assert.True(maskA.NotHasAnyAvx(in maskB));
        }

        [Fact]
        public static void Mask256_NotHasAny_FindsAny_Software()
        {
            var maskA = Mask256.Create(bit: 100);
            var maskB = Mask256.Create(new byte[] { 50, 75, 100, 125 });
            Assert.False(maskA.NotHasAnySoftware(in maskB));
        }

        [Fact]
        public static void Mask256_NotHasAny_DoesntFindsAny_Software()
        {
            var maskA = Mask256.Create(bit: 100);
            var maskB = Mask256.Create(new byte[] { 50, 75, 125 });
            Assert.True(maskA.NotHasAnySoftware(in maskB));
        }

        [AvxFact]
        public static void Mask256_HasAll_FindsAll_Avx()
        {
            var maskA = Mask256.Create(new byte[] { 50, 75, 100, 125 });
            var maskB = Mask256.Create(new byte[] { 50, 75, 100 });
            Assert.True(maskA.HasAllAvx(in maskB));
        }

        [Fact]
        public static void Mask256_HasAll_FindsAll_Software()
        {
            var maskA = Mask256.Create(new byte[] { 50, 75, 100, 125 });
            var maskB = Mask256.Create(new byte[] { 50, 75, 100 });
            Assert.True(maskA.HasAllSoftware(in maskB));
        }

        [AvxFact]
        public static void Mask256_HasAll_DoesntFindAll_Avx()
        {
            var maskA = Mask256.Create(new byte[] { 75, 100, 125 });
            var maskB = Mask256.Create(new byte[] { 50, 75, 100 });
            Assert.False(maskA.HasAllAvx(in maskB));
        }

        [Fact]
        public static void Mask256_HasAll_DoesntFindAll_Software()
        {
            var maskA = Mask256.Create(new byte[] { 75, 100, 125 });
            var maskB = Mask256.Create(new byte[] { 50, 75, 100 });
            Assert.False(maskA.HasAllSoftware(in maskB));
        }

        [AvxFact]
        public static void Mask256_HasAny_FindsAny_Avx()
        {
            var maskA = Mask256.Create(bit: 100);
            var maskB = Mask256.Create(new byte[] { 50, 75, 100, 125 });
            Assert.True(maskA.HasAnyAvx(in maskB));
        }

        [AvxFact]
        public static void Mask256_HasAny_DoesntFindsAny_Avx()
        {
            var maskA = Mask256.Create(bit: 100);
            var maskB = Mask256.Create(new byte[] { 50, 75, 125 });
            Assert.False(maskA.HasAnyAvx(in maskB));
        }

        [Fact]
        public static void Mask256_HasAny_FindsAny_Software()
        {
            var maskA = Mask256.Create(bit: 100);
            var maskB = Mask256.Create(new byte[] { 50, 75, 100, 125 });
            Assert.True(maskA.HasAnySoftware(in maskB));
        }

        [Fact]
        public static void Mask256_HasAny_DoesntFindsAny_Software()
        {
            var maskA = Mask256.Create(bit: 100);
            var maskB = Mask256.Create(new byte[] { 50, 75, 125 });
            Assert.False(maskA.HasAnySoftware(in maskB));
        }

        [Avx2Theory]
        [MemberData(nameof(GetCombinedMasksData))]
        public static void Mask256_CombinedMask_ClearResultsInEmpty_Avx2(Mask256 mask)
        {
            Assert.False(mask.EqualsAvx2(default(Mask256)));
            mask.ClearAvx2();
            Assert.True(mask.EqualsAvx2(default(Mask256)));
        }

        [Theory]
        [MemberData(nameof(GetCombinedMasksData))]
        public static void Mask256_CombinedMask_ClearResultsInEmpty_Software(Mask256 mask)
        {
            Assert.False(mask.EqualsSoftware(default(Mask256)));
            mask.ClearSoftware();
            Assert.True(mask.EqualsSoftware(default(Mask256)));
        }

        [Avx2Theory]
        [MemberData(nameof(GetCombinedMasksData))]
        public static void Mask256_CombinedMask_CanBeAddedAndRemoved_Avx2(Mask256 mask)
        {
            var testMask = Mask256.Create();

            testMask.AddAvx2(in mask);
            Assert.True(testMask.EqualsAvx2(in mask));
            Assert.True(testMask.HasAllAvx(in mask));

            testMask.RemoveAvx2(mask);
            Assert.True(testMask.EqualsAvx2(default(Mask256)));
            Assert.False(testMask.EqualsAvx2(in mask));
            Assert.False(testMask.HasAnyAvx(in mask));
        }

        [AvxTheory]
        [MemberData(nameof(GetCombinedMasksData))]
        public static void Mask256_CombinedMask_CanBeAddedAndRemoved_Avx(Mask256 mask)
        {
            var testMask = Mask256.Create();

            testMask.AddAvx(in mask);
            Assert.True(testMask.EqualsAvx(in mask));
            Assert.True(testMask.HasAllAvx(in mask));

            testMask.RemoveAvx(mask);
            Assert.True(testMask.EqualsAvx(default(Mask256)));
            Assert.False(testMask.EqualsAvx(in mask));
            Assert.False(testMask.HasAnyAvx(in mask));
        }

        [Theory]
        [MemberData(nameof(GetCombinedMasksData))]
        public static void Mask256_CombinedMask_CanBeAddedAndRemoved_Software(Mask256 mask)
        {
            var defaultMask = Mask256.Create();

            defaultMask.AddSoftware(in mask);
            Assert.True(defaultMask.EqualsSoftware(in mask));
            Assert.True(defaultMask.HasAllSoftware(in mask));

            defaultMask.RemoveSoftware(in mask);
            Assert.True(defaultMask.EqualsSoftware(default(Mask256)));
            Assert.False(defaultMask.EqualsSoftware(in mask));
            Assert.False(defaultMask.HasAnySoftware(in mask));
        }

        [Avx2Theory]
        [MemberData(nameof(GetSingleMasksData))]
        public static void Mask256_SingleMask_InvertedMaskHasAllButOriginal_Avx2(Mask256 mask, byte elementNumber)
        {
            mask.InvertAvx2();
            for (int i = 0; i < 256; i++)
            {
                Mask256 testMask = Mask256.Create((byte)i);
                if (i == elementNumber)
                    Assert.False(mask.HasAnyAvx(in testMask));
                else
                    Assert.True(mask.HasAllAvx(in testMask));
            }
        }

        [AvxTheory]
        [MemberData(nameof(GetSingleMasksData))]
        public static void Mask256_SingleMask_InvertedMaskHasAllButOriginal_Avx(Mask256 mask, byte elementNumber)
        {
            mask.InvertAvx();
            for (int i = 0; i < 256; i++)
            {
                Mask256 testMask = Mask256.Create((byte)i);
                if (i == elementNumber)
                    Assert.False(mask.HasAnyAvx(in testMask));
                else
                    Assert.True(mask.HasAllAvx(in testMask));
            }
        }

        [Theory]
        [MemberData(nameof(GetSingleMasksData))]
        public static void Mask256_SingleMask_InvertedMaskHasAllButOriginal_Software(Mask256 mask, byte elementNumber)
        {
            mask.InvertSoftware();
            for (int i = 0; i < 256; i++)
            {
                Mask256 testMask = Mask256.Create((byte)i);
                if (i == elementNumber)
                    Assert.False(mask.HasAnySoftware(in testMask));
                else
                    Assert.True(mask.HasAllSoftware(in testMask));
            }
        }

        [Fact]
        public static void Mask256_ToString_ExpectedOutput()
        {
            var mask = Mask256.Create(new byte[] { 31, 63, 95, 127, 159, 191, 223, 255 });
            var expectedString =
                "0000000000000000000000000000000100000000000000000000000000000001000000000000000000000000000000010000000000000000000000000000000100000000000000000000000000000001000000000000000000000000000000010000000000000000000000000000000100000000000000000000000000000001";
            Assert.Equal(expectedString, mask.ToString());
        }

        public static IEnumerable<object[]> GetSingleMasksData()
        {
            foreach (var tuple in GetSingleMasks())
                yield return new object[] { tuple.mask, tuple.elementNumber };
        }

        public static IEnumerable<object[]> GetCombinedMasksData()
        {
            foreach (var mask in GetCombinedMasks())
                yield return new object[] { mask };
        }

        private static IEnumerable<(Mask256 mask, byte elementNumber)> GetSingleMasks()
        {
            for (int i = 0; i < 256; i++)
            {
                var mask = Mask256.Create((byte)i);
                yield return (mask, (byte)i);
            }
        }

        private static IEnumerable<Mask256> GetCombinedMasks()
        {
            for (int i = 0; i < 256; i++)
            {
                Span<byte> bits = stackalloc byte[3];
                for (int j = 0; j < bits.Length; j++)
                    bits[j] = (byte)((i + j) % 256);

                yield return Mask256.Create(bits);
            }
        }
    }
}
