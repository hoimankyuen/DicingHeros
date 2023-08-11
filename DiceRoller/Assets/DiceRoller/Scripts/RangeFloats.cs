using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct RangeFloat2
{
	public float min;
	public float max;

	public static RangeFloat2 zero => new RangeFloat2(0, 0);

	public static RangeFloat2 norm => new RangeFloat2(-1, 1);

	public static RangeFloat2 operator +(RangeFloat2 a, float b) => new RangeFloat2(a.min + b, a.max + b);

	public static RangeFloat2 operator -(RangeFloat2 a, float b) => new RangeFloat2(a.min - b, a.max - b);

	public static RangeFloat2 operator *(RangeFloat2 a, float b) => new RangeFloat2(a.min * b, a.max * b);

	public static RangeFloat2 operator /(RangeFloat2 a, float b)
	{
		if (b == 0)
		{
			throw new DivideByZeroException();
		}
		return new RangeFloat2(a.min / b, a.max / b);
	}

	public static RangeFloat2 operator +(RangeFloat2 a, RangeFloat2 b) => new RangeFloat2(a.min + b.min, a.max + b.max);

	public static RangeFloat2 operator -(RangeFloat2 a, RangeFloat2 b) => new RangeFloat2(a.min - b.min, a.max - b.max);

	public static bool operator ==(RangeFloat2 a, RangeFloat2 b) => a.Equals(b);

	public static bool operator !=(RangeFloat2 a, RangeFloat2 b) => !a.Equals(b);


	public RangeFloat2(float min, float max)
	{
		this.min = min;
		this.max = max;
	}

	public float Lerp(float t)
	{
		return Mathf.Lerp(min, max, t);
	}

	public float InverseLerp(float value)
	{
		return Mathf.InverseLerp(min, max, value);
	}

	public float Clamp(float value)
	{
		return Mathf.Clamp(value, min, max);
	}

	public bool IsInclude(float value)
	{
		return value >= min && value <= max;
	}

	public override string ToString() => $"RangeFloat2({min}, {max})";

	public override bool Equals(object obj)
	{
		if (!(obj is RangeFloat2 other))
			return false;

		return min == other.min && max == other.max;
	}

	public override int GetHashCode()
	{
		var hashCode = -897720056;
		hashCode = hashCode * -1521134295 + min.GetHashCode();
		hashCode = hashCode * -1521134295 + max.GetHashCode();
		return hashCode;
	}
}

[System.Serializable]
public struct RangeFloat3
{
	public float min;
	public float mid;
	public float max;


	public static RangeFloat3 zero => new RangeFloat3(0, 0, 0);

	public static RangeFloat3 norm => new RangeFloat3(-1, 0, 1);

	public static RangeFloat3 operator +(RangeFloat3 a, float b) => new RangeFloat3(a.min + b, a.mid + b, a.max + b);

	public static RangeFloat3 operator -(RangeFloat3 a, float b) => new RangeFloat3(a.min - b, a.mid - b, a.max - b);

	public static RangeFloat3 operator *(RangeFloat3 a, float b) => new RangeFloat3(a.min * b, a.mid * b, a.max * b);

	public static RangeFloat3 operator /(RangeFloat3 a, float b)
	{
		if (b == 0)
		{
			throw new DivideByZeroException();
		}
		return new RangeFloat3(a.min / b, a.mid / b, a.max / b);
	}

	public static RangeFloat3 operator +(RangeFloat3 a, RangeFloat3 b) => new RangeFloat3(a.min + b.min, a.mid + b.mid, a.max + b.max);

	public static RangeFloat3 operator -(RangeFloat3 a, RangeFloat3 b) => new RangeFloat3(a.min - b.min, a.mid - b.mid, a.max - b.max);

	public static bool operator ==(RangeFloat3 a, RangeFloat3 b) => a.Equals(b);

	public static bool operator !=(RangeFloat3 a, RangeFloat3 b) => !a.Equals(b);

	public RangeFloat3(float min, float mid, float max)
	{
		this.min = min;
		this.mid = mid;
		this.max = max;
	}

	public float Lerp(float t)
	{
		if (t < 0.5f)
		{
			return Mathf.Lerp(min, mid, t);
		}
		else
		{
			return Mathf.Lerp(mid, max, t);
		}
	}

	public float InverseLerp(float value)
	{
		if (value < mid)
		{
			return Mathf.InverseLerp(min, mid, value) / 2f;
		}
		else
		{
			return Mathf.InverseLerp(mid, max, value) / 2f + 0.5f;
		}
	}

	public float Clamp(float value)
	{
		return Mathf.Clamp(value, min, max);
	}

	public bool IsInclude(float value)
	{
		return value >= min && value <= max;
	}

	public override string ToString() => $"RangeFloat3({min}, {mid}, {max})";

	public override bool Equals(object obj)
	{
		if (!(obj is RangeFloat3 other))
			return false;

		return min == other.min && mid == other.mid && max == other.max;
	}

	public override int GetHashCode()
	{
		var hashCode = -897720056;
		hashCode = hashCode * -1521134295 + min.GetHashCode();
		hashCode = hashCode * -1521134295 + mid.GetHashCode();
		hashCode = hashCode * -1521134295 + max.GetHashCode();
		return hashCode;
	}
}


[System.Serializable]
public struct RangeFloat4
{
	public float min;
	public float midl;
	public float midg;
	public float max;

	public static RangeFloat4 zero => new RangeFloat4(0, 0, 0, 0);

	public static RangeFloat4 norm => new RangeFloat4(-1, -1f / 3f, 1f / 3f, 1);

	public static RangeFloat4 operator +(RangeFloat4 a, float b) => new RangeFloat4(a.min + b, a.midl + b, a.midg + b, a.max + b);

	public static RangeFloat4 operator -(RangeFloat4 a, float b) => new RangeFloat4(a.min - b, a.midl - b, a.midg - b, a.max - b);

	public static RangeFloat4 operator *(RangeFloat4 a, float b) => new RangeFloat4(a.min * b, a.midl * b, a.midg * b, a.max * b);

	public static RangeFloat4 operator /(RangeFloat4 a, float b)
	{
		if (b == 0)
		{
			throw new DivideByZeroException();
		}
		return new RangeFloat4(a.min / b, a.midl / b, a.midg / b, a.max / b);
	}

	public static RangeFloat4 operator +(RangeFloat4 a, RangeFloat4 b) => new RangeFloat4(a.min + b.min, a.midl + b.midl, a.midg + b.midg, a.max + b.max);

	public static RangeFloat4 operator -(RangeFloat4 a, RangeFloat4 b) => new RangeFloat4(a.min - b.min, a.midl - b.midl, a.midg - b.midg, a.max - b.max);

	public static bool operator ==(RangeFloat4 a, RangeFloat4 b) => a.Equals(b);

	public static bool operator !=(RangeFloat4 a, RangeFloat4 b) => !a.Equals(b);

	public RangeFloat4(float min, float midl, float midg, float max)
	{
		this.min = min;
		this.midl = midl;
		this.midg = midg;
		this.max = max;
	}

	public float Lerp(float t)
	{
		if (t < 0.3333f)
		{
			return Mathf.Lerp(min, midl, t);
		}
		else if (t < 0.6667f)
		{
			return Mathf.Lerp(midl, midg, t);
		}
		else
		{
			return Mathf.Lerp(midg, max, t);
		}
	}

	public float InverseLerp(float value)
	{
		if (value < midl)
		{
			return Mathf.InverseLerp(min, midl, value) / 3f;
		}
		else if (value < midg)
		{
			return Mathf.InverseLerp(midl, midg, value) / 3f + 1f / 3f;
		}
		else
		{
			return Mathf.InverseLerp(midg, max, value) / 3f + 2f / 3f;
		}
	}

	public float Clamp(float value)
	{
		return Mathf.Clamp(value, min, max);
	}

	public bool IsInclude(float value)
	{
		return value >= min && value <= max;
	}

	public override string ToString() => $"RangeFloat4({min}, {midl}, {midg}, {max})";

	public override bool Equals(object obj)
	{
		if (!(obj is RangeFloat4 other))
			return false;

		return min == other.min && midl == other.midl && midg == other.midg && max == other.max;
	}

	public override int GetHashCode()
	{
		var hashCode = -897720056;
		hashCode = hashCode * -1521134295 + min.GetHashCode();
		hashCode = hashCode * -1521134295 + midl.GetHashCode();
		hashCode = hashCode * -1521134295 + midg.GetHashCode();
		hashCode = hashCode * -1521134295 + max.GetHashCode();
		return hashCode;
	}
}
