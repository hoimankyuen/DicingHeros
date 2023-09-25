using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputUtils
{
	/// <summary>
	/// Utility function for detecting a non dragging mouse press with the help of a Vector2 cache value.
	/// </summary>
	/// <returns></returns>
	public static bool GetMousePress(int button, ref Vector2 pressedPositionCache)
	{
		if (Input.GetMouseButtonDown(button))
		{
			if (!EventSystem.current.IsPointerOverGameObject())
			{
				pressedPositionCache = Input.mousePosition;
			}
		}
		if (Input.GetMouseButtonUp(button) && pressedPositionCache != Vector2.negativeInfinity)
		{
			if (Vector2.Distance(pressedPositionCache, Input.mousePosition) < 2f)
			{
				if (!EventSystem.current.IsPointerOverGameObject())
				{
					return true;
				}
			}
			pressedPositionCache = Vector2.negativeInfinity;
		}
		return false;
	}

	/// <summary>
	/// Reset the Vector2 cache value used for GetMousePress
	/// </summary>
	public static void ResetPressCache(ref Vector2 pressedPositionCache)
	{
		pressedPositionCache = Vector2.negativeInfinity;
	}
}
