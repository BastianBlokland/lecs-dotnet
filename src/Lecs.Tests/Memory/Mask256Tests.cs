using System;
using System.Collections.Generic;
using Xunit;

using Lecs.Memory;
using Lecs.Tests.Attributes;

namespace Lecs.Tests.Memory
{
    public sealed class Mask256Tests
    {
        [Avx2Theory]
        [MemberData(nameof(GetCombinedMasksData))]
        public void Mask256_CombinedMask_ClearResultsInEmpty_Avx2(Mask256 mask)
        {
            Assert.False(mask.EqualsAvx2(default(Mask256)));
            mask.ClearAvx2();
            Assert.True(mask.EqualsAvx2(default(Mask256)));
        }

        [Theory]
        [MemberData(nameof(GetCombinedMasksData))]
        public void Mask256_CombinedMask_ClearResultsInEmpty_Software(Mask256 mask)
        {
            Assert.False(mask.EqualsSoftware(default(Mask256)));
            mask.ClearSoftware();
            Assert.True(mask.EqualsSoftware(default(Mask256)));
        }

        [Avx2Theory]
        [MemberData(nameof(GetCombinedMasksData))]
        public void Mask256_CombinedMask_CanBeAddedAndRemoved_Avx2(Mask256 mask)
        {
            var testMask = default(Mask256);

            testMask.Add(in mask);
            Assert.True(testMask.EqualsAvx2(in mask));
            Assert.True(testMask.HasAvx(in mask));
            Assert.False(testMask.NotHasAvx(in mask));

            testMask.Remove(mask);
            Assert.True(testMask.EqualsAvx2(default(Mask256)));
            Assert.False(testMask.EqualsAvx2(in mask));
            Assert.False(testMask.HasAvx(in mask));
            Assert.True(testMask.NotHasAvx(in mask));
        }

        [AvxTheory]
        [MemberData(nameof(GetCombinedMasksData))]
        public void Mask256_CombinedMask_CanBeAddedAndRemoved_Avx(Mask256 mask)
        {
            var testMask = default(Mask256);

            testMask.Add(in mask);
            Assert.True(testMask.EqualsAvx(in mask));
            Assert.True(testMask.HasAvx(in mask));
            Assert.False(testMask.NotHasAvx(in mask));

            testMask.Remove(mask);
            Assert.True(testMask.EqualsAvx(default(Mask256)));
            Assert.False(testMask.EqualsAvx(in mask));
            Assert.False(testMask.HasAvx(in mask));
            Assert.True(testMask.NotHasAvx(in mask));
        }

        [Theory]
        [MemberData(nameof(GetCombinedMasksData))]
        public void Mask256_CombinedMask_CanBeAddedAndRemoved_Software(Mask256 mask)
        {
            var defaultMask = default(Mask256);

            defaultMask.Add(in mask);
            Assert.True(defaultMask.EqualsSoftware(in mask));
            Assert.True(defaultMask.HasSoftware(in mask));
            Assert.False(defaultMask.NotHasSoftware(in mask));

            defaultMask.Remove(in mask);
            Assert.True(defaultMask.EqualsSoftware(default(Mask256)));
            Assert.False(defaultMask.EqualsSoftware(in mask));
            Assert.False(defaultMask.HasSoftware(in mask));
            Assert.True(defaultMask.NotHasSoftware(in mask));
        }

        [Avx2Theory]
        [MemberData(nameof(GetSingleMasksData))]
        public void Mask256_SingleMask_InvertedMaskHasAllButOriginal_Avx2(Mask256 mask, byte elementNumber)
        {
            mask.InvertAvx2();
            for (int i = 0; i < 256; i++)
            {
                Mask256 testMask = Mask256.Create((byte)i);
                if (i == elementNumber)
                    mask.NotHasAvx(testMask);
                else
                    mask.HasAvx(testMask);
            }
        }

        [AvxTheory]
        [MemberData(nameof(GetSingleMasksData))]
        public void Mask256_SingleMask_InvertedMaskHasAllButOriginal_Avx(Mask256 mask, byte elementNumber)
        {
            mask.InvertAvx();
            for (int i = 0; i < 256; i++)
            {
                Mask256 testMask = Mask256.Create((byte)i);
                if (i == elementNumber)
                    mask.NotHasAvx(testMask);
                else
                    mask.HasAvx(testMask);
            }
        }

        [Theory]
        [MemberData(nameof(GetSingleMasksData))]
        public void Mask256_SingleMask_InvertedMaskHasAllButOriginal_Software(Mask256 mask, byte elementNumber)
        {
            mask.InvertSoftware();
            for (int i = 0; i < 256; i++)
            {
                Mask256 testMask = Mask256.Create((byte)i);
                if (i == elementNumber)
                    mask.NotHasSoftware(testMask);
                else
                    mask.HasSoftware(testMask);
            }
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
                var mask = Mask256.Create((byte)i);
                for (int j = 0; j < 3; j++)
                {
                    var otherMask = Mask256.Create((byte)((i + j) % 256));
                    mask.Add(otherMask);
                }
                yield return mask;
            }
        }
    }
}
