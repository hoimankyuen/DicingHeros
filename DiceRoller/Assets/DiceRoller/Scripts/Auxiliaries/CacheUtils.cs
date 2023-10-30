using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CacheUtils
{
	// ========================================================= Cached Values =========================================================

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

	public static void ResetValueCache<T>(ref T cache) where T : IEquatable<T>
	{
		cache = default;
	}

	// ========================================================= Cached Enum =========================================================

	public static bool HasEnumChanged<T>(T target, ref T cache) where T : IConvertible
	{
		if (!typeof(T).IsEnum)
			throw new ArgumentException("T must be an enumerated type");

		if (target.ToInt32(null) != cache.ToInt32(null))
		{
			cache = target;
			return true;
		}
		else
		{
			return false;
		}
	}

	public static void ResetEnumCache<T>(ref T cache) where T : IConvertible
	{
		if (!typeof(T).IsEnum)
			throw new ArgumentException("T must be an enumerated type");

		cache = default;
	}

	// ========================================================= Cached Referencces =========================================================

	public static bool HasReferenceChanged(object target, object cache)
	{
		if ((target == null && cache != null) || (target != null && target != cache))
		{
			cache = target;
			return true;
		}
		else
		{
			return false;
		}
	}

	public static void ResetReferenceCache(object cache)
	{
		cache = null;
	}

	// ========================================================= Cached Collections =========================================================

	public static bool HasCollectionChanged<T>(IEnumerable<T> target, ICollection<T> cache)
	{
		if (!target.SequenceEqual(cache))
		{
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
	public static bool HasCollectionChanged<T>(IEnumerable<T> target, ICollection<T> cache, ICollection<T> affected)
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

	public static void ResetCollectionCache<T>(ICollection<T> cache)
	{
		cache.Clear();
	}
	public static void ResetCollectionCache<T>(ICollection<T> cache, ICollection<T> affected)
	{
		cache.Clear();
		affected.Clear();
	}
}
