namespace PolyPartition;

partial class TPPLPartition
{
    public static bool ConvexPartition_HM(TPPLPoly poly, out List<TPPLPoly> parts)
    {
        var len = poly.Count;
        parts = [];
        if (!poly.IsValid) return false;

        int numReflex = 0;
        for (int i = 0; i < len; i++)
        {
            int i1 = (i + len - 1) % len;
            int i2 = (i + 1) % len;
            if (TPPLPointExt.IsReflex(poly[i1], poly[i], poly[i2]))
            {
                numReflex = 1;
                break;
            }
        }

        if (numReflex == 0)
        {
            parts.Add(poly);
            return true;
        }

        if (!Triangulate_EC(poly, out var triangles)) return false;

        for (int i = 0; i < triangles.Count; i++)
        {
            TPPLPoly poly1 = triangles[i];
            for (int i11 = 0; i11 < poly1.Count; i11++)
            {
                TPPLPoint d1 = poly1[i11];
                int i12 = (i11 + 1) % poly1.Count;
                TPPLPoint d2 = poly1[i12];

                bool isDiagonal = false;

                for (int j = i; j < triangles.Count; j++)
                {
                    if (i == j) continue;
                    TPPLPoly poly2 = triangles[j];

                    for (int i21 = 0; i21 < poly2.Count; i21++)
                    {
                        if (d2 != poly2[i21]) continue;
                        int i22 = (i21 + 1) % poly2.Count;
                        if (d1 != poly2[i22]) continue;

                        isDiagonal = true;

                        TPPLPoint p2 = poly1[i11];
                        int i13 = (i11 + poly1.Count - 1) % poly1.Count;
                        TPPLPoint p1 = poly1[i13];
                        int i23 = (i22 == poly2.Count - 1) ? 0 : i22 + 1;
                        TPPLPoint p3 = poly2[i23];

                        if (!TPPLPointExt.IsConvex(p1, p2, p3))
                        {
                            isDiagonal = false;
                            continue;
                        }

                        p2 = poly1[i12];
                        i13 = (i12 == poly1.Count - 1) ? 0 : i12 + 1;
                        p3 = poly1[i13];
                        i23 = (i21 == 0) ? poly2.Count - 1 : i21 - 1;
                        p1 = poly2[i23];

                        if (!TPPLPointExt.IsConvex(p1, p2, p3))
                        {
                            isDiagonal = false;
                            continue;
                        }

                        TPPLPoly newpoly = new(poly1.Count + poly2.Count - 2);
                        for (int m = i12; m != i11; m = (m + 1) % poly1.Count)
                            newpoly.Add(poly1[m]);
                        for (int m = i22; m != i21; m = (m + 1) % poly2.Count)
                            newpoly.Add(poly2[m]);

                        triangles.RemoveAt(j);
                        triangles[i] = newpoly;
                        poly1 = triangles[i];
                        i11 = -1;
                        break;
                    }
                    if (isDiagonal) break;
                }
                if (i11 == -1) break;
            }
        }

        parts.AddRange(triangles);
        return true;
    }

    public static bool ConvexPartition_HM(List<TPPLPoly> inPolys, out List<TPPLPoly> parts)
    {
        if (!RemoveHoles(inPolys, out var outPolys))
        {
            parts = [];
            return false;
        }
        foreach (var poly in outPolys)
            if (!ConvexPartition_HM(poly, out parts))
                return false;
        parts = [];
        return true;
    }

}