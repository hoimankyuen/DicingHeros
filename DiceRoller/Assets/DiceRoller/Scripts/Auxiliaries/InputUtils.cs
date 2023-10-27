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

	// ========================================================= Mouse Simulation =========================================================

	private static bool usingSimulatedMouse = false;
	private static Vector3 simulatedMousePosition = Vector3.one * -100f;
	private static bool[] simulatedPresses = new bool[] { false, false, false };
	
	/// <summary>
	/// Start of stop mouse simulation. This will take away control for the player. Used for mouse action look alikes.
	/// </summary>
	public static void EnablePressSimulation(bool enable)
	{
		usingSimulatedMouse = enable;

		simulatedMousePosition = Vector3.one * -100f;
		simulatedPresses[0] = false;
		simulatedPresses[1] = false;
		simulatedPresses[2] = false;
	}

	/// <summary>
	/// Simulate a mouse press. Used for mouse action look alikes.
	/// </summary>
	public static void SimulatePress(int button, bool press)
	{
		simulatedPresses[button] = press;
	}

	/// <summary>
	/// <summary>
	/// Simulate a mouse position. 
	/// </summary>
	public static void SimulateMousePosition(Vector3 position)
	{
		simulatedMousePosition = position;
	}

	// ========================================================= Mouse Position Detection =========================================================

	/// <summary>
	/// Utility function for getting the currect mouse position.
	/// </summary>
	public static Vector3 MousePosition
	{
		get
		{
			if (usingSimulatedMouse)
			{
				return simulatedMousePosition;
			}
			else
			{
				return Input.mousePosition;
			}
		}
	}

	// ========================================================= Mouse BUtton Press Detection =========================================================

	/// <summary>
	/// Utility function for detecting a non dragging mouse press with the help of a Vector2 cache value.
	/// </summary>
	/// <returns></returns>
	public static bool GetMousePress(int button, ref Vector2 pressedPositionCache)
	{
		if (usingSimulatedMouse)
		{
			// read simulated presses
			return simulatedPresses[button];
		}
		else
		{
			// read input from mouse
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
