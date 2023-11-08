using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DicingHeros
{
	[CreateAssetMenu(fileName = "NewTileStyle", menuName = "Data/TileStyle", order = 1)]
	public class TileStyle : ScriptableObject
	{
		[System.Serializable]
		public class VisualSpriteEntry
		{
			public Tile.DisplayType type;
			public Color frameColor;
			public Color backgroundColor;
			public bool dashed;
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
		public Dictionary<Tile.DisplayType, Color> frameColors;
		[HideInInspector]
		public Dictionary<Tile.DisplayType, Color> backgroundColors;
		[HideInInspector]
		public Dictionary<Tile.DisplayType, bool> dashed;

		[SerializeField]
		protected List<PathDirectionSpriteEntry> pathDirecitonSpriteEntries = new List<PathDirectionSpriteEntry>();
		[HideInInspector]
		public Dictionary<Tile.PathDirections, Sprite> pathDirectionSprites;

		[UnityEngine.Serialization.FormerlySerializedAs("frameSprites")]
		public List<Sprite> frameSolidSprites = new List<Sprite>();
		public List<Sprite> frameDashedSprites = new List<Sprite>();

		public List<Sprite> frameMasks = new List<Sprite>();

		protected void OnEnable()
		{
			frameColors = new Dictionary<Tile.DisplayType, Color>();
			backgroundColors = new Dictionary<Tile.DisplayType, Color>();
			dashed = new Dictionary<Tile.DisplayType, bool>();
			foreach (VisualSpriteEntry entry in visualSpriteEntries)
			{
				frameColors[entry.type] = entry.frameColor;
				backgroundColors[entry.type] = entry.backgroundColor;
				dashed[entry.type] = entry.dashed;
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
