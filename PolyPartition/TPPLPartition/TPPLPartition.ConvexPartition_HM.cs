namespace PolyPartition;

partial class TPPLPartition
{

    public static bool ConvexPartition_HM(List<TPPLPoint[]> inPolys, out List<TPPLPoint[]> parts, TPPLOrientation holeOrientation = TPPLOrientation.CCW)
    {
        parts = [];
        if (!RemoveHoles(inPolys, out var outPolys, holeOrientation))
            return false;
        foreach (var poly in outPolys)
            if (!ConvexPartition_HM(poly, parts))
                return false;
        return true;
    }

    public static bool ConvexPartition_HM(TPPLPoint[] poly, List<TPPLPoint[]> parts)
    {
        var len = poly.Length;
        if (len < 3) return false;

        bool reflex = false;
        for (int i = 0; i < len; i++)
        {
            int i1 = (i + len - 1) % len;
            int i2 = (i + 1) % len;
            if (TPPLPointUtil.IsReflex(poly[i1], poly[i], poly[i2]))
            {
                reflex = true;
                break;
            }
        }

        if (!reflex)
        {
            parts.Add(poly);
            return true;
        }

        var triangles = new List<TPPLPoint[]>();
        if (!Triangulate_EC(poly, triangles))
        {
            return false;
        }

        for (int i = 0; i < triangles.Count; i++)
        {
            TPPLPoint[] poly1 = triangles[i];
            for (int i11 = 0; i11 < poly1.Length; i11++)
            {
                TPPLPoint d1 = poly1[i11];
                int i12 = (i11 + 1) % poly1.Length;
                TPPLPoint d2 = poly1[i12];

                bool isDiagonal = false;

                for (int j = i; j < triangles.Count; j++)
                {
                    if (i == j) continue;
                    TPPLPoint[] poly2 = triangles[j];

                    for (int i21 = 0; i21 < poly2.Length; i21++)
                    {
                        if (d2 != poly2[i21]) continue;
                        int i22 = (i21 + 1) % poly2.Length;
                        if (d1 != poly2[i22]) continue;

                        isDiagonal = true;

                        TPPLPoint p2 = poly1[i11];
                        int i13 = (i11 + poly1.Length - 1) % poly1.Length;
                        TPPLPoint p1 = poly1[i13];
                        int i23 = (i22 == poly2.Length - 1) ? 0 : i22 + 1;
                        TPPLPoint p3 = poly2[i23];

                        if (!TPPLPointUtil.IsConvex(p1, p2, p3))
                        {
                            isDiagonal = false;
                            continue;
                        }

                        p2 = poly1[i12];
                        i13 = (i12 == poly1.Length - 1) ? 0 : i12 + 1;
                        p3 = poly1[i13];
                        i23 = (i21 == 0) ? poly2.Length - 1 : i21 - 1;
                        p1 = poly2[i23];

                        if (!TPPLPointUtil.IsConvex(p1, p2, p3))
                        {
                            isDiagonal = false;
                            continue;
                        }

                        var newpoly = new TPPLPoint[poly1.Length + poly2.Length - 2];
                        var idx = 0;

                        for (int m = i12; m != i11; m = (m + 1) % poly1.Length)
                            newpoly[idx++] = poly1[m];

                        for (int m = i22; m != i21; m = (m + 1) % poly2.Length)
                            newpoly[idx++] = poly2[m];

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

}