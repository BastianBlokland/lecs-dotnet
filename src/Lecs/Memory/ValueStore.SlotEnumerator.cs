using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Lecs.Memory
{
    public static partial class ValueStore
    {
        /* Use this non-generic class for putting static data that does not need to be 'instantiated'
        per generic type */

        public struct SlotEnumerator : IEnumerator<ValueStore.SlotToken>, IEnumerator
        {
            private readonly int[] keys;
            private int currentIndex;

            internal SlotEnumerator(int[] keys)
            {
                this.keys = keys;
                this.currentIndex = -1;
            }

            public SlotToken Current
            {
                get
                {
                    /*
                    NOTE: This is not super safe because this can hand-out tokens pointing to -1 when
                    calling 'Current' without calling 'MoveNext' (or when 'MoveNext' returns false).
                    But adding checks here also feels wastefull because it will make the common
                    'foreach (var slot in valueStore)' case much slower.
                    So for the moment i'll leave it as a quirk to add to documentation.
                    */

                    Debug.Assert(this.currentIndex >= 0, "'Current' called when index is invalid");
                    return Unsafe.As<int, SlotToken>(ref this.currentIndex);
                }
            }

            [ExcludeFromCodeCoverage] // Non-boxing version already covered
            object IEnumerator.Current => this.Current;

            public bool MoveNext()
            {
                /*
                Logic here is to skip a 'FreeKey' and to exit when we find a 'UnavailableKey' (those
                are always at the end of the keys array).

                I considered implementing this with simd instructions but will probably not be faster
                as we try to have as little gaps in our arrays as possible so this should only loop
                for a few spaces maximum.
                */

                while (true)
                {
                    this.currentIndex++;

                    // Assert that we can never index out of bounds
                    Debug.Assert(this.currentIndex < this.keys.Length, "'currentIndex' is out of bounds");

                    // 'Unsafe.Add' instead of array indexing because we do not need the bounds checking
                    switch (Unsafe.Add(ref this.keys[0], this.currentIndex))
                    {
                        case FreeKey:
                            continue;

                        case UnavailableKey:
                        {
                            this.currentIndex = -1;
                            return false;
                        }

                        default:
                            return true;
                    }
                }
            }

            void IEnumerator.Reset() => this.currentIndex = -1;

            void IDisposable.Dispose()
            {
            }
        }
    }
}
