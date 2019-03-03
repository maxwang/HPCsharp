﻿// TODO: Create a generic version that can sort multiple data types, possibly like was done with Fill(), where we check which data type it is and call the appropriate
//       function underneath. This seems to be really hard in C#
// TODO: Use Array.Sort as the base algorithm (recursion termination case) for MSD Radix Sort, since it's in-place and uses Introspective Sort in the latest version
//       of .NET. Find the optimal threshold, which could be pretty large.
// TODO: One way to experiment to small % performance enhancements is to create two versions and compare their performance against each other. Plus find your statistical
//       analysis stuff and apply it as well. We need to be able to capture many small performance improvements.
// TODO: Implement Malte's discovery of taking advantage of ILP of the CPU, by "unrolling" series of swaps by reducing dependencies across several swaps.
//       It may be possible to create a function to generalize this method and make it available to developers, to extract more performance out of cascaded swaps.
//       And, make this a part of the swapping suite of functions.
// TODO: Consider accelerating the special case of all array elements going into one or a few bins. The case of one bin is common due to small arrays that use
//       large indexes. In the case of a single bin, there is nothing to do. In the case of a small number of bins, can we do it faster? Let's think about this case.
// TODO: Unrolling cascaded swaps may require an additional array to go thru, maybe, if that makes it simpler to place elements there and then to take them out of there.
//       Walk thru the cascaded flow by hand and see what kind of structure is needed and would work. Start by hardcoding to 2 level unroll to get it working and see if there is a benefit.
// TODO: Create a check of each bin against the size of the CPU cache (cache aware) and sort it using Array.Sort if the bin fits within L2 cache entirely (minus a bit for overhead)
// TODO: Parallelize Histogram of components of 64-bit elements (ulong, long, and double).
// TODO: Create an inner loop function that uses union to pull out the desired byte/digit, since we know this is faster than shifting and masking
// TODO: Create an adaptive MSD Radix Sort implementation, where if the array/bin is > 64K elements then use 32-bit entries in the count array, but if the array/bin is 64K elements or fewer
//       then use ushort count entries, which will fit more entries into L1 and L2 cache allowing for processing of more bits of each element per iteration - e.g. instead of 8-bits/digit we
//       could go up to 9-bits per digit, or from 10-bits/digit to 11-bits/digit, and the reduce the number of passes.
// TODO: Try 9-bits per digit to start with for MSD Radix Sort and leave the last digit to be 10-bits, since most likely we won't even get to that digit, or will use serial algorithm.
//       We could use the same idea with 10-bit and leave the last 4 digits to be 11-bits.
//       Or, we could start with 11-bit digits hoping that the upper bits will be all the same and be in one bin (optimistic).
//       11-bit digits with an adaptive 64K bin size could really help. Actually, the 64K adaptivity is a totally orthogonal idea to others.
// TODO: I figured out Malte's technique for more CPU ILP - it's simply handling multiple array items at a time. Instead of handling a single array item and then cascading swaps from there until
//       it loops back, Malte's realized that he could process two or more array items at a time, probably with some inner checking to make sure the loop isn't just two items. This will need to be
//       tested very carefully, especially using two values thru the entire array (with both possible phases). Yeah, you process two or more array elements at a time, which will add more complexity,
//       at a significant gain of performance. Once we do this, it would be worth contacting Brad's son to discuss with him how to add this to compilers in general to recogmize this kind of a pattern
//       and unroll it to gain performance by more ILP.
// TODO: It should be possible to generalize most significant digit detection (instead of hardcoding 56 right shift for 8-bit digit) by detecting if the MSD includes the most significant bit in it.
//       Could possibly codify into a function that gets rightShiftAmount and digit size. This leads to generalization of most significant digit detection to support digits of any size.
// TODO: To optimize performance of MSB Radix Sort apply optimizations discussed in https://www.youtube.com/watch?v=zqs87a_7zxw, which may bring performance closer
//       to the not-in-place version
// TODO: Consider implementing an unsafe version of MSD Radix Sort to see if performance increases because of fewer checks done C#.
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace HPCsharp
{
    static public partial class Algorithm
    {
#if false
        // This is a possible eventual goal of generic RadixSort implementation which will support more data types over time
        private static void SortRadixMsd<T>(this T[] arrayToBeSorted) where T : struct
        {
            int numBytesInItem = 0;
            if (typeof(T) == typeof(byte) || typeof(T) == typeof(sbyte))
                SortCountingInPlace(arrayToBeSorted);
            else if (typeof(T) == typeof(ushort) || typeof(T) != typeof(short))
                numBytesInItem = 2;
            else if (typeof(T) == typeof(uint) || typeof(T) != typeof(int))
                numBytesInItem = 4;
            else if (typeof(T) == typeof(ulong) || typeof(T) != typeof(long))
                numBytesInItem = 8;
            else
                throw new ArgumentException(string.Format("Type '{0}' is unsupported.", typeof(T).ToString()));
        }
#endif

        /// <summary>
        /// In-place Radix Sort (Most Significant Digit), not stable.
        /// </summary>
        /// <param name="arrayToBeSorted">array that is to be sorted in place</param>
        public static void SortRadixMsd(this byte[] arrayToBeSorted)
        {
            arrayToBeSorted.SortCountingInPlace();
        }

        /// <summary>
        /// In-place Radix Sort (Most Significant Digit), not stable. Functional style interface, which returns the input array, but sorted.
        /// </summary>
        /// <param name="arrayToBeSorted">array that is to be sorted in place</param>
        /// <returns>returns the input array itself, but sorted</returns>
        public static byte[] SortRadixMsdInPlaceFunc(this byte[] arrayToBeSorted)
        {
            arrayToBeSorted.SortCountingInPlace();
            return arrayToBeSorted;
        }

        /// <summary>
        /// In-place Radix Sort (Most Significant Digit), not stable.
        /// </summary>
        /// <param name="arrayToBeSorted">array that is to be sorted in place</param>
        public static void SortRadixMsd(this sbyte[] arrayToBeSorted)
        {
            arrayToBeSorted.SortCountingInPlace();
        }

        /// <summary>
        /// In-place Radix Sort (Most Significant Digit), not stable. Functional style interface, which returns the input array, but sorted.
        /// </summary>
        /// <param name="arrayToBeSorted">array that is to be sorted in place</param>
        /// <returns>returns the input array itself, but sorted</returns>
        public static sbyte[] SortRadixMsdInPlaceFunc(this sbyte[] arrayToBeSorted)
        {
            arrayToBeSorted.SortCountingInPlace();
            return arrayToBeSorted;
        }

        /// <summary>
        /// In-place Radix Sort (Most Significant Digit), not stable.
        /// </summary>
        /// <param name="arrayToBeSorted">array that is to be sorted in place</param>
        public static void SortRadixMsd(this ushort[] arrayToBeSorted)
        {
            arrayToBeSorted.SortCountingInPlace();
        }

        /// <summary>
        /// In-place Radix Sort (Most Significant Digit), not stable. Functional style interface, which returns the input array, but sorted.
        /// </summary>
        /// <param name="arrayToBeSorted">array that is to be sorted in place</param>
        /// <returns>returns the input array itself, but sorted</returns>
        public static ushort[] SortRadixMsdInPlaceFunc(this ushort[] arrayToBeSorted)
        {
            arrayToBeSorted.SortCountingInPlace();
            return arrayToBeSorted;
        }

        /// <summary>
        /// In-place Radix Sort (Most Significant Digit), not stable.
        /// </summary>
        /// <param name="arrayToBeSorted">array that is to be sorted in place</param>
        public static void SortRadixMsd(this short[] arrayToBeSorted)
        {
            arrayToBeSorted.SortCountingInPlace();
        }

        /// <summary>
        /// In-place Radix Sort (Most Significant Digit), not stable. Functional style interface, which returns the input array, but sorted.
        /// </summary>
        /// <param name="arrayToBeSorted">array that is to be sorted in place</param>
        /// <returns>returns the input array itself, but sorted</returns>
        public static short[] SortRadixMsdInPlaceFunc(this short[] arrayToBeSorted)
        {
            arrayToBeSorted.SortCountingInPlace();
            return arrayToBeSorted;
        }

        private static void SortRadixMsd(this int[] arrayToBeSorted)
        {
            // TODO: Implement me
        }

        private static void SortRadixMsd(this uint[] arrayToBeSorted)
        {
            // TODO: Implement me
        }

        public static Int32 SortRadixMsdShortThreshold { get; set; } = 1024;
        public static Int32 SortRadixMsdUShortThreshold { get; set; } = 1024;
        public static Int32 SortRadixMsdULongThreshold { get; set; } = 1024;
        public static Int32 SortRadixMsdLongThreshold { get; set; } = 64;
        public static Int32 SortRadixMsdDoubleThreshold { get; set; } = 1024;


        // Port of Victor's articles in Dr. Dobb's Journal January 14, 2011
        // Plain function In-place MSD Radix Sort implementation (simplified).
        private const int PowerOfTwoRadix       = 256;
        private const int Log2ofPowerOfTwoRadix =   8;
        private const int PowerOfTwoRadixDouble       = 4096;
        private const int Log2ofPowerOfTwoRadixDouble =   12;

        private static void RadixSortMsdULongInner(ulong[] a, int first, int length, int shiftRightAmount, Action<ulong[], int, int> baseCaseInPlaceSort)
        {
            if (length < SortRadixMsdULongThreshold)
            {
                baseCaseInPlaceSort(a, first, length);
                return;
            }
            int last = first + length - 1;
            const ulong bitMask = PowerOfTwoRadix - 1;

            var count = HistogramOneByteComponent(a, first, last, shiftRightAmount);

            var startOfBin = new int[PowerOfTwoRadix + 1];
            var endOfBin   = new int[PowerOfTwoRadix];
            int nextBin = 1;
            startOfBin[0] = endOfBin[0] = first; startOfBin[PowerOfTwoRadix] = -1;         // sentinal
            for (int i = 1; i < PowerOfTwoRadix; i++)
                startOfBin[i] = endOfBin[i] = startOfBin[i - 1] + count[i - 1];

            for (int _current = first; _current <= last;)
            {
                ulong digit;
                ulong current_element = a[_current];  // get the compiler to recognize that a register can be used for the loop instead of a[_current] memory location
                while (endOfBin[digit = (current_element >> shiftRightAmount) & bitMask] != _current)
                    Swap(ref current_element, a, endOfBin[digit]++);
                a[_current] = current_element;

                endOfBin[digit]++;
                while (endOfBin[nextBin - 1] == startOfBin[nextBin]) nextBin++;   // skip over empty and full bins, when the end of the current bin reaches the start of the next bin
                _current = endOfBin[nextBin - 1];
            }
            if (shiftRightAmount > 0)          // end recursion when all the bits have been processes
            {
                if (shiftRightAmount >= Log2ofPowerOfTwoRadix ) shiftRightAmount -= Log2ofPowerOfTwoRadix;
                else                                            shiftRightAmount  = 0;

                for (int i = 0; i < PowerOfTwoRadix; i++ )
                    RadixSortMsdULongInner( a, startOfBin[i], endOfBin[i] - startOfBin[i], shiftRightAmount, baseCaseInPlaceSort );
            }
        }
        private static void RadixSortMsdULongInner1(ulong[] a, int first, int length, int shiftRightAmount, Action<ulong[], int, int> baseCaseInPlaceSort)
        {
            if (length < SortRadixMsdULongThreshold)
            {
                baseCaseInPlaceSort(a, first, length);
                return;
            }
            int last = first + length - 1;
            const ulong bitMask = PowerOfTwoRadix - 1;

            var count = HistogramOneByteComponent(a, first, last, shiftRightAmount);

            var startOfBin = new int[PowerOfTwoRadix + 1];
            var endOfBin = new int[PowerOfTwoRadix];
            int nextBin = 1;
            startOfBin[0] = endOfBin[0] = first; startOfBin[PowerOfTwoRadix] = -1;         // sentinal
            for (int i = 1; i < PowerOfTwoRadix; i++)
                startOfBin[i] = endOfBin[i] = startOfBin[i - 1] + count[i - 1];

            for (int currIndex = first; currIndex <= last;)
            {
#if false
                ulong digit;
                while (endOfBin[digit = (a[currIndex] >> shiftRightAmount) & bitMask] != currIndex)
                    Swap(a, currIndex, endOfBin[digit]++);
                endOfBin[digit]++;
#else
                // TODO: Also the next element is not necessarily (currIndex + 1), since this element could be part of a Bin with already processed elements. So, we need to have a procedure to search for the
                //       next element that has not been processed yet, using the procedure as we do at the end of the "for" loop.
                if (currIndex + 1 <= last)      // Can we process two array elements concurrently
                {
                    ulong currDigit;
                    ulong nextDigit;
                    bool doneWith2ndElement = false;
                    while (true)
                    {
                        currDigit = (a[currIndex] >> shiftRightAmount) & bitMask;
                        if (endOfBin[currDigit] != currIndex)
                        {
                            Swap(a, currIndex, endOfBin[currDigit]++);

                            // TODO: Need to guard against the (current+1) element hitting the (current) element when it needs to place an element into that Bin (e.g. Bin0 at the beginning of processing)
                            //       I need to figure out what to do in that case and draw this case out. When the second element hits the first! I think in this case processing needs to stop, possibly, and
                            //       we go to the top of the for loop again. Most likely that's the problem with the algorithm at the moment, as processing the second element is interfereing with processing of
                            //       the first, since this is an in-place algorithm.

                            if (!doneWith2ndElement)
                            {
                                nextDigit = (a[currIndex + 1] >> shiftRightAmount) & bitMask;   // concurrently process the next (currIndex+1) element of the array
                                if (endOfBin[nextDigit] == currIndex)       // 2nd element (currIndex + 1) hits the 1st element (currIndex)
                                {
                                    Swap(a, currIndex + 1, endOfBin[nextDigit]++);
                                    break;
                                }

                                if (endOfBin[nextDigit] != (currIndex + 1))
                                {
                                    Swap(a, currIndex + 1, endOfBin[nextDigit]++);
                                }
                                else
                                {
                                    endOfBin[nextDigit]++;
                                    doneWith2ndElement = true;
                                }
                            }
                        }
                        else
                        {
                            endOfBin[currDigit]++;
                            break;
                        }
                    }
                }
                else
                {
                    ulong digit;
                    while (endOfBin[digit = (a[currIndex] >> shiftRightAmount) & bitMask] != currIndex)
                        Swap(a, currIndex, endOfBin[digit]++);
                    endOfBin[digit]++;
                }
#endif
                while (endOfBin[nextBin - 1] == startOfBin[nextBin]) nextBin++;   // skip over empty and full bins, when the end of the current bin reaches the start of the next bin
                currIndex = endOfBin[nextBin - 1];
            }
            if (shiftRightAmount > 0)          // end recursion when all the bits have been processes
            {
                if (shiftRightAmount >= Log2ofPowerOfTwoRadix) shiftRightAmount -= Log2ofPowerOfTwoRadix;
                else shiftRightAmount = 0;

                for (int i = 0; i < PowerOfTwoRadix; i++)
                    RadixSortMsdULongInner1(a, startOfBin[i], endOfBin[i] - startOfBin[i], shiftRightAmount, baseCaseInPlaceSort);
            }
        }

        private static void devFunction(ulong[] a, int first, int length, int shiftRightAmount)
        {
            var endOfBin = new int[PowerOfTwoRadix];
            int last = first + length - 1;
            const ulong bitMask = PowerOfTwoRadix - 1;

            for (int _current = first; _current <= last;)
            {
                ulong digit;
                ulong current_element = a[_current];
                while (true)
                {
                    digit = (current_element >> shiftRightAmount) & bitMask;
                    if (endOfBin[digit] == _current) break;
                    Swap(ref current_element, a, endOfBin[digit]++);
                }
                a[_current] = current_element;
            }
        }
        private static void devFunctionUnrolled(ulong[] a, int first, int length, int shiftRightAmount)
        {
            var endOfBin = new int[PowerOfTwoRadix];
            int last = first + length - 1;
            const ulong bitMask = PowerOfTwoRadix - 1;

            for (int _current = first; _current <= last;)
            {
                ulong digit;
                var elementBuffer = new ulong[4];
                int index = 0;
                elementBuffer[index] = a[_current];
                while (true)
                {
                    digit = (elementBuffer[index] >> shiftRightAmount) & bitMask;
                    if (endOfBin[digit] == _current) break;
                    elementBuffer[++index] = a[endOfBin[digit]];
                    digit = (elementBuffer[index] >> shiftRightAmount) & bitMask;
                    if (endOfBin[digit] == _current)
                    {
                        a[endOfBin[digit]++] = elementBuffer[index];
                        break;
                    }
                    elementBuffer[++index] = a[endOfBin[digit]++];
                }
                a[_current] = elementBuffer[index];
            }
        }

        /// <summary>
        /// In-place Radix Sort (Most Significant Digit), not stable.
        /// </summary>
        /// <param name="arrayToBeSorted">array that is to be sorted in place</param>
        public static void SortRadixMsd(this ulong[] arrayToBeSorted)
        {
            int shiftRightAmount = sizeof(ulong) * 8 - Log2ofPowerOfTwoRadix;
            // InsertionSort could be passed in as another base case since it's in-place
            RadixSortMsdULongInner(arrayToBeSorted, 0, arrayToBeSorted.Length, shiftRightAmount, Array.Sort);
        }

        /// <summary>
        /// In-place Radix Sort (Most Significant Digit), not stable. Functional style interface, which returns the input array, but sorted.
        /// </summary>
        /// <param name="arrayToBeSorted">array that is to be sorted in place</param>
        /// <returns>returns the input array itself, but sorted</returns>
        public static ulong[] SortRadixMsdInPlaceFunc(this ulong[] arrayToBeSorted)
        {
            arrayToBeSorted.SortRadixMsd();
            return arrayToBeSorted;
        }

        private static void RadixSortMsdLongInner(long[] a, int first, int length, int shiftRightAmount, Action<long[], int, int> baseCaseInPlaceSort)
        {
            int last = first + length - 1;
            const ulong bitMask = PowerOfTwoRadix - 1;
            const ulong halfOfPowerOfTwoRadix = PowerOfTwoRadix / 2;
            //Stopwatch stopwatch = new Stopwatch();
            //long frequency = Stopwatch.Frequency;
            //Console.WriteLine("  Timer frequency in ticks per second = {0}", frequency);
            //long nanosecPerTick = (1000L * 1000L * 1000L) / frequency;

            //stopwatch.Restart();

            var count = HistogramOneByteComponent(a, first, last, shiftRightAmount);

            //stopwatch.Stop();
            //double timeForCounting = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;
            //Console.WriteLine("Time for counting: {0}", timeForCounting);

            var startOfBin = new int[PowerOfTwoRadix + 1];
            var endOfBin   = new int[PowerOfTwoRadix];
            int nextBin = 1;
            startOfBin[0] = endOfBin[0] = first; startOfBin[PowerOfTwoRadix] = -1;         // sentinal
            for (int i = 1; i < PowerOfTwoRadix; i++)
                startOfBin[i] = endOfBin[i] = startOfBin[i - 1] + count[i - 1];
            int bucketsUsed = 0;
            for (int i = 0; i < count.Length; i++)
                if (count[i] > 0) bucketsUsed++;

            //stopwatch.Restart();

            if (bucketsUsed > 1)
            {
                if (shiftRightAmount == 56)     // Most significant digit
                {
                    for (int _current = first; _current <= last;)
                    {
                        ulong digit;
                        long current_element = a[_current];  // get the compiler to recognize that a register can be used for the loop instead of a[_current] memory location
                        while (endOfBin[digit = ((ulong)current_element >> shiftRightAmount) ^ halfOfPowerOfTwoRadix] != _current)
                            Swap(ref current_element, a, endOfBin[digit]++);
                        a[_current] = current_element;                          // place the current_element in the a[_current] location, since we hit the end of the current loop, and advance its current bin end
                        endOfBin[digit]++;

                        while (endOfBin[nextBin - 1] == startOfBin[nextBin]) nextBin++;   // skip over empty and full bins, when the end of the current bin reaches the start of the next bin
                        _current = endOfBin[nextBin - 1];
                    }
                }
                else
                {
                    for (int _current = first; _current <= last;)
                    {
                        ulong digit;
                        long current_element = a[_current];  // get the compiler to recognize that a register can be used for the loop instead of a[_current] memory location
                        while (endOfBin[digit = ((ulong)current_element >> shiftRightAmount) & bitMask] != _current)
                            Swap(ref current_element, a, endOfBin[digit]++);
                        a[_current] = current_element;
                        endOfBin[digit]++;

                        while (endOfBin[nextBin - 1] == startOfBin[nextBin]) nextBin++;   // skip over empty and full bins, when the end of the current bin reaches the start of the next bin
                        _current = endOfBin[nextBin - 1];

                    }
                }
                //stopwatch.Stop();
                //double timeForPermuting = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;
                //Console.WriteLine("Size = {0}, Time for counting: {1}, Time for permuting: {2}, Ratio = {3:0.00}", length, timeForCounting, timeForPermuting, timeForCounting/timeForPermuting);

                if (shiftRightAmount > 0)    // end recursion when all the bits have been processes
                {
                    shiftRightAmount = shiftRightAmount >= Log2ofPowerOfTwoRadix ? shiftRightAmount -= Log2ofPowerOfTwoRadix : 0;

                    for (int i = 0; i < PowerOfTwoRadix; i++)
                    {
                        int numElements = endOfBin[i] - startOfBin[i];

                        if (numElements >= SortRadixMsdLongThreshold)
                            RadixSortMsdLongInner(a, startOfBin[i], numElements, shiftRightAmount, baseCaseInPlaceSort);
                        else if (numElements >= 2)
                            //InsertionSort(a, startOfBin[i], numElements);
                            baseCaseInPlaceSort(a, startOfBin[i], numElements);
                    }
                }
            }
            else
            {
                if (shiftRightAmount > 0)    // end recursion when all the bits have been processes
                {
                    shiftRightAmount = shiftRightAmount >= Log2ofPowerOfTwoRadix ? shiftRightAmount -= Log2ofPowerOfTwoRadix : 0;

                    if (length >= SortRadixMsdLongThreshold)
                        RadixSortMsdLongInner(a, first, length, shiftRightAmount, baseCaseInPlaceSort);
                    else if (length >= 2)
                        //InsertionSort(a, first, length);
                        baseCaseInPlaceSort(a, first, length);
                }
            }
        }

        // Simpler implementation, which just uses the array elements to swap and never extracts the current element into a variable on the inner swap loop
        // This version is easier to reason about
        private static void RadixSortMsdLongInner1(long[] a, int first, int length, int shiftRightAmount, Action<long[], int, int> baseCaseInPlaceSort)
        {
            if (length < SortRadixMsdLongThreshold)
            {
                baseCaseInPlaceSort(a, first, length);
                //InsertionSort(a, first, length);
                return;
            }
            int last = first + length - 1;
            const ulong bitMask = PowerOfTwoRadix - 1;
            const ulong halfOfPowerOfTwoRadix = PowerOfTwoRadix / 2;

            var count = HistogramByteComponentsUsingUnion(a, first, last, shiftRightAmount);

            var startOfBin = new int[PowerOfTwoRadix + 1];
            var endOfBin = new int[PowerOfTwoRadix];
            int nextBin = 1;
            startOfBin[0] = endOfBin[0] = first; startOfBin[PowerOfTwoRadix] = -1;         // sentinal
            for (int i = 1; i < PowerOfTwoRadix; i++)
                startOfBin[i] = endOfBin[i] = startOfBin[i - 1] + count[i - 1];
            int bucketsUsed = 0;
            for (int i = 0; i < count.Length; i++)
                if (count[i] > 0) bucketsUsed++;

            if (bucketsUsed > 1)
            {
                if (shiftRightAmount == 56)     // Most significant digit
                {
                    for (int _current = first; _current <= last;)
                    {
                        ulong digit;
                        while (endOfBin[digit = ((ulong)a[_current] >> shiftRightAmount) ^ halfOfPowerOfTwoRadix] != _current)
                            Swap(a, _current, endOfBin[digit]++);
                        endOfBin[digit]++;

                        while (endOfBin[nextBin - 1] == startOfBin[nextBin]) nextBin++;   // skip over empty and full bins, when the end of the current bin reaches the start of the next bin
                        _current = endOfBin[nextBin - 1];
                    }
                }
                else
                {
                    for (int _current = first; _current <= last;)
                    {
                        ulong digit;
                        while (endOfBin[digit = ((ulong)a[_current] >> shiftRightAmount) & bitMask] != _current)
                            Swap(a, _current, endOfBin[digit]++);
                        endOfBin[digit]++;

                        while (endOfBin[nextBin - 1] == startOfBin[nextBin]) nextBin++;   // skip over empty and full bins, when the end of the current bin reaches the start of the next bin
                        _current = endOfBin[nextBin - 1];

                    }
                }
                if (shiftRightAmount > 0)    // end recursion when all the bits have been processes
                {
                    shiftRightAmount = shiftRightAmount >= Log2ofPowerOfTwoRadix ? shiftRightAmount -= Log2ofPowerOfTwoRadix : 0;

                    for (int i = 0; i < PowerOfTwoRadix; i++)
                        RadixSortMsdLongInner1(a, startOfBin[i], endOfBin[i] - startOfBin[i], shiftRightAmount, baseCaseInPlaceSort);
                }
            }
            else
            {
                if (shiftRightAmount > 0)    // end recursion when all the bits have been processes
                {
                    shiftRightAmount = shiftRightAmount >= Log2ofPowerOfTwoRadix ? shiftRightAmount -= Log2ofPowerOfTwoRadix : 0;
                    RadixSortMsdLongInner1(a, first, length, shiftRightAmount, baseCaseInPlaceSort);
                }
            }
        }

        // This implementation unrolls swapping to process several array elements in the swap cascade/chain
        private static void RadixSortMsdLongInner2(long[] a, int first, int length, int shiftRightAmount, Action<long[], int, int> baseCaseInPlaceSort)
        {
            int last = first + length - 1;
            const ulong bitMask = PowerOfTwoRadix - 1;
            const ulong halfOfPowerOfTwoRadix = PowerOfTwoRadix / 2;

            var count = HistogramByteComponentsUsingUnion(a, first, last, shiftRightAmount);

            var startOfBin = new int[PowerOfTwoRadix + 1];
            var endOfBin = new int[PowerOfTwoRadix];
            int nextBin = 1;
            startOfBin[0] = endOfBin[0] = first; startOfBin[PowerOfTwoRadix] = -1;         // sentinal
            for (int i = 1; i < PowerOfTwoRadix; i++)
                startOfBin[i] = endOfBin[i] = startOfBin[i - 1] + count[i - 1];
            int bucketsUsed = 0;
            for (int i = 0; i < count.Length; i++)
                if (count[i] > 0) bucketsUsed++;

            if (bucketsUsed > 1)
            {
                if (shiftRightAmount == 56)     // Most significant digit
                {
                    for (int _current = first; _current <= last;)
                    {
                        ulong digit;
                        long current_element = a[_current];  // get the compiler to recognize that a register can be used for the loop instead of a[_current] memory location
                        while (endOfBin[digit = ((ulong)current_element >> shiftRightAmount) ^ halfOfPowerOfTwoRadix] != _current)
                            Swap(ref current_element, a, endOfBin[digit]++);
                        a[_current] = current_element;
                        endOfBin[digit]++;

                        while (endOfBin[nextBin - 1] == startOfBin[nextBin]) nextBin++;   // skip over empty and full bins, when the end of the current bin reaches the start of the next bin
                        _current = endOfBin[nextBin - 1];
                    }
                }
                else
                {
                    for (int currIndex = first; currIndex <= last;)
                    {
                        ulong ceDigit;                      // digit of the current element
                        ulong neDigit;                      // digit of the next    element
                        long currentElement = a[currIndex]; // get the compiler to recognize that a register can be used for the loop instead of a[_current] memory location
                        long nextElement;
                        while (true)
                        {
                            ceDigit = ((ulong)currentElement >> shiftRightAmount) & bitMask;
                            if (endOfBin[ceDigit] == currIndex)
                            {
                                a[currIndex] = currentElement;      // since we've been swapping with the currentElement, it has the latest element and we need to put it back into the array
                                endOfBin[ceDigit]++;                // place the current_element in the a[_current] location, since we hit the end of the current loop, and advance its current bin end
                                break;
                            }
                            nextElement = a[endOfBin[ceDigit]];
                            neDigit = ((ulong)nextElement >> shiftRightAmount) & bitMask;
                            if (endOfBin[neDigit] == currIndex)
                            {
                                a[endOfBin[ceDigit]++] = currentElement;  // move the currentElement into its location within the array, and advance that Bin
                                a[currIndex] = nextElement;
                                endOfBin[neDigit]++;
                                break;
                            }
                            a[endOfBin[ceDigit]++] = currentElement;  // move the currentElement into its location within the array, and advance that Bin
                            currentElement = nextElement;
                        }

                        while (endOfBin[nextBin - 1] == startOfBin[nextBin]) nextBin++;   // skip over empty and full bins, when the end of the current bin reaches the start of the next bin
                        currIndex = endOfBin[nextBin - 1];
                    }
                }
                if (shiftRightAmount > 0)    // end recursion when all the bits have been processes
                {
                    shiftRightAmount = shiftRightAmount >= Log2ofPowerOfTwoRadix ? shiftRightAmount -= Log2ofPowerOfTwoRadix : 0;

                    for (int i = 0; i < PowerOfTwoRadix; i++)
                    {
                        int numElements = endOfBin[i] - startOfBin[i];

                        if (numElements >= SortRadixMsdLongThreshold)
                            RadixSortMsdLongInner2(a, startOfBin[i], numElements, shiftRightAmount, baseCaseInPlaceSort);
                        else if (numElements >= 2)
                            baseCaseInPlaceSort(a, startOfBin[i], numElements);
                    }
                }
            }
            else
            {
                if (shiftRightAmount > 0)    // end recursion when all the bits have been processes
                {
                    shiftRightAmount = shiftRightAmount >= Log2ofPowerOfTwoRadix ? shiftRightAmount -= Log2ofPowerOfTwoRadix : 0;

                    if (length >= SortRadixMsdLongThreshold)
                        RadixSortMsdLongInner2(a, first, length, shiftRightAmount, baseCaseInPlaceSort);
                    else if (length >= 2)
                        baseCaseInPlaceSort(a, first, length);
                }
            }
        }

        private static void RadixSortMsdLongNbitInner(long[] a, int first, int length, int shiftRightAmount, int numberOfBitsPerDigit, Action<long[], int, int> baseCaseInPlaceSort)
        {
            int last = first + length - 1;
            const int NumBitsInLong = sizeof(long) * 8;
            ulong numberOfBins  = 1UL << numberOfBitsPerDigit;
            ulong bitMask       = numberOfBins - 1;
            ulong halfOfPowerOfTwoRadix = numberOfBins / 2;

            var count = HistogramNbitComponents(a, first, last, shiftRightAmount, numberOfBitsPerDigit);

            var startOfBin = new int[numberOfBins + 1];
            var endOfBin   = new int[numberOfBins];
            int nextBin = 1;
            startOfBin[0] = endOfBin[0] = first; startOfBin[numberOfBins] = -1;         // sentinal
            for (int i = 1; i < (int)numberOfBins; i++)
                startOfBin[i] = endOfBin[i] = startOfBin[i - 1] + count[i - 1];
            int bucketsUsed = 0;
            for (int i = 0; i < count.Length; i++)
                if (count[i] > 0) bucketsUsed++;

            if (bucketsUsed > 1)
            {
                if (shiftRightAmount == (NumBitsInLong - numberOfBitsPerDigit))     // Most significant digit
                {
                    for (int _current = first; _current <= last;)
                    {
                        ulong digit;
                        long current_element = a[_current];  // get the compiler to recognize that a register can be used for the loop instead of a[_current] memory location
                        while (endOfBin[digit = ((ulong)current_element >> shiftRightAmount) ^ halfOfPowerOfTwoRadix] != _current)
                            Swap(ref current_element, a, endOfBin[digit]++);
                        a[_current] = current_element;                          // place the current_element in the a[_current] location, since we hit the end of the current loop, and advance its current bin end
                        endOfBin[digit]++;

                        while (endOfBin[nextBin - 1] == startOfBin[nextBin]) nextBin++;   // skip over empty and full bins, when the end of the current bin reaches the start of the next bin
                        _current = endOfBin[nextBin - 1];
                    }
                }
                else
                {
                    for (int _current = first; _current <= last;)
                    {
                        ulong digit;
                        long current_element = a[_current];  // get the compiler to recognize that a register can be used for the loop instead of a[_current] memory location
                        while (endOfBin[digit = ((ulong)current_element >> shiftRightAmount) & bitMask] != _current)
                            Swap(ref current_element, a, endOfBin[digit]++);
                        a[_current] = current_element;
                        endOfBin[digit]++;

                        while (endOfBin[nextBin - 1] == startOfBin[nextBin]) nextBin++;   // skip over empty and full bins, when the end of the current bin reaches the start of the next bin
                        _current = endOfBin[nextBin - 1];

                    }
                }
                if (shiftRightAmount > 0)    // end recursion when all the bits have been processes
                {
                    shiftRightAmount = shiftRightAmount >= numberOfBitsPerDigit ? shiftRightAmount -= numberOfBitsPerDigit : 0;

                    for (int i = 0; i < (int)numberOfBins; i++)
                    {
                        int numElements = endOfBin[i] - startOfBin[i];

                        if (numElements >= SortRadixMsdLongThreshold)
                            RadixSortMsdLongNbitInner(a, startOfBin[i], numElements, shiftRightAmount, numberOfBitsPerDigit, baseCaseInPlaceSort);
                        else if (numElements >= 2)
                            baseCaseInPlaceSort(a, startOfBin[i], numElements);
                    }
                }
            }
            else
            {
                if (shiftRightAmount > 0)    // end recursion when all the bits have been processes
                {
                    shiftRightAmount = shiftRightAmount >= numberOfBitsPerDigit ? shiftRightAmount -= numberOfBitsPerDigit : 0;

                    if (length >= SortRadixMsdLongThreshold)
                        RadixSortMsdLongNbitInner(a, first, length, shiftRightAmount, numberOfBitsPerDigit, baseCaseInPlaceSort);
                    else if (length >= 2)
                        baseCaseInPlaceSort(a, first, length);
                }
            }
        }

        /// <summary>
        /// In-place Radix Sort (Most Significant Digit), not stable.
        /// </summary>
        /// <param name="arrayToBeSorted">array that is to be sorted in place</param>
        public static void SortRadixMsd(this long[] arrayToBeSorted)
        {
            int shiftRightAmount = sizeof(ulong) * 8 - Log2ofPowerOfTwoRadix;
            // InsertionSort could be passed in as another base case since it's in-place
            RadixSortMsdLongInner(arrayToBeSorted, 0, arrayToBeSorted.Length, shiftRightAmount, Array.Sort);
        }

        /// <summary>
        /// In-place Radix Sort (Most Significant Digit), not stable. Functional style interface, which returns the input array, but sorted.
        /// </summary>
        /// <param name="arrayToBeSorted">array that is to be sorted in place</param>
        /// <returns>returns the input array itself, but sorted</returns>
        public static long[] SortRadixMsdInPlaceFunc(this long[] arrayToBeSorted)
        {
            arrayToBeSorted.SortRadixMsd();
            return arrayToBeSorted;
        }

        /// <summary>
        /// In-place Radix Sort (Most Significant Digit), not stable.
        /// </summary>
        /// <param name="arrayToBeSorted">array that is to be sorted in place</param>
        public static void SortRadixNbitMsd(this long[] arrayToBeSorted, int numberOfBitsPerDigit = 10)
        {
            int shiftRightAmount = sizeof(ulong) * 8 - numberOfBitsPerDigit;
            // InsertionSort could be passed in as another base case since it's in-place
            RadixSortMsdLongNbitInner(arrayToBeSorted, 0, arrayToBeSorted.Length, shiftRightAmount, numberOfBitsPerDigit, Array.Sort);
        }

        /// <summary>
        /// In-place Radix Sort (Most Significant Digit), not stable. Functional style interface, which returns the input array, but sorted.
        /// </summary>
        /// <param name="arrayToBeSorted">array that is to be sorted in place</param>
        /// <returns>returns the input array itself, but sorted</returns>
        public static long[] SortRadixMsdNbitInPlaceFunc(this long[] arrayToBeSorted, int numberOfBitsPerDigit = 10)
        {
            arrayToBeSorted.SortRadixNbitMsd(numberOfBitsPerDigit);
            return arrayToBeSorted;
        }

        private static void RadixSortDoubleInner(double[] a, int first, int length, int shiftRightAmount, Action<double[], int, int> baseCaseInPlaceSort)
        {
            if (length < SortRadixMsdDoubleThreshold)
            {
                baseCaseInPlaceSort(a, first, length);
                return;
            }
            int last = first + length - 1;
            const ulong bitMask = 0xfff;        // 12-bits for the exponent and process mantissa 12-bits at a time

            var count = Histogram12bitComponents(a, first, last, shiftRightAmount);

            var startOfBin = new int[PowerOfTwoRadixDouble + 1];
            var endOfBin   = new int[PowerOfTwoRadixDouble];
            int nextBin = 1;
            startOfBin[0] = endOfBin[0] = first; startOfBin[PowerOfTwoRadixDouble] = -1;         // sentinal
            for (int i = 1; i < PowerOfTwoRadixDouble; i++)
                startOfBin[i] = endOfBin[i] = startOfBin[i - 1] + count[i - 1];

            if (shiftRightAmount == 52)     // Exponent
            {
                for (int _current = first; _current <= last;)
                {
                    ulong digit;
                    double current_element = a[_current];  // get the compiler to recognize that a register can be used for the loop instead of a[_current] memory location
                    while (endOfBin[digit = ((ulong)current_element >> shiftRightAmount) ^ 2048] != _current)
                        Swap(ref current_element, a, endOfBin[digit]++);
                    a[_current] = current_element;

                    endOfBin[digit]++;
                    while (endOfBin[nextBin - 1] == startOfBin[nextBin]) nextBin++;   // skip over empty and full bins, when the end of the current bin reaches the start of the next bin
                    _current = endOfBin[nextBin - 1];
                }
            }
            else     // Mantissa
            {
                for (int _current = first; _current <= last;)
                {
                    ulong digit;
                    double current_element = a[_current];  // get the compiler to recognize that a register can be used for the loop instead of a[_current] memory location
                    while (endOfBin[digit = ((ulong)current_element >> shiftRightAmount) & bitMask] != _current)
                        Swap(ref current_element, a, endOfBin[digit]++);
                    a[_current] = current_element;

                    endOfBin[digit]++;
                    while (endOfBin[nextBin - 1] == startOfBin[nextBin]) nextBin++;   // skip over empty and full bins, when the end of the current bin reaches the start of the next bin
                    _current = endOfBin[nextBin - 1];
                }
            }
            if (shiftRightAmount > 0)    // end recursion when all the bits have been processes
            {
                if (shiftRightAmount >= Log2ofPowerOfTwoRadixDouble) shiftRightAmount -= Log2ofPowerOfTwoRadixDouble;
                else shiftRightAmount = 0;

                for (int i = 0; i < PowerOfTwoRadixDouble; i++)
                    RadixSortDoubleInner(a, startOfBin[i], endOfBin[i] - startOfBin[i], shiftRightAmount, baseCaseInPlaceSort);
            }
        }
        /// <summary>
        /// In-place Radix Sort (Most Significant Digit), not stable.
        /// </summary>
        /// <param name="arrayToBeSorted">array that is to be sorted in place</param>
        public static void SortRadixMsd(this double[] arrayToBeSorted)
        {
            int shiftRightAmount = sizeof(double) * 8 - Log2ofPowerOfTwoRadixDouble;
            // InsertionSort could be passed in as another base case since it's in-place
            RadixSortDoubleInner(arrayToBeSorted, 0, arrayToBeSorted.Length, shiftRightAmount, Array.Sort);
        }

        /// <summary>
        /// In-place Radix Sort (Most Significant Digit), not stable. Functional style interface, which returns the input array, but sorted.
        /// </summary>
        /// <param name="arrayToBeSorted">array that is to be sorted in place</param>
        /// <returns>returns the input array itself, but sorted</returns>
        public static double[] SortRadixMsdInPlaceFunc(this double[] arrayToBeSorted)
        {
            arrayToBeSorted.SortRadixMsd();
            return arrayToBeSorted;
        }

#if false
        private static void RadixSortUnsignedPowerOf2RadixSimple1(ulong[] a, int first, int length, int currentDigit, int Threshold)
        {
            if (length < Threshold)
            {
                //InsertionSort(a, first, length);
                Array.Sort(a, first, length);
                return;
            }
            int last = first + length - 1;

            var count = HistogramByteComponents(a, first, length, currentDigit);

            var startOfBin = new int[PowerOfTwoRadix + 1];
            var endOfBin = new int[PowerOfTwoRadix];
            int nextBin = 1;
            startOfBin[0] = endOfBin[0] = first; startOfBin[PowerOfTwoRadix] = -1;         // sentinal
            for (int i = 1; i < PowerOfTwoRadix; i++)
                startOfBin[i] = endOfBin[i] = startOfBin[i - 1] + count[i - 1];

            var union = new UInt64ByteUnion();
            for (int _current = first; _current <= last;)
            {
                ulong digit;
                ulong current_element = a[_current];  // get the compiler to recognize that a register can be used for the loop instead of a[_current] memory location
                while (true)
                {
                    union.integer = current_element;
                    if (endOfBin[digit = (current_element & bitMask) >> shiftRightAmount] != _current)
                        Swap(ref current_element, a, endOfBin[digit]++);
                }
                a[_current] = current_element;

                endOfBin[digit]++;
                while (endOfBin[nextBin - 1] == startOfBin[nextBin]) nextBin++;   // skip over empty and full bins, when the end of the current bin reaches the start of the next bin
                _current = endOfBin[nextBin - 1];
            }
            currentDigit--;
            if (currentDigit >= 0)                     // end recursion when all the bits have been processes
            {
                for (int i = 0; i < PowerOfTwoRadix; i++)
                    RadixSortUnsignedPowerOf2RadixSimple1(a, startOfBin[i], endOfBin[i] - startOfBin[i], currentDigit, Threshold);
            }
        }
        public static ulong[] RadixSortMsd1(this ulong[] arrayToBeSorted)
        {
            int shiftRightAmount = sizeof(ulong) * 8 - Log2ofPowerOfTwoRadix;
            ulong bitMask = ((ulong)(PowerOfTwoRadix - 1)) << shiftRightAmount;  // bitMask controls/selects how many and which bits we process at a time
            const int Threshold = 1000;
            int currentDigit = 7;
            RadixSortUnsignedPowerOf2RadixSimple1(arrayToBeSorted, 0, arrayToBeSorted.Length, currentDigit, Threshold);
            return arrayToBeSorted;
        }
#endif
        private static void RadixSortMsdUShortInner(ushort[] a, int first, int length, ushort bitMask, int shiftRightAmount, Action<ushort[], int, int> baseCaseInPlaceSort)
        {
            //Console.WriteLine("Lower: first = {0} length = {1} bitMask = {2:X} shiftRightAmount = {3} ", first, length, bitMask, shiftRightAmount);
            if (length < SortRadixMsdUShortThreshold)
            {
                baseCaseInPlaceSort(a, first, length);
                return;
            }
            int last = first + length - 1;

            var count = new int[PowerOfTwoRadix];
            for (int i = 0; i < PowerOfTwoRadix; i++)
                count[i] = 0;
            //Console.WriteLine("inArray: ");
            for (int _current = first; _current <= last; _current++)
            {
                //Console.Write("{0:X} ", a[_current]);
                count[(a[_current] & bitMask) >> shiftRightAmount]++;
            }
            //Console.WriteLine();

            //Console.WriteLine("count: ");
            //for (int i = 0; i < PowerOfTwoRadix; i++)
            //    Console.Write(count[i] + " ");
            //Console.WriteLine();

            var startOfBin = new int[PowerOfTwoRadix + 1];
            var endOfBin   = new int[PowerOfTwoRadix];
            int nextBin = 1;
            //Console.WriteLine("EndOfBin: ");
            startOfBin[0] = endOfBin[0] = first; startOfBin[PowerOfTwoRadix] = -1;         // sentinal
            //Console.Write(endOfBin[0] + " ");
            for (int i = 1; i < PowerOfTwoRadix; i++)
            {
                startOfBin[i] = endOfBin[i] = startOfBin[i - 1] + count[i - 1];
                //Console.Write(endOfBin[i] + " ");
            }
            //Console.WriteLine();

            for (int _current = first; _current <= last;)
            {
                ushort digit;
                ushort current_element = a[_current];  // get the compiler to recognize that a register can be used for the loop instead of a[_current] memory location
                while (endOfBin[digit = (ushort)((current_element & bitMask) >> shiftRightAmount)] != _current)
                    Swap(ref current_element, a, endOfBin[digit]++);
                a[_current] = current_element;

                endOfBin[digit]++;
                while (endOfBin[nextBin - 1] == startOfBin[nextBin]) nextBin++;   // skip over empty and full bins, when the end of the current bin reaches the start of the next bin
                _current = endOfBin[nextBin - 1];
            }
            bitMask >>= Log2ofPowerOfTwoRadix;
            if (bitMask != 0)                     // end recursion when all the bits have been processes
            {
                if (shiftRightAmount >= Log2ofPowerOfTwoRadix) shiftRightAmount -= Log2ofPowerOfTwoRadix;
                else shiftRightAmount = 0;

                for (int i = 0; i < PowerOfTwoRadix; i++)
                {
                    if (endOfBin[i] - startOfBin[i] > 0)    // TODO: This should not be needed and is only there to ease porting to C#
                        RadixSortMsdUShortInner(a, startOfBin[i], endOfBin[i] - startOfBin[i], bitMask, shiftRightAmount, baseCaseInPlaceSort);
                }
            }
        }
        private static ushort[] SortRadixMsdInPlaceFunc2(this ushort[] arrayToBeSorted)
        {
            int shiftRightAmount = sizeof(ushort) * 8 - Log2ofPowerOfTwoRadix;
            ushort bitMask = (ushort)(((ushort)(PowerOfTwoRadix - 1)) << shiftRightAmount);  // bitMask controls/selects how many and which bits we process at a time
            // InsertionSort could be passed in as another base case since it's in-place
            RadixSortMsdUShortInner(arrayToBeSorted, 0, arrayToBeSorted.Length, bitMask, shiftRightAmount, Array.Sort);
            return arrayToBeSorted;
        }
    }
}
