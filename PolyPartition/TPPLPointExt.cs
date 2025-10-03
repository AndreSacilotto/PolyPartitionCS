using System.Runtime.CompilerServices;

namespace PolyPartition;

internal static class TPPLPointExt
{
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

    public static TPPLOrientation IsCounterClockwise(ReadOnlySpan<TPPLPoint> points)
    {
        var area = Area(points);
        if (area > 0f) return TPPLOrientation.CCW;
        if (area < 0f) return TPPLOrientation.CW;
        return TPPLOrientation.Collinear;
    }

    public static TPPLPoint[] Triangle(TPPLPoint p1, TPPLPoint p2, TPPLPoint p3) => [p1, p2, p3];

    public static TPPLPoint Normalize(TPPLPoint p)
    {
        float n = MathF.Sqrt(p.X * p.X + p.Y * p.Y);
        if (n != 0)
            return p / n;
        return new(0, 0);
    }

    [MethodImpl(INLINE)] public static float Cross2D(TPPLPoint p1, TPPLPoint p2, TPPLPoint p3) => (p3.Y - p1.Y) * (p2.X - p1.X) - (p3.X - p1.X) * (p2.Y - p1.Y);
    [MethodImpl(INLINE)] public static float Distance(TPPLPoint p1, TPPLPoint p2) => TPPLPoint.Distance(p1, p2);

    public static bool Intersects(TPPLPoint p11, TPPLPoint p12, TPPLPoint p21, TPPLPoint p22)
    {
        if (p11 == p21 || p11 == p22 || p12 == p21 || p12 == p22)
            return false;

        TPPLPoint v1ort = new(p12.Y - p11.Y, p11.X - p12.X);
        TPPLPoint v2ort = new(p22.Y - p21.Y, p21.X - p22.X);

        TPPLPoint v = p21 - p11;
        float dot21 = v.X * v1ort.X + v.Y * v1ort.Y;
        v = p22 - p11;
        float dot22 = v.X * v1ort.X + v.Y * v1ort.Y;

        v = p11 - p21;
        float dot11 = v.X * v2ort.X + v.Y * v2ort.Y;
        v = p12 - p21;
        float dot12 = v.X * v2ort.X + v.Y * v2ort.Y;

        if (dot11 * dot12 > 0) return false;
        if (dot21 * dot22 > 0) return false;

        return true;
    }

    public static bool IsConvex(TPPLPoint p1, TPPLPoint p2, TPPLPoint p3) => Cross2D(p1, p2, p3) > 0;
    public static bool IsReflex(TPPLPoint p1, TPPLPoint p2, TPPLPoint p3) => Cross2D(p1, p2, p3) < 0;

    public static bool IsInside(TPPLPoint p1, TPPLPoint p2, TPPLPoint p3, TPPLPoint p)
    {
        if (IsConvex(p1, p, p2)) return false;
        if (IsConvex(p2, p, p3)) return false;
        if (IsConvex(p3, p, p1)) return false;
        return true;
    }
    public static bool InCone(TPPLPoint p1, TPPLPoint p2, TPPLPoint p3, TPPLPoint p)
    {
        bool convex = IsConvex(p1, p2, p3);
        if (convex)
        {
            if (!IsConvex(p1, p2, p)) return false;
            if (!IsConvex(p2, p3, p)) return false;
            return true;
        }
        else
        {
            if (IsConvex(p1, p2, p)) return true;
            if (IsConvex(p2, p3, p)) return true;
            return false;
        }
    }


}
