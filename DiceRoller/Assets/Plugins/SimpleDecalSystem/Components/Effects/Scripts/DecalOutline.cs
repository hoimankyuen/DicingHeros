using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimpleDecalSystem
{
	public class DecalOutline : MonoBehaviour
	{
		[SerializeField]
		protected Color color = Color.white;
		public Color Color
		{
			get
			{
				return color;
			}
			set
			{
				if (color != value)
				{
					color = value;
					needsUpdate = true;
				}
			}
		}

		[Range(0, 100)]
		[SerializeField]
		protected float width = 10;
		public float Width
		{
			get
			{
				return width;
			}
			set
			{
				if (width != value)
				{
					width = Mathf.Clamp(value, 0, 100);
					needsUpdate = true;
				}
			}
		}

		[SerializeField]
		protected bool show = true;
		public bool Show
		{
			get
			{
				return show;
			}
			set
			{
				if (show != value)
				{
					show = value;
					needsUpdate = true;
				}
			}
		}

		protected Renderer bodyRenderer;
		protected MaterialPropertyBlock bodyMpb;
		protected Material effectMaterial;

		protected bool needsUpdate;

		void Awake()
		{
			RetrieveReferences();

			needsUpdate = true;
		}

		void OnEnable()
		{
			List<Material> materials = bodyRenderer.sharedMaterials.ToList();
			materials.Add(effectMaterial);
			bodyRenderer.materials = materials.ToArray();
		}

		void OnDisable()
		{
			List<Material> materials = bodyRenderer.sharedMaterials.ToList();
			materials.Remove(effectMaterial);
			bodyRenderer.materials = materials.ToArray();
		}

		void OnValidate()
		{
			needsUpdate = true;
		}

		void Update()
		{
			if (needsUpdate)
			{
				UpdateMaterialProperties();
				needsUpdate = false;
			}
		}

		/// <summary>
		/// Retrieve required references.
		/// </summary>
		private void RetrieveReferences()
		{
			bodyRenderer = transform.Find("DecalBody").GetComponent<Renderer>();
			bodyMpb = new MaterialPropertyBlock();

			effectMaterial = Resources.Load<Material>(@"DecalOutline");
		}

		/// <summary>
		/// Update the material to appear as the cofiguration.
		/// </summary>
		private void UpdateMaterialProperties()
		{
			bodyRenderer.GetPropertyBlock(bodyMpb);
			bodyMpb.SetColor("_Color", color);
			bodyMpb.SetFloat("_Show", show ? 1 : 0);
			bodyMpb.SetFloat("_Width", width);
			bodyRenderer.SetPropertyBlock(bodyMpb);
		}

	}
}