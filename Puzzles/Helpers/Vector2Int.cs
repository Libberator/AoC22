using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace System.Numerics;

/// <summary>
/// Created this to be useful for Grids, when floating point arithmetic could be a bad idea
/// </summary>
public struct Vector2Int : IEquatable<Vector2Int>, IFormattable
{
    public int X;
    public int Y;

    /// <summary>Creates a new Vector2Int object whose two elements have the same value</summary>
    public Vector2Int(int value)
    {
        X = value;
        Y = value;
    }
    /// <summary>Creates a vector whose elements have the specified values.</summary>
    public Vector2Int(int x, int y)
    {
        X = x;
        Y = y;
    }

    /// <summary>Gets or sets the element at the specified index (0 = X, 1 = Y).</summary>
    public int this[int index]
    {
        get => index switch
        {
            0 => X,
            1 => Y,
            _ => throw new ArgumentOutOfRangeException($"{index} is not a valid index for Vector2Int")
        };
        set
        {
            switch (index)
            {
                case 0: X = value; break;
                case 1: Y = value; break;
                default: throw new ArgumentOutOfRangeException($"{index} is not a valid index for Vector2Int");
            }
        }
    }

    #region Static Properties

    /// <summary>Gets the vector (1,0).</summary>
    public static readonly Vector2Int Right = new(1, 0);
    /// <summary>Gets the vector (0,1).</summary>
    public static readonly Vector2Int Up = new(0, 1);
    /// <summary>Gets the vector (-1,0).</summary>
    public static readonly Vector2Int Left = new(-1, 0);
    /// <summary>Gets the vector (0,-1).</summary>
    public static readonly Vector2Int Down = new(0, -1);
    ///<summary>A vector whose two elements are equal to one (that is, it returns the vector (1, 1)</summary>
    public static readonly Vector2Int One = new(1, 1);
    ///<summary>A vector whose two elements are equal to zero (that is, it returns the vector (0, 0)</summary>
    public static readonly Vector2Int Zero = new(0, 0);
    /// <summary>Returns the four cardinal directions N, E, S, W in that order.</summary>
    public static readonly Vector2Int[] CompassDirections = new Vector2Int[4] { Up, Right, Down, Left };

    #endregion

    #region Non-Static Methods

    /// <summary>Returns the length of the vector.</summary>
    public readonly double Length() => Math.Sqrt(LengthSquared());
    /// <summary>Returns the length of the vector squared.</summary>
    public readonly int LengthSquared() => X * X + Y * Y;
    /// <summary>Resets the vector to the origin (0, 0).</summary>
    public void Reset() { X = 0; Y = 0; }

    #endregion

    #region Interface and Overrides

    /// <summary>Returns a value that indicates whether this instance and another vector are equal.</summary>
    public readonly bool Equals(Vector2Int other) => this == other;
    /// <summary>Returns a value that indicates whether this instance and another vector are equal.</summary>
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is Vector2Int other && Equals(other);
    /// <summary>Returns the hash code for this instance.</summary>
    public override int GetHashCode() => ToString().GetHashCode();
    /// <summary>
    /// Returns the string representation of the current instance using the specified
    /// format string to format individual elements and the specified format provider
    /// to define culture-specific formatting.
    /// </summary>
    public string ToString([StringSyntax("NumericFormat")] string? format, IFormatProvider? formatProvider) => $"{X.ToString(format, formatProvider)}, {Y.ToString(format, formatProvider)}";
    /// <summary>Returns the string representation of the current instance using default formatting: "X,Y".</summary>
    public readonly override string ToString() => $"{X}, {Y}";
    /// <summary>Returns the string representation of the current instance using the specified format string to format individual elements.</summary>
    public readonly string ToString([StringSyntax("NumericFormat")] string? format) => $"{X.ToString(format)}, {Y.ToString(format)}";

    #endregion

    #region Static Methods

    /// <summary>Adds two vectors together.</summary>
    public static Vector2Int Add(Vector2Int left, Vector2Int right) => left + right;
    /// <summary>Divides the first vector by the second.</summary>
    public static Vector2Int Divide(Vector2Int left, Vector2Int right) => left / right;
    /// <summary>Divides the specified vector by a specified scalar value.</summary>
    public static Vector2Int Divide(Vector2Int left, int divisor) => left / divisor;
    /// <summary>Returns the dot product of two vectors.</summary>
    public static int Dot(Vector2Int a, Vector2Int b) => a.X * b.X + a.Y * b.Y;
    /// <summary>Returns a vector whose elements are the maximum of each of the pairs of elements in two specified vectors.</summary>
    public static Vector2Int Max(Vector2Int a, Vector2Int b) => new(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
    /// <summary>Returns a vector whose elements are the minimum of each of the pairs of elements in two specified vectors.</summary>
    public static Vector2Int Min(Vector2Int a, Vector2Int b) => new(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
    /// <summary>Multiplies a scalar value by a specified vector.</summary>
    public static Vector2Int Multiply(int left, Vector2Int right) => left * right;
    /// <summary>Multiplies a vector by a specified scalar.</summary>
    public static Vector2Int Multiply(Vector2Int left, int right) => left * right;
    /// <summary>Returns a new vector whose values are the product of each pair of elements in two specified vectors.</summary>
    public static Vector2Int Multiply(Vector2Int left, Vector2Int right) => left * right;
    /// <summary>Subtracts the second vector from the first.</summary>
    public static Vector2Int Subtract(Vector2Int left, Vector2Int right) => left - right;

    #endregion

    #region Operators

    /// <summary>Adds two vectors together.</summary>
    public static Vector2Int operator +(Vector2Int left, Vector2Int right) => new(left.X + right.X, left.Y + right.Y);
    /// <summary>Negates the specified vector.</summary>
    public static Vector2Int operator -(Vector2Int value) => new(-value.X, -value.Y);
    /// <summary>Subtracts the second vector from the first.</summary>
    public static Vector2Int operator -(Vector2Int left, Vector2Int right) => new(left.X - right.X, left.Y - right.Y);
    /// <summary>Returns the pair-wise multiple of the two vectors</summary>
    public static Vector2Int operator *(Vector2Int left, Vector2Int right) => new(left.X * right.X, left.Y * right.Y);
    /// <summary>Multiples the scalar value by the specified vector.</summary>
    public static Vector2Int operator *(int left, Vector2Int right) => new(left * right.X, left * right.Y);
    /// <summary>Multiples the scalar value by the specified vector.</summary>
    public static Vector2Int operator *(Vector2Int left, int right) => new(left.X * right, left.Y * right);
    /// <summary>Divides the first vector by the second pair-wise using integer division.</summary>
    public static Vector2Int operator /(Vector2Int left, Vector2Int right) => new(left.X / right.X, left.Y / right.Y);
    /// <summary>Divides the specified vector by a specified scalar value using integer division.</summary>
    public static Vector2Int operator /(Vector2Int value, int divisor) => new(value.X / divisor, value.Y / divisor);
    ///<summary>Returns a value that indicates whether each pair of elements in two specified vectors is equal.</summary>     
    public static bool operator ==(Vector2Int left, Vector2Int right) => left.X == right.X && left.Y == right.Y;
    ///<summary>Returns a value that indicates whether two specified vectors are not equal.</summary>     
    public static bool operator !=(Vector2Int left, Vector2Int right) => left.X != right.X || left.Y != right.Y;

    #endregion
}

public static class Vector2IntExtensions
{
    /// <summary>Returns a vector whose elements are the absolute values of each of the specified vector's elements.</summary>
    public static Vector2Int Abs(this Vector2Int value) => new(Math.Abs(value.X), Math.Abs(value.Y));
    /// <summary>Restricts a vector between a minimum and a maximum value.</summary>
    public static Vector2Int Clamp(this Vector2Int value, Vector2Int min, Vector2Int max) => value.Clamp(min.X, max.X, min.Y, max.Y);
    /// <summary>Restricts a vector between a minimum and a maximum value.</summary>
    public static Vector2Int Clamp(this Vector2Int value, int minX, int maxX, int minY, int maxY) => new(Math.Clamp(value.X, minX, maxX), Math.Clamp(value.Y, minY, maxY));
    /// <summary>Computes the Euclidian distance between the two given points.</summary>
    public static double Distance(this Vector2Int a, Vector2Int b) => Math.Sqrt(DistanceSquared(a, b));
    /// <summary>Computes the Chebyshev distance, also known as chessboard distance - the amount of moves a king would take to get from a to b.</summary>
    public static int DistanceChebyshev(this Vector2Int a, Vector2Int b) => Math.Max(Math.Abs(b.X - a.X), Math.Abs(b.Y - a.Y));
    /// <summary>Computes the Manhattan distance between the two given points. No diagonal moves.</summary>
    public static int DistanceManhattan(this Vector2Int a, Vector2Int b) => Math.Abs(b.X - a.X) + Math.Abs(b.Y - a.Y);
    /// <summary>Returns the Euclidean distance squared between two specified points.</summary>
    public static int DistanceSquared(this Vector2Int a, Vector2Int b) => (b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y);
    /// <summary>Assuming no obstacles or difference in cost between the points, this returns a predictable path.</summary>
    public static IEnumerable<Vector2Int> GetChebyshevPathTo(this Vector2Int from, Vector2Int to)
    {
        while (from != to)
        {
            yield return from;
            from.X += from.X < to.X ? 1 : from.X > to.X ? -1 : 0;
            from.Y += from.Y < to.Y ? 1 : from.Y > to.Y ? -1 : 0;
        }
        yield return to;
    }
    /// <summary>Returns a value that indicates if two vectors are next to each other laterally.</summary>
    public static bool IsAdjacentTo(this Vector2Int a, Vector2Int b) => (a.X == b.X && Math.Abs(a.Y - b.Y) == 1) || (a.Y == b.Y && Math.Abs(a.X - b.X) == 1);
    /// <summary>Returns a value that indicates if two vectors are next to each other diagonally.</summary>
    public static bool IsDiagonalTo(this Vector2Int a, Vector2Int b) => Math.Abs(a.X - b.X) == 1 && Math.Abs(a.Y - b.Y) == 1;
    /// <summary>Horizontally or Vertically aligned at any distance, but not the same position</summary>
    public static bool IsLateralTo(this Vector2Int a, Vector2Int b) => a.X == b.X ^ a.Y == b.Y;
    /// <summary>Returns a value that indicates if two vectors are parralel and neither is Vector2Int.Zero</summary>
    public static bool IsParallelTo(this Vector2Int a, Vector2Int b) => a != Vector2Int.Zero && b != Vector2Int.Zero && a.X * b.Y == a.Y * b.X;
    /// <summary>Returns a value that indicates if two vectors are perpendicular and neither is Vector2Int.Zero</summary>
    public static bool IsPerpendicularTo(this Vector2Int a, Vector2Int b) => a != Vector2Int.Zero && b != Vector2Int.Zero && a.X * b.X == -a.Y * b.Y;
    /// <summary>Performs a linear interpolation between two vectors based on the given weighting (0f to 1f).</summary>
    public static Vector2Int Lerp(this Vector2Int from, Vector2Int to, float weight)
    {
        weight = Math.Clamp(weight, 0f, 1f);
        var x = (int)Math.Round(from.X + (to.X - from.X) * weight);
        var y = (int)Math.Round(from.Y + (to.Y - from.Y) * weight);
        return new(x, y);
    }
    /// <summary>Returns a negative copy of a specified vector.</summary>
    public static Vector2Int Negate(this Vector2Int value) => -value;
    /// <summary>Rotates a Vector2Int clockwise 90° from the perspective of the standard XY grid.</summary>
    public static Vector2Int RotateRight(this Vector2Int value) => new(value.Y, -value.X);
    /// <summary>Rotates a Vector2Int counter-clockwise 90° from the perspective of the standard XY grid.</summary>
    public static Vector2Int RotateLeft(this Vector2Int value) => new(-value.Y, value.X);
}