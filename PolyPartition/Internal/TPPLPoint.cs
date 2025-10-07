// Where TPPLPoint is "defined", you can either define your own 
// TPPLPoint here or use some one else if it implements all basics
// operations and replace the functions at TPPLPointMath
// and it should work.

#if GODOT
global using TPPLPoint = Godot.Vector2;
#if REAL_T_IS_DOUBLE
global using real_t = double;
#else
global using real_t = float;
#endif
#else
global using TPPLPoint = System.Numerics.Vector2;
global using real_t = float;
#endif

using System.Runtime.CompilerServices;

namespace PolyPartition;

internal static class TPPLPointMath
{
    [MethodImpl(INLINE)] public static real_t Abs(real_t p) => Math.Abs(p);

#if GODOT
    [MethodImpl(INLINE)] public static real_t Distance(TPPLPoint p1, TPPLPoint p2) => p1.DistanceTo(p2);
    [MethodImpl(INLINE)] public static real_t Dot(TPPLPoint p1, TPPLPoint p2) => p1.Dot(p2);
    public static TPPLPoint Normalize(TPPLPoint p) => p.Normalized();
#else
    [MethodImpl(INLINE)] public static real_t Distance(TPPLPoint p1, TPPLPoint p2) => TPPLPoint.Distance(p1, p2);
    [MethodImpl(INLINE)] public static real_t Dot(TPPLPoint p1, TPPLPoint p2) => TPPLPoint.Dot(p1, p2);
    public static TPPLPoint Normalize(TPPLPoint p) => TPPLPoint.Normalize(p);
#endif
}
