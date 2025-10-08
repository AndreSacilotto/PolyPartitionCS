namespace PolyPartition;

//Convex partition using Hertel-Mehlhorn algorithm
//Complexity: O(n^2)/O(n)
//Holes: Yes, by calling RemoveHoles.
//Solution: Optimal, even though it creates 4 times the minimum number of convex polygons.

partial class TPPLPartition
{
    public static bool ConvexPartition_HM(TPPLPolygonList inPolys, out List<TPPLPoint[]> parts) => ConvexPartition_HM(inPolys, parts = []);
    public static bool ConvexPartition_HM(TPPLPolygonList inPolys, List<TPPLPoint[]> parts)
    {
        if (!RemoveHoles(inPolys, out var outPolys))
            return false;
        foreach (var poly in outPolys)
            if (!ConvexPartition_HM(poly, parts))
                return false;
        return true;
    }

    public static bool ConvexPartition_HM(ReadOnlySpan<TPPLPoint> poly, List<TPPLPoint[]> parts)
    {
        var vertexCount = poly.Length;
        if (vertexCount < 3) return false;

        bool hasReflexVertex = false;
        for (int currentIndex = 0; currentIndex < vertexCount; currentIndex++)
        {
            int prevIndex = (currentIndex + vertexCount - 1) % vertexCount;
            int nextIndex = (currentIndex + 1) % vertexCount;
            if (TPPLUtil.IsReflex(poly[prevIndex], poly[currentIndex], poly[nextIndex]))
            {
                hasReflexVertex = true;
                break;
            }
        }

        if (!hasReflexVertex)
        {
            parts.Add([.. poly]);
            return true;
        }

        var triangles = new List<TPPLPoint[]>();
        if (!Triangulate_EC(poly, triangles))
        {
            return false;
        }

        for (int triangleIdx = 0; triangleIdx < triangles.Count; triangleIdx++)
        {
            TPPLPoint[] currentTriangle = triangles[triangleIdx];

            for (int edgeStartIdx = 0; edgeStartIdx < currentTriangle.Length; edgeStartIdx++)
            {
                TPPLPoint edgeStart = currentTriangle[edgeStartIdx];
                int edgeEndIndex = (edgeStartIdx + 1) % currentTriangle.Length;
                TPPLPoint edgeEnd = currentTriangle[edgeEndIndex];

                bool foundSharedEdge = false;
                for (int otherTriangleIndex = triangleIdx; otherTriangleIndex < triangles.Count; otherTriangleIndex++)
                {
                    if (triangleIdx == otherTriangleIndex) continue;

                    TPPLPoint[] otherTriangle = triangles[otherTriangleIndex];
                    for (int otherEdgeStartIndex = 0; otherEdgeStartIndex < otherTriangle.Length; otherEdgeStartIndex++)
                    {
                        if (edgeEnd != otherTriangle[otherEdgeStartIndex]) continue;

                        int otherEdgeEndIndex = (otherEdgeStartIndex + 1) % otherTriangle.Length;
                        if (edgeStart != otherTriangle[otherEdgeEndIndex]) continue;

                        foundSharedEdge = true;

                        // Check convexity at edgeStart
                        TPPLPoint middlePoint = currentTriangle[edgeStartIdx];
                        int prevIdx = (edgeStartIdx + currentTriangle.Length - 1) % currentTriangle.Length;
                        TPPLPoint prevPoint = currentTriangle[prevIdx];
                        int nextIdx = (otherEdgeEndIndex == otherTriangle.Length - 1) ? 0 : otherEdgeEndIndex + 1;
                        TPPLPoint nextPoint = otherTriangle[nextIdx];

                        if (!TPPLUtil.IsConvex(prevPoint, middlePoint, nextPoint))
                        {
                            foundSharedEdge = false;
                            continue;
                        }

                        // Check convexity at edgeEnd
                        middlePoint = currentTriangle[edgeEndIndex];
                        int nextInCurrentIndex = (edgeEndIndex == currentTriangle.Length - 1) ? 0 : edgeEndIndex + 1;
                        nextPoint = currentTriangle[nextInCurrentIndex];
                        int prevInOtherIndex = (otherEdgeStartIndex == 0) ? otherTriangle.Length - 1 : otherEdgeStartIndex - 1;
                        prevPoint = otherTriangle[prevInOtherIndex];

                        if (!TPPLUtil.IsConvex(prevPoint, middlePoint, nextPoint))
                        {
                            foundSharedEdge = false;
                            continue;
                        }

                        // Merge triangles by removing shared edge
                        var mergedPolygon = new TPPLPoint[currentTriangle.Length + otherTriangle.Length - 2];
                        var idx = 0;

                        for (int sourceIndex = edgeEndIndex; sourceIndex != edgeStartIdx; sourceIndex = (sourceIndex + 1) % currentTriangle.Length)
                            mergedPolygon[idx++] = currentTriangle[sourceIndex];

                        for (int sourceIndex = otherEdgeEndIndex; sourceIndex != otherEdgeStartIndex; sourceIndex = (sourceIndex + 1) % otherTriangle.Length)
                            mergedPolygon[idx++] = otherTriangle[sourceIndex];

                        triangles.RemoveAt(otherTriangleIndex);
                        triangles[triangleIdx] = mergedPolygon;
                        currentTriangle = triangles[triangleIdx];
                        edgeStartIdx = -1;
                        break;
                    }
                    if (foundSharedEdge) break;
                }
                if (edgeStartIdx == -1) break;
            }
        }

        parts.AddRange(triangles);
        return true;
    }
}