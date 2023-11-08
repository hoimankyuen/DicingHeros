using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine.Rendering;
using System;
using System.Collections;
using System.Reflection;

namespace SimpleMaskCutoff
{
	[CustomEditor(typeof(CutoffSpriteRenderer))]
	[CanEditMultipleObjects]
	public class CutoffSpriteRendererEditor : Editor
	{
		protected SerializedProperty cutoffMaskProp;
		protected SerializedProperty cutoffFromValueProp;
		protected SerializedProperty cutoffToValueProp;

		protected SerializedProperty spriteProp;
		protected SerializedProperty colorProp;
		protected SerializedProperty flipXProp;
		protected SerializedProperty flipYProp;
		protected SerializedProperty materialProp;
		protected SerializedProperty drawModeProp;
		protected SerializedProperty sizeProp;
		protected SerializedProperty tileModeProp;
		protected SerializedProperty stretchValueProp;
		protected SerializedProperty sortingLayerNameProp;
		protected SerializedProperty sortingOrderProp;
		protected SerializedProperty maskInteractionProp;
		protected SerializedProperty spriteSortPointProp;

		protected string[] sortingLayerNames;

		void OnEnable()
		{
			cutoffMaskProp = serializedObject.FindProperty("_cutoffMask");
			cutoffFromValueProp = serializedObject.FindProperty("_cutoffFromValue");
			cutoffToValueProp = serializedObject.FindProperty("_cutoffToValue");

			spriteProp = serializedObject.FindProperty("_sprite");
			colorProp = serializedObject.FindProperty("_color");
			flipXProp = serializedObject.FindProperty("_flip").FindPropertyRelative("flipX");
			flipYProp = serializedObject.FindProperty("_flip").FindPropertyRelative("flipY");
			materialProp = serializedObject.FindProperty("_material");
			drawModeProp = serializedObject.FindProperty("_drawMode");
			sizeProp = serializedObject.FindProperty("_size");
			tileModeProp = serializedObject.FindProperty("_tileMode");
			stretchValueProp = serializedObject.FindProperty("_stretchValue");
			sortingLayerNameProp = serializedObject.FindProperty("_sortingLayerName");
			sortingOrderProp = serializedObject.FindProperty("_sortingOrder");
			maskInteractionProp = serializedObject.FindProperty("_maskInteraction");
			spriteSortPointProp = serializedObject.FindProperty("_spriteSortPoint");

			Type internalEditorUtilityType = typeof(InternalEditorUtility);
			PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
			sortingLayerNames = (string[])sortingLayersProperty.GetValue(null, new object[0]);
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			EditorGUILayout.PropertyField(spriteProp, new GUIContent("Sprite"));
			EditorGUILayout.PropertyField(colorProp, new GUIContent("Color"));

			EditorGUILayout.PropertyField(cutoffMaskProp, new GUIContent("Cutoff Mask"));
			EditorGUILayout.Slider(cutoffFromValueProp, 0, 1, new GUIContent("Cutoff From"));
			EditorGUILayout.Slider(cutoffToValueProp, 0, 1, new GUIContent("Cutoff To"));

			// custom filp field
			Rect filpRect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, EditorGUIUtility.singleLineHeight);
			Rect flipContentRect = EditorGUI.PrefixLabel(filpRect, GUIUtility.GetControlID(FocusType.Passive), new GUIContent("Flip"));
			Rect flipXRect = new Rect(flipContentRect.x, flipContentRect.y, 25, flipContentRect.height);
			Rect flipYRect = new Rect(flipContentRect.x + 30, flipContentRect.y, 55, flipContentRect.height);
			flipXProp.boolValue = EditorGUI.ToggleLeft(flipXRect, "X", flipXProp.boolValue);
			flipYProp.boolValue = EditorGUI.ToggleLeft(flipYRect, "Y", flipYProp.boolValue);

			EditorGUILayout.PropertyField(materialProp, new GUIContent("Material"));

			// draw mode
			EditorGUILayout.PropertyField(drawModeProp, new GUIContent("DrawMode"));
			EditorGUI.indentLevel++;
			int drawMode = drawModeProp.enumValueIndex;
			if (drawMode >= 1)
			{
				EditorGUILayout.PropertyField(sizeProp, new GUIContent("Size"));
			}
			if (drawMode == 2)
			{
				EditorGUILayout.PropertyField(tileModeProp, new GUIContent("Tile Mode"));
				if (tileModeProp.enumValueIndex == 1)
				{
					EditorGUI.indentLevel++;
					EditorGUILayout.Slider(stretchValueProp, 0, 1, new GUIContent("Stretch Value"));
					EditorGUI.indentLevel--;
				}
			}
			EditorGUI.indentLevel--;
			EditorGUILayout.Space();

			// sorting layer
			int[] picks = new int[sortingLayerNames.Length];
			string name = sortingLayerNameProp.stringValue;
			int choice = 0;
			for (int i = 0; i < sortingLayerNames.Length; i++)
			{
				picks[i] = i;
				if (name == sortingLayerNames[i])
				{
					choice = i;
				}
			}
			choice = EditorGUILayout.IntPopup("Sorting Layer", choice, sortingLayerNames, picks);
			sortingLayerNameProp.stringValue = sortingLayerNames[choice];
			EditorGUILayout.PropertyField(sortingOrderProp, new GUIContent("Order in Layer"));
			//renderer.sortingOrder = EditorGUILayout.IntField("Order in Layer", renderer.sortingOrder);

			EditorGUILayout.PropertyField(maskInteractionProp, new GUIContent("Mask Interaction"));
			if (drawMode == 0)
			{
				EditorGUILayout.PropertyField(spriteSortPointProp, new GUIContent("Sprite Sort Point"));
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