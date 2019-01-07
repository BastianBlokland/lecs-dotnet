#pragma warning disable CA1710 // Identifiers should have correct suffix (Collection)

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lecs.Memory
{
    /// <summary>
    /// Non-generic helpers for the <See cRef="IntMap{T}"/>
    /// </summary>
    public static partial class IntMap
    {
        /// <summary>
        /// Remove all given keys from the map
        /// </summary>
        /// <param name="map">Map to remove from</param>
        /// <param name="keys">Collection to keys to remove</param>
        /// <typeparam name="T">Type of the data</typeparam>
        public static void RemoveAll<T>(this IntMap<T> map, Span<int> keys)
        {
            /* Note: At the moment this does a simple loop so this method does not add much, but
            its a path we can actually optimize in the future. For example we could do the bit mixing
            and modulo operations that 'Find' does in parallel using simd instructions.*/

            for (int i = 0; i < keys.Length; i++)
                map.Remove(keys[i]);
        }

        /// <summary>
        /// Convenience extension for removing a given key
        /// </summary>
        /// <param name="map">Map to remove from</param>
        /// <param name="key">Key to remove</param>
        /// <typeparam name="T">Type of the data</typeparam>
        public static void Remove<T>(this IntMap<T> map, int key)
        {
            // Find the slot for this item
            if (!map.Find(key, out SlotToken slot)) // If not found: No need to remove it
                return;

            // Assert that we actually found the correct key
            Debug.Assert(map.GetKey(slot) == key, "Key that was found does not equal the requested key");

            map.Remove(slot);
        }

        /// <summary>
        /// Add (or update) data 'value' for given key. Is functionally the same as
        /// <See cref="IntMap{T}.Set(int, in T)"/> but having a 'Add' method enables the nice
        /// collection initialization syntax.
        /// </summary>
        /// <param name="map">Map to add to</param>
        /// <param name="key">Key to set the value for</param>
        /// <param name="value">Value to set</param>
        /// <typeparam name="T">Type of the value</typeparam>
        /// <returns>Slot that the item was inserted at</returns>
        public static SlotToken Add<T>(this IntMap<T> map, int key, in T value) => map.Set(key, value);

        /// <summary>
        /// Convenience extension for retrieving a value for given key.
        /// </summary>
        /// <remarks>
        /// Its perfectly legal to use this syntax to change the value
        /// </remarks>
        /// <exception cref="KeyNotFoundException">Thrown when there is no value for given key</exception>
        /// <param name="map">Map to get a value from</param>
        /// <param name="key">Key to get a value from</param>
        /// <typeparam name="T">Type of the data</typeparam>
        /// <returns> Reference to the value in the map </returns>
        public static ref T GetValue<T>(this IntMap<T> map, int key)
        {
            if (!map.Find(key, out SlotToken slot))
                throw new KeyNotFoundException($"Key: '{key}' is not found in this map");

            // Assert that we actually found the correct key
            Debug.Assert(map.GetKey(slot) == key, "Key that was found does not equal the requested key");

            return ref map.GetValue(slot);
        }
    }
}
