
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

    public List<TPPLPoint[]> DeepToPoints()
    {
        var polys = new List<TPPLPoint[]>(Count);
        foreach (var poly in this)
            polys.Add([.. poly.Polygon]);
        return polys;
    }
    public List<TPPLPoint[]> ToPoints()
    {
        var polys = new List<TPPLPoint[]>(Count);
        foreach (var poly in this)
            polys.Add(poly.Polygon);
        return polys;
    }

    public TPPLPolygonList DeepClone()
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

    /// <param name="outerOrientation">hole orientation will be the opposite, pass <see cref="TPPLOrientation.None"/> to skip</param>
    public static TPPLPolygonList CreateAndSetOrientation(ReadOnlySpan<TPPLPoint[]> outerPolys, TPPLOrientation outerOrientation, ReadOnlySpan<TPPLPoint[]> holesPolys, bool positiveIsCCW = true, bool deepCopy = false)
    {
        var polys = new TPPLPolygonList(outerPolys.Length + holesPolys.Length);

        foreach (var poly in outerPolys)
        {
            var p = deepCopy ? [.. poly] : poly;
            if (outerOrientation != TPPLOrientation.None)
                TPPLUtil.SetOrientation(p, outerOrientation, positiveIsCCW);
            polys.Add(new(poly, false));
        }

        var holeOrientation = (TPPLOrientation)(-(int)outerOrientation);
        foreach (var poly in holesPolys)
        {
            var p = deepCopy ? [.. poly] : poly;
            if (holeOrientation != TPPLOrientation.None)
                TPPLUtil.SetOrientation(poly, holeOrientation, positiveIsCCW);
            polys.Add(new(poly, true));
        }

        return polys;
    }
}
