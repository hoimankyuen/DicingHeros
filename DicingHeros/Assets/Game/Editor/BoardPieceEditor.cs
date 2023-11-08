using UnityEngine;
using System.Collections;
using UnityEditor;

namespace DicingHeros
{

    [CustomEditor(typeof(BoardPiece))]
    public class BoardPieceEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            BoardPiece boardPiece = (BoardPiece)target;

            if (boardPiece == null || boardPiece.gameObject == null)
                return;

            if (GUILayout.Button("Regenerate All Tiles"))
            {
                boardPiece.RegenerateAllTiles();
            }
        }

        /*
        protected void OnSceneGUI()
        {
            Board board = (Board)target;

            if (board == null || board.gameObject == null)
                return;

            //Handles.DrawWireCube(tile.transform.position, Vector3.one * 0.1f);

        }
        */
    }
}