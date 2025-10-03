namespace PolyPartition;

internal class PartitionVertex(PartitionVertex prev, PartitionVertex next)
{
    public bool IsActive;
    public bool IsConvex;
    public bool IsEar;
    public TPPLPoint Point;
    public float Angle;
    public PartitionVertex Previous = prev;
    public PartitionVertex Next = next;

    public bool InCone(TPPLPoint p) => TPPLPointExt.InCone(Previous.Point, Point, Next.Point, p);
    public void UpdateVertexReflexity() => IsConvex = TPPLPointExt.IsConvex(Previous.Point, Point, Next.Point);
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
            return TPPLPointExt.IsConvex(P1, P2, other.P1) ? -1 : 1;
        }
        if (P1.Y == P2.Y)
            return !TPPLPointExt.IsConvex(other.P1, other.P2, P1) ? -1 : 1;
        if (P1.Y < other.P1.Y)
            return !TPPLPointExt.IsConvex(other.P1, other.P2, P1) ? -1 : 1;
        return TPPLPointExt.IsConvex(P1, P2, other.P1) ? -1 : 1;
    }
}
