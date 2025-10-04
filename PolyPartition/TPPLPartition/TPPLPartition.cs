using System.Diagnostics.CodeAnalysis;

namespace PolyPartition;

public static partial class TPPLPartition
{
    //public static void PrintArray2D<T>(IEnumerable<T[]> values, string separator = "\t")
    //{
    //    foreach (var val in values)
    //    {
    //        string str = "";
    //        foreach (var item in val)
    //            str += item + separator;
    //        GD.PrintT(str.Substring(0, str.Length - separator.Length));
    //    }
    //}

    //public static void PrintArray<T>(IEnumerable<T> values)
    //{
    //    foreach (var val in values)
    //        GD.PrintT(val);
    //}

    private record PolyHole(TPPLPoint[] Polygon, bool IsHole)
    {
        public int Count => Polygon.Length;
        public TPPLPoint this[int index] => Polygon[index];
    }

    public static bool RemoveHoles(in List<TPPLPoint[]> inPolys, [NotNullWhen(true)] out List<TPPLPoint[]>? outPolys, TPPLOrientation holeOrientation)
    {
        var polys = new List<PolyHole>(inPolys.Count);

        bool hasHoles = false;
        for (int i = 0; i < inPolys.Count; i++)
        {
            var poly = inPolys[i];
            var orientation = TPPLPointUtil.GetOrientation(poly);
            if (orientation == holeOrientation)
            {
                polys.Add(new PolyHole([.. poly], true));
                hasHoles = true;
            }
            else
                polys.Add(new PolyHole([.. poly], false));
        }

        if (!hasHoles)
        {
            outPolys = [.. inPolys];
            return true;
        }

        while (true)
        {
            int holeIndex = -1;
            int holePointIndex = 0;

            for (int i = 0; i < polys.Count; i++)
            {
                var poly = polys[i];
                if (!poly.IsHole) continue;

                if (holeIndex == -1)
                {
                    holeIndex = i;
                    holePointIndex = 0;
                }

                for (int j = 0; j < poly.Count; j++)
                {
                    if (poly[j].X > polys[holeIndex][holePointIndex].X)
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
                var poly = polys[i];
                if (poly.IsHole) continue;

                for (int j = 0; j < poly.Count; j++)
                {
                    if (poly[j].X <= holePoint.X) continue;

                    int prev = (j + poly.Count - 1) % poly.Count;
                    int next = (j + 1) % poly.Count;

                    if (!TPPLPointUtil.InCone(poly[prev], poly[j], poly[next], holePoint))
                        continue;

                    TPPLPoint polyPoint = poly[j];
                    if (pointFound)
                    {
                        float v1Dist = TPPLPointUtil.Distance(holePoint, polyPoint);
                        float v2Dist = TPPLPointUtil.Distance(holePoint, bestPolyPoint);
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
                            if (TPPLPointUtil.Intersects(holePoint, polyPoint, lineP1, lineP2))
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

            if (!pointFound)
            {
                outPolys = null;
                return false;
            }

            var polyIter = polys[polyIndex];
            var holeIter = polys[holeIndex];

            var newPoly = new TPPLPoint[holeIter.Count + polyIter.Count + 2];
            var idx = 0;

            for (int i = 0; i <= polyPointIndex; i++)
                newPoly[idx++] = polyIter[i];

            for (int i = 0; i <= holeIter.Count; i++)
                newPoly[idx++] = holeIter[(i + holePointIndex) % holeIter.Count];

            for (int i = polyPointIndex; i < polyIter.Count; i++)
                newPoly[idx++] = polyIter[i];


            var first = Math.Max(holeIndex, polyIndex);
            var second = Math.Min(holeIndex, polyIndex);
            polys.RemoveAt(first);
            polys.RemoveAt(second);
            polys.Add(new(newPoly, false));
        }

        outPolys = new List<TPPLPoint[]>(polys.Count);
        foreach (var item in polys)
            outPolys.Add(item.Polygon);

        return true;
    }
}