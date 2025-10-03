namespace PolyPartition;

public static partial class TPPLPartition
{
    public static bool RemoveHoles(List<TPPLPoly> inPolys, out List<TPPLPoly> polys)
    {
        bool hasHoles = false;
        foreach (var p in inPolys)
        {
            if (p.IsHole)
            {
                hasHoles = true;
                break;
            }
        }

        polys = [.. inPolys];
        if (!hasHoles)
            return true;

        while (true)
        {
            int holeIndex = -1;
            int holePointIndex = 0;

            for (int i = 0; i < polys.Count; i++)
            {
                if (!polys[i].IsHole) continue;

                if (holeIndex == -1)
                {
                    holeIndex = i;
                    holePointIndex = 0;
                }

                for (int j = 0; j < polys[i].Count; j++)
                {
                    if (polys[i][j].X > polys[holeIndex][holePointIndex].X)
                    {
                        holeIndex = i;
                        holePointIndex = j;
                    }
                }
            }

            if (holeIndex == -1) break;

            var holePoint = polys[holeIndex][holePointIndex];

            bool pointFound = false;
            int polyIndex = -1;
            int polyPointIndex = 0;
            TPPLPoint bestPolyPoint = new();

            for (int i = 0; i < polys.Count; i++)
            {
                if (polys[i].IsHole) continue;

                for (int j = 0; j < polys[i].Count; j++)
                {
                    if (polys[i][j].X <= holePoint.X) continue;

                    int prev = (j + polys[i].Count - 1) % polys[i].Count;
                    int next = (j + 1) % polys[i].Count;

                    if (!TPPLPointExt.InCone(polys[i][prev], polys[i][j], polys[i][next], holePoint))
                        continue;

                    TPPLPoint polyPoint = polys[i][j];
                    if (pointFound)
                    {
                        float v1Dist = TPPLPointExt.Distance(holePoint, polyPoint);
                        float v2Dist = TPPLPointExt.Distance(holePoint, bestPolyPoint);
                        if (v2Dist < v1Dist) continue;
                    }

                    bool pointVisible = true;
                    for (int k = 0; k < polys.Count; k++)
                    {
                        if (polys[k].IsHole) continue;

                        for (int l = 0; l < polys[k].Count; l++)
                        {
                            TPPLPoint lineP1 = polys[k][l];
                            TPPLPoint lineP2 = polys[k][(l + 1) % polys[k].Count];
                            if (TPPLPointExt.Intersects(holePoint, polyPoint, lineP1, lineP2))
                            {
                                pointVisible = false;
                                break;
                            }
                        }
                        if (!pointVisible) break;
                    }

                    if (pointVisible)
                    {
                        pointFound = true;
                        bestPolyPoint = polyPoint;
                        polyIndex = i;
                        polyPointIndex = j;
                    }
                }
            }

            if (!pointFound) return false;

            var polyIter = polys[polyIndex];
            var holeIter = polys[holeIndex];

            TPPLPoly newPoly = new(holeIter.Count + polyIter.Count + 2);
            for (int i = 0; i <= polyPointIndex; i++)
                newPoly.Add(polyIter[i]);

            for (int i = 0; i <= holeIter.Count; i++)
                newPoly.Add(holeIter[(i + holePointIndex) % holeIter.Count]);

            for (int i = polyPointIndex; i < polyIter.Count; i++)
                newPoly.Add(polyIter[i]);

            var first = Math.Max(holeIndex, polyIndex);
            var second = Math.Min(holeIndex, polyIndex);
            polys.RemoveAt(first);
            polys.RemoveAt(second);
            polys.Add(newPoly);
        }

        return true;
    }
}