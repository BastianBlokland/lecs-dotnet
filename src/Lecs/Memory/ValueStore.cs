#pragma warning disable CA1710 // Identifiers should have correct suffix (Collection)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

using static Lecs.Memory.ValueStore;

namespace Lecs.Memory
{
    public static partial class ValueStore
    {
        /* Use this non-generic class for putting static data that does not need to be 'instantiated'
        per generic type */

        internal const int FreeKey = -1;
        internal const int UnavailableKey = -2;
    }

    public sealed partial class ValueStore<T> : IEnumerable<ValueStore.SlotToken>
        where T : unmanaged
    {
        private readonly float maxLoadFactor;

        private int[] keys; // Allocated 7 elements bigger for safe 256 bit jumps without range checking
        private T[] values;
        private int capacity; // Only power-of-two for fast modulo
        private int capacityMinusOne;
        private int maxCount;
        private int count;

        public ValueStore(int initialCapacity = 256, float maxLoadFactor = .75f)
        {
            if (initialCapacity <= 1 || initialCapacity > HashHelpers.BiggestPowerOfTwo)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(initialCapacity),
                    $"Must between 1 and {HashHelpers.BiggestPowerOfTwo + 1}'");
            }
            if (maxLoadFactor <= 0f || maxLoadFactor >= 1f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxLoadFactor),
                    "Must be between '0' and '1'");
            }

            this.maxLoadFactor = maxLoadFactor;
            this.Initialize(capacity: HashHelpers.RoundUpToPowerOfTwo(initialCapacity));
        }

        public int Count => this.count;

        public void Set(int key, in T value)
        {
            // Find a slot for this item
            bool exists = this.Find(key, out SlotToken slot);

            if (exists)
            {
                // If it already exists in the store we can just update it
                GetValueRef(slot, this.values) = value;
            }
            else // !exists
            {
                Debug.Assert(GetKeyRef(slot, this.keys) == FreeKey, "Slot to insert to has to be empty");

                // Set the key and value to this free slot
                GetKeyRef(slot, this.keys) = key;
                GetValueRef(slot, this.values) = value;

                // Increment the count and grow the store if it now contains too many elements
                this.count++;
                if (this.count > this.maxCount)
                {
                    var oldKeys = this.keys;
                    var oldValues = this.values;

                    // Grow the store
                    this.Initialize(capacity: HashHelpers.NextPowerOfTwo(capacity));

                    // Re-insert the old keys and values
                    var enumerator = new SlotEnumerator(oldKeys);
                    while (enumerator.MoveNext())
                    {
                        var oldSlot = enumerator.Current;
                        this.Set(
                            key: GetKeyRef(oldSlot, oldKeys),
                            value: in GetValueRef(oldSlot, oldValues));
                    }
                }
            }
        }

        public void Remove(int key)
        {
            // Find the slot for this item
            if (!this.Find(key, out SlotToken slot)) // If the key does not exist in the store there is no need to remove it
                return;

            // Decrement count of items in store
            this.count--;
            Debug.Assert(this.count >= 0, "Count should never go negative");

            // Shift the next keys until we find a empty slot or a key that is currently in its desired slot
            SlotToken nextSlot;
            while (true)
            {
                nextSlot = this.GetNextSlot(slot);
                Debug.Assert(nextSlot != slot, "Unable to find the next slot");

                ref int nextKey = ref GetKeyRef(nextSlot, this.keys);
                Debug.Assert(nextKey != key, "Either the same key lives in the store twice or we looped the entire store without finding a free-key");

                // If its the end of a chain then we are done shifting
                if (nextKey == FreeKey)
                    break;

                SlotToken desiredSlot = this.GetDesiredSlot(nextKey);
                // If this key is already in its desired slot then we are done shiting
                if (desiredSlot == nextSlot)
                    break;

                // Shift this slot over
                GetKeyRef(slot, this.keys) = GetKeyRef(nextSlot, this.keys);
                GetValueRef(slot, this.values) = GetValueRef(nextSlot, this.values);

                slot = nextSlot;
            }

            // Mark slot as now being free
            GetKeyRef(slot, this.keys) = FreeKey;
        }

        public void Clear()
        {
            Array.Fill(this.keys, value: FreeKey, startIndex: 0, count: capacity);
            this.count = 0;
        }

        public bool Find(int key, out SlotToken slot)
        {
            if (Avx2.IsSupported)
                return this.FindAvx2(key, out slot);
            else
                throw new NotImplementedException();
        }

        public int GetKey(SlotToken slot) =>
            // NOTE: We CANNOT hand out a ref to the key as we need to be in tight control of the
            // sequence of keys
            GetKeyRef(slot, this.keys);

        public ref T GetValue(SlotToken slot) =>
            // NOTE: Its mostly safe to hand out a ref to the value as its okay the caller to change
            // this without us knowing. The only exception to this is when we resize the store.
            ref GetValueRef(slot, this.values);

        public SlotEnumerator GetEnumerator() => new SlotEnumerator(this.keys);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref int GetKeyRef(SlotToken slot, int[] keys)
        {
            ref int index = ref Unsafe.As<SlotToken, int>(ref slot);

            // Assert that we are not indexing out of bounds
            Debug.Assert(index >= 0 && index < keys.Length, "'SlotToken' out of bounds");

            // Using explicit memory offsets to avoid the range checks that the array indexer does
            // normally. As long as we never shrink the arrays this should be safe because users cannot
            // create a 'SlotToken' themselves (without using reflection or Unsafe api's).
            return ref Unsafe.Add(ref keys[0], index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref T GetValueRef(SlotToken slot, T[] values)
        {
            ref int index = ref Unsafe.As<SlotToken, int>(ref slot);

            // Assert that we are not indexing out of bounds
            Debug.Assert(index >= 0 && index < values.Length, "'SlotToken' out of bounds");

            // Using explicit memory offsets to avoid the range checks that the array indexer does
            // normally. As long as we never shrink the arrays this should be safe because users cannot
            // create a 'SlotToken' themselves (without using reflection or Unsafe api's).
            return ref Unsafe.Add(ref values[0], index);
        }

        private static int GetMaxCount(int capacity, float maxLoadFactor)
        {
            /*
            Calculate how far we are allowed to fill up the store based on the configured load-factor.
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
            this.maxCount = GetMaxCount(capacity, maxLoadFactor);
            this.count = 0;

            // Initialize the keys, set 0 through capacity to 'FreeKey' and then add 7 more keys and
            // set them to 'UnavailableKey', this allows us to always load 8 keys at the time without
            // having to do range checks
            this.keys = new int[capacity + 7];
            Array.Fill(this.keys, value: FreeKey, startIndex: 0, count: capacity);
            Array.Fill(this.keys, value: UnavailableKey, startIndex: capacity, count: 7);

            // Initialize the value, leave them at default
            this.values = new T[capacity];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private SlotToken GetDesiredSlot(int key)
        {
            int hash = HashHelpers.Mix(key); // Mix the key to avoid sequential input causing many collisions
            int index = HashHelpers.ModuloPowerOfTwoMinusOne(hash, this.capacityMinusOne);
            return Unsafe.As<int, SlotToken>(ref index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private SlotToken GetNextSlot(SlotToken slot)
        {
            // Its safe to interpret 'SlotToken' as an integer because they have the same layout
            ref int index = ref Unsafe.As<SlotToken, int>(ref slot);

            // NOTE: We actually change the 'slot' parameter here because the 'index' is a int
            // reference over that variable, its safe because 'slot' parameter is passed as a copy
            index = HashHelpers.ModuloPowerOfTwoMinusOne(index + 1, this.capacityMinusOne);
            return slot;
        }

        IEnumerator<SlotToken> IEnumerable<SlotToken>.GetEnumerator() => new SlotEnumerator(this.keys);

        IEnumerator IEnumerable.GetEnumerator() => new SlotEnumerator(this.keys);
    }
}
