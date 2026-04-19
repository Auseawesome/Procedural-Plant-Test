using System.Collections;
using System.Collections.Generic;
using Godot;

namespace ProceduralPlantTest.Provider;

[GlobalClass]
public abstract partial class PointProvider: Node, IEnumerable<Point>
{
    [Signal] public delegate void PointsUpdatedEventHandler();
    public abstract List<Point> GetPoints();
    
    public abstract Aabb GetBounds();
    
    public IEnumerator<Point> GetEnumerator()
    {
        return GetPoints().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}