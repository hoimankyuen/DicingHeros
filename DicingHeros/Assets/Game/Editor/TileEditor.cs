using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DicingHeros
{
    //[CustomEditor(typeof(Tile))]
    public class TileEditor : Editor
    {
        protected void OnSceneGUI()
        {
            Tile tile = target as Tile;

            if (tile == null || tile.gameObject == null)
                return;

            Handles.DrawWireCube(tile.transform.position, Vector3.one * 0.1f);
        }
    }
}