﻿// TODO: Add histogramming of arrays of other data types, with byte and ushort counts
// TODO: Add histogramming of 2-D and jagged arrays of variety of data types, with byte and ushort counts
// TODO: Develop more general histograms (serial and parallel) that allow components to not just be bytes, but also any number of bits within a word. For example, 10-bit or 14-bit components.
//       and possibly even different samplings between color components.
//       This may pay off big, and us being able to find the optimal number of bits for Radix Sort to minimize the number of passes or recursion levels.
//       Counting array should fit into L1-cache and possibly as large as fitting into L2-cache, since these are separate for each core in Intel CPUs, whereas L3-cache is
//       shared between all cores.
// TODO: Pull out the Histogram/Counting algorithm from LSD Radix Sort where multiple components are being counted in one pass, generalize it and parallelize it.
// TODO: Generalize multi-digit/multi-component Histogram to support digits of any number of bits - at least common ones, like 8, 10, 14 and 16 bits.
using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;

namespace HPCsharp
{
    static public partial class Algorithm
    {
        public static int[] Histogram(this byte[] inArray)
        {
            const int numberOfBins = 256;
            int[] counts = new int[numberOfBins];

            for (uint currIndex = 0; currIndex < inArray.Length; currIndex++)
                counts[inArray[currIndex]]++;

            return counts;
        }

        public static int[] Histogram(this sbyte[] inArray)
        {
            const int numberOfBins = 256;
            int[] counts = new int[numberOfBins];

            for (uint currIndex = 0; currIndex < inArray.Length; currIndex++)
                counts[(int)inArray[currIndex] + 128]++;

            return counts;
        }

        public static int[] Histogram(this ushort[] inArray)
        {
            const int numberOfBins = 256 * 256;
            int[] counts = new int[numberOfBins];

            for (uint currIndex = 0; currIndex < inArray.Length; currIndex++)
                counts[inArray[currIndex]]++;

            return counts;
        }

        public static int[] Histogram(this short[] inArray)
        {
            const int numberOfBins = 256 * 256;
            int[] counts = new int[numberOfBins];

            for (uint currIndex = 0; currIndex < inArray.Length; currIndex++)
                counts[(int)inArray[currIndex] + 32768]++;

            return counts;
        }

        public static int[] Histogram(this uint[] inArray, int numberOfBits)
        {
            if (numberOfBits > 31)
                throw new ArgumentOutOfRangeException("numberOfBits must be <= 31");

            int numberOfBins = 1 << numberOfBits;
            int[] counts = new int[numberOfBins];
            uint mask = (uint)(numberOfBins - 1);

            for (uint currIndex = 0; currIndex < inArray.Length; currIndex++)
                counts[mask & inArray[currIndex]]++;

            return counts;
        }

        public static uint[][] HistogramByteComponents(uint[] inArray, Int32 l, Int32 r)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(uint);
            uint[][] countLeft = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                countLeft[i] = new uint[numberOfBins];
#if true
            var union  = new UInt32ByteUnion();
            for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
            {
                union.integer = inArray[current];
                countLeft[0][union.byte0]++;
                countLeft[1][union.byte1]++;
                countLeft[2][union.byte2]++;
                countLeft[3][union.byte3]++;
            }
#else
            for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
            {
                uint value = inArray[current];
                countLeft[0][ value &       0xff       ]++;
                countLeft[1][(value &     0xff00) >>  8]++;
                countLeft[2][(value &   0xff0000) >> 16]++;
                countLeft[3][(value & 0xff000000) >> 24]++;
            }
#endif
            return countLeft;
        }

        public static uint[][] HistogramByteComponents(ulong[] inArray, Int32 l, Int32 r)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(ulong);
            uint[][] countLeft = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                countLeft[i] = new uint[numberOfBins];

            var union  = new UInt64ByteUnion();
            for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
            {
                union.integer = inArray[current];
                countLeft[0][union.byte0]++;
                countLeft[1][union.byte1]++;
                countLeft[2][union.byte2]++;
                countLeft[3][union.byte3]++;
                countLeft[4][union.byte4]++;
                countLeft[5][union.byte5]++;
                countLeft[6][union.byte6]++;
                countLeft[7][union.byte7]++;
            }
            return countLeft;
        }

        public static uint[][] HistogramNBitsPerComponents(uint[] inArray, Int32 l, Int32 r, int bitsPerComponent)
        {
            int numberOfBins = 1 << bitsPerComponent;
            int numberOfDigits = (sizeof(uint) * 8 + bitsPerComponent - 1) / bitsPerComponent;  // round up
            uint[][] countLeft = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                countLeft[i] = new uint[numberOfBins];
            if (bitsPerComponent == 8)
            {
                var union = new UInt32ByteUnion();
                for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                {
                    union.integer = inArray[current];
                    countLeft[0][union.byte0]++;
                    countLeft[1][union.byte1]++;
                    countLeft[2][union.byte2]++;
                    countLeft[3][union.byte3]++;
                }
            }
            else if (bitsPerComponent == 9)
            {
                for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                {
                    uint value = inArray[current];
                    countLeft[0][value  &      0x1ff       ]++;
                    countLeft[1][(value &    0x3fe00) >>  9]++;
                    countLeft[2][(value &  0x7fc0000) >> 18]++;
                    countLeft[3][(value & 0xf8000000) >> 27]++;
                }
            }
            else if (bitsPerComponent == 10)    // useful for 64-bit
            {
                for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                {
                    uint value = inArray[current];
                    countLeft[0][ value &      0x3ff       ]++;
                    countLeft[1][(value &    0xffc00) >> 10]++;
                    countLeft[2][(value & 0x3ff00000) >> 20]++;
                    countLeft[3][(value & 0xc0000000) >> 30]++;
                }
            }
            else if (bitsPerComponent == 11)    // useful for 32-bit and 64-bit
            {
                for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                {
                    uint value = inArray[current];
                    countLeft[0][ value &      0x7ff       ]++;
                    countLeft[1][(value &   0x3ff800) >> 11]++;
                    countLeft[2][(value & 0xffc00000) >> 22]++;
                }
            }
            else if (bitsPerComponent == 12)
            {
                for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                {
                    uint value = inArray[current];
                    countLeft[0][ value &      0xfff       ]++;
                    countLeft[1][(value &   0xfff000) >> 12]++;
                    countLeft[2][(value & 0xff000000) >> 24]++;
                }
            }
            else if (bitsPerComponent == 13)    // useful for 64-bit
            {
                for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                {
                    uint value = inArray[current];
                    countLeft[0][ value &     0x1fff       ]++;
                    countLeft[1][(value &  0x3ffe000) >> 13]++;
                    countLeft[2][(value & 0xfc000000) >> 26]++;
                }
            }
            else
            {
                uint componentMask = (uint)numberOfBins - 1;
                for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                {
                    uint value = inArray[current];
                    for (int i = 0; i < numberOfDigits; i++)
                    {
                        countLeft[i][value & componentMask]++;
                        componentMask <<= bitsPerComponent;
                    }
                }
            }
            return countLeft;
        }
    }
}
