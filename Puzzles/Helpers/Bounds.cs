﻿using System;
using System.Numerics;

namespace AoC22;

public struct Bounds
{
    public int XMin { get; private set; }
    public int XMax { get; private set; }
    public int YMin { get; private set; }
    public int YMax { get; private set; }

    public Bounds() : this(0, 0, 0, 0) { }

    public Bounds(Vector2Int point) : this(point.X, point.X, point.Y, point.Y) { }
    
    public Bounds(int xMin, int xMax, int yMin, int yMax)
    {
        XMin = xMin;
        XMax = xMax;
        YMin = yMin;
        YMax = yMax;
    }
    
    public Bounds(Vector2Int center, Vector2Int extents)
    {
        XMin = center.X - extents.X;
        XMax = center.X + extents.X;
        YMin = center.Y - extents.Y;
        YMax = center.Y + extents.Y;
    }

    /// <summary>
    /// Returns the center point. If the *number of points* on a side is even (even though Size will be odd),
    /// it rounds down towards lower-left corner.  e.g. If XMin = 1, and XMax = 4, it will return 2 for the XCenter
    /// </summary>
    public Vector2Int Center => Min + Extents;
    /// <summary>
    /// The extents of the Bounding Box. This is always half of the size of the Bounds. 
    /// Note: Integer division rounds down if Width or Height is odd.
    /// </summary>
    public Vector2Int Extents => Size / 2;
    /// <summary>Top-right most point.</summary>
    public Vector2Int Max => new(XMax, YMax);
    /// <summary>Bottom-left most point.</summary>
    public Vector2Int Min => new(XMin, YMin);
    /// <summary>The total size of the bounding box. Note: This is not the number of distinct points along the edges, but rather the space between.</summary>
    public Vector2Int Size => new(Width, Height);
    /// <summary>Difference between XMin and XMax. e.g. If XMin = 1, and XMax = 4, it will return 3 even though there are 4 points contained.</summary>
    public int Width => XMax - XMin;
    /// <summary>Difference between YMin and YMax. e.g. If YMin = 1, and YMax = 4, it will return 3 even though there are 4 points contained.</summary>
    public int Height => YMax - YMin;
    /// <summary>Returns a value to indicate if a point is within the bounding box.</summary>
    public bool Contains(Vector2Int pos) => Contains(pos.X, pos.Y);
    /// <summary>Returns a value to indicate if a point is within the bounding box.</summary>
    public bool Contains(int x, int y) => IsInHorizontalBounds(x) && IsInVerticalBounds(y);
    /// <summary>Grows the Bounds to include the point.</summary>
    public void Encapsulate(Vector2Int point)
    {
        XMin = Math.Min(XMin, point.X);
        XMax = Math.Max(XMax, point.X);
        YMin = Math.Min(YMin, point.Y);
        YMax = Math.Max(YMax, point.Y);
    }
    /// <summary>Expand the bounds by increasing its size by amount along each side.</summary>
    public void Expand(int amount) => Expand(amount, amount);
    /// <summary>Expand the bounds by increasing its size by each amount along their respective side.</summary>
    public void Expand(int xAmount, int yAmount)
    {
        XMin -= xAmount;
        XMax += xAmount;
        YMin -= yAmount;
        YMax += yAmount;
    }
    /// <summary>Returns a value to indicate if another bounding box intersects or shares an edge with this bounding box.</summary>
    public bool Overlaps(Bounds other) => !(other.XMin > XMax || other.XMax < XMin || other.YMin > YMax || other.YMax < YMin);
    /// <summary>Sets the bounds to the min and max value of the box.</summary>
    public void SetMinMax(Vector2Int min, Vector2Int max)
    {
        XMin = min.X;
        XMax = max.X;
        YMin = min.Y;
        YMax = max.Y;
    }

    public bool IsInHorizontalBounds(int x) => x >= XMin && x <= XMax;
    public bool IsInVerticalBounds(int y) => y >= YMin && y <= YMax;
    //public bool HasOvershot(Vector2Int pos) => pos.X > XMax || pos.Y > YMax;
    //public bool HasUndershot(Vector2Int pos) => pos.X < XMin || pos.Y < YMin;
}