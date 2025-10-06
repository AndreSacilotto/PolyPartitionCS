namespace PolyPartition;

public static partial class TPPLPartition
{
    //public static void PrintArray2D<T>(IEnumerable<T[]> values, string separator = ", ")
    //{
    //    foreach (var val in values)
    //        PrintArray(val, separator);
    //}
    //public static void PrintArray<T>(IEnumerable<T> values, string separator = ", ")
    //{
    //    string str = "";
    //    foreach (var item in values)
    //        str += item + separator;
    //    Godot.GD.Print(str.Substring(0, str.Length - separator.Length));
    //}

    public static bool Partition(TPPLPartitionMethod method, TPPLPolygonList inPolys, out List<TPPLPoint[]> outPolys) => Partition(method, inPolys, outPolys = []);
    public static bool Partition(TPPLPartitionMethod method, TPPLPolygonList inPolys, List<TPPLPoint[]> outPolys)
    {
        return method switch
        {
            TPPLPartitionMethod.ConvexPartitionHM => ConvexPartition_HM(inPolys, outPolys),
            TPPLPartitionMethod.ConvexPartitionOPT => ConvexPartition_OPT(inPolys, outPolys),
            TPPLPartitionMethod.TriangulateEC => Triangulate_EC(inPolys, outPolys),
            TPPLPartitionMethod.TriagulateMONO => Triangulate_MONO(inPolys.ToPoints(), outPolys),
            TPPLPartitionMethod.TriangulateOPT => Triangulate_OPT(inPolys, outPolys),
            _ => false,
        };
    }

    public static bool RemoveHoles(TPPLPolygonList polys, out List<TPPLPoint[]> outPolys) => RemoveHoles(polys, outPolys = []);

    /// <summary> Eliminates holes from polygons by merging them with their containing outer polygons </summary>
    /// <param name="polys">List of polygons with hole defined - WARN: It will be modified</param>
    /// <param name="outPolys">shallow copy</param>
    /// <returns>If had success on removing the holes</returns>
    public static bool RemoveHoles(TPPLPolygonList polys, List<TPPLPoint[]> outPolys)
    {
        if (polys.HasHoles)
        {
            while (true)
            {
                int holeIndex = -1;
                int holePointIndex = 0;

                for (int i = 0; i < polys.Count; i++)
                {
                    var polyI = polys[i];

                    if (!polyI.IsHole) continue;

                    if (holeIndex == -1)
                    {
                        holeIndex = i;
                        holePointIndex = 0;
                    }

                    for (int j = 0; j < polyI.Count; j++)
                    {
                        if (polyI[j].X > polys[holeIndex][holePointIndex].X)
                        {
                            holeIndex = i;
                            holePointIndex = j;
                        }
                    }
                }

                if (holeIndex == -1) break;

                var holePoly = polys[holeIndex];
                var holePoint = holePoly[holePointIndex];

                int polyIndex = -1;
                int polyPointIndex = 0;
                TPPLPoint bestPolyPoint = new();

                for (int i = 0; i < polys.Count; i++)
                {
                    var polyI = polys[i];
                    if (polyI.IsHole) continue;

                    for (int j = 0; j < polyI.Count; j++)
                    {
                        TPPLPoint polyPoint = polyI[j];

                        if (polyPoint.X <= holePoint.X) continue;

                        int prev = (j + polyI.Count - 1) % polyI.Count;
                        int next = (j + 1) % polyI.Count;

                        if (!TPPLUtil.InCone(polyI[prev], polyPoint, polyI[next], holePoint))
                            continue;

                        if (polyIndex != -1)
                        {
                            float v1Dist = TPPLUtil.Distance(holePoint, polyPoint);
                            float v2Dist = TPPLUtil.Distance(holePoint, bestPolyPoint);
                            if (v2Dist < v1Dist) continue;
                        }

                        bool pointInvisible = false;
                        foreach (var polyK in polys)
                        {
                            if (polyK.IsHole) continue;

                            for (int l = 0; l < polyK.Count; l++)
                            {
                                var lineP1 = polyK[l];
                                var lineP2 = polyK[(l + 1) % polyK.Count];
                                if (TPPLUtil.Intersects(holePoint, polyPoint, lineP1, lineP2))
                                {
                                    pointInvisible = true;
                                    break;
                                }
                            }
                            if (pointInvisible) break;
                        }

                        if (!pointInvisible)
                        {
                            bestPolyPoint = polyPoint;
                            polyIndex = i;
                            polyPointIndex = j;
                        }
                    }
                }

                if (polyIndex == -1)
                    return false;

                var poly = polys[polyIndex];

                var newPoly = new TPPLPoint[holePoly.Count + poly.Count + 2];
                var idx = 0;

                for (int i = 0; i <= polyPointIndex; i++)
                    newPoly[idx++] = poly[i];

                for (int i = 0; i <= holePoly.Count; i++)
                    newPoly[idx++] = holePoly[(i + holePointIndex) % holePoly.Count];

                for (int i = polyPointIndex; i < poly.Count; i++)
                    newPoly[idx++] = poly[i];

                var first = Math.Max(holeIndex, polyIndex);
                var second = Math.Min(holeIndex, polyIndex);
                polys.RemoveAt(first);
                polys.RemoveAt(second);
                polys.Add(new(newPoly, false));
            }
        }

        outPolys.EnsureCapacity(outPolys.Count + polys.Count);
        foreach (var item in polys)
            outPolys.Add(item.Polygon);

        return true;
    }

}