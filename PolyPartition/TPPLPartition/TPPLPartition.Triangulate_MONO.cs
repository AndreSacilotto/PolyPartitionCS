namespace PolyPartition;

partial class TPPLPartition
{
    #region Helper MonotonePartition
    private static void AddDiagonal(MonotoneVertex[] vertices, ref int numVertices, int index1, int index2,
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

    private static bool TriangulateMonotone(TPPLPoint[] inPolys, out List<TPPLPoint[]> triangles)
    {
        int len = inPolys.Length;
        triangles = [];

        if (len < 3) return false;

        if (len == 3)
        {
            triangles.Add(inPolys);
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
                        triangle = TPPLPointUtil.Triangle(inPolys[stack[j + 1]], inPolys[stack[j]], inPolys[vindex]);
                    else
                        triangle = TPPLPointUtil.Triangle(inPolys[stack[j]], inPolys[stack[j + 1]], inPolys[vindex]);
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
                        if (TPPLPointUtil.IsConvex(inPolys[vindex], inPolys[stack[stackPtr - 1]], inPolys[stack[stackPtr]]))
                        {
                            TPPLPoint[] triangle = TPPLPointUtil.Triangle(inPolys[vindex], inPolys[stack[stackPtr - 1]], inPolys[stack[stackPtr]]);
                            triangles.Add(triangle);
                            stackPtr--;
                        }
                        else break;
                    }
                    else
                    {
                        if (TPPLPointUtil.IsConvex(inPolys[vindex], inPolys[stack[stackPtr]], inPolys[stack[stackPtr - 1]]))
                        {
                            TPPLPoint[] triangle = TPPLPointUtil.Triangle(inPolys[vindex], inPolys[stack[stackPtr]], inPolys[stack[stackPtr - 1]]);
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
                triangle = TPPLPointUtil.Triangle(inPolys[stack[j]], inPolys[stack[j + 1]], inPolys[lastVindex]);
            else
                triangle = TPPLPointUtil.Triangle(inPolys[stack[j + 1]], inPolys[stack[j]], inPolys[lastVindex]);
            triangles.Add(triangle);
        }

        return true;
    }
    #endregion

    public static bool MonotonePartition(List<TPPLPoint[]> inPolys, out List<TPPLPoint[]> monotonePolys)
    {
        monotonePolys = [];
        int numVertices = 0;
        foreach (var poly in inPolys)
        {
            if (!TPPLPointUtil.IsValidPolygon(poly)) return false;
            numVertices += poly.Length;
        }

        int maxNumVertices = numVertices * 3;
        MonotoneVertex[] vertices = new MonotoneVertex[maxNumVertices];
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
                vertices[i + polyStartIndex].Next = i == poly.Length - 1 ?
                    polyStartIndex : i + polyStartIndex + 1;
            }
            polyStartIndex = polyEndIndex + 1;
        }

        // Construct priority queue
        int[] priority = new int[numVertices];
        for (int i = 0; i < numVertices; i++)
            priority[i] = i;
        Array.Sort(priority, new VertexSorter(vertices));

        // Determine vertex types
        TPPLVertexType[] vertexTypes = new TPPLVertexType[maxNumVertices];
        for (int i = 0; i < numVertices; i++)
        {
            MonotoneVertex v = vertices[i];
            MonotoneVertex vprev = vertices[v.Previous];
            MonotoneVertex vnext = vertices[v.Next];

            if (Below(vprev.Point, v.Point) && Below(vnext.Point, v.Point))
            {
                vertexTypes[i] = TPPLPointUtil.IsConvex(vnext.Point, vprev.Point, v.Point) ?
                    TPPLVertexType.Start : TPPLVertexType.Split;
            }
            else if (Below(v.Point, vprev.Point) && Below(v.Point, vnext.Point))
            {
                vertexTypes[i] = TPPLPointUtil.IsConvex(vnext.Point, vprev.Point, v.Point) ?
                    TPPLVertexType.End : TPPLVertexType.Merge;
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
            int vindex = priority[i];
            MonotoneVertex v = vertices[vindex];
            int vindex2 = vindex;
            MonotoneVertex v2 = v;

            switch (vertexTypes[vindex])
            {
                case TPPLVertexType.Start:
                ScanLineEdge newEdge = new()
                {
                    P1 = v.Point,
                    P2 = vertices[v.Next].Point,
                    Index = vindex
                };
                edgeTree.Add(newEdge);
                edgeTreeIterators[vindex] = newEdge;
                helpers[vindex] = vindex;
                break;

                case TPPLVertexType.End:
                if (!edgeTreeIterators.ContainsKey(v.Previous))
                {
                    error = true;
                    break;
                }
                if (vertexTypes[helpers[v.Previous]] == TPPLVertexType.Merge)
                {
                    AddDiagonal(vertices, ref newNumVertices, vindex, helpers[v.Previous],
                               vertexTypes, edgeTreeIterators, edgeTree, helpers);
                }
                edgeTree.Remove(edgeTreeIterators[v.Previous]);
                break;

                case TPPLVertexType.Split:
                ScanLineEdge searchEdge = new() { P1 = v.Point, P2 = v.Point };
                var edgeIter = edgeTree.GetViewBetween(edgeTree.Min, searchEdge).Max;
                if (edgeIter == null)
                {
                    error = true;
                    break;
                }
                AddDiagonal(vertices, ref newNumVertices, vindex, helpers[edgeIter.Index],
                           vertexTypes, edgeTreeIterators, edgeTree, helpers);
                vindex2 = newNumVertices - 2;
                v2 = vertices[vindex2];
                helpers[edgeIter.Index] = vindex;
                newEdge = new ScanLineEdge
                {
                    P1 = v2.Point,
                    P2 = vertices[v2.Next].Point,
                    Index = vindex2
                };
                edgeTree.Add(newEdge);
                edgeTreeIterators[vindex2] = newEdge;
                helpers[vindex2] = vindex2;
                break;

                case TPPLVertexType.Merge:
                if (!edgeTreeIterators.ContainsKey(v.Previous))
                {
                    error = true;
                    break;
                }
                if (vertexTypes[helpers[v.Previous]] == TPPLVertexType.Merge)
                {
                    AddDiagonal(vertices, ref newNumVertices, vindex, helpers[v.Previous],
                               vertexTypes, edgeTreeIterators, edgeTree, helpers);
                    vindex2 = newNumVertices - 2;
                }
                edgeTree.Remove(edgeTreeIterators[v.Previous]);
                searchEdge = new ScanLineEdge { P1 = v.Point, P2 = v.Point };
                edgeIter = edgeTree.GetViewBetween(edgeTree.Min, searchEdge).Max;
                if (edgeIter == null)
                {
                    error = true;
                    break;
                }
                if (vertexTypes[helpers[edgeIter.Index]] == TPPLVertexType.Merge)
                {
                    AddDiagonal(vertices, ref newNumVertices, vindex2, helpers[edgeIter.Index],
                               vertexTypes, edgeTreeIterators, edgeTree, helpers);
                }
                helpers[edgeIter.Index] = vindex2;
                break;

                case TPPLVertexType.Regular:
                if (Below(v.Point, vertices[v.Previous].Point))
                {
                    if (!edgeTreeIterators.ContainsKey(v.Previous))
                    {
                        error = true;
                        break;
                    }
                    if (vertexTypes[helpers[v.Previous]] == TPPLVertexType.Merge)
                    {
                        AddDiagonal(vertices, ref newNumVertices, vindex, helpers[v.Previous],
                                   vertexTypes, edgeTreeIterators, edgeTree, helpers);
                        vindex2 = newNumVertices - 2;
                        v2 = vertices[vindex2];
                    }
                    edgeTree.Remove(edgeTreeIterators[v.Previous]);
                    newEdge = new ScanLineEdge
                    {
                        P1 = v2.Point,
                        P2 = vertices[v2.Next].Point,
                        Index = vindex2
                    };
                    edgeTree.Add(newEdge);
                    edgeTreeIterators[vindex2] = newEdge;
                    helpers[vindex2] = vindex;
                }
                else
                {
                    searchEdge = new ScanLineEdge { P1 = v.Point, P2 = v.Point };
                    edgeIter = edgeTree.GetViewBetween(edgeTree.Min, searchEdge).Max;
                    if (edgeIter == null)
                    {
                        error = true;
                        break;
                    }
                    if (vertexTypes[helpers[edgeIter.Index]] == TPPLVertexType.Merge)
                    {
                        AddDiagonal(vertices, ref newNumVertices, vindex, helpers[edgeIter.Index],
                                   vertexTypes, edgeTreeIterators, edgeTree, helpers);
                    }
                    helpers[edgeIter.Index] = vindex;
                }
                break;
            }

            if (error) break;
        }

        if (error) return false;

        bool[] used = new bool[newNumVertices];
        for (int i = 0; i < newNumVertices; i++)
        {
            if (used[i]) continue;

            MonotoneVertex v = vertices[i];
            MonotoneVertex vnext = vertices[v.Next];
            int size = 1;
            while (vnext != v)
            {
                vnext = vertices[vnext.Next];
                size++;
            }

            var mpoly = new TPPLPoint[size];
            v = vertices[i];
            mpoly[0] = v.Point;
            vnext = vertices[v.Next];
            size = 1;
            used[i] = true;
            used[v.Next] = true;

            while (vnext != v)
            {
                mpoly[size] = vnext.Point;
                used[vnext.Next] = true;
                vnext = vertices[vnext.Next];
                size++;
            }
            monotonePolys.Add(mpoly);
        }

        return true;
    }

    public static bool Triangulate_MONO(List<TPPLPoint[]> inPolys, out List<TPPLPoint[]> triangles)
    {
        if (!MonotonePartition(inPolys, out var monotone))
        {
            triangles = [];
            return false;
        }
        foreach (var poly in monotone)
            if (!TriangulateMonotone(poly, out triangles))
                return false;
        triangles = [];
        return true;
    }

    public static bool Triangulate_MONO(TPPLPoint[] poly, out List<TPPLPoint[]> triangles)
    {
        return Triangulate_MONO([poly], out triangles);
    }

}
