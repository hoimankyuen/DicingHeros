using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceRoller
{
	public class UnitMovement
	{
		public List<Tile> startingTiles;
		public List<Tile> path;

		public UnitMovement(IReadOnlyCollection<Tile> startingTiles, IReadOnlyList<Tile> path)
		{
			this.startingTiles = new List<Tile>();
			this.startingTiles.AddRange(startingTiles);

			this.path = new List<Tile>();
			this.path.AddRange(path);
		}
	}
}