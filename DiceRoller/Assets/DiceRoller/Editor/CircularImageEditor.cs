using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CanEditMultipleObjects]
[CustomEditor(typeof(CircularImage), true)]
public class CircularImageEditor : Editor
{
	protected MonoScript script;

	protected SerializedProperty colorProp;
	protected SerializedProperty materialProp;
	protected SerializedProperty raycastTargetProp;
	protected SerializedProperty maskableProp;

	protected SerializedProperty offsetProp;
	protected SerializedProperty sizeProp;
	protected SerializedProperty innerRadiusProp;
	protected SerializedProperty outerRadiusProp;
	protected SerializedProperty degreePerSegmentProp;
	protected SerializedProperty paddingProp;
	protected SerializedProperty subdivideProp;
	protected SerializedProperty subdivisionsProp;
	protected SerializedProperty subdivisionPaddingProp;
	protected SerializedProperty spriteProp;
	protected SerializedProperty slicedSpriteProp;
	protected SerializedProperty pixelsPerUnitMultiplierProp;

	void OnEnable()
	{
		// Setup the SerializedProperties.
		SerializedPropertiesSetup();
	}

	protected virtual void SerializedPropertiesSetup()
	{
		script = MonoScript.FromMonoBehaviour((MonoBehaviour)target);

		colorProp = serializedObject.FindProperty("m_Color");
		materialProp = serializedObject.FindProperty("m_Material");
		raycastTargetProp = serializedObject.FindProperty("m_RaycastTarget");
		maskableProp = serializedObject.FindProperty("m_Maskable");

		offsetProp = serializedObject.FindProperty("m_Offset");
		sizeProp = serializedObject.FindProperty("m_Size");
		innerRadiusProp = serializedObject.FindProperty("m_InnerRadius");
		outerRadiusProp = serializedObject.FindProperty("m_OuterRadius");
		degreePerSegmentProp = serializedObject.FindProperty("m_DegreePerSegment");
		paddingProp = serializedObject.FindProperty("m_Padding");
		subdivideProp = serializedObject.FindProperty("m_Subdivide");
		subdivisionsProp = serializedObject.FindProperty("m_Subdivisions");
		subdivisionPaddingProp = serializedObject.FindProperty("m_SubdivisionPadding");
		spriteProp = serializedObject.FindProperty("m_Sprite");
		slicedSpriteProp = serializedObject.FindProperty("m_SlicedSprite");
		pixelsPerUnitMultiplierProp = serializedObject.FindProperty("m_PixelsPerUnitMultiplier");
	}

	public override void OnInspectorGUI()
	{
		// Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
		serializedObject.Update();

		// Show script
		GUI.enabled = false;
		EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false);
		GUI.enabled = true;

		// Show propreties
		EditorGUILayout.PropertyField(spriteProp, new GUIContent("Source Image"));
		EditorGUILayout.PropertyField(colorProp, new GUIContent("Color"));
		EditorGUILayout.PropertyField(materialProp, new GUIContent("Material"));

		EditorGUILayout.PropertyField(offsetProp, new GUIContent("Angular Offset (Degree)"));
		EditorGUILayout.PropertyField(sizeProp, new GUIContent("Angular Size (Degree)"));
		EditorGUILayout.PropertyField(innerRadiusProp, new GUIContent("Inner Radius (%)"));
		EditorGUILayout.PropertyField(outerRadiusProp, new GUIContent("Outer Radius (%)"));
		EditorGUILayout.PropertyField(degreePerSegmentProp, new GUIContent("Degree Per Segment"));
		EditorGUILayout.PropertyField(paddingProp, new GUIContent("Padding"));

		EditorGUILayout.PropertyField(subdivideProp, new GUIContent("Subdivide"));
		if (subdivideProp.boolValue)
		{
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField(subdivisionsProp, new GUIContent("Subdivisions"));
			EditorGUILayout.PropertyField(subdivisionPaddingProp, new GUIContent("Subdivision Padding"));
			EditorGUI.indentLevel--;
		}

		EditorGUILayout.PropertyField(raycastTargetProp, new GUIContent("Raycast Target"));
		EditorGUILayout.PropertyField(maskableProp, new GUIContent("Maskable"));
		EditorGUILayout.PropertyField(slicedSpriteProp, new GUIContent("Sliced Image"));
		if (slicedSpriteProp.boolValue)
		{
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField(pixelsPerUnitMultiplierProp, new GUIContent("Pixels Per Unit Multiplier"));
			EditorGUI.indentLevel--;
		}

		// Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();
	}
}