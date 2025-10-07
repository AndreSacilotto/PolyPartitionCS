namespace PolyPartition;

//Triangulation by partition into monotone polygons
//Complexity: O(n* log(n))/O(n)
//Holes: Yes, by nature
//Solution: Poor, many thin triangles are created in most cases

partial class TPPLPartition
{
    #region Helper MonotonePartition
    private static void AddDiagonal(ReadOnlySpan<MonotoneVertex> vertices, ref int numVertices, int index1, int index2,
                            TPPLVertexType[] vertexTypes, Dictionary<int, ScanLineEdge> edgeTreeIterators,
                            SortedSet<ScanLineEdge> edgeTree, int[] helpers)
    {
        int newIndex1 = numVertices++;
        int newIndex2 = numVertices++;

        vertices[newIndex1].Point = vertices[index1].Point;
        vertices[newIndex2].Point = vertices[index2].Point;

        vertices[newIndex2].Next = vertices[index2].Next;
        vertices[newIndex1].Next = vertices[index1].Next;

        vertices[vertices[index2].Next].Previous = newIndex2;
        vertices[vertices[index1].Next].Previous = newIndex1;

        vertices[index1].Next = newIndex2;
        vertices[newIndex2].Previous = index1;

        vertices[index2].Next = newIndex1;
        vertices[newIndex1].Previous = index2;

        vertexTypes[newIndex1] = vertexTypes[index1];
        if (edgeTreeIterators.TryGetValue(index1, out ScanLineEdge? edge))
        {
            edgeTree.Remove(edge);
            edge.Index = newIndex1;
            edgeTree.Add(edge);
            edgeTreeIterators[newIndex1] = edge;
        }
        helpers[newIndex1] = helpers[index1];

        vertexTypes[newIndex2] = vertexTypes[index2];
        if (edgeTreeIterators.TryGetValue(index2, out edge))
        {
            edgeTree.Remove(edge);
            edge.Index = newIndex2;
            edgeTree.Add(edge);
            edgeTreeIterators[newIndex2] = edge;
        }
        helpers[newIndex2] = helpers[index2];
    }
    private static bool Below(TPPLPoint p1, TPPLPoint p2)
    {
        if (p1.Y < p2.Y) return true;
        if (p1.Y == p2.Y && p1.X < p2.X) return true;
        return false;
    }

    private static bool TriangulateMonotone(ReadOnlySpan<TPPLPoint> inPolys, List<TPPLPoint[]> triangles)
    {
        int len = inPolys.Length;
        if (len < 3) return false;
        if (len == 3)
        {
            triangles.Add([.. inPolys]);
            return true;
        }

        int topIndex = 0, bottomIndex = 0;
        for (int i = 1; i < len; i++)
        {
            if (Below(inPolys[i], inPolys[bottomIndex])) bottomIndex = i;
            if (Below(inPolys[topIndex], inPolys[i])) topIndex = i;
        }

        // Verify monotonicity
        int idx = topIndex;
        while (idx != bottomIndex)
        {
            int i2 = (idx + 1) % len;
            if (!Below(inPolys[i2], inPolys[idx])) return false;
            idx = i2;
        }
        idx = bottomIndex;
        while (idx != topIndex)
        {
            int i2 = (idx + 1) % len;
            if (!Below(inPolys[idx], inPolys[i2])) return false;
            idx = i2;
        }

        sbyte[] vertexTypes = new sbyte[len];
        int[] priority = new int[len];

        priority[0] = topIndex;
        vertexTypes[topIndex] = 0;
        int leftIndex = (topIndex + 1) % len;
        int rightIndex = (topIndex - 1 + len) % len;

        for (int i = 1; i < len - 1; i++)
        {
            if (leftIndex == bottomIndex)
            {
                priority[i] = rightIndex;
                rightIndex = (rightIndex - 1 + len) % len;
                vertexTypes[priority[i]] = -1;
            }
            else if (rightIndex == bottomIndex)
            {
                priority[i] = leftIndex;
                leftIndex = (leftIndex + 1) % len;
                vertexTypes[priority[i]] = 1;
            }
            else
            {
                if (Below(inPolys[leftIndex], inPolys[rightIndex]))
                {
                    priority[i] = rightIndex;
                    rightIndex = (rightIndex - 1 + len) % len;
                    vertexTypes[priority[i]] = -1;
                }
                else
                {
                    priority[i] = leftIndex;
                    leftIndex = (leftIndex + 1) % len;
                    vertexTypes[priority[i]] = 1;
                }
            }
        }
        priority[len - 1] = bottomIndex;
        vertexTypes[bottomIndex] = 0;

        int[] stack = new int[len];
        int stackPtr = 0;
        stack[stackPtr++] = priority[0];
        stack[stackPtr++] = priority[1];

        for (int i = 2; i < len - 1; i++)
        {
            int vindex = priority[i];
            if (vertexTypes[vindex] != vertexTypes[stack[stackPtr - 1]])
            {
                for (int j = 0; j < stackPtr - 1; j++)
                {
                    TPPLPoint[] triangle;
                    if (vertexTypes[vindex] == 1)
                        triangle = TPPLUtil.Triangle(inPolys[stack[j + 1]], inPolys[stack[j]], inPolys[vindex]);
                    else
                        triangle = TPPLUtil.Triangle(inPolys[stack[j]], inPolys[stack[j + 1]], inPolys[vindex]);
                    triangles.Add(triangle);
                }
                stack[0] = priority[i - 1];
                stack[1] = priority[i];
                stackPtr = 2;
            }
            else
            {
                stackPtr--;
                while (stackPtr > 0)
                {
                    if (vertexTypes[vindex] == 1)
                    {
                        if (TPPLUtil.IsConvex(inPolys[vindex], inPolys[stack[stackPtr - 1]], inPolys[stack[stackPtr]]))
                        {
                            TPPLPoint[] triangle = TPPLUtil.Triangle(inPolys[vindex], inPolys[stack[stackPtr - 1]], inPolys[stack[stackPtr]]);
                            triangles.Add(triangle);
                            stackPtr--;
                        }
                        else break;
                    }
                    else
                    {
                        if (TPPLUtil.IsConvex(inPolys[vindex], inPolys[stack[stackPtr]], inPolys[stack[stackPtr - 1]]))
                        {
                            TPPLPoint[] triangle = TPPLUtil.Triangle(inPolys[vindex], inPolys[stack[stackPtr]], inPolys[stack[stackPtr - 1]]);
                            triangles.Add(triangle);
                            stackPtr--;
                        }
                        else break;
                    }
                }
                stackPtr++;
                stack[stackPtr++] = vindex;
            }
        }

        int lastVindex = priority[len - 1];
        for (int j = 0; j < stackPtr - 1; j++)
        {
            TPPLPoint[] triangle;
            if (vertexTypes[stack[j + 1]] == 1)
                triangle = TPPLUtil.Triangle(inPolys[stack[j]], inPolys[stack[j + 1]], inPolys[lastVindex]);
            else
                triangle = TPPLUtil.Triangle(inPolys[stack[j + 1]], inPolys[stack[j]], inPolys[lastVindex]);
            triangles.Add(triangle);
        }

        return true;
    }
    #endregion

    public static bool Triangulate_MONO(List<TPPLPoint[]> inPolys, out List<TPPLPoint[]> triangles) => Triangulate_MONO(inPolys, triangles = []);
    public static bool Triangulate_MONO(List<TPPLPoint[]> inPolys, List<TPPLPoint[]> triangles)
    {
        if (!MonotonePartition(inPolys, out var monotone))
            return false;
        foreach (var poly in monotone)
            if (!TriangulateMonotone(poly, triangles))
                return false;
        return true;
    }

    public static bool MonotonePartition(List<TPPLPoint[]> inPolys, out List<TPPLPoint[]> monotonePolys)
    {
        monotonePolys = [];
        int numVertices = 0;
        foreach (var poly in inPolys)
        {
            if (poly.Length < 3) return false;
            numVertices += poly.Length;
        }

        int maxNumVertices = numVertices * 3;
        var vertices = new MonotoneVertex[maxNumVertices];
        for (int i = 0; i < maxNumVertices; i++)
            vertices[i] = new MonotoneVertex();

        int newNumVertices = numVertices;
        int polyStartIndex = 0;

        foreach (var poly in inPolys)
        {
            int polyEndIndex = polyStartIndex + poly.Length - 1;
            for (int i = 0; i < poly.Length; i++)
            {
                vertices[i + polyStartIndex].Point = poly[i];
                vertices[i + polyStartIndex].Previous = i == 0 ? polyEndIndex : i + polyStartIndex - 1;
                vertices[i + polyStartIndex].Next = i == poly.Length - 1 ? polyStartIndex : i + polyStartIndex + 1;
            }
            polyStartIndex = polyEndIndex + 1;
        }

        // Construct priority queue
        int[] priority = new int[numVertices];
        for (int i = 0; i < numVertices; i++)
            priority[i] = i;
        Array.Sort(priority, new VertexSorter(vertices));

        // Determine vertex types
        var vertexTypes = new TPPLVertexType[maxNumVertices];
        for (int i = 0; i < numVertices; i++)
        {
            MonotoneVertex vtx = vertices[i];
            MonotoneVertex vtxPrev = vertices[vtx.Previous];
            MonotoneVertex vtxNext = vertices[vtx.Next];

            if (Below(vtxPrev.Point, vtx.Point) && Below(vtxNext.Point, vtx.Point))
            {
                vertexTypes[i] = TPPLUtil.IsConvex(vtxNext.Point, vtxPrev.Point, vtx.Point) ? TPPLVertexType.Start : TPPLVertexType.Split;
            }
            else if (Below(vtx.Point, vtxPrev.Point) && Below(vtx.Point, vtxNext.Point))
            {
                vertexTypes[i] = TPPLUtil.IsConvex(vtxNext.Point, vtxPrev.Point, vtx.Point) ? TPPLVertexType.End : TPPLVertexType.Merge;
            }
            else
            {
                vertexTypes[i] = TPPLVertexType.Regular;
            }
        }

        int[] helpers = new int[maxNumVertices];
        SortedSet<ScanLineEdge> edgeTree = [];
        Dictionary<int, ScanLineEdge> edgeTreeIterators = [];

        bool error = false;

        for (int i = 0; i < numVertices; i++)
        {
            int vtxIdx = priority[i];
            MonotoneVertex vtx = vertices[vtxIdx];
            int vtxIdx2 = vtxIdx;
            MonotoneVertex vtx2 = vtx;

            switch (vertexTypes[vtxIdx])
            {
                case TPPLVertexType.Start:
                {
                    ScanLineEdge newEdge = new() {
                        P1 = vtx.Point,
                        P2 = vertices[vtx.Next].Point,
                        Index = vtxIdx
                    };
                    edgeTree.Add(newEdge);
                    edgeTreeIterators[vtxIdx] = newEdge;
                    helpers[vtxIdx] = vtxIdx;
                    break;
                }
                case TPPLVertexType.End:
                {
                    if (!edgeTreeIterators.ContainsKey(vtx.Previous))
                    {
                        error = true;
                        break;
                    }
                    if (vertexTypes[helpers[vtx.Previous]] == TPPLVertexType.Merge)
                    {
                        AddDiagonal(vertices, ref newNumVertices, vtxIdx, helpers[vtx.Previous],
                                   vertexTypes, edgeTreeIterators, edgeTree, helpers);
                    }
                    edgeTree.Remove(edgeTreeIterators[vtx.Previous]);
                    break;
                }
                case TPPLVertexType.Split:
                {
                    ScanLineEdge searchEdge = new() { P1 = vtx.Point, P2 = vtx.Point };
                    var edgeIter = edgeTree.GetViewBetween(edgeTree.Min, searchEdge).Max;
                    if (edgeIter == null)
                    {
                        error = true;
                        break;
                    }
                    AddDiagonal(vertices, ref newNumVertices, vtxIdx, helpers[edgeIter.Index], vertexTypes, edgeTreeIterators, edgeTree, helpers);
                    vtxIdx2 = newNumVertices - 2;
                    vtx2 = vertices[vtxIdx2];
                    helpers[edgeIter.Index] = vtxIdx;
                    var newEdge = new ScanLineEdge {
                        P1 = vtx2.Point,
                        P2 = vertices[vtx2.Next].Point,
                        Index = vtxIdx2
                    };
                    edgeTree.Add(newEdge);
                    edgeTreeIterators[vtxIdx2] = newEdge;
                    helpers[vtxIdx2] = vtxIdx2;
                    break;
                }
                case TPPLVertexType.Merge:
                {
                    if (!edgeTreeIterators.ContainsKey(vtx.Previous))
                    {
                        error = true;
                        break;
                    }
                    if (vertexTypes[helpers[vtx.Previous]] == TPPLVertexType.Merge)
                    {
                        AddDiagonal(vertices, ref newNumVertices, vtxIdx, helpers[vtx.Previous],
                                   vertexTypes, edgeTreeIterators, edgeTree, helpers);
                        vtxIdx2 = newNumVertices - 2;
                    }
                    edgeTree.Remove(edgeTreeIterators[vtx.Previous]);
                    var searchEdge = new ScanLineEdge { P1 = vtx.Point, P2 = vtx.Point };
                    var edgeIter = edgeTree.GetViewBetween(edgeTree.Min, searchEdge).Max;
                    if (edgeIter == null)
                    {
                        error = true;
                        break;
                    }
                    if (vertexTypes[helpers[edgeIter.Index]] == TPPLVertexType.Merge)
                    {
                        AddDiagonal(vertices, ref newNumVertices, vtxIdx2, helpers[edgeIter.Index],
                                   vertexTypes, edgeTreeIterators, edgeTree, helpers);
                    }
                    helpers[edgeIter.Index] = vtxIdx2;
                    break;
                }
                case TPPLVertexType.Regular:
                {
                    if (Below(vtx.Point, vertices[vtx.Previous].Point))
                    {
                        if (!edgeTreeIterators.ContainsKey(vtx.Previous))
                        {
                            error = true;
                            break;
                        }
                        if (vertexTypes[helpers[vtx.Previous]] == TPPLVertexType.Merge)
                        {
                            AddDiagonal(vertices, ref newNumVertices, vtxIdx, helpers[vtx.Previous], vertexTypes, edgeTreeIterators, edgeTree, helpers);
                            vtxIdx2 = newNumVertices - 2;
                            vtx2 = vertices[vtxIdx2];
                        }
                        edgeTree.Remove(edgeTreeIterators[vtx.Previous]);
                        var newEdge = new ScanLineEdge {
                            P1 = vtx2.Point,
                            P2 = vertices[vtx2.Next].Point,
                            Index = vtxIdx2
                        };
                        edgeTree.Add(newEdge);
                        edgeTreeIterators[vtxIdx2] = newEdge;
                        helpers[vtxIdx2] = vtxIdx;
                    }
                    else
                    {
                        var searchEdge = new ScanLineEdge { P1 = vtx.Point, P2 = vtx.Point };
                        var edgeIter = edgeTree.GetViewBetween(edgeTree.Min, searchEdge).Max;
                        if (edgeIter == null)
                        {
                            error = true;
                            break;
                        }
                        if (vertexTypes[helpers[edgeIter.Index]] == TPPLVertexType.Merge)
                        {
                            AddDiagonal(vertices, ref newNumVertices, vtxIdx, helpers[edgeIter.Index],
                                       vertexTypes, edgeTreeIterators, edgeTree, helpers);
                        }
                        helpers[edgeIter.Index] = vtxIdx;
                    }
                    break;
                }
            }

            if (error) break;
        }

        if (error) return false;

        var used = new bool[newNumVertices];
        for (int i = 0; i < newNumVertices; i++)
        {
            if (used[i]) continue;

            MonotoneVertex vtx = vertices[i];
            MonotoneVertex vtxNext = vertices[vtx.Next];
            int size = 1;
            while (vtxNext != vtx)
            {
                vtxNext = vertices[vtxNext.Next];
                size++;
            }

            var mpoly = new TPPLPoint[size];
            vtx = vertices[i];
            mpoly[0] = vtx.Point;
            vtxNext = vertices[vtx.Next];
            size = 1;
            used[i] = true;
            used[vtx.Next] = true;

            while (vtxNext != vtx)
            {
                mpoly[size] = vtxNext.Point;
                used[vtxNext.Next] = true;
                vtxNext = vertices[vtxNext.Next];
                size++;
            }
            monotonePolys.Add(mpoly);
        }

        return true;
    }

}
