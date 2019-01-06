#pragma warning disable CA1710 // Identifiers should have correct suffix (Collection)

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lecs.Memory
{
    public static partial class ValueStore
    {
        /// <summary>
        /// Remove all given keys from the store
        /// </summary>
        /// <param name="store">Store to remove from</param>
        /// <param name="keys">Collection to keys to remove</param>
        /// <typeparam name="T">Type of the data</typeparam>
        public static void RemoveAll<T>(this ValueStore<T> store, Span<int> keys)
            where T : unmanaged
        {
            /* Note: At the moment this does a simple loop so this method does not add much, but
            its a path we can actually optimize in the future. For example we could do the bit mixing
            and modulo operations that 'Find' does in parallel using simd instructions.*/

            for (int i = 0; i < keys.Length; i++)
                store.Remove(keys[i]);
        }

        /// <summary>
        /// Convenience extension for removing a given key
        /// </summary>
        /// <param name="store">Store to remove from</param>
        /// <param name="key">Key to remove</param>
        /// <typeparam name="T">Type of the data</typeparam>
        public static void Remove<T>(this ValueStore<T> store, int key)
            where T : unmanaged
        {
            // Find the slot for this item
            if (!store.Find(key, out SlotToken slot)) // If not found: No need to remove it
                return;

            // Assert that we actually found the correct key
            Debug.Assert(store.GetKey(slot) == key, "Key that was found does not equal the requested key");

            store.Remove(slot);
        }

        /// <summary>
        /// Add (or update) data 'value' for given key. Is functionally the same as
        /// <See cref="ValueStore{T}.Set(int, in T)"/> but having a 'Add' method enables the nice
        /// collection initialization syntax.
        /// </summary>
        /// <param name="store">Store to add to</param>
        /// <param name="key">Key to set the value for</param>
        /// <param name="value">Value to set</param>
        /// <typeparam name="T">Type of the value</typeparam>
        /// <returns>Slot that the item was inserted at</returns>
        public static SlotToken Add<T>(this ValueStore<T> store, int key, in T value)
            where T : unmanaged => store.Set(key, value);

        /// <summary>
        /// Convenience extension for retrieving a value for given key.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown when there is no value for given key</exception>
        /// <param name="store">Store to get a value from</param>
        /// <param name="key">Key to get a value from</param>
        /// <typeparam name="T">Type of the data</typeparam>
        /// <returns>
        /// Reference to the value in the store
        /// NOTE: Its perfectly legal to use this syntax to change the value
        /// </returns>
        public static ref T GetValue<T>(this ValueStore<T> store, int key)
            where T : unmanaged
        {
            if (!store.Find(key, out SlotToken slot))
                throw new KeyNotFoundException($"Key: '{key}' is not found in this store");

            // Assert that we actually found the correct key
            Debug.Assert(store.GetKey(slot) == key, "Key that was found does not equal the requested key");

            return ref store.GetValue(slot);
        }
    }
}
