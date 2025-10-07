# PolyPartitionCS
A port of https://github.com/ivanfratric/polypartition to c#, with many changes.

## NOTICEABLE CODE CHANGES

- Originally designed for use with Godot4 C#, using .NET 9 with C#12.

- Structure has been modified; classes are now more isolated.

- TPPLPoint is not the same structure, in the redesign it become a custom class. Explanation of how to alter it are inside the file.
  - This change removes an internally unused Id field.

- Addition of TPPLOrientation, a utility class about CW and CCW.

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
