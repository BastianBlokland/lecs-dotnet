using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Lecs.Memory
{
    internal static class HashHelpers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Mix(int key)
        {
            // TODO: Add some form of bit mixing to avoid sequential keys (common case) having
            // sequential hashes (which is bad for our distribution)
            return key;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int UnMix(int key)
        {
            // TODO: Add some form of bit mixing to avoid sequential keys (common case) having
            // sequential hashes (which is bad for our distribution)
            return key;
        }

        /// <summary>
        /// Test if a given integer is a power-of-two
        /// </summary>
        /// <param name="num">Interger to test</param>
        /// <returns>'True' if its a power-of-two, otherwise 'False'</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsPowerOfTwo(int num)
        {
            Debug.Assert(num > 0, "Input has to be a non-zero positive integer");

            /* Details: https://graphics.stanford.edu/~seander/bithacks.html#DetermineIfPowerOf2 */
            return (num & (num - 1)) == 0;
        }

        /// <summary>
        /// Get the next power-of-two for given number.
        /// Note: Number does not need to be a power-of-two itself
        /// </summary>
        /// <param name="num">Number to get the next power-of-two for</param>
        /// <returns>Next power-of-two</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int NextPowerOfTwo(int num)
        {
            Debug.Assert(num >= 0, "Input has to be a positive integer");
            Debug.Assert(num < 1073741824, "No bigger power-of-two can be represented by an integer");

            return RoundUpToPowerOfTwo(num + 1);
        }

        /// <summary>
        /// Round the given value up to the next power-of-two
        /// </summary>
        /// <param name="num">Number to round up</param>
        /// <returns>Power-of-two</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int RoundUpToPowerOfTwo(int num)
        {
            Debug.Assert(num > 0, "Input has to be a non-zero positive integer");
            Debug.Assert(num <= 1073741824, "No power-of-two for given num can be represented by an integer");

            /* Details: https://graphics.stanford.edu/~seander/bithacks.html#RoundUpPowerOf2 */
            unchecked
            {
                num--;
                num |= num >> 1;
                num |= num >> 2;
                num |= num >> 4;
                num |= num >> 8;
                num |= num >> 16;
                num++;
            }
            return num;
        }

        /// <summary>
        /// Performs a fast modulo of a power-of-two.
        /// Behaves like: 'Abs(num % powerOfTwo)', Absolute because this algorithm does not handle
        /// negative numbers.
        /// </summary>
        /// <param name="num">Integer to modulo</param>
        /// <param name="powerOfTwo">Power-of-two to modulo two. NOTE: ONLY pass powers-of-two</param>
        /// <returns>Result of 'Abs(num % powerOfTwo)'</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int ModuloPowerOfTwo(int num, int powerOfTwo)
        {
            Debug.Assert(IsPowerOfTwo(powerOfTwo), "'powerOfTwo' is not a power-of-two");
            return ModuloPowerOfTwoMinusOne(num, powerOfTwo - 1);
        }

        /// <summary>
        /// Performs a fast modulo of a (power-of-two - 1).
        /// Behaves like: 'Abs(num % (powerOfTwoMinusOne + 1))'.
        /// MinusOne because it saves a operation and can often be cached
        /// </summary>
        /// <param name="num">Integer to modulo</param>
        /// <param name="powerOfTwoMinusOne">
        /// (Power-of-two - 1) to modulo to. NOTE: ONLY pass (powers-of-two - 1)
        /// </param>
        /// <returns>Result of 'Abs(num % (powerOfTwoMinusOne + 1))'</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int ModuloPowerOfTwoMinusOne(int num, int powerOfTwoMinusOne)
        {
            Debug.Assert(IsPowerOfTwo(powerOfTwoMinusOne + 1), "'powerOfTwoMinusOne' + 1 is not a power-of-two");
            return Abs(num) & powerOfTwoMinusOne;
        }

        /// <summary>
        /// Returns the absolute version of the given integer
        /// </summary>
        /// <param name="value">Value to absolute</param>
        /// <returns>Absolute version of <paramref name="value"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Abs(int value)
        {
            if (value < 0)
            {
                value = -value;
                Debug.Assert(value >= 0, "Negative value was not positive after negating");
            }
            return value;
        }
    }
}
