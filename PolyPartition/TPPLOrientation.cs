using System.Runtime.CompilerServices;

namespace PolyPartition;

/// <summary>
/// Left-Hand rule (Y-down): CW (+) and CCW (−) [e.g. Godot2D, Screen space]<br/>
/// Right-Hand rule (Y-up): CW (-) and CCW (+) [e.g. Godot3D, Cartesian coordinate, World space]<br/>
/// <br/>
/// Important for the <see cref="TPPLOrientation(bool)"/>
/// </summary>
public enum TPPLWindingOrder : int
{
    /// <summary>clockwise</summary>
    CW = -1,
    /// <summary>degenerate/collinear/unset</summary>
    Collinear = 0,
    /// <summary>counter-clockwise</summary>
    CCW = 1
}

/// <summary>
/// TPPLOrientation is a singleton class for <see cref="PositiveCW"/> 
/// and <see cref="NegativeCW"/> no other instances should exist.<br/>
/// Because they represent all possible permutations of the class.
/// </summary>
public sealed class TPPLOrientation
{
    public bool IsClockwisePositive { get; }

    /// <summary>Outer boundary of a polygon</summary>
    public TPPLWindingOrder Outer { get; }

    /// <summary>Inner boundaries (holes) inside the polygon </summary>
    public TPPLWindingOrder Inner { get; }

    private TPPLOrientation(bool isClockwisePositive)
    {
        IsClockwisePositive = isClockwisePositive;
        Outer = isClockwisePositive ? TPPLWindingOrder.CW : TPPLWindingOrder.CCW;
        Inner = isClockwisePositive ? TPPLWindingOrder.CCW : TPPLWindingOrder.CW;
    }

    /// <summary> Left-Hand rule (Y-down) </summary>
    public static TPPLOrientation PositiveCW { get; } = new(true);
    /// <summary> Right-Hand rule (Y-up) <br/> The expected winding order to use with <see cref="PolyPartition"/> Lib </summary>
    public static TPPLOrientation NegativeCW { get; } = new(false);

    [MethodImpl(INLINE)] public static real_t ShoelaceFormula(TPPLPoint p1, TPPLPoint p2) => p1.X * p2.Y - p2.X * p1.Y;
    [MethodImpl(INLINE)] public static real_t ShoelaceFormula(real_t x1, real_t x2, real_t y1, real_t y2) => x1 * y2 - x2 * y1;

    public static real_t SignedAreaEnumerable(IEnumerable<TPPLPoint> polygon)
    {
        var enumerator = polygon.GetEnumerator();

        if (!enumerator.MoveNext())
            return 0;

        var i0 = enumerator.Current;

        if (!enumerator.MoveNext())
            return 0;

        var i1 = enumerator.Current;
        var area = ShoelaceFormula(i0, i1);

        if (!enumerator.MoveNext())
            return 0;

        TPPLPoint point = enumerator.Current;

        area += ShoelaceFormula(i1, point);
        while (enumerator.MoveNext())
        {
            var current = enumerator.Current;
            area += ShoelaceFormula(point, current);
            point = current;
        }

        area += ShoelaceFormula(point, i0);

        return area;
    }
    public static real_t SignedArea(ReadOnlySpan<TPPLPoint> polygon)
    {
        var len = polygon.Length;
        if (len < 3) return 0;
        real_t area = 0;
        for (int i0 = len - 1, i1 = 0; i1 < len; i0 = i1++)
            area += ShoelaceFormula(polygon[i0], polygon[i1]);
        return area;
    }

    public static real_t CalculateAreaEnumerable(IEnumerable<TPPLPoint> polygon) => TPPLPointMath.Abs(SignedAreaEnumerable(polygon)) / 2;
    public static real_t CalculateArea(ReadOnlySpan<TPPLPoint> polygon) => TPPLPointMath.Abs(SignedArea(polygon)) / 2;

    public static bool IsCollinear(IEnumerable<TPPLPoint> polygon) => SignedAreaEnumerable(polygon) == 0;
    public static bool IsCollinear(ReadOnlySpan<TPPLPoint> polygon) => SignedArea(polygon) == 0;

    public bool IsCounterClockWiseEnumerable(IEnumerable<TPPLPoint> polygon)
    {
        var area = SignedAreaEnumerable(polygon);
        if (IsClockwisePositive)
            area = -area;
        return area > 0;
    }
    public bool IsCounterClockWise(ReadOnlySpan<TPPLPoint> polygon)
    {
        var area = SignedArea(polygon);
        if (IsClockwisePositive)
            area = -area;
        return area > 0;
    }

    public bool IsClockWiseEnumerable(IEnumerable<TPPLPoint> polygon)
    {
        var area = SignedAreaEnumerable(polygon);
        if (IsClockwisePositive)
            area = -area;
        return area < 0;
    }
    public bool IsClockWise(ReadOnlySpan<TPPLPoint> polygon)
    {
        var area = SignedArea(polygon);
        if (IsClockwisePositive)
            area = -area;
        return area < 0;
    }

    public TPPLWindingOrder GetOrientationEnumerable(IEnumerable<TPPLPoint> polygon)
    {
        var area = SignedAreaEnumerable(polygon);
        if (IsClockwisePositive)
            area = -area;
        if (area > 0) return TPPLWindingOrder.CCW;
        if (area < 0) return TPPLWindingOrder.CW;
        return TPPLWindingOrder.Collinear;
    }
    public TPPLWindingOrder GetOrientation(ReadOnlySpan<TPPLPoint> polygon)
    {
        var area = SignedArea(polygon);
        if (IsClockwisePositive)
            area = -area;
        if (area > 0) return TPPLWindingOrder.CCW;
        if (area < 0) return TPPLWindingOrder.CW;
        return TPPLWindingOrder.Collinear;
    }

    public void SetOrientation(List<TPPLPoint> list, TPPLWindingOrder orientation)
    {
        var polyOrientation = GetOrientationEnumerable(list);
        if (polyOrientation != TPPLWindingOrder.Collinear && polyOrientation != orientation)
            list.Reverse();
    }
    public void SetOrientation(TPPLPoint[] array, TPPLWindingOrder orientation)
    {
        var polyOrientation = GetOrientation(array.AsSpan());
        if (polyOrientation != TPPLWindingOrder.Collinear && polyOrientation != orientation)
            Array.Reverse(array);
    }
    public void SetOrientationOuter(List<TPPLPoint> list) => SetOrientation(list, Outer);
    public void SetOrientationOuter(TPPLPoint[] arr) => SetOrientation(arr, Outer);
    public void SetOrientationInner(List<TPPLPoint> list) => SetOrientation(list, Inner);
    public void SetOrientationInner(TPPLPoint[] arr) => SetOrientation(arr, Inner);
}