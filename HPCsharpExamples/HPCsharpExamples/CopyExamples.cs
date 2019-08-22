﻿using HPCsharp.ParallelAlgorithms;
using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

namespace HPCsharpExamples
{
    class CopyExamples
    {
        public static void ListToArrayCopy()
        {
            // HPCsharp parallel List.ToArray() variations
            var listSource = new List<int> { 5, 7, 16, 3 };
            int[] arrayDestination = new int[4];

            // C# built-in convert from List to Array
            arrayDestination = listSource.ToArray();

            // HPCsharp parallel (multi-core) copy from List to Array
            arrayDestination = listSource.ToArrayPar();
            arrayDestination = listSource.ToArrayPar(0, listSource.Count);  // additional interface

            // C# built-in copy from List to an existing Array
            listSource.CopyTo(arrayDestination);
            listSource.CopyTo(arrayDestination, 0);
            listSource.CopyTo(0, arrayDestination, 0, listSource.Count);

            // HPCsharp parallel (multi-core) copy from List to an existing Array
            listSource.CopyToPar(arrayDestination);
            listSource.CopyToPar(arrayDestination, 0);
            listSource.CopyToPar(0, arrayDestination, 0, listSource.Count);
        }

        public static void ArrayToArrayCopy()
        {
            int[] arraySource = new int[] { 5, 7, 16, 3 };
            int[] arrayDestination = new int[4];

            // C# built-in Array.Copy() variations
            Array.Copy(arraySource, arrayDestination, arraySource.Length);
            Array.Copy(arraySource, 0, arrayDestination, 0, arraySource.Length);

            // HPCsharp parallel (multi-core) Array.Copy() variations. Similar interface
            ArrayHpc.CopyPar(arraySource, arrayDestination, arraySource.Length);
            ArrayHpc.CopyPar(arraySource, 0, arrayDestination, 0, arraySource.Length);

            // HPCsharp parallel (multi-core) Array.Copy() variations. More modern and convenient interface
            arraySource.CopyPar(arrayDestination, arraySource.Length);
            arraySource.CopyPar(0, arrayDestination, 0, arraySource.Length);

            // HPCsharp parallel (multi-core) Array.Copy() variations. Additional convenient interfaces
            arraySource.CopyPar(arrayDestination);
            arrayDestination = arraySource.CopyPar();


            // C# built-in Array.CopyTo() variations
            arraySource.CopyTo(arrayDestination, 0);

            // HPCsharp parallel (multi-core) Array.CopyTo()
            arraySource.CopyToPar(arrayDestination, 0);
        }
    }
}
