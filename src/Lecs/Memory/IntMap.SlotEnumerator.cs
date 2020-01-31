using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

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
        /// behaviour.
        ///
        /// - Tokens returned from <See cRef="Current"/> should not be cached because the map might be
        /// reordered when adding / removing items.
        ///
        /// - Changing the map while enumerating over it is considered undefined behaviour.
        /// </remarks>
        public struct SlotEnumerator : IEnumerator<IntMap.SlotToken>, IEnumerator
        {
            private readonly long[] keyData;
            private int currentIndex;

            internal SlotEnumerator(long[] keyData)
            {
                this.keyData = keyData;
                this.currentIndex = -1;
            }

            /// <summary>
            /// Gets a token pointing to the current slot.
            /// </summary>
            /// <remarks>
            /// Few gotchas:
            /// - Only returns a valid <See cRef="SlotToken"/> when a call to  <See cRef="MoveNext()"/>
            /// returned true, trying to use a token that was acquired in other scenarios is considered
            /// undefined behaviour.
            ///
            /// - Tokens returned from this should NOT be cached because the map might be reordered
            /// when adding or removing items.
            /// </remarks>
            /// <value>
            /// Token pointing to the current slot, this token can be used in:
            /// - <See cRef="IntMap{T}.GetKey(SlotToken slot)"/>
            /// - <See cRef="IntMap{T}.GetValueRef(SlotToken slot)"/>
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
                    return AsSlotToken(ref this.currentIndex);
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
                */

                while (true)
                {
                    this.currentIndex++;

                    // Assert that we can never index out of bounds
                    Debug.Assert(this.currentIndex < this.keyData.Length, "'currentIndex' is out of bounds");

                    // Mask of the lower 32 bit to get to our control data
                    switch (this.keyData[this.currentIndex] & ControlMask)
                    {
                        case FreeControl:
                            continue;

                        case EndControl:
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
