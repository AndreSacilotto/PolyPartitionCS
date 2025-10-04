
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
        vtx.IsConvex = TPPLPointUtil.IsConvex(vtx.Previous.Point, vtx.Point, vtx.Next.Point);

        var vec1 = TPPLPointUtil.Normalize(vtx.Previous.Point - vtx.Point);
        var vec3 = TPPLPointUtil.Normalize(vtx.Next.Point - vtx.Point);
        vtx.Angle = vec1.X * vec3.X + vec1.Y * vec3.Y;

        if (vtx.IsConvex)
        {
            vtx.IsEar = true;
            for (int i = 0; i < vertices.Length; i++)
            {
                if (vertices[i].Point == vtx.Point || vertices[i].Point == vtx.Previous.Point || vertices[i].Point == vtx.Next.Point)
                    continue;
                if (TPPLPointUtil.IsInside(vtx.Previous.Point, vtx.Point, vtx.Next.Point, vertices[i].Point))
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

    private static PartitionVertex[] PartitionFromPoly(TPPLPoint[] poly)
    {
        var len = poly.Length;
        PartitionVertex[] vertices = new PartitionVertex[len];
        for (int i = 0; i < len; i++)
        {
            vertices[i] = new(null!, null!) // Temporary null values
            {
                IsActive = true,
                Point = poly[i],
            };
        }

        // back loop
        vertices[0].Previous = vertices[^1];
        vertices[0].Next = vertices[1];

        for (int i = 1; i < len - 1; i++)
        {
            vertices[i].Previous = vertices[i - 1];
            vertices[i].Next = vertices[i + 1];
        }

        // front loop
        vertices[^1].Previous = vertices[^2];
        vertices[^1].Next = vertices[0];

        return vertices;
    }
    #endregion

    public static bool Triangulate_EC(List<TPPLPoint[]> inPolys, out List<TPPLPoint[]> triangles, TPPLOrientation holeOrientation = TPPLOrientation.CCW)
    {
        triangles = [];
        if (!RemoveHoles(inPolys, out var outPolys, holeOrientation))
            return false;
        foreach (var poly in outPolys)
            if (!Triangulate_EC(poly, triangles))
                return false;
        return true;
    }

    public static bool Triangulate_EC(TPPLPoint[] poly, List<TPPLPoint[]> triangles)
    {
        int len = poly.Length;
        if (len < 3) return false;
        if (len == 3)
        {
            triangles.Add(poly);
            return true;
        }

        PartitionVertex[] vertices = PartitionFromPoly(poly);

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

            TPPLPoint[] triangle = TPPLPointUtil.Triangle(ear.Previous.Point, ear.Point, ear.Next.Point);
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
                TPPLPoint[] triangle = TPPLPointUtil.Triangle(vertices[i].Previous.Point, vertices[i].Point, vertices[i].Next.Point);
                triangles.Add(triangle);
                break;
            }
        }

        return true;
    }

}
