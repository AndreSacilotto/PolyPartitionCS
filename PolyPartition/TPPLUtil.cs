using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PolyPartition;

public static class TPPLUtil
{
    #region Array
    public static bool IsValidPolygon(ReadOnlySpan<TPPLPoint> points) => points.Length >= 3;

    public static real_t SingedArea(ReadOnlySpan<TPPLPoint> points)
    {
        real_t area = 0;
        for (int i0 = 0, i1 = points.Length - 1; i0 < points.Length; i1 = i0++)
        {
            var p0 = points[i0];
            var p1 = points[i1];
            area += p0.X * p1.Y - p0.Y * p1.X;
        }
        return area;
    }

    public static real_t CalculateArea(ReadOnlySpan<TPPLPoint> points) => TPPLPointMath.Abs(SingedArea(points)) / 2;

    /// <param name="positiveIsCCW">
    /// Left-hand rule (Y down): CW (+) and CCW (−) [e.g. Unity, Unreal, Screen space]<br/>
    /// Right-hand rule (Y up): CCW (+) and CW (−) [e.g. Godot, OpenGL, World space]
    /// </param>
    public static TPPLOrientation GetOrientation(ReadOnlySpan<TPPLPoint> points, bool positiveIsCCW = true)
    {
        var area = SingedArea(points);
        if (!positiveIsCCW)
            area = -area;
        if (area > 0) return TPPLOrientation.CCW;
        if (area < 0) return TPPLOrientation.CW;
        return TPPLOrientation.None;
    }

    public static void SetOrientation(List<TPPLPoint> list, TPPLOrientation orientation, bool positiveIsCCW = true)
    {
        TPPLOrientation polyOrientation = GetOrientation(CollectionsMarshal.AsSpan(list), positiveIsCCW);
        if (polyOrientation != TPPLOrientation.None && polyOrientation != orientation)
            list.Reverse();
    }
    public static void SetOrientation(TPPLPoint[] array, TPPLOrientation orientation, bool positiveIsCCW = true)
    {
        TPPLOrientation polyOrientation = GetOrientation(array, positiveIsCCW);
        if (polyOrientation != TPPLOrientation.None && polyOrientation != orientation)
            Array.Reverse(array);
    }

    public static TPPLPoint[] Triangle(TPPLPoint p1, TPPLPoint p2, TPPLPoint p3) => [p1, p2, p3];
    #endregion

    public static bool Intersects(TPPLPoint a1, TPPLPoint a2, TPPLPoint b1, TPPLPoint b2)
    {
        // If any endpoints are shared, treat as non-intersecting
        if (a1 == b1 || a1 == b2 || a2 == b1 || a2 == b2)
            return false;

        // Perpendicular (normal) vectors to the segment directions
        TPPLPoint normalA = new(a2.Y - a1.Y, a1.X - a2.X);
        TPPLPoint normalB = new(b2.Y - b1.Y, b1.X - b2.X);

        // Project segment B’s endpoints onto line A
        real_t projB1 = TPPLPointMath.Dot(b1 - a1, normalA);
        real_t projB2 = TPPLPointMath.Dot(b2 - a1, normalA);

        // Project segment A’s endpoints onto line B
        real_t projA1 = TPPLPointMath.Dot(a1 - b1, normalB);
        real_t projA2 = TPPLPointMath.Dot(a2 - b1, normalB);

        // If both points of one segment lie on the same side of the other segment's line → no intersection
        return (projA1 * projA2 <= 0) && (projB1 * projB2 <= 0);
    }

    [MethodImpl(INLINE)] private static real_t Cross2D(TPPLPoint p1, TPPLPoint p2, TPPLPoint p3) => (p3.Y - p1.Y) * (p2.X - p1.X) - (p3.X - p1.X) * (p2.Y - p1.Y);
    public static bool IsConvex(TPPLPoint p1, TPPLPoint p2, TPPLPoint p3) => Cross2D(p1, p2, p3) > 0;
    public static bool IsReflex(TPPLPoint p1, TPPLPoint p2, TPPLPoint p3) => Cross2D(p1, p2, p3) < 0;

    public static bool IsInside(TPPLPoint p1, TPPLPoint p2, TPPLPoint p3, TPPLPoint p) =>
        !IsConvex(p1, p, p2) && !IsConvex(p2, p, p3) && !IsConvex(p3, p, p1);

    public static bool InCone(TPPLPoint p1, TPPLPoint p2, TPPLPoint p3, TPPLPoint p) =>
        IsConvex(p1, p2, p3) ? IsConvex(p1, p2, p) && IsConvex(p2, p3, p) : IsConvex(p1, p2, p) || IsConvex(p2, p3, p);
}
