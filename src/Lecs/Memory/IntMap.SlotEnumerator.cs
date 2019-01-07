using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Lecs.Memory
{
    /// <summary>
    /// Non-generic helpers for the <See cRef="IntMap{T}"/>
    /// </summary>
    public static partial class IntMap
    {
        /* Use this non-generic class for putting static data that does not need to be 'instantiated'
        per generic type */

        /// <summary>
        /// Enumerator for enumerating all used slots in the map.
        /// </summary>
        /// <remarks>
        /// There are a few gotchas with this enumerator:
        ///
        /// - <See cRef="Current"/> only returns a valid token after a call to <See cRef="MoveNext()"/>
        /// returned true, trying to use a token that was acquired in other cases leads to undefined
        /// behaviour and will probably crash. This is because for performance reasons there are very
        /// little checks on the usage of <See cRef="SlotToken"/>.
        ///
        /// - Tokens returned from <See cRef="Current"/> should not be cached because the map might be
        /// reordered when adding / removing items.
        ///
        /// - Changing the map while enumerating over it is considered undefined behaviour.
        /// </remarks>
        public struct SlotEnumerator : IEnumerator<IntMap.SlotToken>, IEnumerator
        {
            private readonly int[] keys;
            private int currentIndex;

            internal SlotEnumerator(int[] keys)
            {
                this.keys = keys;
                this.currentIndex = -1;
            }

            /// <summary>
            /// Gets a token pointing to the current slot.
            /// </summary>
            /// <remarks>
            /// Few gotchas:
            /// - Only returns a valid <See cRef="SlotToken"/> when a call to  <See cRef="MoveNext()"/>
            /// returned true, trying to use a token that was acquired in other scenarios is considered
            /// undefined behaviour and will probably crash.
            ///
            /// - Tokens returned from this should NOT be cached because the map might be reordered
            /// when adding or removing items.
            /// </remarks>
            /// <value>
            /// Token pointing to the current slot, this token can be used in:
            /// - <See cRef="IntMap{T}.GetKey(SlotToken slot)"/>
            /// - <See cRef="IntMap{T}.GetValue(SlotToken slot)"/>
            /// </value>
            public SlotToken Current
            {
                get
                {
                    /*
                    NOTE: This is not super safe because this can hand-out tokens pointing to -1 when
                    calling 'Current' without calling 'MoveNext' (or when 'MoveNext' returns false).
                    But adding checks here also feels wastefull because it will make the common
                    'foreach (var slot in map)' case much slower.
                    So for the moment i'll leave it as a quirk to add to documentation.
                    */

                    Debug.Assert(this.currentIndex >= 0, "'Current' called when index is invalid");
                    return Unsafe.As<int, SlotToken>(ref this.currentIndex);
                }
            }

            /// <summary>
            /// Gets a boxed version of the token. Avoid calling.
            /// </summary>
            /// <returns>Boxed token</returns>
            [ExcludeFromCodeCoverage] // Non-boxing version already covered
            object IEnumerator.Current => this.Current;

            /// <summary>
            /// Move the enumerator to the next used slot.
            /// </summary>
            /// <returns>'True' when we found a next used slot, otherwise 'False'</returns>
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

            /// <summary>
            /// Reset the enumerator
            /// </summary>
            public void Reset() => this.currentIndex = -1;

            /// <summary>
            /// Avoid calling, no need on this enumerator
            /// </summary>
            void IDisposable.Dispose()
            {
            }
        }
    }
}
