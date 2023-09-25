using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CachedValueUtils
{
	public static bool HasValueChanged<T>(T target, ref T cache) where T : IEquatable<T>
	{
		if ((target == null && cache != null) || (target != null && !target.Equals(cache)))
		{
			cache = target;
			return true;
		}
		else
		{
			return false;
		}
	}

	public static bool HasValueChanged<T>(T target, ref T cache, out T lastValue) where T : IEquatable<T>
	{
		if ((target == null && cache != null) || (target != null && !target.Equals(cache)))
		{
			lastValue = cache;
			cache = target;
			return true;
		}
		else
		{
			lastValue = target;
			return false;
		}
	}

	public static void ResetValueCache<T>(ref T cache)
	{
		cache = default;
	}

	public static bool HasCollectionChanged<T>(IReadOnlyCollection<T> target, ICollection<T> cache, ICollection<T> affected)
	{
		if (!target.SequenceEqual(cache))
		{
			affected.Clear();
			foreach (T t in target)
			{
				affected.Add(t);
			}
			foreach (T t in cache.Except(target))
			{
				affected.Add(t);
			}

			cache.Clear();
			foreach (T t in target)
			{
				cache.Add(t);
			}
			
			return true;
		}
		else
		{
			return false;
		}
	}

	public static void ResetCollectionCache<T>(ICollection<T> cache, ICollection<T> affected)
	{
		cache.Clear();
		affected.Clear();
	}
}
