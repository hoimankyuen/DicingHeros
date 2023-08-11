using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DiceRoller
{
    public class Board : MonoBehaviour
    {
        // singleton
        public static Board Instance { get; protected set; }

        public float tileSize = 1f;
        public int boardSizeX = 1;
        public int boardSizeZ = 1;
        public GameObject tilePrefab = null;

        // working variables
        protected List<Tile> tiles = new List<Tile>();

        // ========================================================= Monobehaviour Methods =========================================================

        /// <summary>
        /// Awake is called when the game object was created. It is always called before start and is 
        /// independent of if the game object is active or not.
        /// </summary>
        void Awake()
        {
            Instance = this;
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).CompareTag("Tile"))
                {
                    if (transform.GetChild(i).gameObject.activeInHierarchy)
                    {
                        tiles.Add(transform.GetChild(i).GetComponent<Tile>());
                    }
                }
            }
        }

        /// <summary>
        /// Start is called before the first frame update and/or the game object is first active.
        /// </summary>
        void Start()
        {

        }

        /// <summary>
        /// Update is called once per frame.
        /// </summary>
        void Update()
        {

        }

        /// <summary>
        /// OnDestroy is called when the game object is destroyed.
        /// </summary>
        void OnDestroy()
        {
            Instance = null;
        }

        /// <summary>
        /// OnValidate is called when any inspector value is changed.
        /// </summary>
        void OnValidate()
        {
            
        }

        /// <summary>
        /// OnMouseEnter is called when the mouse is start pointing to the game object.
        /// </summary>
        void OnMouseEnter()
        {

        }

        /// <summary>
        /// OnMouseExit is called when the mouse is stop pointing to the game object.
        /// </summary>
        void OnMouseExit()
        {
        }

        /// <summary>
        /// OnMouseDown is called when a mouse button is pressed when pointing to the game object.
        /// </summary>
        void OnMouseDown()
        {

        }


        // ========================================================= Editor =========================================================

        /// <summary>
        /// Regenerate all components related to this board. Should only be called in editor.
        /// </summary>
        public void RegenerateBoard()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            List<List<Tile>> tileGrid = new List<List<Tile>>();
            for (int i = 0; i < boardSizeX; i++)
            {
                tileGrid.Add(new List<Tile>());
                for (int j = 0; j < boardSizeZ; j++)
                {
                    GameObject go = Instantiate(tilePrefab,
                        new Vector3(
                            (-(float)(boardSizeX - 1) / 2 + i) * tileSize,
                            0.001f,
                            (-(float)(boardSizeZ - 1) / 2 + j) * tileSize),
                        Quaternion.identity,
                        transform);
                    Tile tile = go.GetComponent<Tile>();
                    tile.tileSize = tileSize;
                    tile.RegenerateTile();
                    tileGrid[i].Add(tile);
                }
            }

            for (int i = 0; i < boardSizeX; i++)
            {
                for (int j = 0; j < boardSizeZ; j++)
                {
                    if (i > 0)
                        tileGrid[i][j].connectedTiles.Add(tileGrid[i - 1][j]);
                    if (i < boardSizeX - 1)
                        tileGrid[i][j].connectedTiles.Add(tileGrid[i + 1][j]);
                    if (j > 0)
                        tileGrid[i][j].connectedTiles.Add(tileGrid[i][j - 1]);
                    if (j < boardSizeZ - 1)
                        tileGrid[i][j].connectedTiles.Add(tileGrid[i][j + 1]);
                }
            }
        }

        // ========================================================= Behaviour =========================================================

        /// <summary>
        /// Get all tiles that an object is in.
        /// </summary>
        public List<Tile> GetCurrentTiles(Vector3 position, float size)
        {
            List<Tile> result = new List<Tile>();
            foreach (Tile tile in tiles)
            {
                if (tile.IsInTile(position, size))
                {
                    result.Add(tile);
                }
            }
            return result;
        }

        /// <summary>
        /// Find all tiles within a certain range of this tile, and return them in the supplied list.
        /// </summary>
        public List<Tile> GetTileWithinRange(List<Tile> startingTiles, int range)
        {
            List<Tuple<Tile, int>> search = new List<Tuple<Tile, int>>();
            List<Tile> result = new List<Tile>();
            foreach (Tile startingTile in startingTiles)
            {
                search.Add(new Tuple<Tile, int>(startingTile, range));
            }

            while (search.Count > 0)
            {
                Tuple<Tile, int> current = search[0];
                search.RemoveAt(0);
                result.Add(current.Item1);

                if (current.Item2 > 0)
                {
                    foreach (Tile connectedTile in current.Item1.connectedTiles)
                    {
                        if (!connectedTile.gameObject.activeInHierarchy)
                            continue;
                        if (result.Contains(connectedTile))
                            continue;
                        if (search.Exists(x => x.Item1 == connectedTile))
                            continue;

                        search.Add(new Tuple<Tile, int>(connectedTile, current.Item2 - 1));
                    }
                }
            }
            return result;
        }

        public List<Tile> GetShortestPath(Tile startingTile, Tile targetTile)
        {
            return GetShortestPath(new List<Tile>(new Tile[] { startingTile }), targetTile);
        }
        public List<Tile> GetShortestPath(List<Tile> startingTiles, Tile targetTile)
        {
            if (startingTiles.Contains(targetTile))
                return new List<Tile>(new Tile[] { targetTile });

            List<Tuple<List<Tile>, float>> search = new List<Tuple<List<Tile>, float>>();
            List<Tile> searched = new List<Tile>();

            foreach (Tile startingTile in startingTiles)
            {
                search.Add(new Tuple<List<Tile>, float>(new List<Tile>(new Tile[] { startingTile }), Vector3.Distance(startingTile.transform.position, targetTile.transform.position)));
            }

            while (search.Count > 0)
            {
                search.Sort((a, b) =>  a.Item2 > b.Item2 ? 1 : a.Item2 < b.Item2 ? -1 : 0);
                Tuple<List<Tile>, float> current = search[0];
                search.RemoveAt(0);
                searched.Add(current.Item1[current.Item1.Count - 1]);

                foreach (Tile connectedTile in current.Item1[current.Item1.Count - 1].connectedTiles)
                {
                    if (!connectedTile.gameObject.activeInHierarchy)
                        continue;
                    if (searched.Contains(connectedTile))
                        continue;
                    if (search.Exists(x => x.Item1[x.Item1.Count - 1] == connectedTile))
                        continue;

                    List<Tile> newList = new List<Tile>(current.Item1);
                    newList.Add(connectedTile);
                    if (connectedTile == targetTile)
                    {
                        return newList;
                    }
                    else
                    {
                        search.Add(new Tuple<List<Tile>, float>(newList, Vector3.Distance(connectedTile.transform.position, targetTile.transform.position)));
                    }
                }
            }
            return new List<Tile>();
        }
    }
}