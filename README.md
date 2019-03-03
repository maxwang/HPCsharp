If you like HPCsharp, then help us keep more good stuff like this coming. Let us know what other algorithms could use acceleration

[![paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=LDD8L7UPAC7QL)

# High Performance Computing in C# (HPCsharp)

High performance C# generic algorithms. Community driven to raise C# performance.
Parallel algorithms for Sum, Sort, Merge, Copy, Histogram and others. Parallel Stable Merge Sort and parallel Merge of arrays and lists.
Linear, stable or in-place Radix Sort algorithms for arrays and lists of user defined classes sorted by key. Crazy fast Counting Sort,
and Array.Fill for full and partial arrays. Better .Sum() for arrays that doesn't overflow, uses SSE and multi-core.

Familiar interfaces, which are similar to standard C# Sort. Free and open source HPCsharp package on https://www.nuget.org

Updated VisualStudio 2017 examples solution, demonstrating usage through working examples. Build and run it to see performance gains on your machine.

To get the maximum performance make sure to target x64 processor architecture for the Release build in VisualStudio, increasing performance by as much as 50%.

**_Version 3.3.3_** Just Released! Give it a shot.
- Fixed a bug with .Sum() SSE and multi-core implementation for int and uint arrays
- Fixed inner recursive function of MSD Radix Sort calling the wrong function when recursing

**_Version 3.3.1_** Just Released! Give it a shot.
- Implemented a better .Sum() than Linq provides, which does not overflow and is parallel thru SSE and multi-core for ludicrous speed! (See details below)
- Added .Sum() implementations of Kahan and Neumaier floating-point summation algorithms for higher accuracy
- Slight performance improvements to LSD Radix Sort

**_Version 3.2.5_**

- Added in-place MSD Radix Sort implementations: byte, sbyte, short, ushort, ulong, long, and double arrays.
- These use Counting Sort for byte, sbyte, short and ushort, for ludicrous speed!
- These provide in-place interface and functional interface, which is also in-place but returns the input array that has been sorted.
- Improved performance of MSD Radix Sort of slong arrays (thank you John once again).
- Added LSD Radix Sort for long arrays (serial and parallel), which are not in-place, but have interfaces to support in-place usage

Full release history is in ReleaseNotes.txt file

## Better .Sum() ##
Linq .Sum() for arrays and lists adds up all of the elements to produce a sum. This .Sum() returns the same data type as the elements of the array itself.
For instance, when the array is of integers, then the result is also an integer. This can cause an overflow condition and exception, even when summing
just two elements - e.g. when one of the elements is Int32.MaxValue and the other is a positive value greater than zero.

HPCsharp version of .Sum() returns an slong for all signed integer types (int, short, sbyte) and will not overflow or throw an overflow exception.
Also, unsigned types are supported by HPCsharp .Sum(), such as uint, ushort, and byte. For .Sum() of float arrays, double is returned producing
a more accurate summation result. Also, for float and double arrays, to produce even more accurate summation, Kahan and Neumaier algorithms have been
implemented - serial only to start with. For manny data types .Sum() supports data parallel and multi-core accelerated versions -
.SumSse() and .SumSsePar(). These provide substantial speedup and run many times faster than the standard C# versions, even faster than .AsParallel.Sum(),
plus no worries about overflow. HPCsharp versions run in GigaElements/second speeds for these parallel .Sum() implementations.

## Sorting ##

**_Version 3.1.2_** algorithm performance is shown in the following tables:

*Algorithm*|*Collection*|*Distribution*|*vs .Sort*|*vs Linq*|*vs Linq.AsParallel*|*MegaBytes/sec*|*Data Type*
--- | --- | --- | --- | --- | --- | --- | ---
Counting Sort|Array|Random|27-56X|156-343X|39-70X|846|byte
Counting Sort|Array|Presorted|26-56X|168-344X|38-66X|864|byte
Counting Sort|Array|Constant|30-56X|165-321X|34-70X|847|byte

Counting Sort is linear time O(N) and sorts either an array of byte or ushort. In-place and not-in-place version have been implementated.
This algorithm runs at GPU speeds!

*Algorithm*|*Collection*|*Distribution*|*vs .Sort*|*vs Linq*|*vs Linq.AsParallel*|*MegaInts/sec*|*Data Type*
--- | --- | --- | --- | --- | --- | --- | ---
Radix Sort|Array, List|Random|5X-8X|14X-35X|4X-9X|82|UInt32
Radix Sort|Array, List|Presorted|0.3X-0.6X|3X-5X|1X-3X|48|UInt32
Radix Sort|Array, List|Constant|1.3X-1.8X|5X-8X|2X-3X|50|UInt32

Radix Sort is linear time O(N) and stable. Radix Sort runs on a single core, whereas Linq.AsParallel ran on all the cores.
Only slower when sorting presorted Array or List, but faster in all other cases, even faster than parallel Linq.OrderBy.AsParallel.

Parallel Merge Sort uses multiple CPU cores to accelerate performance. On a quad-core laptop, performance is:

*Algorithm*|*Collection*|*Distribution*|*vs .Sort*|*vs Linq*|*vs Linq.AsParallel*|*MegaInts/sec*
--- | --- | --- | --- | --- | --- | ---
Parallel Merge Sort|Array|Random|3X|12X|5X|25
Parallel Merge Sort|Array|Presorted|2X|22X|13X|110
Parallel Merge Sort|Array|Constant|2X|15X|9X|74

Parallel Merge Sort is not stable, just like Array.Sort. Faster than Array.Sort and List.Sort across all distributions.
Substantially faster than Linq.OrderBy and Linq.OrderBy.AsParallel

**_28-core (56-threads) AWS c5.18xlarge_**

*Algorithm*|*Collection*|*Distribution*|*vs .Sort*|*vs Linq*|*vs Linq.AsParallel*|*Description*
--- | --- | --- | --- | --- | --- | ---
Parallel Merge Sort|Array|Random|5X-14X|19X-90X|7X-47X|
Parallel Merge Sort|Array|Presorted|1X-6X|5X-60X|16X-122X|
Parallel Merge Sort|Array|Constant|TBD|TBD|9X-44X|

*Algorithm*|*Collection*|*Distribution*|*vs .Sort*|*vs Linq*|*vs Linq.AsParallel*|*MegaInts/sec*
--- | --- | --- | --- | --- | --- | ---
Merge Sort (stable)|Array|Random|0.6X|2.5X|1X|5
Merge Sort (stable)|Array|Presorted|0.3X|3X|2X|17
Merge Sort (stable)|Array|Constant|0.5X|3X|2X|15

Merge Sort is O(NlgN), never O(N<sup>2</sup>), generic, stable, and runs on a single CPU core. Faster than Linq.OrderBy and Linq.OrderBy.AsParallel.

Other algorithms provided:
- Insertion Sort which is O(N<sup>2</sup>), and useful for fast in-place sorting of very small collections.
- Binary Search algorithm
- Parallel Merge algorithm, which merges two presorted collections using multiple cores. Used by Parallel Merge Sort.
- A few parallel Linq-style methods for Min, Max, Average, etc.

Radix Sort has been extended to sort user defined classes based on a UInt32 or UInt64 key within the class. Radix Sort is currently using only a single core.

*Algorithm*|*Collection*|*Distribution*|*vs .Sort*|*vs Linq*|*vs Linq.AsParallel*|*Description*
--- | --- | --- | --- | --- | --- | ---
Radix Sort|Array|Random|1X-4X|3X-5X|1X-2X|User defined class
Radix Sort|List|Random|2X-4X|3X-5X|1X-2X|User defined class
Radix Sort|Array|Presorted|1.2X-1.7X|0.9X-2.5X|0.9X-1.4X|User defined class
Radix Sort|List|Presorted|1.0X-1.2X|1.7X-2.1X|0.7X-1.1X|User defined class
Radix Sort|Array|Constant|3X-4X|4X-5X|2X-3X|User defined class
Radix Sort|List|Constant|2X-4X|3X-4X|1.5X-2X|User defined class

Only slightly slower than Array.Sort and List.Sort for presorted distribution, but faster for all other distributions. Uses a single core and is stable.
Faster than Linq.OrderBy and Linq.OrderBy.AsParallel

*Algorithm*|*Collection*|*vs Linq*|*Parallel vs Linq*
--- | --- | --- | ---
SequenceEqual|Array, List|4X faster|up to 11X faster
Min|Array, List|1.5-3X faster
Max|Array, List|1.5X faster

Parallel Copying:

*Method*|*Collection*|*Parallel*
--- | --- | ---
Parallel CopyTo|List to Array|1.7X-2.5X faster

Discussion on when it's appropriate to use parallel copy is coming soon...

# Examples of Usage
See HPCsharpExample folder in this GitHub repository for usage examples - a complete working VisualStudio 2017 solution is provided.

# Related Blogs
For details on the motivation see blog:
https://duvanenko.tech.blog/2018/03/03/high-performance-c/

For more performance discussion see blog:
https://duvanenko.tech.blog/2018/05/23/faster-sorting-in-c/

# Website for Feature Votes
Visit us at https://foostate.com/ and let us know what other high performance algorithms are important to you, and you'd like to see in this NuGet package.

# Encouragement
If you like it, then help us keep more good stuff like this coming. Let us know what other algorithms you could use.

[![paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=LDD8L7UPAC7QL)
