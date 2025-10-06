// Where TPPLPoint is "defined", you can either define your own 
// TPPLPoint here or use some one else if it implements all basics
// operations and replace the functions at TPPLPointMath
// and it should work.

global using TPPLPoint = System.Numerics.Vector2;

using System.Runtime.CompilerServices;

namespace PolyPartition;

internal static class TPPLPointMath
{
    [MethodImpl(INLINE)] public static float Distance(TPPLPoint p1, TPPLPoint p2) => TPPLPoint.Distance(p1, p2);
    [MethodImpl(INLINE)] public static float Dot(TPPLPoint p1, TPPLPoint p2) => TPPLPoint.Dot(p1, p2);
    [MethodImpl(INLINE)] public static float Abs(float p) => MathF.Abs(p);
    public static TPPLPoint Normalize(TPPLPoint p)
    {
        float n = MathF.Sqrt(p.X * p.X + p.Y * p.Y);
        if (n != 0)
            return p / n;
        return new(0, 0);
    }
}
