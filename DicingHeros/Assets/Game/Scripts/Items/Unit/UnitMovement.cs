using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DicingHeros
{
	public class UnitMovement
	{
		public List<Tile> startingTiles = new List<Tile>();
		public List<Tile> path = new List<Tile>();

		public UnitMovement(IReadOnlyCollection<Tile> startingTiles, IReadOnlyList<Tile> path)
		{
			this.startingTiles.AddRange(startingTiles);
			this.path.AddRange(path);
		}
	}
}