# PolyPartitionCS
A port of https://github.com/ivanfratric/polypartition to c#, with many changes.

## NOTICEABLE CODE CHANGES

- Originally designed for use with Godot4 C#, it supports up to .NET 9.

- Structure has been modified; classes are now more isolated.

- System.Numerics is used instead of TPPLPoint to leverage SIMD instructions.
  - This change removes an internally unused Id field (some may expect it).
  - The implementation is therefore limited to single-precision floats.

- I don't think TPPLPoly is required. Removed, in favor of raw array. 

- The use of (out) parameters has been adopted instead of requiring a List to be passed,
  improving readability.
