namespace PolyPartition;

//Optimal convex partition using dynamic programming algorithm by Keil and Snoeyink
//Complexity: O(n^3)/O(n^3)
//Holes: No. Calling RemoveHoles makes the solution non-optimal.
//Solution: Optimal, a minimum number of convex polygons is produced

partial class TPPLPartition
{
    #region Helper functions for ConvexPartition_OPT
    private static void UpdateState(int startIndex, int endIndex, int newWeight, int diagonalStart, int diagonalEnd, DPState2[][] dpstates)
    {
        int currentWeight = dpstates[startIndex][endIndex].Weight;
        if (newWeight > currentWeight) return;

        var diagonalPairs = dpstates[startIndex][endIndex].Pairs;
        Diagonal newDiagonal = new() { Index1 = diagonalStart, Index2 = diagonalEnd };

        if (newWeight < currentWeight)
        {
            diagonalPairs.Clear();
            diagonalPairs.Add(newDiagonal);
            dpstates[startIndex][endIndex].Weight = newWeight;
        }
        else
        {
            if (diagonalPairs.Count > 0 && diagonalStart <= diagonalPairs[0].Index1) return;
            while (diagonalPairs.Count > 0 && diagonalPairs[0].Index2 >= diagonalEnd)
                diagonalPairs.RemoveAt(0);
            diagonalPairs.Insert(0, newDiagonal);
        }
    }

    private static void TypeA(int startVertex, int middleVertex, int endVertex, PartitionVertex[] vertices, DPState2[][] dpstates)
    {
        if (!dpstates[startVertex][middleVertex].Visible) return;

        int topVertex = middleVertex;
        int totalWeight = dpstates[startVertex][middleVertex].Weight;

        if (endVertex - middleVertex > 1)
        {
            if (!dpstates[middleVertex][endVertex].Visible) return;
            totalWeight += dpstates[middleVertex][endVertex].Weight + 1;
        }

        if (middleVertex - startVertex > 1)
        {
            var diagonalPairs = dpstates[startVertex][middleVertex].Pairs;
            int lastValidPairIndex = -1;

            for (int pairIndex = diagonalPairs.Count - 1; pairIndex >= 0; pairIndex--)
            {
                var currentPair = diagonalPairs[pairIndex];
                if (!TPPLUtil.IsReflex(vertices[currentPair.Index2].Point, vertices[middleVertex].Point, vertices[endVertex].Point))
                    lastValidPairIndex = pairIndex;
                else
                    break;
            }

            if (lastValidPairIndex == -1)
            {
                totalWeight++;
            }
            else
            {
                var lastValidPair = diagonalPairs[lastValidPairIndex];
                if (TPPLUtil.IsReflex(vertices[endVertex].Point, vertices[startVertex].Point, vertices[lastValidPair.Index1].Point))
                {
                    totalWeight++;
                }
                else
                {
                    topVertex = lastValidPair.Index1;
                }
            }
        }

        UpdateState(startVertex, endVertex, totalWeight, topVertex, middleVertex, dpstates);
    }

    private static void TypeB(int startVertex, int middleVertex, int endVertex, PartitionVertex[] vertices, DPState2[][] dpstates)
    {
        if (!dpstates[middleVertex][endVertex].Visible) return;

        int topVertex = middleVertex;
        int totalWeight = dpstates[middleVertex][endVertex].Weight;

        if (middleVertex - startVertex > 1)
        {
            if (!dpstates[startVertex][middleVertex].Visible) return;
            totalWeight += dpstates[startVertex][middleVertex].Weight + 1;
        }

        if (endVertex - middleVertex > 1)
        {
            var diagonalPairs = dpstates[middleVertex][endVertex].Pairs;

            if (diagonalPairs.Count > 0 && !TPPLUtil.IsReflex(vertices[startVertex].Point, vertices[middleVertex].Point, vertices[diagonalPairs[0].Index1].Point))
            {
                int lastValidPairIndex = 0;
                for (int pairIndex = 0; pairIndex < diagonalPairs.Count; pairIndex++)
                {
                    if (!TPPLUtil.IsReflex(vertices[pairIndex].Point, vertices[middleVertex].Point, vertices[diagonalPairs[pairIndex].Index1].Point))
                        lastValidPairIndex = pairIndex;
                    else
                        break;
                }

                var lastValidPair = diagonalPairs[lastValidPairIndex];
                if (TPPLUtil.IsReflex(vertices[lastValidPair.Index2].Point, vertices[endVertex].Point, vertices[startVertex].Point))
                    totalWeight++;
                else
                    topVertex = lastValidPair.Index2;
            }
            else
            {
                totalWeight++;
            }
        }

        UpdateState(startVertex, endVertex, totalWeight, middleVertex, topVertex, dpstates);
    }

    #endregion

    /// <summary>Non-Optimal</summary>
    public static bool ConvexPartition_OPT(TPPLPolygonList inPolys, out List<TPPLPoint[]> parts) => ConvexPartition_OPT(inPolys, parts = []);
    /// <summary>Non-Optimal</summary>
    public static bool ConvexPartition_OPT(TPPLPolygonList inPolys, List<TPPLPoint[]> parts)
    {
        if (!RemoveHoles(inPolys, out var outPolys))
            return false;
        foreach (var poly in outPolys)
            if (!ConvexPartition_OPT(poly, parts))
                return false;
        return true;
    }

    public static bool ConvexPartition_OPT(TPPLPoint[] poly, List<TPPLPoint[]> parts)
    {
        int len = poly.Length;
        if (len < 3) return false;

        DPState2[][] dpstates = new DPState2[len][];

        for (int i = 0; i < len; i++)
        {
            dpstates[i] = new DPState2[len];
            for (int j = 0; j < len; j++)
                dpstates[i][j] = new DPState2();
        }

        var vertices = PartitionVertex.PartitionsFromPoly(poly);

        for (int i = 1; i < len; i++)
            vertices[i].UpdateVertexReflexity();

        // Initialize states
        for (int startIndex = 0; startIndex < len - 1; startIndex++)
        {
            for (int endIndex = startIndex + 1; endIndex < len; endIndex++)
            {
                dpstates[startIndex][endIndex].Visible = true;
                dpstates[startIndex][endIndex].Weight = endIndex == startIndex + 1 ? 0 : int.MaxValue;

                if (endIndex != startIndex + 1)
                {
                    if (!vertices[startIndex].InCone(poly[endIndex]) || !vertices[endIndex].InCone(poly[startIndex]))
                    {
                        dpstates[startIndex][endIndex].Visible = false;
                        continue;
                    }

                    for (int edgeStart = 0; edgeStart < len; edgeStart++)
                    {
                        TPPLPoint edgePoint1 = poly[edgeStart];
                        TPPLPoint edgePoint2 = poly[edgeStart == len - 1 ? 0 : edgeStart + 1];
                        if (TPPLUtil.Intersects(poly[startIndex], poly[endIndex], edgePoint1, edgePoint2))
                        {
                            dpstates[startIndex][endIndex].Visible = false;
                            break;
                        }
                    }
                }
            }
        }

        for (int startIndex = 0; startIndex < len - 2; startIndex++)
        {
            int endIndex = startIndex + 2;
            if (dpstates[startIndex][endIndex].Visible)
            {
                dpstates[startIndex][endIndex].Weight = 0;
                dpstates[startIndex][endIndex].Pairs.Add(new Diagonal { Index1 = startIndex + 1, Index2 = startIndex + 1 });
            }
        }

        dpstates[0][len - 1].Visible = true;
        vertices[0].IsConvex = false;

        for (int gap = 3; gap < len; gap++)
        {
            for (int startVertex = 0; startVertex < len - gap; startVertex++)
            {
                if (vertices[startVertex].IsConvex) continue;
                int endVertex = startVertex + gap;
                if (dpstates[startVertex][endVertex].Visible)
                {
                    if (!vertices[endVertex].IsConvex)
                    {
                        for (int middleVertex = startVertex + 1; middleVertex < endVertex; middleVertex++)
                            TypeA(startVertex, middleVertex, endVertex, vertices, dpstates);
                    }
                    else
                    {
                        for (int middleVertex = startVertex + 1; middleVertex < endVertex - 1; middleVertex++)
                        {
                            if (vertices[middleVertex].IsConvex) continue;
                            TypeA(startVertex, middleVertex, endVertex, vertices, dpstates);
                        }
                        TypeA(startVertex, endVertex - 1, endVertex, vertices, dpstates);
                    }
                }
            }

            for (int endVertex = gap; endVertex < len; endVertex++)
            {
                if (vertices[endVertex].IsConvex) continue;
                int startVertex = endVertex - gap;
                if (vertices[startVertex].IsConvex && dpstates[startVertex][endVertex].Visible)
                {
                    TypeB(startVertex, startVertex + 1, endVertex, vertices, dpstates);
                    for (int middleVertex = startVertex + 2; middleVertex < endVertex; middleVertex++)
                    {
                        if (vertices[middleVertex].IsConvex) continue;
                        TypeB(startVertex, middleVertex, endVertex, vertices, dpstates);
                    }
                }
            }
        }

        // Recover solution
        Queue<Diagonal> diagonals = new(1);
        diagonals.Enqueue(new Diagonal { Index1 = 0, Index2 = len - 1 });

        while (diagonals.Count > 0)
        {
            Diagonal currentDiagonal = diagonals.Dequeue();

            if (currentDiagonal.Index2 - currentDiagonal.Index1 <= 1) continue;

            var diagonalPairs = dpstates[currentDiagonal.Index1][currentDiagonal.Index2].Pairs;
            if (diagonalPairs.Count == 0) return false;

            int selectedVertex;
            if (!vertices[currentDiagonal.Index1].IsConvex)
            {
                selectedVertex = diagonalPairs[^1].Index2;
            }
            else
            {
                selectedVertex = diagonalPairs[0].Index1;
            }

            List<int> triangleIndices = [currentDiagonal.Index1, selectedVertex, currentDiagonal.Index2];
            triangleIndices.Sort();

            var newPolygon = new TPPLPoint[triangleIndices.Count];
            for (int i = 0; i < triangleIndices.Count; i++)
                newPolygon[i] = vertices[triangleIndices[i]].Point;
            parts.Add(newPolygon);

            if (selectedVertex > currentDiagonal.Index1 + 1)
                diagonals.Enqueue(new Diagonal { Index1 = currentDiagonal.Index1, Index2 = selectedVertex });
            if (currentDiagonal.Index2 > selectedVertex + 1)
                diagonals.Enqueue(new Diagonal { Index1 = selectedVertex, Index2 = currentDiagonal.Index2 });
        }

        return true;
    }

}