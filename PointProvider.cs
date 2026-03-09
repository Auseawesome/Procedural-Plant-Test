using System.Collections;
using System.Collections.Generic;
using Godot;

namespace ProceduralPlantTest;

[GlobalClass]
public abstract partial class PointProvider: Resource, IEnumerable<Point>
{
    [Signal] public delegate void PointsUpdatedEventHandler();
    public abstract List<Point> GetPoints();
    
    // Default implementation finds the max and minimum positions, inefficient but works in every case
    public virtual Aabb GetBounds()
    {
        var minPos = Vector3.Zero;
        var maxPos = Vector3.Zero;
        foreach (var point in GetPoints())
        {
            minPos = minPos.Min(point.Position);
            maxPos = maxPos.Max(point.Position);
        }
        
        return new Aabb(minPos, maxPos-minPos);
    }
    public IEnumerator<Point> GetEnumerator()
    {
        return GetPoints().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}