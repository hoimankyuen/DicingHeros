using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircularImage : MaskableGraphic
{
	public enum DisplayStatus
	{
		Hide,
		Show
	}

	[SerializeField]
	float m_Offset = 0f;
	public float offset
	{
		get
		{
			return m_Offset;
		}
		set
		{
			if (m_Offset == value)
				return;

			m_Offset = value;
			SetVerticesDirty();
		}
	}

	[SerializeField]
	[Range(1, 360)]
	float m_Size = 30f;
	public float size
	{
		get
		{
			return m_Size;
		}
		set
		{
			if (m_Size == value)
				return;

			m_Size = value;
			SetVerticesDirty();
		}
	}

	[SerializeField]
	[Range(0, 1)]
	float m_InnerRadius = 0.75f;
	public float innerRadius
	{
		get
		{
			return m_InnerRadius;
		}
		set
		{
			if (m_InnerRadius == value)
				return;

			m_InnerRadius = value;
			SetVerticesDirty();
		}
	}

	[SerializeField]
	[Range(0, 1)]
	float m_OuterRadius = 1f;
	public float outerRadius
	{
		get
		{
			return m_OuterRadius;
		}
		set
		{
			if (m_OuterRadius == value)
				return;

			m_OuterRadius = value;
			SetVerticesDirty();
		}
	}

	[SerializeField]
	int m_DegreePerSegment = 5;
	public int degreePerSegment
	{
		get
		{
			return m_DegreePerSegment;
		}
		set
		{
			if (m_DegreePerSegment == value)
				return;

			m_DegreePerSegment = value;
			SetVerticesDirty();
		}
	}

	[SerializeField]
	float m_Padding = 20f;
	public float padding
	{
		get
		{
			return m_Padding;
		}
		set
		{
			if (m_Padding == value)
				return;

			m_Padding = value;
			SetVerticesDirty();
		}
	}

	[SerializeField]
	bool m_Subdivide = false;
	public bool subdivide
	{
		get
		{
			return m_Subdivide;
		}
		set
		{
			if (m_Subdivide == value)
				return;

			m_Subdivide = value;
			SetVerticesDirty();
		}
	}

	[SerializeField]
	List<DisplayStatus> m_Subdivisions = new List<DisplayStatus>(new DisplayStatus[] { DisplayStatus.Show });
	public List<DisplayStatus> subdivisions
	{
		get
		{
			return m_Subdivisions;
		}
		set
		{
			bool equal = true;
			if (value.Count != m_Subdivisions.Count)
				equal = false;
			if (equal)
				for (int i = 0; i < m_Subdivisions.Count; i++)
					if (m_Subdivisions[i] != value[i])
						equal = false;
			if (equal)
				return;

			m_Subdivisions = value;
			SetVerticesDirty();
		}
	}
	readonly List<DisplayStatus> singleSubdivisions = new List<DisplayStatus>(new DisplayStatus[] { DisplayStatus.Show });

	[SerializeField]
	float m_SubdivisionPadding = 10f;
	public float subdivisionPadding
	{
		get
		{
			return m_SubdivisionPadding;
		}
		set
		{
			if (m_SubdivisionPadding == value)
				return;

			m_SubdivisionPadding = value;
			SetVerticesDirty();
		}
	}

	[SerializeField]
	Sprite m_Sprite;
	public Sprite sprite
	{
		get
		{
			return m_Sprite;
		}
		set
		{
			if (m_Sprite == value)
				return;

			m_Sprite = value;
			SetMaterialDirty();
		}
	}

	[SerializeField]
	bool m_SlicedSprite;
	public bool slicedSprite
	{
		get
		{
			return m_SlicedSprite;
		}
		set
		{
			if (m_SlicedSprite == value)
				return;

			m_SlicedSprite = value;
			SetVerticesDirty();
		}
	}

	[SerializeField]
	float m_PixelsPerUnitMultiplier = 1;
	public float pixelsPerUnitMultiplier
	{
		get
		{
			return m_PixelsPerUnitMultiplier;
		}
		set
		{
			if (m_PixelsPerUnitMultiplier == value)
				return;

			m_PixelsPerUnitMultiplier = value;
			SetVerticesDirty();
		}
	}

	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();

		if (gameObject.activeInHierarchy)
		{
			SetVerticesDirty();
			SetMaterialDirty();
		}

	}

	// if no texture is configured, use the default white texture as mainTexture
	public override Texture mainTexture
	{
		get
		{
			if (sprite == null)
			{
				if (material != null && material.mainTexture != null)
				{
					return material.mainTexture;
				}
				return s_WhiteTexture;
			}

			return sprite.texture;
		}
	}

	// actually update our mesh
	protected override void OnPopulateMesh(VertexHelper vh)
	{
		if (slicedSprite && sprite != null && sprite.border.sqrMagnitude > 0)
		{
			PopulateSlicedMesh(vh);
		}
		else
		{
			PopulateSimpleMesh(vh);
		}
	}

	protected void PopulateSlicedMesh(VertexHelper vh)
	{
		// Clear vertex helper to reset vertices, indices etc.
		vh.Clear();

		// preventing population if image is destroying
		if (rectTransform.rect.width == 0)
			return;

		// retrieve slice information
		Vector4 outerUV = UnityEngine.Sprites.DataUtility.GetOuterUV(sprite);
		Vector4 innerUV = UnityEngine.Sprites.DataUtility.GetInnerUV(sprite);
		Vector4 border = sprite.border / pixelsPerUnitMultiplier * 2f;

		// select division settings
		List<DisplayStatus> subdivisions = (subdivide && this.subdivisions.Count > 0) ? this.subdivisions : singleSubdivisions;
		float subdivisionPadding = (subdivide && this.subdivisions.Count > 0) ? this.subdivisionPadding : 0;

		// generate mesh
		float[] xUV = new float[4] { outerUV.x, innerUV.x, innerUV.z, outerUV.z };
		float[] yUV = new float[4] { outerUV.y, innerUV.y, innerUV.w, outerUV.w };
		int[] fanSegments = new int[3] {
				(int)Mathf.Ceil(Mathf.Min(border.x / outerRadius / rectTransform.rect.width * Mathf.Rad2Deg, size / 2) / degreePerSegment / subdivisions.Count),
				(int)Mathf.Ceil(Mathf.Max(size - (border.x + border.z) / outerRadius / rectTransform.rect.width * Mathf.Rad2Deg, 0) / degreePerSegment / subdivisions.Count),
				(int)Mathf.Ceil(Mathf.Min(border.z / outerRadius / rectTransform.rect.width * Mathf.Rad2Deg, size / 2) / degreePerSegment / subdivisions.Count)
			};
		float[] lengths = new float[4] {
				innerRadius,
				Mathf.Min(innerRadius + border.y / rectTransform.rect.width, (innerRadius + outerRadius) / 2f),
				Mathf.Max(outerRadius - border.w / rectTransform.rect.width, (innerRadius + outerRadius) / 2f),
				outerRadius
			};
		float outwardPaddingSize = 0;
		float internalPaddingSize = 0;
		float[] angleBounds = new float[4] { 0, 0, 0, 0 };
		float currentAngle = 0;
		int vertexOffset = 0;
		int vertexCount = 0;

		UIVertex vert = new UIVertex();
		vert.color = this.color;

		for (int i = 0; i < subdivisions.Count; i++)
		{
			if (subdivisions[i] == DisplayStatus.Hide)
				continue;

			vertexCount = 0;

			for (int j = 0; j < 4; j++)
			{
				outwardPaddingSize = Mathf.Asin(padding / rectTransform.rect.width / lengths[j]) * Mathf.Rad2Deg;
				internalPaddingSize = Mathf.Asin(subdivisionPadding / rectTransform.rect.width / lengths[j]) * Mathf.Rad2Deg;
				angleBounds[0] = Mathf.Lerp(-size / 2 + outwardPaddingSize, size / 2 - outwardPaddingSize, (float)(i + 1) / subdivisions.Count) - ((i + 1) == subdivisions.Count ? 0 : internalPaddingSize);
				angleBounds[3] = Mathf.Lerp(-size / 2 + outwardPaddingSize, size / 2 - outwardPaddingSize, (float)i / subdivisions.Count) + (i == 0 ? 0 : internalPaddingSize);
				angleBounds[1] = Mathf.Max(angleBounds[0] - border.x / lengths[j] / rectTransform.rect.width * Mathf.Rad2Deg, (angleBounds[0] + angleBounds[3]) / 2);
				angleBounds[2] = Mathf.Min(angleBounds[3] + border.w / lengths[j] / rectTransform.rect.width * Mathf.Rad2Deg, (angleBounds[0] + angleBounds[3]) / 2);

				for (int k = 0; k < 3; k++)
				{
					for (int l = 0; l < fanSegments[k] + 1; l++)
					{
						if (k != 0 && l == 0)
							continue;

						currentAngle = (Mathf.Lerp(angleBounds[k], angleBounds[k + 1], (float)l / fanSegments[k]) + 90f + offset) * Mathf.Deg2Rad;
						vert.position = new Vector2(
							Mathf.Cos(currentAngle) * lengths[j] * rectTransform.rect.width / 2,
							Mathf.Sin(currentAngle) * lengths[j] * rectTransform.rect.height / 2);
						vert.uv0 = new Vector2(Mathf.Lerp(xUV[k], xUV[k + 1], (float)l / fanSegments[k]), yUV[j]);
						vh.AddVert(vert);
						vertexCount += 1;

						if (j > 0 && l > 0)
						{
							if (l > fanSegments[k] / 2)
							{
								vh.AddTriangle(
									vertexOffset + (j - 1) * (fanSegments[0] + fanSegments[1] + fanSegments[2] + 1) + ((k > 0 ? fanSegments[0] : 0) + (k > 1 ? fanSegments[1] : 0) + l - 1),
									vertexOffset + (j - 0) * (fanSegments[0] + fanSegments[1] + fanSegments[2] + 1) + ((k > 0 ? fanSegments[0] : 0) + (k > 1 ? fanSegments[1] : 0) + l - 1),
									vertexOffset + (j - 1) * (fanSegments[0] + fanSegments[1] + fanSegments[2] + 1) + ((k > 0 ? fanSegments[0] : 0) + (k > 1 ? fanSegments[1] : 0) + l - 0));
								vh.AddTriangle(
									vertexOffset + (j - 1) * (fanSegments[0] + fanSegments[1] + fanSegments[2] + 1) + ((k > 0 ? fanSegments[0] : 0) + (k > 1 ? fanSegments[1] : 0) + l - 0),
									vertexOffset + (j - 0) * (fanSegments[0] + fanSegments[1] + fanSegments[2] + 1) + ((k > 0 ? fanSegments[0] : 0) + (k > 1 ? fanSegments[1] : 0) + l - 1),
									vertexOffset + (j - 0) * (fanSegments[0] + fanSegments[1] + fanSegments[2] + 1) + ((k > 0 ? fanSegments[0] : 0) + (k > 1 ? fanSegments[1] : 0) + l - 0));
							}
							else
							{
								vh.AddTriangle(
									vertexOffset + (j - 1) * (fanSegments[0] + fanSegments[1] + fanSegments[2] + 1) + ((k > 0 ? fanSegments[0] : 0) + (k > 1 ? fanSegments[1] : 0) + l - 0),
									vertexOffset + (j - 1) * (fanSegments[0] + fanSegments[1] + fanSegments[2] + 1) + ((k > 0 ? fanSegments[0] : 0) + (k > 1 ? fanSegments[1] : 0) + l - 1),
									vertexOffset + (j - 0) * (fanSegments[0] + fanSegments[1] + fanSegments[2] + 1) + ((k > 0 ? fanSegments[0] : 0) + (k > 1 ? fanSegments[1] : 0) + l - 0));
								vh.AddTriangle(
									vertexOffset + (j - 0) * (fanSegments[0] + fanSegments[1] + fanSegments[2] + 1) + ((k > 0 ? fanSegments[0] : 0) + (k > 1 ? fanSegments[1] : 0) + l - 0),
									vertexOffset + (j - 1) * (fanSegments[0] + fanSegments[1] + fanSegments[2] + 1) + ((k > 0 ? fanSegments[0] : 0) + (k > 1 ? fanSegments[1] : 0) + l - 1),
									vertexOffset + (j - 0) * (fanSegments[0] + fanSegments[1] + fanSegments[2] + 1) + ((k > 0 ? fanSegments[0] : 0) + (k > 1 ? fanSegments[1] : 0) + l - 1));
							}
						}
					}
				}
			}

			vertexOffset += vertexCount;
		}
	}

	protected void PopulateSimpleMesh(VertexHelper vh)
	{
		// Clear vertex helper to reset vertices, indices etc.
		vh.Clear();

		// preventing population if image is destroying
		if (rectTransform.rect.width == 0)
			return;

		// select division settings
		List<DisplayStatus> subdivisions = (subdivide && this.subdivisions.Count > 0) ? this.subdivisions : singleSubdivisions;
		float subdivisionPadding = (subdivide && this.subdivisions.Count > 0) ? this.subdivisionPadding : 0;

		// generate mesh
		int[] fanSegments = new int[1] { (int)Mathf.Ceil(size / degreePerSegment / subdivisions.Count) };
		float[] lengths = new float[2] { innerRadius, outerRadius };
		float outwardPaddingSize = 0;
		float internalPaddingSize = 0;
		float[] angleBounds = new float[2] { 0, 0 };
		float currentAngle = 0;
		int vertexOffset = 0;
		int vertexCount = 0;

		UIVertex vert = new UIVertex();
		vert.color = this.color;
		for (int i = 0; i < subdivisions.Count; i++)
		{
			if (subdivisions[i] == DisplayStatus.Hide)
				continue;

			vertexCount = 0;

			for (int j = 0; j < 2; j++)
			{
				outwardPaddingSize = Mathf.Asin(padding / rectTransform.rect.width / lengths[j]) * Mathf.Rad2Deg;
				internalPaddingSize = Mathf.Asin(subdivisionPadding / rectTransform.rect.width / lengths[j]) * Mathf.Rad2Deg;
				angleBounds[0] = Mathf.Lerp(-size / 2 + outwardPaddingSize, size / 2 - outwardPaddingSize, (float)(i + 1) / subdivisions.Count) - ((i + 1) == subdivisions.Count ? 0 : internalPaddingSize);
				angleBounds[1] = Mathf.Lerp(-size / 2 + outwardPaddingSize, size / 2 - outwardPaddingSize, (float)i / subdivisions.Count) + (i == 0 ? 0 : internalPaddingSize);

				for (int k = 0; k < fanSegments[0] + 1; k++)
				{
					currentAngle = (Mathf.Lerp(angleBounds[0], angleBounds[1], (float)k / fanSegments[0]) + 90f + offset) * Mathf.Deg2Rad;
					vert.position = new Vector2(
						Mathf.Cos(currentAngle) * lengths[j] * rectTransform.rect.width / 2,
						Mathf.Sin(currentAngle) * lengths[j] * rectTransform.rect.height / 2);
					vert.uv0 = new Vector2((float)k / fanSegments[0], (float)j);
					vh.AddVert(vert);
					vertexCount++;

					if (j > 0 && k > 0)
					{
						if (k > fanSegments[0] / 2)
						{
							vh.AddTriangle(
								vertexOffset + (j - 1) * (fanSegments[0] + 1) + (k - 1),
								vertexOffset + (j - 0) * (fanSegments[0] + 1) + (k - 1),
								vertexOffset + (j - 1) * (fanSegments[0] + 1) + (k - 0));
							vh.AddTriangle(
								vertexOffset + (j - 1) * (fanSegments[0] + 1) + (k - 0),
								vertexOffset + (j - 0) * (fanSegments[0] + 1) + (k - 1),
								vertexOffset + (j - 0) * (fanSegments[0] + 1) + (k - 0));
						}
						else
						{
							vh.AddTriangle(
								vertexOffset + (j - 1) * (fanSegments[0] + 1) + (k - 0),
								vertexOffset + (j - 1) * (fanSegments[0] + 1) + (k - 1),
								vertexOffset + (j - 0) * (fanSegments[0] + 1) + (k - 0));
							vh.AddTriangle(
								vertexOffset + (j - 0) * (fanSegments[0] + 1) + (k - 0),
								vertexOffset + (j - 1) * (fanSegments[0] + 1) + (k - 1),
								vertexOffset + (j - 0) * (fanSegments[0] + 1) + (k - 1));
						}
					}
				}
			}

			vertexOffset += vertexCount;
		}
	}

	public override bool Raycast(Vector2 sp, Camera eventCamera)
	{
		if (base.Raycast(sp, eventCamera))
		{
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, sp, eventCamera, out Vector2 localPoint))
			{
				localPoint.x /= rectTransform.rect.width;
				localPoint.y /= rectTransform.rect.height;
				float distance = Vector2.Distance(Vector2.zero, localPoint * 2);
				if (distance >= innerRadius && distance <= outerRadius)
				{
					if (Mathf.Abs(Vector2.SignedAngle(new Vector2(Mathf.Cos((offset + 90f) * Mathf.Deg2Rad), Mathf.Sin((offset + 90f) * Mathf.Deg2Rad)), localPoint)) < size / 2)
					{
						return true;
					}
				}
			}
		}
		return false;
	}
}