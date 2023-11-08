using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleDecalSystem
{
	[RequireComponent(typeof(Camera))]
	public class DecalRenderEnabler : MonoBehaviour
	{
		private void Start()
		{
			// enabling the camera allowing depth normal renderering
			Camera camera = GetComponent<Camera>();
			camera.depthTextureMode = camera.depthTextureMode | DepthTextureMode.DepthNormals;
		}
	}
}
