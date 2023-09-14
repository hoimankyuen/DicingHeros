using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceRoller
{
	[CreateAssetMenu(fileName = "NewTileStyle", menuName = "Data/TileStyle", order = 1)]
	public class TileStyle : ScriptableObject
	{
		[System.Serializable]
		public class VisualSpriteEntry
		{
			public Tile.DisplayType type;
			public Sprite sprite;
		}

		[System.Serializable]
		public class PathDirectionSpriteEntry
		{
			public Tile.PathDirections directions;
			public Sprite sprite;
		}

		[SerializeField]
		protected List<VisualSpriteEntry> visualSpriteEntries = new List<VisualSpriteEntry>();
		[HideInInspector]
		public Dictionary<Tile.DisplayType, Sprite> visualSprites;

		[SerializeField]
		protected List<PathDirectionSpriteEntry> pathDirecitonSpriteEntries = new List<PathDirectionSpriteEntry>();
		[HideInInspector]
		public Dictionary<Tile.PathDirections, Sprite> pathDirectionSprites;

		protected void OnEnable()
		{
			visualSprites = new Dictionary<Tile.DisplayType, Sprite>();
			foreach (VisualSpriteEntry entry in visualSpriteEntries)
			{
				visualSprites[entry.type] = entry.sprite;
			}

			pathDirectionSprites = new Dictionary<Tile.PathDirections, Sprite>();
			foreach (PathDirectionSpriteEntry entry in pathDirecitonSpriteEntries)
			{
				pathDirectionSprites[entry.directions] = entry.sprite;
				if (entry.directions.from != Tile.PathDirection.Start && entry.directions.from != Tile.PathDirection.End && 
					entry.directions.to != Tile.PathDirection.Start && entry.directions.to != Tile.PathDirection.End)
				{
					pathDirectionSprites[entry.directions.Invsersed()] = entry.sprite;
				}
			}
		}
	}
}
