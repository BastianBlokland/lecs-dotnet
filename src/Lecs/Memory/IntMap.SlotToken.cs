#pragma warning disable CA1034 // Nested types should not be visible

using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Lecs.Memory
{
    /// <summary>
    /// Non-generic helpers for the <See cRef="IntMap{T}"/>
    /// </summary>
    public static partial class IntMap
    {
        /* Use this non-generic class for putting static data that does not need to be 'instantiated'
        per generic type */

        // Utility for retrieving a item from an array by using a slot token.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ref T GetRef<T>(this T[] array, SlotToken token)
        {
            ref int index = ref Unsafe.As<SlotToken, int>(ref token);
            return ref array[index];
        }

        // Utility for interpreting a 'SlotToken' as int (safe because they have the same layout)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ref int AsInt(ref SlotToken token) =>
            ref Unsafe.As<SlotToken, int>(ref token);

        // Utility for interpreting a int as a 'SlotToken' (safe because they have the same layout)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ref SlotToken AsSlotToken(ref int token) =>
            ref Unsafe.As<int, SlotToken>(ref token);

        /// <summary>
        /// Token used to identify a slot in the map
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = sizeof(int))]
        public readonly struct SlotToken : IEquatable<SlotToken>
        {
            /*
            NOTE: This struct has to have the same memory layout as a integer because we want to
            cheaply cast it to integer and vise versa.
            */

            [FieldOffset(0)]
            private readonly int index;

            internal SlotToken(int index) => this.index = index;

            /// <summary>
            /// Are two given tokens pointing to the same slot
            /// </summary>
            /// <param name="a">Slot a</param>
            /// <param name="b">Slot b</param>
            /// <returns>'True' if they point to the same slot, otherwise 'False'</returns>
            public static bool operator ==(SlotToken a, SlotToken b) => a.Equals(b);

            /// <summary>
            /// Are two given tokens pointing to different slots
            /// </summary>
            /// <param name="a">Slot a</param>
            /// <param name="b">Slot b</param>
            /// <returns>'True' if they point to different slots, otherwise 'False'</returns>
            public static bool operator !=(SlotToken a, SlotToken b) => !a.Equals(b);

            /// <summary>
            /// Get a hash to represent this token
            /// </summary>
            /// <returns>Hash representing this token</returns>
            public override int GetHashCode() => this.index;

            /// <summary>
            /// Get a string representation of this token
            /// </summary>
            /// <returns>String representing this token</returns>
            public override string ToString() => this.index.ToString(CultureInfo.InvariantCulture);

            /// <summary>
            /// Is this slot equal to the given object
            /// </summary>
            /// <remarks>
            /// Avoid using this at all costs as it requires boxing the target.
            /// </remarks>
            /// <param name="obj">Object to comapre to</param>
            /// <returns>'True' if they point to the same slot, otherwise 'False'</returns>
            public override bool Equals(object obj)
            {
                switch (obj)
                {
                    case SlotToken slot:
                        return this.Equals(slot);
                    default:
                        return false;
                }
            }

            /// <summary>
            /// Is this token pointing to same slot as the given token
            /// </summary>
            /// <param name="other">Token to comapre to</param>
            /// <returns>'True' if they point to the same slot, otherwise 'False'</returns>
            public bool Equals(SlotToken other) => other.index.Equals(this.index);
        }
    }
}
