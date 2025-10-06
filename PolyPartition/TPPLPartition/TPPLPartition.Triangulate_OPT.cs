namespace PolyPartition;

//Optimal triangulation in terms of edge length using dynamic programming algorithm
//Complexity: O(n^3)/O(n^2)
//Holes: No. Calling RemoveHoles makes the solution non-optimal.
//Solution: Optimal in terms of minimal edge length

partial class TPPLPartition
{
    /// <summary>Non-Optimal</summary>
    public static bool Triangulate_OPT(TPPLPolygonList inPolys, out List<TPPLPoint[]> triangles) => Triangulate_OPT(inPolys, triangles = []);
    /// <summary>Non-Optimal</summary>
    public static bool Triangulate_OPT(TPPLPolygonList inPolys, List<TPPLPoint[]> triangles)
    {
        if (!RemoveHoles(inPolys, out var outPolys))
            return false;
        foreach (var poly in outPolys)
            if (!Triangulate_OPT(poly, triangles))
                return false;
        return true;
    }

    public static bool Triangulate_OPT(ReadOnlySpan<TPPLPoint> poly, List<TPPLPoint[]> triangles)
    {
        int len = poly.Length;

        if (len < 3) return false;

        DPState[][] dpstates = new DPState[len][];
        for (int i = 1; i < len; i++)
        {
            dpstates[i] = new DPState[i];
            for (int j = 0; j < i; j++)
                dpstates[i][j] = new DPState();
        }

        // Initialize states and visibility
        for (int i = 0; i < len - 1; i++)
        {
            TPPLPoint p1 = poly[i];
            for (int j = i + 1; j < len; j++)
            {
                dpstates[j][i].Visible = true;
                dpstates[j][i].Weight = 0;
                dpstates[j][i].BestVertex = -1;

                if (j != i + 1)
                {
                    TPPLPoint p2 = poly[j];

                    // Visibility check
                    TPPLPoint p3 = poly[i == 0 ? len - 1 : i - 1];
                    TPPLPoint p4 = poly[i == len - 1 ? 0 : i + 1];
                    if (!TPPLUtil.InCone(p3, p1, p4, p2))
                    {
                        dpstates[j][i].Visible = false;
                        continue;
                    }

                    p3 = poly[j == 0 ? len - 1 : j - 1];
                    p4 = poly[j == len - 1 ? 0 : j + 1];
                    if (!TPPLUtil.InCone(p3, p2, p4, p1))
                    {
                        dpstates[j][i].Visible = false;
                        continue;
                    }

                    for (int k = 0; k < len; k++)
                    {
                        p3 = poly[k];
                        p4 = poly[k == len - 1 ? 0 : k + 1];
                        if (TPPLUtil.Intersects(p1, p2, p3, p4))
                        {
                            dpstates[j][i].Visible = false;
                            break;
                        }
                    }
                }
            }
        }
        dpstates[len - 1][0].Visible = true;
        dpstates[len - 1][0].Weight = 0;
        dpstates[len - 1][0].BestVertex = -1;

        for (int gap = 2; gap < len; gap++)
        {
            for (int i = 0; i < len - gap; i++)
            {
                int j = i + gap;
                if (!dpstates[j][i].Visible) continue;

                int bestVertex = -1;
                float minWeight = 0;

                for (int k = i + 1; k < j; k++)
                {
                    if (!dpstates[k][i].Visible || !dpstates[j][k].Visible)
                        continue;

                    float d1 = k <= i + 1 ? 0 : TPPLPointMath.Distance(poly[i], poly[k]);
                    float d2 = j <= k + 1 ? 0 : TPPLPointMath.Distance(poly[k], poly[j]);
                    float weight = dpstates[k][i].Weight + dpstates[j][k].Weight + d1 + d2;

                    if (bestVertex == -1 || weight < minWeight)
                    {
                        bestVertex = k;
                        minWeight = weight;
                    }
                }

                if (bestVertex == -1)
                    return false;

                dpstates[j][i].BestVertex = bestVertex;
                dpstates[j][i].Weight = minWeight;
            }
        }

        Queue<Diagonal> diagonals = new();
        diagonals.Enqueue(new Diagonal { Index1 = 0, Index2 = len - 1 });

        while (diagonals.Count > 0)
        {
            Diagonal diagonal = diagonals.Dequeue();

            int bestVertex = dpstates[diagonal.Index2][diagonal.Index1].BestVertex;
            if (bestVertex == -1)
                return false;

            TPPLPoint[] triangle = TPPLUtil.Triangle(
                poly[diagonal.Index1],
                poly[bestVertex],
                poly[diagonal.Index2]
            );
            triangles.Add(triangle);

            if (bestVertex > diagonal.Index1 + 1)
                diagonals.Enqueue(new Diagonal { Index1 = diagonal.Index1, Index2 = bestVertex });
            if (diagonal.Index2 > bestVertex + 1)
                diagonals.Enqueue(new Diagonal { Index1 = bestVertex, Index2 = diagonal.Index2 });
        }

        return true;
    }

}
