using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputUtils
{
	// ========================================================= Drag Passing Token =========================================================

	private static readonly float dragInitializeDistance = 5f;

	/// <summary>
	/// Flag for if the mouse is involved in a drag action.
	/// </summary>
	public static bool IsDragging
	{
		get
		{
			return dragCaller != null;
		}
	}
	private static object dragCaller = null;

	/// <summary>
	/// Start a drag action. The caller is then responsible for stopping the drag action,
	/// and no other object can start a drag action until the previous one is stopped.
	/// </summary>
	public static void StartDragging(object caller)
	{
		if (dragCaller == null)
		{
			dragCaller = caller;
		}
	}

	/// <summary>
	/// Stop a drag action. This can only be done by the caller that start the drag action.
	/// </summary>
	public static void StopDragging(object caller)
	{
		if (dragCaller == caller)
		{
			dragCaller = null;
		}
	}

	// ========================================================= Mouse BUtton Press Detection =========================================================

	/// <summary>
	/// Utility function for detecting a non dragging mouse press with the help of a Vector2 cache value.
	/// </summary>
	/// <returns></returns>
	public static bool GetMousePress(int button, ref Vector2 pressedPositionCache)
	{
		if (Input.GetMouseButtonDown(button) && !IsDragging)
		{
			if (!EventSystem.current.IsPointerOverGameObject())
			{
				pressedPositionCache = Input.mousePosition;
			}
		}
		if (Input.GetMouseButtonUp(button) && pressedPositionCache != Vector2.negativeInfinity)
		{
			if (Vector2.Distance(pressedPositionCache, Input.mousePosition) < dragInitializeDistance)
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

	// ========================================================= UI Prevention =========================================================

	/// <summary>
	/// Check if the mouse is currently on any UI.
	/// </summary>
	public static bool IsMouseOnUI()
	{
		return EventSystem.current.IsPointerOverGameObject();
	}

}
