using System.Collections;
using System.Collections.Generic;
using Godot;

namespace ProceduralPlantTest;

[GlobalClass]
public abstract partial class PointProvider: Resource, IEnumerable<Vector3>
{
    [Signal] public delegate void PointsUpdatedEventHandler();
    public abstract List<Vector3> GetPoints();
    public IEnumerator<Vector3> GetEnumerator()
    {
        return GetPoints().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}