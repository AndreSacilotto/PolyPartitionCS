
namespace PolyPartition;

partial class TPPLPartition
{
    #region Helpers Triangulate_EC
    private static void UpdateVertex(PartitionVertex v, PartitionVertex[] vertices)
    {
        v.IsConvex = TPPLPointExt.IsConvex(v.Previous.Point, v.Point, v.Next.Point);

        TPPLPoint vec1 = TPPLPointExt.Normalize(v.Previous.Point - v.Point);
        TPPLPoint vec3 = TPPLPointExt.Normalize(v.Next.Point - v.Point);
        v.Angle = vec1.X * vec3.X + vec1.Y * vec3.Y;

        if (v.IsConvex)
        {
            v.IsEar = true;
            for (int i = 0; i < vertices.Length; i++)
            {
                if (vertices[i].Point == v.Point || vertices[i].Point == v.Previous.Point || vertices[i].Point == v.Next.Point)
                    continue;
                if (TPPLPointExt.IsInside(v.Previous.Point, v.Point, v.Next.Point, vertices[i].Point))
                {
                    v.IsEar = false;
                    break;
                }
            }
        }
        else
        {
            v.IsEar = false;
        }
    }

    private static PartitionVertex[] PartitionFromPoly(TPPLPoly poly)
    {
        var len = poly.Count;
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

    public static bool Triangulate_EC(List<TPPLPoly> inPolys, out List<TPPLPoly> triangles)
    {
        if (!RemoveHoles(inPolys, out var outPolys)){
            triangles = [];
            return false; 
        }
        foreach (var poly in outPolys)
            if (!Triangulate_EC(poly, out triangles))
                return false;
        triangles = [];
        return true;
    }

    public static bool Triangulate_EC(TPPLPoly poly, out List<TPPLPoly> triangles)
    {
        triangles = [];
        int len = poly.Count;
        if (!poly.IsValid) return false;
        if (len == 3)
        {
            triangles.Add(poly);
            return true;
        }

        PartitionVertex[] vertices = PartitionFromPoly(poly);
        
        for (int i = 0; i < len; i++)
        {
            vertices[i] = new(vertices[(i + len - 1) % len], vertices[(i + 1) % len])
            {
                IsActive = true,
                Point = poly[i],
            };
            //vtx.Next = i == (len - 1) ? vertices[0] : vertices[i + 1];
            //vtx.Previous = i == 0 ? vertices[len - 1] : vertices[i - 1];
        }

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

            if (ear == null) return false;

            TPPLPoly triangle = new(ear.Previous.Point, ear.Point, ear.Next.Point);
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
                TPPLPoly triangle = new(vertices[i].Previous.Point, vertices[i].Point, vertices[i].Next.Point);
                triangles.Add(triangle);
                break;
            }
        }

        return true;
    }

}
