using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace SimpleMaskCutoff
{
	[RequireComponent(typeof(Image))]
	public class CutoffImage : MonoBehaviour
	{
		// references
		protected Image image;
		protected Material originalMaterial;
		protected Material instantiatedMaterial;
		protected bool instantiated = false;

		// cutoff image properties
		[SerializeField]
		protected Sprite _cutoffMask;
		[Range(0, 1)]
		[SerializeField]
		protected float _cutoffFromValue = 0f;
		[Range(0, 1)]
		[SerializeField]
		protected float _cutoffToValue = 0.5f;

		public Sprite cutoffMask { get { return _cutoffMask; } set { _cutoffMask = value; SetCutoffMask(_cutoffMask); } }
		public float cutoffFromValue { get { return _cutoffFromValue; } set { _cutoffFromValue = value; CutoffFrom(_cutoffFromValue); Debug.Log("herre"); } }
		public float cutoffToValue { get { return _cutoffToValue; } set { _cutoffToValue = value; CutoffTo(_cutoffToValue); Debug.Log("herre"); } }

		// image properties
		[SerializeField]
		protected Sprite _sprite;
		[SerializeField]
		protected Color _color = Color.white;
		[SerializeField]
		protected Material _material;
		[SerializeField]
		protected bool _raycastTarget = true;
		[SerializeField]
		protected bool _preserveAspect = false;

		public Sprite sprite { get { return _sprite; } set { _sprite = value; image.sprite = _sprite; } }
		public Color color { get { return _color; } set { _color = value; image.color = _color; } }
		public Material material { get { return _material; } set { _material = value; image.material = _material; } }
		public bool raycastTarget { get { return _raycastTarget; } set { _raycastTarget = value; image.raycastTarget = _raycastTarget; } }
		public bool preserverAspect { get { return _preserveAspect; } set { _preserveAspect = value; image.preserveAspect = _preserveAspect; } }

		void Awake()
		{
			SetMaterialReferences();
			SetInitialValues();
		}

		void Reset()
		{
			// set the default material
			_material = Resources.Load<Material>("ImageCutoff");
			_cutoffMask = Resources.Load<Sprite>("VerticalCutoffMask");
		}

		void OnEnable()
		{
			image.enabled = true;
		}

		void OnDisable()
		{
			image.enabled = false;
		}

		void OnValidate()
		{
			// hide the base sprite compoment
			image = GetComponent<Image>();
			//image.hideFlags = HideFlags.None;
			image.hideFlags = HideFlags.HideInInspector;
			image.enabled = enabled;

			// move the informations to the base sprite component
			image.sprite = _sprite;
			image.color = _color;
			image.material = _material;
			image.raycastTarget = _raycastTarget;
			image.preserveAspect = _preserveAspect;

			// change the customized properties
			image.material.SetTexture("_CutoffMask", _cutoffMask.texture);
			image.material.SetFloat("_CutoffFrom", _cutoffFromValue);
			image.material.SetFloat("_CutoffTo", _cutoffToValue);
		}

		void OnDestroy()
		{
			image.material = originalMaterial;
			DestroyImmediate(instantiatedMaterial, true);
			instantiated = false;
		}

		protected void SetMaterialReferences()
		{
			if (instantiated)
				return;

			// Retrieve references
			image = GetComponent<Image>();
			originalMaterial = image.material;
			instantiatedMaterial = Instantiate(originalMaterial);
			material = instantiatedMaterial;
			SetCutoffMask(cutoffMask);

			instantiated = true;
		}

		protected void SetInitialValues()
		{
			instantiatedMaterial.SetTexture("_CutoffMask", _cutoffMask.texture);
			instantiatedMaterial.SetFloat("_CutoffFrom", Mathf.Clamp01(_cutoffFromValue));
			instantiatedMaterial.SetFloat("_CutoffTo", Mathf.Clamp01(_cutoffToValue));
		}

		/// <summary>
		/// Sets the cutoff mask.
		/// </summary>
		protected void SetCutoffMask(Sprite value)
		{
			if (image == null || instantiatedMaterial == null)
			{
				SetMaterialReferences();
			}

			_cutoffMask = value;
			instantiatedMaterial.SetTexture("_CutoffMask", _cutoffMask.texture);
		}

		/// <summary>
		/// Sets the cutoff from a certain value, hiding image where the alpha of mask is below the value.
		/// </summary>
		public void CutoffFrom(float value)
		{
			if (image == null || instantiatedMaterial == null)
			{
				SetMaterialReferences();
			}

			_cutoffFromValue = value;
			instantiatedMaterial.SetFloat("_CutoffFrom", Mathf.Clamp01(_cutoffFromValue));
		}

		/// <summary>
		/// Sets the cutoff to a certain value, hiding image where the alpha of mask is above the value.
		/// </summary>
		public void CutoffTo(float value)
		{
			if (image == null || instantiatedMaterial == null)
			{
				SetMaterialReferences();
			}

			_cutoffToValue = value;
			instantiatedMaterial.SetFloat("_CutoffTo", Mathf.Clamp01(_cutoffToValue));
		}

		public void SetNativeSize()
		{
			image.SetNativeSize();
		}
	}
}
