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

            Board myScript = (Board)target;

            if (GUILayout.Button("Regenerate"))
            {
                myScript.RegenerateBoard();
            }
        }
    }
}