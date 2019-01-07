#pragma warning disable CA1034 // Nested types should not be visible

using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Lecs.Memory
{
    public static partial class IntMap
    {
        /* Use this non-generic class for putting static data that does not need to be 'instantiated'
        per generic type */

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

            public override int GetHashCode() => this.index;

            public override string ToString() => this.index.ToString(CultureInfo.InvariantCulture);

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

            public bool Equals(SlotToken other) => other.index.Equals(this.index);
        }
    }
}
