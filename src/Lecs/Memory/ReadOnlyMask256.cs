#pragma warning disable SA1600 // Elements must be documented

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;

using static Lecs.Memory.Mask256;

namespace Lecs.Memory
{
    /// <summary>
    /// ReadOnly version of the <see cref="Lecs.Memory.Mask256"/> struct. Can store 256 different flags.
    /// </summary>
    /// <remarks>
    /// Optimized for fast checking if masks overlap, using instrincs support where possible.
    /// Creation is not very fast (turns out its non trivial to vectorize that) so its best to cache
    /// the masks.
    ///
    /// Prefer using the methods with 'in' semantics as it avoids copying this (relatively big) unnecessarily.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 32)] // 4 * 8 byte = 32 byte
    public unsafe partial struct ReadOnlyMask256 : IEquatable<ReadOnlyMask256>, IEquatable<Mask256>
    {
        internal fixed long Data[4]; // 4 * 64 bit = 256 bit

        private static ReadOnlyMask256 empty = default(ReadOnlyMask256);

        // Disable warnings here because 'StyleCop' doesn't understand ref-returns on properties and
        // throws a null-ref.
        #pragma warning disable
        public static ref ReadOnlyMask256 Empty => ref empty;
        #pragma warning restore

        /// <summary>
        /// Compare two masks for equality.
        /// Note: Use with 'in' semantics whenever possible to avoid copying data unnecessarily.
        /// </summary>
        /// <param name="a">Mask to compare.</param>
        /// <param name="b">Mask to compare to.</param>
        /// <returns>True if the masks are equal, otherwise false.</returns>
        public static bool operator ==(in ReadOnlyMask256 a, in ReadOnlyMask256 b) => a.Equals(in b);

        /// <summary>
        /// Compare two masks for equality.
        /// Note: Use with 'in' semantics whenever possible to avoid copying data unnecessarily.
        /// </summary>
        /// <param name="a">Mask to compare.</param>
        /// <param name="b">Mask to compare to.</param>
        /// <returns>True if the masks are equal, otherwise false.</returns>
        public static bool operator ==(in ReadOnlyMask256 a, in Mask256 b) => a.Equals(in b);

        /// <summary>
        /// Compare two masks for inequality.
        /// Note: Use with 'in' semantics whenever possible to avoid copying data unnecessarily.
        /// </summary>
        /// <param name="a">Mask to compare.</param>
        /// <param name="b">Mask to compare to.</param>
        /// <returns>True if the masks are unequal, otherwise false.</returns>
        public static bool operator !=(in ReadOnlyMask256 a, in ReadOnlyMask256 b) => !a.Equals(in b);

        /// <summary>
        /// Compare two masks for inequality.
        /// Note: Use with 'in' semantics whenever possible to avoid copying data unnecessarily.
        /// </summary>
        /// <param name="a">Mask to compare.</param>
        /// <param name="b">Mask to compare to.</param>
        /// <returns>True if the masks are unequal, otherwise false.</returns>
        public static bool operator !=(in ReadOnlyMask256 a, in Mask256 b) => !a.Equals(in b);

        /// <summary>
        /// Create a new mask with a specific bit set.
        /// </summary>
        /// <param name="bit">Bit to set.</param>
        /// <returns>New mask with the given bit set and all other bits unset.</returns>
        public static ReadOnlyMask256 Create(byte bit)
        {
            var result = default(ReadOnlyMask256);
            SetBitSoftware(result.Data, bit);
            return result;
        }

        /// <summary>
        /// Create a new mask with specific bits set.
        /// </summary>
        /// <param name="bits">Bits to set.</param>
        /// <returns>New mask with the given bits set and all other bits unset.</returns>
        public static ReadOnlyMask256 Create(ReadOnlySpan<byte> bits)
        {
            var result = default(ReadOnlyMask256);
            SetBitsSoftware(result.Data, bits);
            return result;
        }

        /// <summary>
        /// Create a mutable copy of this mask.
        /// </summary>
        /// <returns>Mutable copy of this mask.</returns>
        public Mask256 AsMutable() =>
            /* Note: Its safe to cast these around like this because they have identical memory layout
            and this returns a COPY so you won't be able to change the original */
            Unsafe.As<ReadOnlyMask256, Mask256>(ref this);

        /// <summary>
        /// Test if this mask has all bits of the other mask set.
        /// Note: Use with 'in' semantics whenever possible to avoid copying data unnecessarily.
        /// </summary>
        /// <param name="other">Mask containing the bits to check.</param>
        /// <returns>True if it has all bits, otherwise false.</returns>
        [ExcludeFromCodeCoverage] // Individual implementions already tested separately
        public bool HasAll(in ReadOnlyMask256 other)
        {
            fixed (long* dataPointerA = this.Data, dataPointerB = other.Data)
            {
                if (Avx.IsSupported)
                    return HasAllAvx(dataPointerA, dataPointerB);
                else
                    return HasAllSoftware(dataPointerA, dataPointerB);
            }
        }

        /// <summary>
        /// Test if this mask has all bits of the other mask set.
        /// Note: Use with 'in' semantics whenever possible to avoid copying data unnecessarily.
        /// </summary>
        /// <param name="other">Mask containing the bits to check.</param>
        /// <returns>True if it has all bits, otherwise false.</returns>
        [ExcludeFromCodeCoverage] // Individual implementions already tested separately
        public bool HasAll(in Mask256 other)
        {
            fixed (long* dataPointerA = this.Data, dataPointerB = other.Data)
            {
                if (Avx.IsSupported)
                    return HasAllAvx(dataPointerA, dataPointerB);
                else
                    return HasAllSoftware(dataPointerA, dataPointerB);
            }
        }

        /// <summary>
        /// Test if this mask has any bit of the other mask set.
        /// Note: Use with 'in' semantics whenever possible to avoid copying data unnecessarily.
        /// </summary>
        /// <param name="other">Mask containing the bits to check.</param>
        /// <returns>True if this mask has any bit of the given mask set, otherwise false.</returns>
        [ExcludeFromCodeCoverage] // Individual implementions already tested separately
        public bool HasAny(in ReadOnlyMask256 other)
        {
            fixed (long* dataPointerA = this.Data, dataPointerB = other.Data)
            {
                if (Avx.IsSupported)
                    return HasAnyAvx(dataPointerA, dataPointerB);
                else
                    return HasAnySoftware(dataPointerA, dataPointerB);
            }
        }

        /// <summary>
        /// Test if this mask has any bit of the other mask set.
        /// Note: Use with 'in' semantics whenever possible to avoid copying data unnecessarily.
        /// </summary>
        /// <param name="other">Mask containing the bits to check.</param>
        /// <returns>True if this mask has any bit of the given mask set, otherwise false.</returns>
        [ExcludeFromCodeCoverage] // Individual implementions already tested separately
        public bool HasAny(in Mask256 other)
        {
            fixed (long* dataPointerA = this.Data, dataPointerB = other.Data)
            {
                if (Avx.IsSupported)
                    return HasAnyAvx(dataPointerA, dataPointerB);
                else
                    return HasAnySoftware(dataPointerA, dataPointerB);
            }
        }

        /// <summary>
        /// Test if this mask doesn't have any bit of the other mask set.
        /// Note: Use with 'in' semantics whenever possible to avoid copying data unnecessarily.
        /// </summary>
        /// <param name="other">Mask containing the bits to check.</param>
        /// <returns>True if this mask doesn't have any bit of the given mask set, otherwise false.</returns>
        [ExcludeFromCodeCoverage] // Individual implementions already tested separately
        public bool NotHasAny(in ReadOnlyMask256 other)
        {
            fixed (long* dataPointerA = this.Data, dataPointerB = other.Data)
            {
                if (Avx.IsSupported)
                    return NotHasAnyAvx(dataPointerA, dataPointerB);
                else
                    return NotHasAnySoftware(dataPointerA, dataPointerB);
            }
        }

        /// <summary>
        /// Test if this mask doesn't have any bit of the other mask set.
        /// Note: Use with 'in' semantics whenever possible to avoid copying data unnecessarily.
        /// </summary>
        /// <param name="other">Mask containing the bits to check.</param>
        /// <returns>True if this mask doesn't have any bit of the given mask set, otherwise false.
        /// </returns>
        [ExcludeFromCodeCoverage] // Individual implementions already tested separately
        public bool NotHasAny(in Mask256 other)
        {
            fixed (long* dataPointerA = this.Data, dataPointerB = other.Data)
            {
                if (Avx.IsSupported)
                    return NotHasAnyAvx(dataPointerA, dataPointerB);
                else
                    return NotHasAnySoftware(dataPointerA, dataPointerB);
            }
        }

        /// <summary>
        /// Calculate a hashcode for the current data.
        /// Note: Not very optimized, if required in a high frequency code-path then more time should
        /// be invested into looking into a vectorizable solution.
        /// </summary>
        /// <returns>Hashcode to represent the current data.</returns>
        public override int GetHashCode()
        {
            fixed (long* dataPointer = this.Data)
            {
                return GetHashCodeSoftware(dataPointer);
            }
        }

        /// <summary>
        /// Create a string representation of the current bits. Example output: 0101010104212.
        /// </summary>
        /// <returns>New string containing text representation of current data.</returns>
        public override string ToString()
        {
            fixed (long* dataPointer = this.Data)
            {
                return ToStringSoftware(dataPointer);
            }
        }

        /// <summary>
        /// Test equality for this mask and given object.
        /// NOTE: Avoid using this at all costs as it requires boxing the target.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>True if data is equal, otherwise false.</returns>
        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case ReadOnlyMask256 readOnlyMask:
                    return this.Equals(readOnlyMask);
                case Mask256 mask:
                    return this.Equals(mask);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Test if data of this mask and given mask are equal.
        /// Note: Use with 'in' semantics whenever possible to avoid copying data unnecessarily.
        /// </summary>
        /// <param name="other">Mask to compare.</param>
        /// <returns>True if equal, otherwise false.</returns>
        [ExcludeFromCodeCoverage] // Individual implementions already tested separately
        public bool Equals(in ReadOnlyMask256 other)
        {
            fixed (long* dataPointerA = this.Data, dataPointerB = other.Data)
            {
                if (Avx2.IsSupported)
                    return EqualsAvx2(dataPointerA, dataPointerB);
                else
                if (Avx.IsSupported)
                    return EqualsAvx(dataPointerA, dataPointerB);
                else
                    return EqualsSoftware(dataPointerA, dataPointerB);
            }
        }

        /// <summary>
        /// Test if data of this mask and given mask are equal.
        /// Note: Use with 'in' semantics whenever possible to avoid copying data unnecessarily.
        /// </summary>
        /// <param name="other">Mask to compare.</param>
        /// <returns>True if equal, otherwise false.</returns>
        [ExcludeFromCodeCoverage] // Individual implementions already tested separately
        public bool Equals(in Mask256 other)
        {
            fixed (long* dataPointerA = this.Data, dataPointerB = other.Data)
            {
                if (Avx2.IsSupported)
                    return EqualsAvx2(dataPointerA, dataPointerB);
                else
                if (Avx.IsSupported)
                    return EqualsAvx(dataPointerA, dataPointerB);
                else
                    return EqualsSoftware(dataPointerA, dataPointerB);
            }
        }

        /// <summary>
        /// Test if data of this mask and given mask are equal.
        /// NOTE: Prefer to use the version with 'in' semantics to avoid copying data unnecessarily.
        /// </summary>
        /// <param name="other">Mask to compare.</param>
        /// <returns>True if equal, otherwise false.</returns>
        bool IEquatable<ReadOnlyMask256>.Equals(ReadOnlyMask256 other) => this.Equals(other);

        /// <summary>
        /// Test if data of this mask and given mask are equal.
        /// NOTE: Prefer to use the version with 'in' semantics to avoid copying data unnecessarily.
        /// </summary>
        /// <param name="other">Mask to compare.</param>
        /// <returns>True if equal, otherwise false.</returns>
        bool IEquatable<Mask256>.Equals(Mask256 other) => this.Equals(other);
    }
}
