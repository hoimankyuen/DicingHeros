using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Int2 : IEquatable<Int2>
{
    public int x;
    public int z;
    public Int2(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    public static Int2 zero { get { return new Int2(0, 0); } }
    public static Int2 one { get { return new Int2(1, 1); } }

    public static Int2 left { get { return new Int2(-1, 0); } }
    public static Int2 right { get { return new Int2(1, 0); } }
    public static Int2 backward { get { return new Int2(0, -1); } }
    public static Int2 forward { get { return new Int2(0, 1); } }

    public static Int2 MinValue { get { return new Int2(int.MinValue, int.MinValue); } }
    public static Int2 MaxValue { get { return new Int2(int.MaxValue, int.MaxValue); } }

    public static Int2 operator +(Int2 a, Int2 b)
    {
        return new Int2(a.x + b.x, a.z + b.z);
    }

    public static Int2 operator -(Int2 a, Int2 b)
    {
        return new Int2(a.x - b.x, a.z - b.z);
    }

    public static Int2 operator *(Int2 a, int b)
    {
        return new Int2(a.x * b, a.z * b);
    }

    public static Int2 operator /(Int2 a, int b)
    {
        if (b != 0)
            return new Int2(a.x / b, a.z / b);
        else
            throw new DivideByZeroException();

    }

    public static bool operator ==(Int2 a, Int2 b)
    {
        return a.x == b.x && a.z == b.z;
    }

    public static bool operator !=(Int2 a, Int2 b)
    {
        return a.x != b.x || a.z != b.z;
    }

    public override int GetHashCode()
    {
        return x ^ (z << 8);
    }

    public override bool Equals(object obj)
    {
        return obj is Int2 other && x == other.x && z == other.z;
    }

    public bool Equals(Int2 other)
    {
        return x == other.x && z == other.z;
    }

    public override string ToString()
    {
        return string.Format("Int2({0}, {1})", x, z);
    }

    public static int GridDistance(Int2 a, Int2 b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.z - b.z);
    }

    public static float Distance(Int2 a, Int2 b)
    {
        return Mathf.Sqrt((a.x - b.x) * (a.x - b.x )+ (a.z - b.z) * (a.z - b.z));
    }
}
