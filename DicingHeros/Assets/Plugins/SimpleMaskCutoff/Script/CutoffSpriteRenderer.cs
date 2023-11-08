using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleMaskCutoff
{
    [System.Serializable]
    public struct CutoffSpriteFlipXY
    {
        public bool flipX;
        public bool flipY;
    }

    [RequireComponent(typeof(SpriteRenderer))]
    public class CutoffSpriteRenderer : MonoBehaviour
    {

        // references
        protected new SpriteRenderer renderer;
        protected MaterialPropertyBlock mpb;
        public SpriteRenderer spriteRenderer { get { return renderer; } }

        // cutoff sprite renderer properties
        [SerializeField]
        protected Sprite _cutoffMask;
        [Range(0, 1)]
        [SerializeField]
        protected float _cutoffFromValue = 0f;
        [Range(0, 1)]
        [SerializeField]
        protected float _cutoffToValue = 0.5f;

        public Sprite cutoffMask { get { return _cutoffMask; } set { _cutoffMask = value; SetCutoffMask(_cutoffMask); } }
        public float cutoffFromValue { get { return _cutoffFromValue; } set { _cutoffFromValue = value; CutoffFrom(_cutoffFromValue); } }
        public float cutoffToValue { get { return _cutoffToValue; } set { _cutoffToValue = value; CutoffTo(_cutoffToValue); } }

        // sprite renderer properties
        [SerializeField]
        protected Sprite _sprite;
        [SerializeField]
        protected Color _color = Color.white;

        public Sprite sprite { get { return _sprite; } set { _sprite = value; SetSprite(); } }
        public Color color { get { return _color; } set { _color = value; renderer.color = _color; } }

        [SerializeField]
        protected CutoffSpriteFlipXY _flip;
        [SerializeField]
        protected Material _material;
        [SerializeField]
        protected SpriteDrawMode _drawMode;
        [SerializeField]
        protected Vector2 _size;
        [SerializeField]
        protected SpriteTileMode _tileMode;
        [Range(0, 1)]
        [SerializeField]
        protected float _stretchValue = 0f;

        public CutoffSpriteFlipXY flip { get { return _flip; } set { _flip = value; renderer.flipX = _flip.flipX; renderer.flipY = _flip.flipY; } }
        public Material material { get { return _material; } set { _material = value; renderer.material = _material; } }
        public SpriteDrawMode drawMode { get { return _drawMode; } set { _drawMode = value; renderer.drawMode = _drawMode; } }
        public Vector2 size { get { return _size; } set { _size = value; renderer.size.Set(_size.x, _size.y); } }
        public SpriteTileMode tileMode { get { return _tileMode; } set { _tileMode = value; renderer.tileMode = _tileMode; } }
        public float stretchValue { get { return _stretchValue; } set { _stretchValue = value; renderer.adaptiveModeThreshold = _stretchValue; } }


        [SerializeField]
        protected string _sortingLayerName;
        [SerializeField]
        protected int _sortingOrder;
        [SerializeField]
        protected SpriteMaskInteraction _maskInteraction;
        [SerializeField]
        protected SpriteSortPoint _spriteSortPoint;

        public string sortingLayerName { get { return _sortingLayerName; } set { _sortingLayerName = value; renderer.sortingLayerName = _sortingLayerName; } }
        public int sortingOrder { get { return _sortingOrder; } set { _sortingOrder = value; renderer.sortingOrder = _sortingOrder; } }
        public SpriteMaskInteraction maskInteraction { get { return _maskInteraction; } set { _maskInteraction = value; renderer.maskInteraction = _maskInteraction; } }
        public SpriteSortPoint spriteSortPoint { get { return _spriteSortPoint; } set { _spriteSortPoint = value; renderer.spriteSortPoint = _spriteSortPoint; } }

        void Awake()
        {
            SetMaterialReferences();
        }

        void Reset()
        {
            // set the default material
            _material = Resources.Load<Material>("SpriteCutoff");
            _cutoffMask = Resources.Load<Sprite>("VerticalCutoffMask");
        }

        void OnEnable()
        {
            renderer.enabled = true;
        }

        void OnDisable()
        {
            renderer.enabled = false;
        }

        void OnValidate()
        {
            // hide the base sprite compoment
            renderer = GetComponent<SpriteRenderer>();
			//image.hideFlags = HideFlags.None;
			renderer.hideFlags = HideFlags.HideInInspector;
			renderer.enabled = enabled;

            // move the informations to the base sprite component
            SetSprite();
            renderer.color = _color;
            renderer.flipX = _flip.flipX;
            renderer.flipY = _flip.flipY;
            renderer.material = _material;
            renderer.drawMode = _drawMode;
            //renderer.size = size;
            renderer.size.Set(_size.x, _size.y); // supress warning for renderer.size = size
            renderer.tileMode = _tileMode;
            renderer.adaptiveModeThreshold = _stretchValue;
            renderer.sortingLayerName = _sortingLayerName;
            renderer.sortingOrder = _sortingOrder;
            renderer.maskInteraction = _maskInteraction;
            renderer.spriteSortPoint = _spriteSortPoint;

            // change the customized properties
            mpb = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(mpb);
            if (_cutoffMask != null)
            {
                mpb.SetTexture("_CutoffMask", _cutoffMask.texture);
            }
            mpb.SetFloat("_CutoffFrom", _cutoffFromValue);
            mpb.SetFloat("_CutoffTo", _cutoffToValue);
            renderer.SetPropertyBlock(mpb);
        }

        protected void SetMaterialReferences()
        {
            // Retrieve references
            renderer = GetComponent<SpriteRenderer>();
            mpb = new MaterialPropertyBlock();

            // Initially set the customized properties
            renderer.GetPropertyBlock(mpb);
            mpb.SetTexture("_CutoffMask", _cutoffMask.texture);
            mpb.SetFloat("_CutoffFrom", _cutoffFromValue);
            mpb.SetFloat("_CutoffTo", _cutoffToValue);
            renderer.SetPropertyBlock(mpb);
        }

        /// <summary>
        /// Sets the sprite to the sprite renderer.
        /// </summary>
        protected void SetSprite()
        {
			if (renderer == null || mpb == null)
			{
				SetMaterialReferences();
			}

			renderer.sprite = _sprite;
			_size = renderer.size;
		}

        /// <summary>
        /// Sets the cutoff mask.
        /// </summary>
        protected void SetCutoffMask(Sprite value)
        {
			if (renderer == null || mpb == null)
			{
				SetMaterialReferences();
			}

			_cutoffMask = value;
            mpb.SetTexture("_CutoffMask", _cutoffMask.texture);
            renderer.SetPropertyBlock(mpb);
        }

        /// <summary>
        /// Sets the cutoff from a certain value, hiding image where the alpha of mask is below the value.
        /// </summary>
        public void CutoffFrom(float value)
        {
			if (renderer == null || mpb == null)
			{
				SetMaterialReferences();
			}

			_cutoffFromValue = value;
            mpb.SetFloat("_CutoffFrom", Mathf.Clamp01(_cutoffFromValue));
            renderer.SetPropertyBlock(mpb);
        }

        /// <summary>
        /// Sets the cutoff to a certain value, hiding image where the alpha of mask is above the value.
        /// </summary>
        public void CutoffTo(float value)
        {
			if (renderer == null || mpb == null)
			{
				SetMaterialReferences();
			}

			_cutoffToValue = value;
            mpb.SetFloat("_CutoffTo", Mathf.Clamp01(_cutoffToValue));
            renderer.SetPropertyBlock(mpb);
        }
    }
}
