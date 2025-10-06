namespace PolyPartition;

internal class PartitionVertex
{
    public bool IsActive;
    public bool IsConvex;
    public bool IsEar;
    public TPPLPoint Point;
    public float Angle;
    public PartitionVertex Previous;
    public PartitionVertex Next;

    private PartitionVertex()
    {
        Previous = null!;
        Next = null!;
    }

    public bool InCone(TPPLPoint p) => TPPLUtil.InCone(Previous.Point, Point, Next.Point, p);
    public void UpdateVertexReflexity() => IsConvex = TPPLUtil.IsConvex(Previous.Point, Point, Next.Point);
    public static PartitionVertex[] PartitionsFromPoly(TPPLPoint[] poly)
    {
        var len = poly.Length;
        PartitionVertex[] vertices = new PartitionVertex[len];
        for (int i = 0; i < len; i++)
        {
            vertices[i] = new() // Temporary null values
            {
                IsActive = true,
                Point = poly[i],
            };
        }

        // back loop
        vertices[0].Previous = vertices[^1];
        vertices[0].Next = vertices[1];

        for (int i = 1; i < len - 1; i++)
        {
            vertices[i].Previous = vertices[i - 1];
            vertices[i].Next = vertices[i + 1];
        }

        // front loop
        vertices[^1].Previous = vertices[^2];
        vertices[^1].Next = vertices[0];

        return vertices;
    }
}

internal class MonotoneVertex
{
    public TPPLPoint Point;
    public int Previous;
    public int Next;
}

internal class VertexSorter(MonotoneVertex[] v) : IComparer<int>
{
    private readonly MonotoneVertex[] _vertices = v;
    public int Compare(int index1, int index2)
    {
        if (_vertices[index1].Point.Y > _vertices[index2].Point.Y)
            return -1;
        else if (_vertices[index1].Point.Y == _vertices[index2].Point.Y)
        {
            if (_vertices[index1].Point.X > _vertices[index2].Point.X)
                return -1;
        }
        return 1;
    }
}

internal struct Diagonal
{
    public int Index1;
    public int Index2;
}

internal class DPState
{
    public bool Visible;
    public float Weight;
    public int BestVertex;
}

internal class DPState2
{
    public bool Visible;
    public int Weight;
    public List<Diagonal> Pairs = [];
}

internal class ScanLineEdge : IComparable<ScanLineEdge>
{
    public int Index;
    public TPPLPoint P1;
    public TPPLPoint P2;

    public int CompareTo(ScanLineEdge? other)
    {
        if (other is null)
            return 1;

        if (other.P1.Y == other.P2.Y)
        {
            if (P1.Y == P2.Y)
                return P1.Y < other.P1.Y ? -1 : 1;
            return TPPLUtil.IsConvex(P1, P2, other.P1) ? -1 : 1;
        }
        if (P1.Y == P2.Y)
            return !TPPLUtil.IsConvex(other.P1, other.P2, P1) ? -1 : 1;
        if (P1.Y < other.P1.Y)
            return !TPPLUtil.IsConvex(other.P1, other.P2, P1) ? -1 : 1;
        return TPPLUtil.IsConvex(P1, P2, other.P1) ? -1 : 1;
    }
}
