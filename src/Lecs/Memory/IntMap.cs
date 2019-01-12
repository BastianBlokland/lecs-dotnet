#pragma warning disable CA1710 // Identifiers should have correct suffix (Collection)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using static Lecs.Memory.IntMap;

namespace Lecs.Memory
{
    /// <summary>
    /// Non-generic helpers for the <See cRef="IntMap{T}"/>
    /// </summary>
    public static partial class IntMap
    {
        /* Use this non-generic class for putting static data that does not need to be 'instantiated'
        per generic type */

        internal const int FreeKey = -1;
        internal const int EndKey = -2;
    }

    /// <summary>
    /// Associative array that uses a integer as a key. Designed for fast quering and enumerating.
    /// Main usage is that you acquire a <See cRef="SlotToken"/> by either enumerating the slots,
    /// or finding a slot, with that token you can then get or set the value.
    ///
    /// Implemented as a open-indexing hashmap with linear probing for finding keys.
    /// </summary>
    /// <remarks>
    /// This map is designed more for speed then for safety, for example usage of <See cRef="SlotToken"/>
    /// is checked very little. So its up to the user to avoid using old tokens, tokens from another
    /// map or self-constructed tokens, otherwise undefined behaviour will follow.
    /// </remarks>
    /// <typeparam name="T">
    /// Type of the data in the map. There is a fast-path for unmanaged types (structs without any
    /// managed references on them) that avoids clearing data.
    /// </typeparam>
    public sealed partial class IntMap<T> : IEnumerable<IntMap.SlotToken>
    {
        private readonly bool isManaged;
        private readonly float maxLoadFactor;

        private int[] keys; // Allocated 1 element bigger so we don't need range checks
        private T[] values;
        private int capacity; // Only power-of-two for fast modulo
        private int capacityMinusOne;
        private int maxCount;
        private int count;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntMap{T}"/> class
        /// </summary>
        /// <param name="initialCapacity">
        /// Initial capacity for the map, will be rounded up to the nearest power-of-two and then
        /// doubled each time it runs out of space.
        /// </param>
        /// <param name="maxLoadFactor">
        /// How far we will fill up the map before growing, value has to be between 0 and 1.
        /// Low value will result in less collisions but also less data locality and more memory usage.
        /// High values will result in more collisions but higher data locality and less memory usage.
        /// </param>
        public IntMap(int initialCapacity = 256, float maxLoadFactor = .75f)
        {
            if (initialCapacity <= 1 || initialCapacity > HashHelpers.BiggestPowerOfTwo)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(initialCapacity),
                    $"Must between '1' and '{HashHelpers.BiggestPowerOfTwo + 1}'");
            }

            if (maxLoadFactor <= 0f || maxLoadFactor >= 1f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxLoadFactor),
                    "Must be between '0' and '1'");
            }

            this.isManaged = RuntimeHelpers.IsReferenceOrContainsReferences<T>();
            this.maxLoadFactor = maxLoadFactor;
            this.Initialize(capacity: HashHelpers.RoundUpToPowerOfTwo(initialCapacity));
        }

        /// <summary>
        /// Gets how many slots are currently filled in the map.
        /// </summary>
        public int Count => this.count;

        /// <summary>
        /// Find the slot where the given key is in the map.
        /// </summary>
        /// <param name="key">Key to look for</param>
        /// <param name="slot">Slot where the given key is found</param>
        /// <returns>'True' is the key was found, otherwise 'False' </returns>
        public bool Find(int key, out SlotToken slot)
        {
            /* We use linear probing for searching so we get an initial slot from hashing the key
            and then keeping advancing one-by-one until we find our key or we find a empty slot.
            The first empty slot is returned so we can use this slot for insertions. */

            slot = this.GetDesiredSlot(key);

            // Create a int 'view' over the token because its easier to deal with.
            ref int index = ref AsInt(ref slot);

            while (true)
            {
                ref int element = ref this.keys[index];
                if (element == key)
                    return true;

                switch (element)
                {
                    case FreeKey:
                        return false;

                    case EndKey:
                        index = 0;
                        break;

                    default:
                        index++;
                        break;
                }
            }
        }

        /// <summary>
        /// Find a key in the map, if it does not exist then add it.
        /// </summary>
        /// <param name="key">Key to find</param>
        /// <returns>Returns the slot where this key is in the map</returns>
        public SlotToken FindOrAdd(int key)
        {
            // If item already exists then return that slot
            if (this.Find(key, out SlotToken slot))
                return slot;

            Debug.Assert(this.keys[AsInt(ref slot)] == FreeKey, "Slot to insert to has to be empty");
            Debug.Assert(Array.IndexOf(this.keys, key) == -1, "Incorrectly determined that key did not exist yet");

            // Set the key and value to this free slot
            this.keys[AsInt(ref slot)] = key;

            // Increment the count and grow the map if it now contains too many elements
            this.count++;
            if (this.count > this.maxCount)
            {
                var oldKeys = this.keys;
                var oldValues = this.values;

                // Grow the map
                this.Initialize(capacity: HashHelpers.NextPowerOfTwo(this.capacity));

                // Re-insert the old keys and values
                var enumerator = new SlotEnumerator(oldKeys);
                while (enumerator.MoveNext())
                {
                    var oldSlot = enumerator.Current;
                    var newSlot = this.FindOrAdd(key: oldKeys[AsInt(ref oldSlot)]);

                    Debug.Assert(
                        this.keys[AsInt(ref newSlot)] == oldKeys[AsInt(ref oldSlot)],
                        "New slot does not contain the same key as the old slot");

                    // Update our slot to return to the new slot it got
                    if (oldKeys[AsInt(ref oldSlot)] == key)
                        slot = newSlot;

                    this.values[AsInt(ref newSlot)] = oldValues[AsInt(ref oldSlot)];
                }
            }

            Debug.Assert(this.keys[AsInt(ref slot)] == key, "Key does not exist at result slot");
            return slot;
        }

        /// <summary>
        /// Remove a key and value
        /// </summary>
        /// <param name="slot">Slot to remove from</param>
        public void Remove(SlotToken slot)
        {
            /* When removing we need to make sure each key is as 'close' to their desired slot as
            possible, we do this by shiting them if a better slot opened up for them. */

            // Check if the slot not already free, this is to protect 'count' going too low on misuse.
            if (this.keys[AsInt(ref slot)] == FreeKey)
                throw new ArgumentException("Provided slot is already empty");

            // Decrement count of items in map
            this.count--;
            Debug.Assert(this.count >= 0, "Count should never go negative");

            // Shift the next keys until we find a empty slot
            SlotToken curSlot = slot;
            SlotToken nextSlot = this.GetNextSlot(curSlot);
            while (true)
            {
                ref int nextKey = ref this.keys[AsInt(ref nextSlot)];

                Debug.Assert(nextKey != EndKey, "Loop went out of bounds");
                Debug.Assert(nextKey != this.keys[AsInt(ref slot)], "Key lives in the map twice");

                // If its the end of a chain then we are done shifting
                if (nextKey == FreeKey)
                    break;

                // If this is a better slot for the next key then shift it over
                if (IsBetterSlot(nextKey, currentSlot: nextSlot, potentialSlot: curSlot))
                {
                    Copy(copyFrom: ref nextSlot, copyTo: ref curSlot);
                    curSlot = nextSlot;
                }

                // Update 'nextSlot' to point to one further
                nextSlot = this.GetNextSlot(nextSlot);
                Debug.Assert(nextSlot != slot, "We looped the entire map without finding a free-key");
            }

            // Mark the last slot in the chain as now being free
            SetFree(ref curSlot);

            void Copy(ref SlotToken copyFrom, ref SlotToken copyTo)
            {
                ref int fromIndex = ref AsInt(ref copyFrom);
                ref int toIndex = ref AsInt(ref copyTo);
                this.keys[toIndex] = this.keys[fromIndex];
                this.values[toIndex] = this.values[fromIndex];
            }

            void SetFree(ref SlotToken slotToFree)
            {
                ref int index = ref AsInt(ref slotToFree);
                Debug.Assert(this.keys[index] != FreeKey, "Slot was already free");
                this.keys[index] = FreeKey;

                // If 'T' is a managed type we need to clear it so the garbage-collector can clean it up.
                if (this.isManaged)
                    this.values[index] = default(T);
            }

            bool IsBetterSlot(int key, SlotToken currentSlot, SlotToken potentialSlot)
            {
                ref int currentIndex = ref AsInt(ref currentSlot);
                ref int potentialIndex = ref AsInt(ref potentialSlot);

                /*
                D = Desired, C = Current
                Wrapped case: Better if P < C || P > D
                    0 C 0 0 0 0 0 D 0
                Non-wrapped case: Better if P > D && P < C
                    0 0 0 0 D 0 0 C 0
                */

                SlotToken desiredSlot = this.GetDesiredSlot(key);
                ref int desiredIndex = ref AsInt(ref desiredSlot);
                if (potentialIndex == desiredIndex)
                    return true;

                bool wrapped = currentIndex < desiredIndex;
                if (wrapped)
                    return potentialIndex < currentIndex || potentialIndex > desiredIndex;
                else // !wrapped
                    return potentialIndex > desiredIndex && potentialIndex < currentIndex;
            }
        }

        /// <summary>
        /// Remove all entries
        /// </summary>
        public void Clear()
        {
            Array.Fill(this.keys, value: FreeKey, startIndex: 0, count: this.capacity);
            this.count = 0;

            // If 'T' is a managed type we need to clear it so the garbage-collector can clean it up.
            if (this.isManaged)
                Array.Clear(this.values, index: 0, length: this.values.Length);
        }

        /// <summary>
        /// Get the key for the given slot.
        /// </summary>
        /// <param name="slot">Slot to get the key for</param>
        /// <returns>Key</returns>
        public int GetKey(SlotToken slot) => this.keys[AsInt(ref slot)];

        /// <summary>
        /// Get a reference to the the value for the given slot.
        /// </summary>
        /// <remarks>
        /// Returned as a ref so it can also be used to change the value.
        /// </remarks>
        /// <param name="slot">Slot to get the value for</param>
        /// <returns>Ref to the value</returns>
        public ref T GetValueRef(SlotToken slot) => ref this.values[AsInt(ref slot)];

        /// <summary>
        /// Get a enumerator for enumerating all used slots.
        /// </summary>
        /// <returns>Enumerator to enumerate the used slots</returns>
        public SlotEnumerator GetEnumerator() => new SlotEnumerator(this.keys);

        /// <summary>
        /// Get a boxed enumerator. Avoid used this at all costs.
        /// </summary>
        /// <returns>Boxed enumerator</returns>
        IEnumerator<SlotToken> IEnumerable<SlotToken>.GetEnumerator() => new SlotEnumerator(this.keys);

        /// <summary>
        /// Get a boxed enumerator. Avoid used this at all costs.
        /// </summary>
        /// <returns>Boxed enumerator</returns>
        [ExcludeFromCodeCoverage] // Non-boxed version already covered
        IEnumerator IEnumerable.GetEnumerator() => new SlotEnumerator(this.keys);

        private static int GetMaxCount(int capacity, float maxLoadFactor)
        {
            /*
            Calculate how far we are allowed to fill up the map based on the configured load-factor.
            Always return at least 1.
            */

            Debug.Assert(capacity > 1, "Capacity has to be more then '1'");
            return Math.Max(1, (int)Math.Floor(capacity * maxLoadFactor));
        }

        private void Initialize(int capacity)
        {
            Debug.Assert(HashHelpers.IsPowerOfTwo(capacity), "Capacity has to be a power-of-two");

            this.capacity = capacity;
            this.capacityMinusOne = capacity - 1;
            this.maxCount = GetMaxCount(this.capacity, this.maxLoadFactor);
            this.count = 0;

            // Initialize the keys, add a 'EndKey' at the end so we can avoid range checks in a few
            // loops
            this.keys = new int[capacity + 1];
            Array.Fill(this.keys, value: FreeKey, startIndex: 0, count: capacity);
            this.keys[capacity] = EndKey;

            // Initialize the value, leave them at default
            this.values = new T[capacity];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private SlotToken GetDesiredSlot(int key)
        {
            int hash = HashHelpers.Mix(key); // Mix the key to avoid sequential input causing many collisions
            int index = HashHelpers.ModuloPowerOfTwoMinusOne(hash, this.capacityMinusOne);
            return AsSlotToken(ref index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private SlotToken GetNextSlot(SlotToken slot)
        {
            ref int index = ref AsInt(ref slot);

            // NOTE: We actually change the 'slot' parameter here because the 'index' is a int
            // reference over that variable, its safe because 'slot' parameter is passed as a copy
            index = HashHelpers.ModuloPowerOfTwoMinusOne(index + 1, this.capacityMinusOne);
            return slot;
        }
    }
}
