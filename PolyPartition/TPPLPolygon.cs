
namespace PolyPartition;

public record TPPLPolygon(TPPLPoint[] Polygon, bool IsHole = false)
{
    public int Count => Polygon.Length;
    public TPPLPoint this[int index] => Polygon[index];
}

public class TPPLPolygonList : List<TPPLPolygon>
{
    public bool HasHoles { get; set; }

    public TPPLPolygonList() : base() { }
    public TPPLPolygonList(IEnumerable<TPPLPolygon> collection) : base(collection) { }
    public TPPLPolygonList(int capacity) : base(capacity) { }

    public List<TPPLPoint[]> ToPoints(bool deepCopy = false)
    {
        var polys = new List<TPPLPoint[]>(Count);
        foreach (var poly in this)
        {
            polys.Add(deepCopy ? [.. poly.Polygon] : poly.Polygon);
        }
        return polys;
    }

    public TPPLPolygonList Clone()
    {
        TPPLPolygonList polyList = new(Count) { HasHoles = HasHoles };
        foreach (var item in this)
            polyList.Add(new([.. item.Polygon], item.IsHole));
        return polyList;
    }

    public static TPPLPolygonList CreateByOrientation(ReadOnlySpan<TPPLPoint[]> inPolys, TPPLOrientation holeOrientation, bool positiveIsCCW = true, bool deepCopy = false)
    {
        var polys = new TPPLPolygonList(inPolys.Length);
        foreach (var inPoly in inPolys)
        {
            var orientation = TPPLUtil.GetOrientation(inPoly, positiveIsCCW);
            var poly = deepCopy ? [.. inPoly] : inPoly;
            if (orientation == holeOrientation)
            {
                polys.Add(new(poly, true));
                polys.HasHoles = true;
            }
            else
                polys.Add(new(poly, false));
        }
        return polys;
    }
}
