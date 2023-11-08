using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SimpleDecalSystem
{
	[CustomEditor(typeof(DecalProjector))]
	[CanEditMultipleObjects]
	public class DecalProjectorEditor : Editor
	{
		protected MonoScript script;

		protected SerializedProperty modeProp;
		protected SerializedProperty colorProp;
		protected SerializedProperty spriteProp;
		protected SerializedProperty metallicProp;
		protected SerializedProperty smoothnessProp;
		protected SerializedProperty normalProp;
		protected SerializedProperty normalStrengthProp;
		protected SerializedProperty showHandleProp;

		void OnEnable()
		{
			script = MonoScript.FromMonoBehaviour((MonoBehaviour)target);

			modeProp = serializedObject.FindProperty("_mode");
			colorProp = serializedObject.FindProperty("_color");
			spriteProp = serializedObject.FindProperty("_sprite");
			metallicProp = serializedObject.FindProperty("_metallic");
			smoothnessProp = serializedObject.FindProperty("_smoothness");
			normalProp = serializedObject.FindProperty("_normal");
			normalStrengthProp = serializedObject.FindProperty("_normalStrength");
			showHandleProp = serializedObject.FindProperty("_showHandle");
		}

		public override void OnInspectorGUI()
		{
			// Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
			EditorGUI.BeginChangeCheck();

			GUI.enabled = false;
			EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false);
			GUI.enabled = true;

			EditorGUILayout.PropertyField(modeProp, new GUIContent("Render Mode"));
			EditorGUILayout.PropertyField(colorProp, new GUIContent("Color"));
			EditorGUILayout.PropertyField(spriteProp, new GUIContent("Sprite"));
			if ((DecalProjector.Mode)modeProp.enumValueIndex == DecalProjector.Mode.Lit)
			{
				metallicProp.floatValue = EditorGUILayout.Slider(new GUIContent("Metallic"), metallicProp.floatValue, 0, 1);
				smoothnessProp.floatValue = EditorGUILayout.Slider(new GUIContent("Smoothness"), smoothnessProp.floatValue, 0, 1);
				EditorGUILayout.PropertyField(normalProp, new GUIContent("Normal"));
				normalStrengthProp.floatValue = EditorGUILayout.Slider(new GUIContent("Normal Strength"), normalStrengthProp.floatValue, 0, 10);
			}
			EditorGUILayout.PropertyField(showHandleProp, new GUIContent("Show Handle"));

			// save changes
			if (EditorGUI.EndChangeCheck())
			{
				SceneView.RepaintAll();
			}

			// Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
			serializedObject.ApplyModifiedProperties();
		}
	}
}