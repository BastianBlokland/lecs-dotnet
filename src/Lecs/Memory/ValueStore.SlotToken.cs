#pragma warning disable CA1034 // Nested types should not be visible

using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Lecs.Memory
{
    public static partial class ValueStore
    {
        /* Use this non-generic class for putting static data that does not need to be 'instantiated'
        per generic type */

        [StructLayout(LayoutKind.Explicit, Size = sizeof(int))]
        public readonly struct SlotToken : IEquatable<SlotToken>
        {
            /*
            Note: This struct has to have the same memory layout as a integer because we want to
            cheaply cast it to integer and vise versa.
            */

            [FieldOffset(0)]
            private readonly int index;

            internal SlotToken(int index) => this.index = index;

            public static bool operator ==(SlotToken a, SlotToken b) => a.Equals(b);

            public static bool operator !=(SlotToken a, SlotToken b) => !a.Equals(b);

            public override int GetHashCode() => this.index;

            public override string ToString() => this.index.ToString(CultureInfo.InvariantCulture);

            public override bool Equals(object obj)
            {
                switch (obj)
                {
                    case SlotToken token:
                        return this.Equals(token);
                    default:
                        return false;
                }
            }

            public bool Equals(SlotToken other) => other.index.Equals(other.index);
        }
    }
}
