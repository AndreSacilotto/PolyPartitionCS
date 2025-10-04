using System.Runtime.CompilerServices;

namespace PolyPartition;

public static class TPPLPointUtil
{
    #region Array
    //public static TPPLPoint[] Copy(TPPLPoint[] polygon)
    //{
    //    var newPoints = new TPPLPoint[polygon.Length];
    //    polygon.CopyTo(newPoints, 0);
    //    return newPoints;
    //}
    //public static List<TPPLPoint[]> Copy(List<TPPLPoint[]> polygons)
    //{
    //    var len = polygons.Count;
    //    List<TPPLPoint[]> list = new(len);
    //    for (int i = 0; i < len; i++)
    //        list[i] = Copy(polygons[i]);
    //    return list;
    //}

    public static bool IsValidPolygon(ReadOnlySpan<TPPLPoint> points) => points.Length >= 3;

    private static float Area(ReadOnlySpan<TPPLPoint> points)
    {
        float area = 0;
        for (int i0 = 0, i1 = points.Length - 1; i0 < points.Length; i1 = i0++)
        {
            var p0 = points[i0];
            var p1 = points[i1];
            area += p0.X * p1.Y - p0.Y * p1.X;
        }
        return area;
    }

    public static float CalculateArea(ReadOnlySpan<TPPLPoint> points) => MathF.Abs(Area(points)) * 0.5f;

    public static TPPLOrientation GetOrientation(ReadOnlySpan<TPPLPoint> points)
    {
        var area = Area(points);
        if (area > 0f) return TPPLOrientation.CCW;
        if (area < 0f) return TPPLOrientation.CW;
        return TPPLOrientation.None;
    }

    public static void SetOrientation(TPPLPoint[] array, TPPLOrientation orientation)
    {
        TPPLOrientation polyOrientation = GetOrientation(array);
        if (polyOrientation != TPPLOrientation.None && polyOrientation != orientation)
            Array.Reverse(array);
    }

    public static TPPLPoint[] Triangle(TPPLPoint p1, TPPLPoint p2, TPPLPoint p3) => [p1, p2, p3];
    #endregion

    public static TPPLPoint Normalize(TPPLPoint p)
    {
        float n = MathF.Sqrt(p.X * p.X + p.Y * p.Y);
        if (n != 0)
            return p / n;
        return new(0, 0);
    }

    [MethodImpl(INLINE)] public static float Cross2D(TPPLPoint p1, TPPLPoint p2, TPPLPoint p3) => (p3.Y - p1.Y) * (p2.X - p1.X) - (p3.X - p1.X) * (p2.Y - p1.Y);
    [MethodImpl(INLINE)] public static float Distance(TPPLPoint p1, TPPLPoint p2) => TPPLPoint.Distance(p1, p2);

    public static bool Intersects(TPPLPoint a1, TPPLPoint a2, TPPLPoint b1, TPPLPoint b2)
    {
        // If any endpoints are shared, treat as non-intersecting
        if (a1 == b1 || a1 == b2 || a2 == b1 || a2 == b2)
            return false;

        // Perpendicular (normal) vectors to the segment directions
        TPPLPoint normalA = new(a2.Y - a1.Y, a1.X - a2.X);
        TPPLPoint normalB = new(b2.Y - b1.Y, b1.X - b2.X);

        // Project segment B’s endpoints onto line A
        float projB1 = TPPLPoint.Dot(b1 - a1, normalA);
        float projB2 = TPPLPoint.Dot(b2 - a1, normalA);

        // Project segment A’s endpoints onto line B
        float projA1 = TPPLPoint.Dot(a1 - b1, normalB);
        float projA2 = TPPLPoint.Dot(a2 - b1, normalB);

        // If both points of one segment lie on the same side of the other segment's line → no intersection
        return (projA1 * projA2 <= 0) && (projB1 * projB2 <= 0);
    }

    public static bool IsConvex(TPPLPoint p1, TPPLPoint p2, TPPLPoint p3) => Cross2D(p1, p2, p3) > 0;
    public static bool IsReflex(TPPLPoint p1, TPPLPoint p2, TPPLPoint p3) => Cross2D(p1, p2, p3) < 0;

    public static bool IsInside(TPPLPoint p1, TPPLPoint p2, TPPLPoint p3, TPPLPoint p) =>
        !IsConvex(p1, p, p2) && !IsConvex(p2, p, p3) && !IsConvex(p3, p, p1);

    public static bool InCone(TPPLPoint p1, TPPLPoint p2, TPPLPoint p3, TPPLPoint p) =>
        IsConvex(p1, p2, p3) ? IsConvex(p1, p2, p) && IsConvex(p2, p3, p) : IsConvex(p1, p2, p) || IsConvex(p2, p3, p);
}
