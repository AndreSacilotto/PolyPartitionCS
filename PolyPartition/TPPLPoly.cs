namespace PolyPartition;

public class TPPLPoly : List<TPPLPoint>
{
    public bool IsHole { get; set; }

    public bool IsValid => Count >= 3;

    public TPPLPoly() : base()
    {
    }

    public TPPLPoly(IEnumerable<TPPLPoint> collection) : base(collection)
    {
    }
    public TPPLPoly(int capacity) : base(capacity)
    {
    }

    // Triangle init
    public TPPLPoly(TPPLPoint p1, TPPLPoint p2, TPPLPoint p3) : base(3)
    {
        Add(p1);
        Add(p2);
        Add(p3);
    }

    public TPPLPoly Copy() => new(this) { IsHole = IsHole };

    public float Area()
    {
        float area = 0;
        for (int i1 = 0; i1 < Count; i1++)
        {
            int i2 = (i1 + 1) % Count;
            area += this[i1].X * this[i2].Y - this[i1].Y * this[i2].X;
        }
        return area;
    }

    public TPPLOrientation GetOrientation()
    {
        var area = Area();
        if (area > 0) return TPPLOrientation.CCW;
        if (area < 0) return TPPLOrientation.CW;
        return TPPLOrientation.Collinear;
    }

    public void SetOrientation(TPPLOrientation orientation)
    {
        TPPLOrientation polyOrientation = GetOrientation();
        if (polyOrientation != TPPLOrientation.Collinear && polyOrientation != orientation)
            Reverse();
    }
}
