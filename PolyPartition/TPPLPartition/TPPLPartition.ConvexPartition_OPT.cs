namespace PolyPartition;

partial class TPPLPartition
{
    #region Helper functions for ConvexPartition_OPT
    private static void UpdateState(int a, int b, int w, int i, int j, DPState2[][] dpstates)
    {
        int w2 = dpstates[a][b].Weight;
        if (w > w2) return;

        var pairs = dpstates[a][b].Pairs;
        Diagonal newDiagonal = new() { Index1 = i, Index2 = j };

        if (w < w2)
        {
            pairs.Clear();
            pairs.Add(newDiagonal);
            dpstates[a][b].Weight = w;
        }
        else
        {
            if (pairs.Count > 0 && i <= pairs[0].Index1) return;
            while (pairs.Count > 0 && pairs[0].Index2 >= j)
                pairs.RemoveAt(0);
            pairs.Insert(0, newDiagonal);
        }
    }

    private static void TypeA(int i, int j, int k, PartitionVertex[] vertices, DPState2[][] dpstates)
    {
        if (!dpstates[i][j].Visible) return;

        int top = j;
        int w = dpstates[i][j].Weight;

        if (k - j > 1)
        {
            if (!dpstates[j][k].Visible) return;
            w += dpstates[j][k].Weight + 1;
        }

        if (j - i > 1)
        {
            var pairs = dpstates[i][j].Pairs;
            int lastIterIndex = -1;

            for (int idx = pairs.Count - 1; idx >= 0; idx--)
            {
                var iter = pairs[idx];
                if (!TPPLPointExt.IsReflex(vertices[iter.Index2].Point, vertices[j].Point, vertices[k].Point))
                    lastIterIndex = idx;
                else
                    break;
            }

            if (lastIterIndex == -1)
            {
                w++;
            }
            else
            {
                var lastIter = pairs[lastIterIndex];
                if (TPPLPointExt.IsReflex(vertices[k].Point, vertices[i].Point, vertices[lastIter.Index1].Point))
                {
                    w++;
                }
                else
                {
                    top = lastIter.Index1;
                }
            }
        }

        UpdateState(i, k, w, top, j, dpstates);
    }

    private static void TypeB(int i, int j, int k, PartitionVertex[] vertices, DPState2[][] dpstates)
    {
        if (!dpstates[j][k].Visible) return;

        int top = j;
        int w = dpstates[j][k].Weight;

        if (j - i > 1)
        {
            if (!dpstates[i][j].Visible) return;
            w += dpstates[i][j].Weight + 1;
        }

        if (k - j > 1)
        {
            var pairs = dpstates[j][k].Pairs;

            if (pairs.Count > 0 && !TPPLPointExt.IsReflex(vertices[i].Point, vertices[j].Point, vertices[pairs[0].Index1].Point))
            {
                int lastIterIndex = 0;
                for (int idx = 0; idx < pairs.Count; idx++)
                {
                    if (!TPPLPointExt.IsReflex(vertices[idx].Point, vertices[j].Point, vertices[pairs[idx].Index1].Point))
                        lastIterIndex = idx;
                    else
                        break;
                }

                var lastIter = pairs[lastIterIndex];
                if (TPPLPointExt.IsReflex(vertices[lastIter.Index2].Point, vertices[k].Point, vertices[i].Point))
                    w++;
                else
                    top = lastIter.Index2;
            }
            else
            {
                w++;
            }
        }

        UpdateState(i, k, w, j, top, dpstates);
    }

    #endregion

    public static bool ConvexPartition_OPT(TPPLPoly poly, out List<TPPLPoly> parts)
    {
        parts = [];

        int len = poly.Count;
        if (!poly.IsValid) return false;

        PartitionVertex[] vertices = new PartitionVertex[len];
        DPState2[][] dpstates = new DPState2[len][];

        for (int i = 0; i < len; i++)
        {
            dpstates[i] = new DPState2[len];
            for (int j = 0; j < len; j++)
                dpstates[i][j] = new DPState2();
        }

        // Initialize vertices
        for (int i = 0; i < len; i++)
        {
            vertices[i] = new(vertices[i == 0 ? len - 1 : i - 1], vertices[i == len - 1 ? 0 : i + 1])
            {
                Point = poly[i],
                IsActive = true
            };
        }

        for (int i = 1; i < len; i++)
            vertices[i].UpdateVertexReflexity();

        // Initialize states
        for (int i = 0; i < len - 1; i++)
        {
            for (int j = i + 1; j < len; j++)
            {
                dpstates[i][j].Visible = true;
                dpstates[i][j].Weight = j == i + 1 ? 0 : int.MaxValue;

                if (j != i + 1)
                {
                    if (!vertices[i].InCone(poly[j]) || !vertices[j].InCone(poly[i]))
                    {
                        dpstates[i][j].Visible = false;
                        continue;
                    }

                    for (int k = 0; k < len; k++)
                    {
                        TPPLPoint p3 = poly[k];
                        TPPLPoint p4 = poly[k == len - 1 ? 0 : k + 1];
                        if (TPPLPointExt.Intersects(poly[i], poly[j], p3, p4))
                        {
                            dpstates[i][j].Visible = false;
                            break;
                        }
                    }
                }
            }
        }

        for (int i = 0; i < len - 2; i++)
        {
            int j = i + 2;
            if (dpstates[i][j].Visible)
            {
                dpstates[i][j].Weight = 0;
                dpstates[i][j].Pairs.Add(new Diagonal { Index1 = i + 1, Index2 = i + 1 });
            }
        }

        dpstates[0][len - 1].Visible = true;
        vertices[0].IsConvex = false;

        for (int gap = 3; gap < len; gap++)
        {
            for (int i = 0; i < len - gap; i++)
            {
                if (vertices[i].IsConvex) continue;
                int k = i + gap;
                if (dpstates[i][k].Visible)
                {
                    if (!vertices[k].IsConvex)
                    {
                        for (int j = i + 1; j < k; j++)
                            TypeA(i, j, k, vertices, dpstates);
                    }
                    else
                    {
                        for (int j = i + 1; j < k - 1; j++)
                        {
                            if (vertices[j].IsConvex) continue;
                            TypeA(i, j, k, vertices, dpstates);
                        }
                        TypeA(i, k - 1, k, vertices, dpstates);
                    }
                }
            }

            for (int k = gap; k < len; k++)
            {
                if (vertices[k].IsConvex) continue;
                int i = k - gap;
                if (vertices[i].IsConvex && dpstates[i][k].Visible)
                {
                    TypeB(i, i + 1, k, vertices, dpstates);
                    for (int j = i + 2; j < k; j++)
                    {
                        if (vertices[j].IsConvex) continue;
                        TypeB(i, j, k, vertices, dpstates);
                    }
                }
            }
        }

        // Recover solution (simplified version)
        List<Diagonal> diagonals = [new Diagonal { Index1 = 0, Index2 = len - 1 }];

        while (diagonals.Count > 0)
        {
            Diagonal diagonal = diagonals[0];
            diagonals.RemoveAt(0);

            if (diagonal.Index2 - diagonal.Index1 <= 1) continue;

            var pairs = dpstates[diagonal.Index1][diagonal.Index2].Pairs;
            if (pairs.Count == 0) return false;

            int j;
            if (!vertices[diagonal.Index1].IsConvex)
            {
                j = pairs[pairs.Count - 1].Index2;
            }
            else
            {
                j = pairs[0].Index1;
            }

            List<int> indices = [diagonal.Index1, j, diagonal.Index2];
            indices.Sort();
            TPPLPoly newPoly = new(indices.Count);
            for (int i = 0; i < indices.Count; i++)
                newPoly[i] = vertices[indices[i]].Point;
            parts.Add(newPoly);

            if (j > diagonal.Index1 + 1)
                diagonals.Add(new Diagonal { Index1 = diagonal.Index1, Index2 = j });
            if (diagonal.Index2 > j + 1)
                diagonals.Add(new Diagonal { Index1 = j, Index2 = diagonal.Index2 });
        }

        return true;
    }

}
