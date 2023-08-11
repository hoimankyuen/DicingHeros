using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceRoller
{
    public class CameraController : MonoBehaviour
	{   
		// references
		public Transform MainTarget { get; protected set; }
		public Camera Main { get; protected set; }

		private float keyboardMoveSpeed = 1f;
		private float keyboardRotateSpeed = 90f;
		private float keyboardZoomSpeed = 1f;

		private float mouseMoveFactor = 4f;
		private float mouseRotateFactor = 180f;
		private float mouseZoomFactor = 3f;

		private RangeFloat2 zoomRange = new RangeFloat2(0.4f, 6f);

		private bool midDragging = false;
		private bool rightDragging = false;
		private Vector3 midDraggingLastPos = Vector2.zero;
		private Vector3 rightDraggingLastPos = Vector2.zero;

		//private Coroutine gotoCoroutine = null;

		// ========================================================= Monobehaviour Methods =========================================================

		/// <summary>
		/// Awake is called when the game object was created. It is always called before start and is 
		/// independent of if the game object is active or not.
		/// </summary>
		void Awake()
        {
			MainTarget = transform.Find("MainCameraTarget");
            Main = MainTarget.Find("MainCamera").GetComponent<Camera>();
        }

        /// <summary>
        /// Start is called before the first frame update and/or the game object is first active.
        /// </summary>
        void Start()
        {

        }

        /// <summary>
        /// Update is called once per frame.
        /// </summary>
        void Update()
        {
            KeyboardControls();
            MouseControls();
        }

		// ========================================================= Player Input =========================================================

		/// <summary>
		/// Control the camera via keyboard.
		/// </summary>
		private void KeyboardControls()
		{
			// translate view
			if (Input.GetKey(KeyCode.W))
			{
				MainTarget.Translate(Quaternion.FromToRotation(MainTarget.up, Vector3.up) * MainTarget.forward *  Vector3.Distance(Main.transform.localPosition, Vector3.zero) * keyboardMoveSpeed * Time.deltaTime, Space.World);
			}
			if (Input.GetKey(KeyCode.S))
			{
				MainTarget.Translate(Quaternion.FromToRotation(MainTarget.up, Vector3.up) * MainTarget.forward * -1f * Vector3.Distance(Main.transform.localPosition, Vector3.zero) * keyboardMoveSpeed * Time.deltaTime, Space.World);
			}
			if (Input.GetKey(KeyCode.A))
			{
				MainTarget.Translate(Quaternion.FromToRotation(MainTarget.up, Vector3.up) * MainTarget.right * -1f * Vector3.Distance(Main.transform.localPosition, Vector3.zero) * keyboardMoveSpeed * Time.deltaTime, Space.World);
			}
			if (Input.GetKey(KeyCode.D))
			{
				MainTarget.Translate(Quaternion.FromToRotation(MainTarget.up, Vector3.up) * MainTarget.right *  Vector3.Distance(Main.transform.localPosition, Vector3.zero) * keyboardMoveSpeed * Time.deltaTime, Space.World);
			}

			// rotate view
			if (Input.GetKey(KeyCode.Q))
			{
				MainTarget.Rotate(Vector3.up * keyboardRotateSpeed * Time.deltaTime, Space.World);
			}
			if (Input.GetKey(KeyCode.E))
			{
				MainTarget.Rotate(Vector3.down * keyboardRotateSpeed * Time.deltaTime, Space.World);
			}

			// zoom view
			if (Input.GetKey(KeyCode.F))
			{
				if (Vector3.Distance(Main.transform.localPosition, Vector3.zero) < zoomRange.max)
				{
					Main.transform.Translate(Vector3.back * keyboardZoomSpeed * Vector3.Distance(Main.transform.localPosition, Vector3.zero) * Time.deltaTime, Space.Self);
				}
			}
			if (Input.GetKey(KeyCode.R))
			{
				if (Vector3.Distance(Main.transform.localPosition, Vector3.zero) > zoomRange.min)
				{
					Main.transform.Translate(Vector3.forward * keyboardZoomSpeed * Vector3.Distance(Main.transform.localPosition, Vector3.zero) * Time.deltaTime, Space.Self);
				}
			}
		}

		/// <summary>
		/// Control the camera via mouse.
		/// </summary>
		protected void MouseControls()
		{
			// detect mouse input
			if (Input.GetMouseButtonDown(2))
            {
				midDragging = true;
				midDraggingLastPos = Main.ScreenToViewportPoint(Input.mousePosition);
			}
			if (Input.GetMouseButtonUp(2))
            {
				midDragging = false;
            }
			if (Input.GetMouseButtonDown(1))
			{
				rightDragging = true;
				rightDraggingLastPos = Main.ScreenToViewportPoint(Input.mousePosition);
			}
			if (Input.GetMouseButtonUp(1))
			{
				rightDragging = false;
			}
			
			// translate view
			if (midDragging)
            {
				Vector3 delta = Main.ScreenToViewportPoint(Input.mousePosition) - midDraggingLastPos;
				MainTarget.Translate(Quaternion.FromToRotation(MainTarget.up, Vector3.up) * MainTarget.forward * -1f * delta.y * mouseMoveFactor, Space.World);
				MainTarget.Translate(Quaternion.FromToRotation(MainTarget.up, Vector3.up) * MainTarget.right * -1f * delta.x * mouseMoveFactor, Space.World);
				midDraggingLastPos = Main.ScreenToViewportPoint(Input.mousePosition);
			}
			
			// rotate view
			if (rightDragging)
            {
				Vector3 delta = Main.ScreenToViewportPoint(Input.mousePosition) - rightDraggingLastPos;
				MainTarget.Rotate(Vector3.up * delta.x * mouseRotateFactor, Space.World);
				rightDraggingLastPos = Main.ScreenToViewportPoint(Input.mousePosition);
			}

			// zoom
			if (Input.mouseScrollDelta.y > 0)
			{
				if (Vector3.Distance(Main.transform.localPosition, Vector3.zero) > zoomRange.min)
				{
					Main.transform.Translate(Vector3.forward * keyboardZoomSpeed * Vector3.Distance(Main.transform.localPosition, Vector3.zero) * Input.mouseScrollDelta.y * mouseZoomFactor * Time.deltaTime, Space.Self);
				}
			}
			else if (Input.mouseScrollDelta.y < 0)
			{
				if (Vector3.Distance(Main.transform.localPosition, Vector3.zero) < zoomRange.max)
				{
					Main.transform.Translate(Vector3.forward * keyboardZoomSpeed * Vector3.Distance(Main.transform.localPosition, Vector3.zero) * Input.mouseScrollDelta.y * mouseZoomFactor * Time.deltaTime, Space.Self);
				}
			}
		}
	}
}