using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;

namespace Lecs.Memory
{
    /// <summary>
    /// Struct for storing 256 different 'flags', 256 bit is chosen because with Avx2 most operations
    /// can be performed with a single intrinsic.
    /// </summary>
    /// <remarks>
    /// For performance reasons this is a mutable struct but a immutable version 'ReadOnlyMask256'
    /// is availble that can be safely cached.
    ///
    /// Optimized for fast checking if masks overlap, using instrincs support where possible.
    /// Creation is not very fast (turns out its non trivial to vectorize that) so its best to cache
    /// the masks.
    ///
    /// Prefer using the methods with 'in' semantics as it avoids copying this (relatively big) struct
    /// unnecessarily.
    /// </remarks>
    [StructLayout(LayoutKind.Explicit, Size = 32)] // 4 * 8 byte = 32 byte
    public unsafe partial struct Mask256 : IEquatable<Mask256>, IEquatable<ReadOnlyMask256>
    {
        [FieldOffset(0)]
        internal fixed long Data[4]; // 4 * 64 bit = 256 bit

        /// <summary>
        /// Compare two masks for equality.
        /// NOTE: Use with 'in' semantics whenever possible to avoid copying data unnecessarily.
        /// </summary>
        /// <param name="a">Mask to compare.</param>
        /// <param name="b">Mask to compare to.</param>
        /// <returns>True if the masks are equal, otherwise false.</returns>
        [ExcludeFromCodeCoverage] // Implemention already tested
        public static bool operator ==(in Mask256 a, in Mask256 b) => a.Equals(in b);

        /// <summary>
        /// Compare two masks for equality.
        /// NOTE: Use with 'in' semantics whenever possible to avoid copying data unnecessarily.
        /// </summary>
        /// <param name="a">Mask to compare.</param>
        /// <param name="b">Mask to compare to.</param>
        /// <returns>True if the masks are equal, otherwise false.</returns>
        [ExcludeFromCodeCoverage] // Implemention already tested
        public static bool operator ==(in Mask256 a, in ReadOnlyMask256 b) => a.Equals(in b);

        /// <summary>
        /// Compare two masks for inequality.
        /// NOTE: Use with 'in' semantics whenever possible to avoid copying data unnecessarily.
        /// </summary>
        /// <param name="a">Mask to compare.</param>
        /// <param name="b">Mask to compare to.</param>
        /// <returns>True if the masks are unequal, otherwise false.</returns>
        [ExcludeFromCodeCoverage] // Implemention already tested
        public static bool operator !=(in Mask256 a, in Mask256 b) => !a.Equals(in b);

        /// <summary>
        /// Compare two masks for inequality.
        /// NOTE: Use with 'in' semantics whenever possible to avoid copying data unnecessarily.
        /// </summary>
        /// <param name="a">Mask to compare.</param>
        /// <param name="b">Mask to compare to.</param>
        /// <returns>True if the masks are unequal, otherwise false.</returns>
        [ExcludeFromCodeCoverage] // Implemention already tested
        public static bool operator !=(in Mask256 a, in ReadOnlyMask256 b) => !a.Equals(in b);

        /// <summary>
        /// Create a new mask with all bits unset.
        /// </summary>
        /// <returns>New empty mutable mask.</returns>
        public static Mask256 Create() => default(Mask256);

        /// <summary>
        /// Create a new mask with a specific bit set.
        /// </summary>
        /// <param name="bit">Bit to set.</param>
        /// <returns>New mask with the given bit set and all other bits unset.</returns>
        public static Mask256 Create(byte bit)
        {
            var result = default(Mask256);
            SetBitSoftware(result.Data, bit);
            return result;
        }

        /// <summary>
        /// Create a new mask with specific bits set.
        /// </summary>
        /// <param name="bits">Bits to set.</param>
        /// <returns>New mask with the given bits set and all other bits unset.</returns>
        public static Mask256 Create(ReadOnlySpan<byte> bits)
        {
            var result = default(Mask256);
            SetBitsSoftware(result.Data, bits);
            return result;
        }

        /// <summary>
        /// Create a readonly copy of this mask.
        /// </summary>
        /// <returns>ReadOnly copy of this mask.</returns>
        public ReadOnlyMask256 AsReadOnly() =>
            /* NOTE: Its safe to cast these around like this because they have identical memory layout
            and this returns a COPY so you won't be able to change the original */
            Unsafe.As<Mask256, ReadOnlyMask256>(ref this);

        /// <summary>
        /// Test if this mask has all bits of the other mask set.
        /// NOTE: Use with 'in' semantics whenever possible to avoid copying data unnecessarily.
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
        /// Test if this mask has all bits of the other mask set.
        /// NOTE: Use with 'in' semantics whenever possible to avoid copying data unnecessarily.
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
        /// Test if this mask has any bit of the other mask set.
        /// NOTE: Use with 'in' semantics whenever possible to avoid copying data unnecessarily.
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
        /// Test if this mask has any bit of the other mask set.
        /// NOTE: Use with 'in' semantics whenever possible to avoid copying data unnecessarily.
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
        /// Test if this mask doesn't have any bit of the other mask set.
        /// NOTE: Use with 'in' semantics whenever possible to avoid copying data unnecessarily.
        /// </summary>
        /// <param name="other">Mask containing the bits to check.</param>
        /// <returns>True if this mask doesn't have any bit of the given mask set, otherwise false.</returns>
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
        /// Test if this mask doesn't have any bit of the other mask set.
        /// NOTE: Use with 'in' semantics whenever possible to avoid copying data unnecessarily.
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
        /// Set all the bits of the other mask on this mask.
        /// NOTE: Use with 'in' semantics whenever possible to avoid copying data unnecessarily.
        /// </summary>
        /// <param name="other">Other mask containing the bits to set.</param>
        [ExcludeFromCodeCoverage] // Individual implementions already tested separately
        public void Add(in Mask256 other)
        {
            fixed (long* dataPointerA = this.Data, dataPointerB = other.Data)
            {
                if (Avx2.IsSupported)
                    AddAvx2(dataPointerA, dataPointerB);
                else
                if (Avx.IsSupported)
                    AddAvx(dataPointerA, dataPointerB);
                else
                    AddSoftware(dataPointerA, dataPointerB);
            }
        }

        /// <summary>
        /// Set all the bits of the other mask on this mask.
        /// NOTE: Use with 'in' semantics whenever possible to avoid copying data unnecessarily.
        /// </summary>
        /// <param name="other">Other mask containing the bits to set.</param>
        [ExcludeFromCodeCoverage] // Individual implementions already tested separately
        public void Add(in ReadOnlyMask256 other)
        {
            fixed (long* dataPointerA = this.Data, dataPointerB = other.Data)
            {
                if (Avx2.IsSupported)
                    AddAvx2(dataPointerA, dataPointerB);
                else
                if (Avx.IsSupported)
                    AddAvx(dataPointerA, dataPointerB);
                else
                    AddSoftware(dataPointerA, dataPointerB);
            }
        }

        /// <summary>
        /// Unset all the bits of the other mask on this mask.
        /// NOTE: Use with 'in' semantics whenever possible to avoid copying data unnecessarily.
        /// </summary>
        /// <param name="other">Other mask containing the bits to unset.</param>
        [ExcludeFromCodeCoverage] // Individual implementions already tested separately
        public void Remove(in Mask256 other)
        {
            fixed (long* dataPointerA = this.Data, dataPointerB = other.Data)
            {
                if (Avx2.IsSupported)
                    RemoveAvx2(dataPointerA, dataPointerB);
                else
                if (Avx.IsSupported)
                    RemoveAvx(dataPointerA, dataPointerB);
                else
                    RemoveSoftware(dataPointerA, dataPointerB);
            }
        }

        /// <summary>
        /// Unset all the bits of the other mask on this mask.
        /// NOTE: Use with 'in' semantics whenever possible to avoid copying data unnecessarily.
        /// </summary>
        /// <param name="other">Other mask containing the bits to unset.</param>
        [ExcludeFromCodeCoverage] // Individual implementions already tested separately
        public void Remove(in ReadOnlyMask256 other)
        {
            fixed (long* dataPointerA = this.Data, dataPointerB = other.Data)
            {
                if (Avx2.IsSupported)
                    RemoveAvx2(dataPointerA, dataPointerB);
                else
                if (Avx.IsSupported)
                    RemoveAvx(dataPointerA, dataPointerB);
                else
                    RemoveSoftware(dataPointerA, dataPointerB);
            }
        }

        /// <summary>
        /// Invert all the bits in this mask.
        /// </summary>
        [ExcludeFromCodeCoverage] // Individual implementions already tested separately
        public void Invert()
        {
            fixed (long* dataPointer = this.Data)
            {
                if (Avx2.IsSupported)
                    InvertAvx2(dataPointer);
                else
                if (Avx.IsSupported)
                    InvertAvx(dataPointer);
                else
                    InvertSoftware(dataPointer);
            }
        }

        /// <summary>
        /// Unset all bits in this mask.
        /// </summary>
        [ExcludeFromCodeCoverage] // Individual implementions already tested separately
        public void Clear()
        {
            fixed (long* dataPointer = this.Data)
            {
                if (Avx2.IsSupported)
                    ClearAvx2(dataPointer);
                else
                    ClearSoftware(dataPointer);
            }
        }

        /// <summary>
        /// Calculate a hashcode for the current data.
        /// NOTE: Not very optimized, if required in a high frequency code-path then more time should
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
        /// Create a string representation of the current bits. Example output: 0101010100101.
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
                case Mask256 mask:
                    return this.Equals(mask);
                case ReadOnlyMask256 readOnlyMask:
                    return this.Equals(readOnlyMask);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Test if data of this mask and given mask are equal.
        /// NOTE: Use with 'in' semantics whenever possible to avoid copying data unnecessarily.
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
        /// NOTE: Use with 'in' semantics whenever possible to avoid copying data unnecessarily.
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
        /// NOTE: Prefer to use the version with 'in' semantics to avoid copying data unnecessarily.
        /// </summary>
        /// <param name="other">Mask to compare.</param>
        /// <returns>True if equal, otherwise false.</returns>
        [ExcludeFromCodeCoverage] // Individual implementions already tested separately
        bool IEquatable<Mask256>.Equals(Mask256 other) => this.Equals(other);

        /// <summary>
        /// Test if data of this mask and given mask are equal.
        /// NOTE: Prefer to use the version with 'in' semantics to avoid copying data unnecessarily.
        /// </summary>
        /// <param name="other">Mask to compare.</param>
        /// <returns>True if equal, otherwise false.</returns>
        [ExcludeFromCodeCoverage] // Individual implementions already tested separately
        bool IEquatable<ReadOnlyMask256>.Equals(ReadOnlyMask256 other) => this.Equals(other);
    }
}
