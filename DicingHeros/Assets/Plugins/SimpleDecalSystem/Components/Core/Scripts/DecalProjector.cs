using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleDecalSystem
{
	public class DecalProjector : MonoBehaviour
	{
		public enum Mode
		{
			Unlit,
			Lit,
		}

		// references
		protected Renderer bodyRenderer;
		protected Renderer handleRenderer;
		protected LineRenderer lineRenderer;
		protected MaterialPropertyBlock bodyMpb;
		protected Material unlitMaterial;
		protected Material litMaterial;
		protected Material cachedLineMaterial;
		protected Material cachedPreviewMaterial;

		// decal material properties
		[SerializeField]
		protected Mode _mode = Mode.Unlit;
		[SerializeField]
		protected Color _color = Color.white;
		[SerializeField]
		protected Texture _sprite;
		[Range(0, 1)]
		[SerializeField]
		protected float _metallic = 0;
		[Range(0, 1)]
		[SerializeField]
		protected float _smoothness = 0;
		[SerializeField]
		protected Texture _normal;
		[Range(0, 10)]
		[SerializeField]
		protected float _normalStrength = 1;
		[SerializeField]
		protected bool _showHandle = false;

		public Mode mode { get { return _mode; } set { SetMode(value); } }
		public Color color { get { return _color; } set { SetColor(value); } }
		public Texture sprite { get { return _sprite; } set { SetSprite(value); } }
		public float metallic { get { return _metallic; } set { SetMetallic(value); } }
		public float smoothness { get { return _smoothness; } set { SetSmoothness(value); } }
		public Texture normal { get { return _normal; } set { SetNormal(value); } }
		public float normalStrength { get { return _normalStrength; } set { SetNormalStrength(value); } }
		public bool showHandle { get { return _showHandle; } set { SetShowHandle(value); } }

		// caches
		[SerializeField]
		protected Texture2D defaultWhiteTexture;
		[SerializeField]
		protected Texture2D defaultNormalTexture;

		protected void Awake()
		{
			RetrieveReferences();
			RetrieveRuntimeReferences();

			// modify the handle and line
			handleRenderer.sharedMaterials[0].renderQueue = bodyRenderer.sharedMaterials[0].renderQueue + 1;
			cachedLineMaterial.renderQueue = bodyRenderer.sharedMaterials[0].renderQueue + 1;
			cachedPreviewMaterial.renderQueue = bodyRenderer.sharedMaterials[0].renderQueue + 1;
		}

		protected void Start()
		{
			// enabling the camera allowing depth normal renderering
			Camera.main.depthTextureMode = Camera.main.depthTextureMode | DepthTextureMode.DepthNormals;

			// initial setup the materials
			SetMode(mode);
			SetColor(color);
			SetSprite(sprite);
			if (mode == Mode.Lit)
			{
				SetMetallic(metallic);
				SetSmoothness(smoothness);
				SetNormal(normal);
				SetNormalStrength(normalStrength);
			}
			SetShowHandle(showHandle);
		}

		protected void Update()
		{
			SetHandleScale();
			AnimateLine();
		}

		protected void OnValidate()
		{
			RetrieveReferences();

			SetMode(mode);
			SetColor(color);
			SetSprite(sprite);
			if (mode == Mode.Lit)
			{
				SetMetallic(metallic);
				SetSmoothness(smoothness);
				SetNormal(normal);
				SetNormalStrength(normalStrength);
			}
			SetShowHandle(showHandle);
			SetHandleScale();
		}

		protected void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			Gizmos.matrix = bodyRenderer.transform.localToWorldMatrix;
			Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
		}

		protected void OnDestroy()
		{
			Destroy(cachedLineMaterial);
			Destroy(cachedPreviewMaterial);
		}

		/// <summary>
		/// Retrieve required references.
		/// </summary>
		protected void RetrieveReferences()
		{
			if (bodyRenderer == null)
				bodyRenderer = transform.Find("DecalBody").GetComponent<Renderer>();
			if (handleRenderer == null)
				handleRenderer = transform.Find("DecalHandle").GetComponent<Renderer>();
			if (lineRenderer == null)
				lineRenderer = transform.Find("DecalLine").GetComponent<LineRenderer>();
			if (bodyMpb == null)
				bodyMpb = new MaterialPropertyBlock();
			if (unlitMaterial == null)
				unlitMaterial = Resources.Load<Material>("DecalUnlit");
			if (litMaterial == null)
				litMaterial = Resources.Load<Material>("DecalLit");

			if (defaultWhiteTexture == null)
				defaultWhiteTexture = Texture2D.whiteTexture;
			if (defaultNormalTexture == null)
			{
				Color[] textureColors = new Color[] {
					new Color(0.5f, 0.5f, 1f, 1f),
					new Color(0.5f, 0.5f, 1f, 1f),
					new Color(0.5f, 0.5f, 1f, 1f),
					new Color(0.5f, 0.5f, 1f, 1f)
					};
				defaultNormalTexture = new Texture2D(2, 2);
				defaultNormalTexture.SetPixels(textureColors);
			}
		}

		/// <summary>
		/// Retrieve required references that are only needed when in runtime.
		/// </summary>
		protected void RetrieveRuntimeReferences()
		{
			lineRenderer = transform.Find("DecalLine").GetComponent<LineRenderer>();
			cachedLineMaterial = Instantiate(lineRenderer.sharedMaterial);
			lineRenderer.material = cachedLineMaterial;

			for (int i = 0; i < handleRenderer.transform.childCount; i++)
			{
				Renderer previewRenderer = handleRenderer.transform.GetChild(i).GetComponent<Renderer>();
				if (cachedPreviewMaterial == null)
				{
					cachedPreviewMaterial = Instantiate(previewRenderer.sharedMaterial);
				}
				previewRenderer.material = cachedPreviewMaterial;
			}
			
		}

		/// <summary>
		/// Set the mode of the decal.
		/// </summary>
		protected void SetMode(Mode mode)
		{
			_mode = mode;
			Material[] materials = bodyRenderer.sharedMaterials;
			switch (mode)
			{
				case Mode.Unlit:	
					materials[0] = unlitMaterial;	
					break;
				case Mode.Lit:
					materials[0] = litMaterial;
					break;
			}
			bodyRenderer.materials = materials;
		}

		/// <summary>
		/// Set the color of the decal.
		/// </summary>
		protected void SetColor(Color color)
		{
			_color = color;
			bodyRenderer.GetPropertyBlock(bodyMpb);
			bodyMpb.SetColor("_Tint", color);
			bodyRenderer.SetPropertyBlock(bodyMpb);
		}

		/// <summary>
		/// Set the sprite of the decal.
		/// </summary>
		protected void SetSprite(Texture sprite)
		{
			_sprite = sprite;
			bodyRenderer.GetPropertyBlock(bodyMpb);
			bodyMpb.SetTexture("_MainTex", sprite == null ? defaultWhiteTexture : sprite);
			bodyRenderer.SetPropertyBlock(bodyMpb);

			if (cachedPreviewMaterial != null)
				cachedPreviewMaterial.SetTexture("_MainTex", sprite);
		}

		/// <summary>
		/// Set the metallic value of the decal. Only work when in lit mode.
		/// </summary>
		protected void SetMetallic(float metallic)
		{
			_metallic = Mathf.Clamp01(metallic);
			if (mode == Mode.Lit)
			{
				bodyRenderer.GetPropertyBlock(bodyMpb);
				bodyMpb.SetFloat("_Metallic", metallic);
				bodyRenderer.SetPropertyBlock(bodyMpb);
			}
		}

		/// <summary>
		/// Set the smoothness value of the decal. Only work when in lit mode.
		/// </summary>
		protected void SetSmoothness(float smoothness)
		{
			_smoothness = Mathf.Clamp01(smoothness);
			if (mode == Mode.Lit)
			{
				bodyRenderer.GetPropertyBlock(bodyMpb);
				bodyMpb.SetFloat("_Smoothness", smoothness);
				bodyRenderer.SetPropertyBlock(bodyMpb);
			}
		}

		/// <summary>
		/// Set the normal map of the decal. Only work when in lit mode.
		/// </summary>
		protected void SetNormal(Texture normal)
		{
			_normal = normal;
			if (mode == Mode.Lit)
			{
				bodyRenderer.GetPropertyBlock(bodyMpb);
				bodyMpb.SetTexture("_NormalMap", normal == null ? defaultNormalTexture : normal);
				bodyRenderer.SetPropertyBlock(bodyMpb);
			}
		}

		// <summary>
		/// Set the normal strength of the decal. Only work when in lit mode.
		/// </summary>
		protected void SetNormalStrength(float normalStrength)
		{
			_normalStrength = Mathf.Clamp(normalStrength, 0, 10);
			if (mode == Mode.Lit)
			{
				bodyRenderer.GetPropertyBlock(bodyMpb);
				bodyMpb.SetFloat("_NormalStrength", normalStrength);
				bodyRenderer.SetPropertyBlock(bodyMpb);
			}
		}

		/// <summary>
		/// Set to show the decal handle.
		/// </summary>
		protected void SetShowHandle(bool showHandle)
		{
			_showHandle = showHandle;
			handleRenderer.gameObject.SetActive(showHandle);
			lineRenderer.gameObject.SetActive(showHandle);
		}

		/// <summary>
		/// Set the scale of the handle to make it constant regardless of the projector scale.
		/// </summary>
		protected void SetHandleScale()
		{
			if (showHandle)
			{
				Vector3 scale = handleRenderer.transform.localScale;
				scale.x = transform.lossyScale.x == 0 ? 0f : 0.5f / transform.lossyScale.x;
				scale.y = transform.lossyScale.z == 0 ? 0f : 0.5f / transform.lossyScale.z;
				scale.z = transform.lossyScale.y == 0 ? 0f : 0.5f / transform.lossyScale.y;
				handleRenderer.transform.localScale = scale;
			}
		}

		/// <summary>
		/// Animate the decal line to make it more visible.
		/// </summary>
		protected void AnimateLine()
		{
			if (showHandle)
			{
				Vector2 offset = cachedLineMaterial.mainTextureOffset;
				offset.x = Time.unscaledTime % 1 * -1;
				cachedLineMaterial.mainTextureOffset = offset;
			}
		}
	}
}
