using UnityEngine;
using System.Collections;
using UnityEditor;

namespace DiceRoller
{

    [CustomEditor(typeof(Board))]
    public class BoardEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Board board = (Board)target;

            if (board == null || board.gameObject == null)
                return;

            if (GUILayout.Button("Regenerate"))
            {
                board.RegenerateBoard();
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