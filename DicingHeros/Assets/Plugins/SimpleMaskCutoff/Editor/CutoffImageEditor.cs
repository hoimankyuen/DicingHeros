using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SimpleMaskCutoff
{
	[CustomEditor(typeof(CutoffImage))]
	[CanEditMultipleObjects]
	public class CutoffImageEditor : Editor
	{
		protected SerializedProperty cutoffMaskProp;
		protected SerializedProperty cutoffFromValueProp;
		protected SerializedProperty cutoffToValueProp;

		protected SerializedProperty spriteProp;
		protected SerializedProperty colorProp;
		protected SerializedProperty materialProp;
		protected SerializedProperty raycastTargetProp;
		protected SerializedProperty preserveAspectProp;


		void OnEnable()
		{
			cutoffMaskProp = serializedObject.FindProperty("_cutoffMask");
			cutoffFromValueProp = serializedObject.FindProperty("_cutoffFromValue");
			cutoffToValueProp = serializedObject.FindProperty("_cutoffToValue");

			spriteProp = serializedObject.FindProperty("_sprite");
			colorProp = serializedObject.FindProperty("_color");
			materialProp = serializedObject.FindProperty("_material");
			raycastTargetProp = serializedObject.FindProperty("_raycastTarget");
			preserveAspectProp = serializedObject.FindProperty("_preserveAspect");
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			EditorGUILayout.PropertyField(spriteProp, new GUIContent("Source Image"));
			EditorGUILayout.PropertyField(colorProp, new GUIContent("Color"));

			EditorGUILayout.PropertyField(cutoffMaskProp, new GUIContent("Cutoff Mask"));
			EditorGUILayout.Slider(cutoffFromValueProp, 0, 1, new GUIContent("Cutoff From"));
			EditorGUILayout.Slider(cutoffToValueProp, 0, 1, new GUIContent("Cutoff To"));

			EditorGUILayout.PropertyField(materialProp, new GUIContent("Material"));
			EditorGUILayout.PropertyField(raycastTargetProp, new GUIContent("Raycast Target"));
			EditorGUILayout.PropertyField(preserveAspectProp, new GUIContent("Preserve Aspect"));

			if (GUILayout.Button("Set Native Size"))
			{
				((CutoffImage)target).SetNativeSize();
			}

			// save changes
			if (EditorGUI.EndChangeCheck())
			{
				SceneView.RepaintAll();
			}
			serializedObject.ApplyModifiedProperties();
		}
	}
}