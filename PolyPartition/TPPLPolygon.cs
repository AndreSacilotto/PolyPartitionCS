
namespace PolyPartition;

public record TPPLPolygon(TPPLPoint[] Polygon, bool IsHole = false)
{
    public int Count => Polygon.Length;
    public TPPLPoint this[int index] => Polygon[index];
    public TPPLPolygon CloneDeep()
    {
        var len = Polygon.Length;
        var poly = new TPPLPoint[len];
        Polygon.CopyTo(poly, 0);
        return new TPPLPolygon(poly, IsHole);
    }
}

public class TPPLPolygonList : List<TPPLPolygon>
{
    public bool HasHoles { get; set; }

    public TPPLPolygonList() : base() { }
    public TPPLPolygonList(IEnumerable<TPPLPolygon> collection) : base(collection) { }
    public TPPLPolygonList(int capacity) : base(capacity) { }

    public List<TPPLPoint[]> ToPoints()
    {
        var polys = new List<TPPLPoint[]>(Count);
        foreach (var poly in this)
            polys.Add(poly.Polygon);
        return polys;
    }
    public List<TPPLPoint[]> ToPointsDeep()
    {
        var polys = new List<TPPLPoint[]>(Count);
        foreach (var poly in this)
            polys.Add([.. poly.Polygon]);
        return polys;
    }
    public TPPLPolygonList Clone()
    {
        TPPLPolygonList polyList = new(Count) { HasHoles = HasHoles };
        foreach (var item in this)
            polyList.Add(new(item.Polygon, item.IsHole));
        return polyList;
    }
    public TPPLPolygonList CloneDeep()
    {
        TPPLPolygonList polyList = new(Count) { HasHoles = HasHoles };
        foreach (var item in this)
            polyList.Add(new([.. item.Polygon], item.IsHole));
        return polyList;
    }
}
