
namespace PolyPartition;

//Triangulation by ear clipping
//Complexity: O(n^2)/O(n)
//Holes: Yes, by calling RemoveHoles
//Solution: Satisfactory in most cases

partial class TPPLPartition
{
    #region Helpers Triangulate_EC
    private static void UpdateVertex(PartitionVertex vtx, PartitionVertex[] vertices)
    {
        vtx.UpdateVertexReflexity();

        var vec1 = TPPLPointMath.Normalize(vtx.Previous.Point - vtx.Point);
        var vec3 = TPPLPointMath.Normalize(vtx.Next.Point - vtx.Point);
        vtx.Angle = TPPLPointMath.Dot(vec1, vec3);

        if (vtx.IsConvex)
        {
            vtx.IsEar = true;
            for (int i = 0; i < vertices.Length; i++)
            {
                if (vertices[i].Point == vtx.Point || vertices[i].Point == vtx.Previous.Point || vertices[i].Point == vtx.Next.Point)
                    continue;
                if (TPPLUtil.IsInside(vtx.Previous.Point, vtx.Point, vtx.Next.Point, vertices[i].Point))
                {
                    vtx.IsEar = false;
                    break;
                }
            }
        }
        else
        {
            vtx.IsEar = false;
        }
    }

    #endregion


    public static bool Triangulate_EC(TPPLPolygonList inPolys, out List<TPPLPoint[]> triangles) => Triangulate_EC(inPolys, triangles = []);
    public static bool Triangulate_EC(TPPLPolygonList inPolys, List<TPPLPoint[]> triangles)
    {
        if (!RemoveHoles(inPolys, out var outPolys))
            return false;
        foreach (var poly in outPolys)
            if (!Triangulate_EC(poly, triangles))
                return false;
        return true;
    }

    public static bool Triangulate_EC(ReadOnlySpan<TPPLPoint> poly, List<TPPLPoint[]> triangles)
    {
        int len = poly.Length;
        if (len < 3) return false;
        if (len == 3)
        {
            triangles.Add([.. poly]);
            return true;
        }

        var vertices = PartitionVertex.PartitionsFromPoly(poly);

        for (int i = 0; i < len; i++)
            UpdateVertex(vertices[i], vertices);

        for (int i = 0; i < len - 3; i++)
        {
            PartitionVertex? ear = null;

            for (int j = 0; j < len; j++)
            {
                if (!vertices[j].IsActive || !vertices[j].IsEar) continue;

                if (ear == null || vertices[j].Angle > ear.Angle)
                {
                    ear = vertices[j];
                }
            }

            if (ear == null) { return false; }

            TPPLPoint[] triangle = TPPLUtil.Triangle(ear.Previous.Point, ear.Point, ear.Next.Point);
            triangles.Add(triangle);

            ear.IsActive = false;
            ear.Previous.Next = ear.Next;
            ear.Next.Previous = ear.Previous;

            if (i == len - 4) break;

            UpdateVertex(ear.Previous, vertices);
            UpdateVertex(ear.Next, vertices);
        }

        for (int i = 0; i < len; i++)
        {
            if (vertices[i].IsActive)
            {
                TPPLPoint[] triangle = TPPLUtil.Triangle(vertices[i].Previous.Point, vertices[i].Point, vertices[i].Next.Point);
                triangles.Add(triangle);
                break;
            }
        }

        return true;
    }

}
