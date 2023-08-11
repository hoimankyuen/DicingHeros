using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceRoller
{
    public class RangeProjection : MonoBehaviour
    {
        private Transform wall;
        private Transform floor;
        private Renderer wallRenderer;
		private Renderer floorRenderer;
        private MaterialPropertyBlock mpb;

        [SerializeField]
        private bool _show = true;
        public bool show { get { return _show; } set { SetShow(value); } }

        [SerializeField]
        private float _range = 1f;
        public float range { get { return _range; } set { SetRange(value); } }

        [SerializeField]
        private Color _flareColor = Color.red;
        public Color flareColor { get { return _flareColor; } set { SetFlareColor(value); } }

        [SerializeField]
        private float _flareWidth = 0.05f;
        public float flareWidth { get { return _flareWidth; } set { SetFlareWidth(value); } }

        [SerializeField]
        private float _flareHeight = 1f;
        public float flareHeight { get { return _flareHeight; } set { SetFlareHeight(value); } }

        [SerializeField]
        private Color _lightColor = Color.white;
        public Color lightColor { get { return _lightColor; } set { SetLightColor(value); } }

        [SerializeField]
        private float _lightWidth = 0.001f;
        public float lightWidth { get { return _lightWidth; } set { SetLightWidth(value); } }

        [SerializeField]
        private float _lightHeight = 0.1f;
        public float lightHeight { get { return _lightHeight; } set { SetLightHeight(value); } }

        protected void Awake()
        {
            RetrieveReferences();
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

		}

        protected void OnValidate()
        {
            RetrieveReferences();
            SetShow(show);
            SetRange(range);
            SetFlareColor(flareColor);
            SetFlareWidth(flareWidth);
            SetFlareHeight(flareHeight);
            SetLightColor(lightColor);
            SetLightWidth(lightWidth);
            SetLightHeight(lightHeight);
        }


        /// <summary>
        /// Retrieve required references.
        /// </summary>
        protected void RetrieveReferences()
        {
            mpb = new MaterialPropertyBlock();

            wall = transform.Find("RangeWall");
            floor = transform.Find("RangeFloor");
            wallRenderer = wall.GetComponent<Renderer>();
            floorRenderer = floor.GetComponent<Renderer>();
        }

        void SetShow(bool show)
        {
            _show = show;
            wall.gameObject.SetActive(show);
            floor.gameObject.SetActive(show);
        }

        void SetRange(float range)
        {
            _range = range;
            floorRenderer.GetPropertyBlock(mpb);
            mpb.SetFloat("_Range", range);
            floorRenderer.SetPropertyBlock(mpb);

            floor.transform.localScale = new Vector3(
                ((range + flareWidth) * 2 + lightWidth) / transform.lossyScale.x,
                2 / transform.lossyScale.y,
                ((range + flareWidth) * 2 + lightWidth) / transform.lossyScale.z);

            wall.transform.localScale = new Vector3(
                range * 2 / transform.lossyScale.x,
                1 / transform.lossyScale.y,
                range * 2 / transform.lossyScale.z);
        }

        void SetFlareColor(Color color)
        {
            _flareColor = color;
            floorRenderer.GetPropertyBlock(mpb);
            mpb.SetColor("_FlareColor", color);
            floorRenderer.SetPropertyBlock(mpb);
            wallRenderer.GetPropertyBlock(mpb);
            mpb.SetColor("_FlareColor", color);
            wallRenderer.SetPropertyBlock(mpb);
        }

        void SetFlareWidth(float width)
        {
            _flareWidth = width;
            floorRenderer.GetPropertyBlock(mpb);
            mpb.SetFloat("_FlareWidth", width);
            floorRenderer.SetPropertyBlock(mpb);
        }

        void SetFlareHeight(float height)
        {
            _flareHeight = height;
            wallRenderer.GetPropertyBlock(mpb);
            mpb.SetFloat("_FlareHeight", height);
            wallRenderer.SetPropertyBlock(mpb);
        }

        void SetLightColor(Color color)
        {
            _lightColor = color;
            floorRenderer.GetPropertyBlock(mpb);
            mpb.SetColor("_LightColor", color);
            floorRenderer.SetPropertyBlock(mpb);
            wallRenderer.GetPropertyBlock(mpb);
            mpb.SetColor("_LightColor", color);
            wallRenderer.SetPropertyBlock(mpb);
        }

        void SetLightWidth(float width)
        {
            _lightWidth = width;
            floorRenderer.GetPropertyBlock(mpb);
            mpb.SetFloat("_LightWidth", width);
            floorRenderer.SetPropertyBlock(mpb);
        }

        void SetLightHeight(float height)
        {
            _lightHeight = height;
            wallRenderer.GetPropertyBlock(mpb);
            mpb.SetFloat("_LightHeight", height);
            wallRenderer.SetPropertyBlock(mpb);
        }
    }
}
