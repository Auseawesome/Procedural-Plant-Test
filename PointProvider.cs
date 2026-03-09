using System.Collections;
using System.Collections.Generic;
using Godot;

namespace ProceduralPlantTest;

[GlobalClass]
public abstract partial class PointProvider: Resource, IEnumerable<Point>
{
    [Signal] public delegate void PointsUpdatedEventHandler();
    public abstract List<Point> GetPoints();
    public IEnumerator<Point> GetEnumerator()
    {
        return GetPoints().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}