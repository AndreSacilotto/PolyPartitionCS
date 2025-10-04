# PolyPartitionCS
A port of https://github.com/ivanfratric/polypartition to c#, with many changes.

## NOTICEABLE CODE CHANGES

- Originally designed for use with Godot4 C#, it supports up to .NET 9.

- Structure has been modified; classes are now more isolated.

- System.Numerics is used instead of TPPLPoint to leverage SIMD instructions.
  - This change removes an internally unused Id field (some may expect it).
  - The implementation is therefore limited to single-precision floats.

- TPPLPoly is believed to be unnecessary; it has been removed in favor of using a raw array.

- The use of (out) parameters has been adopted instead of requiring a List to be passed.

### PolyPartition

- `Triangulate_EC`
 - `O(n^2)/O(n)`
 - Holes: `RemoveHoles`

- `Triangulate_OPT`
 - `O(n^3)/O(n^2)`
 - Holes: `No`

- `Triangulate_MONO`
 - `O(n*log(n))/O(n)`
 - Holes: `Yes`

- `ConvexPartition_HM`
 - `O(n^3)/O(n^2)`
 - Holes: `RemoveHoles`

- `ConvexPartition_OPT`
 - `O(n^3)/O(n^2)`
 - Holes: `No`
