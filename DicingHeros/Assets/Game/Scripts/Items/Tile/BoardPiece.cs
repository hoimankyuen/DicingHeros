using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DicingHeros
{
	public class BoardPiece : MonoBehaviour
	{
		// reference
		private GameController game => GameController.current;
		private Board board => Board.current;

		[Header("Board Piece Setup")]
		public Int2 boardPiecePos = Int2.zero;

		[Header("Tile Setup")]
		public GameObject tilePrefab = null;

		// ========================================================= Monobehaviour Methods =========================================================

		/// <summary>
		/// Awake is called when the game object was created. It is always called before start and is 
		/// independent of if the game object is active or not.
		/// </summary>
		private void Awake()
		{
			RetrieveAllTiles();
			RefreshTileConnections();
		}

		/// <summary>
		/// Start is called before the first frame update and/or the game object is first active.
		/// </summary>
		private void Start()
		{
		}

		/// <summary>
		/// Update is called once per frame.
		/// </summary>
		private void Update()
		{
		}

		/// <summary>
		/// OnDestroy is called when the game object is destroyed.
		/// </summary>
		private void OnDestroy()
		{
		}

		// ========================================================= Editor =========================================================

		#if UNITY_EDITOR
		
		/// <summary>
		/// Regenerate all tiles of this board. Should only be called in editor.
		/// </summary>
		public void RegenerateAllTiles()
		{
			// remove previous tiles
			for (int i = transform.childCount - 1; i >= 0; i--)
			{
				DestroyImmediate(transform.GetChild(i).gameObject);
			}

			// spawn new tiles
			Tile[,] tileGrid = new Tile[Board.boardPieceSize, Board.boardPieceSize];
			for (int i = 0; i < Board.boardPieceSize; i++)
			{
				for (int j = 0; j < Board.boardPieceSize; j++)
				{
					GameObject go = UnityEditor.PrefabUtility.InstantiatePrefab(tilePrefab, transform) as GameObject;
					go.transform.position = LocalBoardToWorld(new Int2(i, j));
					go.transform.localRotation = Quaternion.identity;
					Tile tile = go.GetComponent<Tile>();
					tile.boardPiece = this;
					tile.localBoardPos = new Int2(i, j);
					tile.RegenerateTile();

					tileGrid[i, j] = tile;
				}
			}
		}


		#endif


		/// <summary>
		/// Refresh all connections of the tiles of this board. SHould only be called in editor only.
		/// </summary>
		public void RefreshTileConnections()
		{
			// retrieve all tiles
			Tile[,] tileGrid = new Tile[Board.boardPieceSize, Board.boardPieceSize];
			Tile tile = null;
			for (int i = 0; i < transform.childCount - 1; i ++)
			{
				tile = transform.GetChild(i).GetComponent<Tile>();
				tileGrid[tile.localBoardPos.x, tile.localBoardPos.z] = tile;
			}

			// reconnect tiles
			for (int i = 0; i < Board.boardPieceSize; i++)
			{
				for (int j = 0; j < Board.boardPieceSize; j++)
				{
					// skip tile that does not exists
					if (tileGrid[i, j] != null)
					{
						// clear all connections first
						tileGrid[i, j].Connect(null, Int2.left);
						tileGrid[i, j].Connect(null, Int2.forward);
						tileGrid[i, j].Connect(null, Int2.right);
						tileGrid[i, j].Connect(null, Int2.backward);

						// reconnect all
						if (i > 0 && tileGrid[i - 1, j] != null)
							tileGrid[i, j].Connect(tileGrid[i - 1, j], Int2.left);

						if (j < Board.boardPieceSize - 1 && tileGrid[i, j + 1] != null)
							tileGrid[i, j].Connect(tileGrid[i, j + 1], Int2.forward);

						if (i < Board.boardPieceSize - 1 && tileGrid[i + 1, j] != null)
							tileGrid[i, j].Connect(tileGrid[i + 1, j], Int2.right);

						if (j > 0 && tileGrid[i, j - 1] != null)
							tileGrid[i, j].Connect(tileGrid[i, j - 1], Int2.backward);
					}
				}
			}
		}

		// ========================================================= Position Conversion =========================================================

		/// <summary>
		/// Convert any local board position to a world position.
		/// </summary>
		public Vector3 LocalBoardToWorld(Int2 localBoardPos)
		{
			return transform.TransformPoint(new Vector3(
				(localBoardPos.x - (Board.boardPieceSize - 1) / 2f) * Board.tileSize,
				0.001f,
				(localBoardPos.z - (Board.boardPieceSize - 1) / 2f) * Board.tileSize));
		}

		/// <summary>
		/// Convert any local world position to the nearest local board position.
		/// </summary>
		public Int2 WorldToLocalBoard(Vector3 localPos)
		{
			Vector3 localWorldPos = transform.InverseTransformPoint(localPos);
			return new Int2(
				Convert.ToInt32(localWorldPos.x / Board.tileSize + (float)(Board.boardPieceSize - 1) / 2),
				Convert.ToInt32(localWorldPos.z / Board.tileSize + (float)(Board.boardPieceSize - 1) / 2));
		}

		/// <summary>
		/// Convert a boardPos from global to local of this board piece.
		/// </summary>
		public Int2 ToLocalBoard(Int2 globalPos)
		{
			return globalPos - boardPiecePos * Board.boardPieceSize;
		}

		/// <summary>
		/// Convert a boardPos from local of this board piece to global.
		/// </summary>
		public Int2 ToGlobalBoard(Int2 localPos)
		{
			return localPos + boardPiecePos * Board.boardPieceSize;
		}

		// ========================================================= Tiles =========================================================

		/// <summary>
		/// A collection of all tiles this board piece contains.
		/// </summary>
		public IReadOnlyList<Tile> Tiles
		{	
			get
			{
				return _Tiles;
			}
		}
		private readonly List<Tile> _Tiles = new List<Tile>();

		/// <summary>
		/// Retrieve all available tiles of this board.
		/// </summary>
		private void RetrieveAllTiles()
		{
			for (int i = 0; i < transform.childCount; i++)
			{
				if (transform.GetChild(i).gameObject.activeInHierarchy)
				{
					Tile tile = transform.GetChild(i).GetComponent<Tile>();
					if (tile != null)
					{
						_Tiles.Add(tile);
					}
				}
			}
		}

		/// <summary>
		/// Update the world pos and board pos of the tiles of this board piece. Should be done after connected to a board.
		/// </summary>
		public void UpateAllTilesPositions()
		{
			foreach (Tile tile in Tiles)
			{
				tile.UpdateWordPos();
				tile.UpdateBoardPos();
			}
		}
	}
}
