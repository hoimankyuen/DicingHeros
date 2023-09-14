using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CachedValueUtils
{
	public static bool HasValueChanged<T>(T target, ref T cache) where T : IEquatable<T>
	{
		if (!target.Equals(cache))
		{
			cache = target;
			return true;
		}
		else
		{
			return false;
		}
	}

	public static void ResetValueCache<T>(ref T cache)
	{
		cache = default;
	}

	public static bool HasCollectionChanged<T>(IReadOnlyCollection<T> target, ICollection<T> cache, ICollection<T> add, ICollection<T> remove)
	{
		if (!target.SequenceEqual(cache))
		{
			add.Clear();
			foreach (T t in target.Except(cache))
			{
				add.Add(t);
			}
			remove.Clear();
			foreach (T t in cache.Except(target))
			{
				remove.Add(t);
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

	public static void ResetCollectionCache<T>(ICollection<T> cache, ICollection<T> add, ICollection<T> remove)
	{
		cache.Clear();
		add.Clear();
		remove.Clear();
	}
}
