using System;

namespace Lecs.Memory
{
    public unsafe partial struct Mask256 : IEquatable<Mask256>
    {
        internal bool HasSoftware(in Mask256 other)
        {
            fixed (int* dataPointer = this.data)
            fixed (int* otherDataPointer = other.data)
            {
                // Todo: Benchmark if branching would be faster here
                bool result = true;
                for (int i = 0; i < 8; i++)
                    result &= (dataPointer[i] & otherDataPointer[i]) == otherDataPointer[i];
                return result;
            }
        }

        internal bool NotHasSoftware(in Mask256 other)
        {
            fixed (int* dataPointer = this.data)
            fixed (int* otherDataPointer = other.data)
            {
                // Todo: Benchmark if branching would be faster here
                bool result = true;
                for (int i = 0; i < 8; i++)
                    result &= (dataPointer[i] & otherDataPointer[i]) == 0;
                return result;
            }
        }

        internal void AddSoftware(in Mask256 other)
        {
            fixed (int* dataPointer = this.data)
            fixed (int* otherDataPointer = other.data)
            {
                for (int i = 0; i < 8; i++)
                    dataPointer[i] |= otherDataPointer[i];
            }
        }

        internal void RemoveSoftware(in Mask256 other)
        {
            fixed (int* dataPointer = this.data)
            fixed (int* otherDataPointer = other.data)
            {
                for (int i = 0; i < 8; i++)
                    dataPointer[i] &= ~otherDataPointer[i];
            }
        }

        internal void InvertSoftware()
        {
            fixed (int* dataPointer = this.data)
            {
                for (int i = 0; i < 8; i++)
                    dataPointer[i] = ~dataPointer[i];
            }
        }

        internal void ClearSoftware()
        {
            fixed (int* dataPointer = this.data)
            {
                for (int i = 0; i < 8; i++)
                    dataPointer[i] = 0;
            }
        }

        internal bool EqualsSoftware(in Mask256 other)
        {
            fixed (int* dataPointer = this.data)
            fixed (int* otherDataPointer = other.data)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (dataPointer[i] != otherDataPointer[i])
                        return false;
                }

                return true;
            }
        }
    }
}
