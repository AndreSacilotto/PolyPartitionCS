using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PolyPartition;

/// <summary>
/// Left-hand rule (Y down): CW (+) and CCW (−) [e.g. Unity, Unreal, Screen space]<br/>
/// Right-hand rule (Y up): CCW (+) and CW (−) [e.g. Godot, OpenGL, World space]<br/>
/// <br/>
/// Important for the TPPLOrientation::isClockwisePositive 
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

public class TPPLOrientation(bool isClockwisePositive)
{
    public readonly bool IsClockwisePositive = isClockwisePositive;

    public static TPPLOrientation NegativeCW { get; } = new(false);
    public static TPPLOrientation PositiveCW { get; } = new(true);

    [MethodImpl(INLINE)] public static real_t ShoelaceFormula(TPPLPoint p1, TPPLPoint p2) => p1.X * p2.Y - p1.Y * p2.X;
    [MethodImpl(INLINE)] public static real_t ShoelaceFormula(real_t x1, real_t x2, real_t y1, real_t y2) => x1 * y2 - y1 * x2;

    public static real_t SignedArea(IEnumerable<TPPLPoint> polygon)
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
        if (polygon.Length < 3) return 0;
        real_t area = 0;
        for (int i0 = 0, i1 = polygon.Length - 1; i0 < polygon.Length; i1 = i0++)
            area += ShoelaceFormula(polygon[i0], polygon[i1]);
        return area;
    }

    public static real_t CalculateArea(IEnumerable<TPPLPoint> polygon) => TPPLPointMath.Abs(SignedArea(polygon)) / 2;
    public static real_t CalculateArea(ReadOnlySpan<TPPLPoint> polygon) => TPPLPointMath.Abs(SignedArea(polygon)) / 2;

    public bool IsCounterClockWise(ReadOnlySpan<TPPLPoint> polygon)
    {
        var area = SignedArea(polygon);
        if (IsClockwisePositive)
            area = -area;
        return area > 0;
    }
    public bool IsClockWise(ReadOnlySpan<TPPLPoint> polygon)
    {
        var area = SignedArea(polygon);
        if (IsClockwisePositive)
            area = -area;
        return area < 0;
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

    public TPPLWindingOrder GetOrientation(IEnumerable<TPPLPoint> polygon)
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
        var polyOrientation = GetOrientation(list);
        if (polyOrientation != TPPLWindingOrder.Collinear && polyOrientation != orientation)
            list.Reverse();
    }
    public void SetOrientation(TPPLPoint[] array, TPPLWindingOrder orientation)
    {
        var polyOrientation = GetOrientation(array.AsSpan());
        if (polyOrientation != TPPLWindingOrder.Collinear && polyOrientation != orientation)
            Array.Reverse(array);
    }
}