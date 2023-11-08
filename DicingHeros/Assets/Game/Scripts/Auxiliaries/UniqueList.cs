using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class UniqueList<T> : List<T>
{
	public new void Add(T item)
	{
		if (!this.Contains(item))
			base.Add(item);
	}

	public new void AddRange(IEnumerable<T> collection)
	{
		base.AddRange(collection.Except(this));
	}
}
