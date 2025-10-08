
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
    public bool HasHoles { get; set; } = false;

    public TPPLPolygonList() : base() { }
    public TPPLPolygonList(IEnumerable<TPPLPolygon> collection) : base(collection) { }
    public TPPLPolygonList(int capacity) : base(capacity) { }

    public void AddOuter(TPPLPoint[] polygon)
    {
        if (polygon.Length >= 3) 
            Add(new(polygon, false));
    }

    public void AddHole(TPPLPoint[] polygon)
    {
        if (polygon.Length >= 3)
        {
            Add(new(polygon, true));
            HasHoles = true;
        }
    }

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

    /// <param name="deepCopy"><see cref="TPPLPartition.RemoveHoles"/> will alter it <see cref="TPPLPolygonList"/>, but not the array</param>
    public static TPPLPolygonList CreateByOrientation(ReadOnlySpan<TPPLPoint[]> inPolys, TPPLOrientation orientation, bool deepCopy = false)
    {
        var polys = new TPPLPolygonList(inPolys.Length);
        foreach (var inPoly in inPolys)
        {
            if (inPoly.Length < 3)
                continue;

            var winding = orientation.GetOrientation(inPoly);
            var poly = deepCopy ? [.. inPoly] : inPoly;
            if (winding == orientation.Inner)
            {
                polys.Add(new(poly, true));
                polys.HasHoles = true;
            }
            else
                polys.Add(new(poly, false));
        }
        return polys;
    }

    /// <param name="outerOrientation">pass <see cref="null"/> to skip</param>
    /// <param name="deepCopy"><see cref="TPPLPartition.RemoveHoles"/> will alter it <see cref="TPPLPolygonList"/>, but not the array</param>
    public static TPPLPolygonList CreateAndSetOrientation(ReadOnlySpan<TPPLPoint[]> outerPolys, ReadOnlySpan<TPPLPoint[]> holesPolys, TPPLOrientation? orientation, bool deepCopy = false)
    {
        var polys = new TPPLPolygonList(outerPolys.Length + holesPolys.Length) {
            HasHoles = holesPolys.Length > 0
        };

        foreach (var poly in outerPolys)
        {
            var p = deepCopy ? [.. poly] : poly;
            orientation?.SetOrientation(p, orientation.Outer);
            polys.Add(new(poly, false));
        }

        foreach (var poly in holesPolys)
        {
            var p = deepCopy ? [.. poly] : poly;
            orientation?.SetOrientation(p, orientation.Inner);
            polys.Add(new(poly, true));
        }

        return polys;
    }
}
