using System;

namespace Lecs.Memory
{
    public unsafe partial struct Mask256 : IEquatable<Mask256>
    {
        internal bool HasAllSoftware(in Mask256 other)
        {
            fixed (long* dataPointer = this.data)
            fixed (long* otherDataPointer = other.data)
            {
                // Todo: Benchmark if branching would be faster here
                bool result = true;
                for (int i = 0; i < 4; i++)
                    result &= (dataPointer[i] & otherDataPointer[i]) == otherDataPointer[i];
                return result;
            }
        }

        internal bool HasAnySoftware(in Mask256 other)
        {
            fixed (long* dataPointer = this.data)
            fixed (long* otherDataPointer = other.data)
            {
                // Todo: Benchmark if branching would be faster here
                bool result = false;
                for (int i = 0; i < 4; i++)
                    result |= (dataPointer[i] & otherDataPointer[i]) != 0;
                return result;
            }
        }

        internal void AddSoftware(in Mask256 other)
        {
            fixed (long* dataPointer = this.data)
            fixed (long* otherDataPointer = other.data)
            {
                for (int i = 0; i < 4; i++)
                    dataPointer[i] |= otherDataPointer[i];
            }
        }

        internal void RemoveSoftware(in Mask256 other)
        {
            fixed (long* dataPointer = this.data)
            fixed (long* otherDataPointer = other.data)
            {
                for (int i = 0; i < 4; i++)
                    dataPointer[i] &= ~otherDataPointer[i];
            }
        }

        internal void InvertSoftware()
        {
            fixed (long* dataPointer = this.data)
            {
                for (int i = 0; i < 4; i++)
                    dataPointer[i] = ~dataPointer[i];
            }
        }

        internal void ClearSoftware()
        {
            fixed (long* dataPointer = this.data)
            {
                for (int i = 0; i < 4; i++)
                    dataPointer[i] = 0;
            }
        }

        internal bool EqualsSoftware(in Mask256 other)
        {
            fixed (long* dataPointer = this.data)
            fixed (long* otherDataPointer = other.data)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (dataPointer[i] != otherDataPointer[i])
                        return false;
                }

                return true;
            }
        }
    }
}
