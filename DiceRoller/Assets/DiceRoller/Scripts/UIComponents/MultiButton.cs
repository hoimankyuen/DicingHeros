using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MultiButton : Button
{
	List<MultiButtonChild> buttonChildren = new List<MultiButtonChild>();

	// ========================================================= Monobehaviour Methods =========================================================

	/// <summary>
	/// Awake is called when the game object was created. It is always called before start and is 
	/// independent of if the game object is active or not.
	/// </summary>
	protected override void Awake()
	{
		base.Awake();

		RecursiveSearchForChild(transform);
	}

	// ========================================================= Extra Functionalities =========================================================

	/// <summary>
	/// Find all multibutton children that should be associated this multibutton.
	/// </summary>
	protected void RecursiveSearchForChild(Transform transform)
	{
		if (transform.GetComponent<MultiButtonChild>() != null)
		{
			buttonChildren.Add(transform.GetComponent<MultiButtonChild>());
		}

		for (int i = 0; i < transform.childCount; i++)
		{
			if (transform.GetChild(i).GetComponent<MultiButton>() == null)
			{
				RecursiveSearchForChild(transform.GetChild(i));
			}
		}
	}

	/// <summary>
	/// Propergate state transition to all multibutton children when performing button state transition.
	/// </summary>
	protected override void DoStateTransition(SelectionState state, bool instant)
	{
		Color targetColor =
			state == SelectionState.Disabled ? colors.disabledColor :
			state == SelectionState.Highlighted ? colors.highlightedColor :
			state == SelectionState.Normal ? colors.normalColor :
			state == SelectionState.Pressed ? colors.pressedColor : Color.white;

		switch (state)
		{
			case SelectionState.Disabled:
				targetGraphic.CrossFadeColor(colors.disabledColor, instant ? 0f : colors.fadeDuration, true, true);
				foreach (MultiButtonChild child in buttonChildren)
				{
					child.CrossFadeColor(child.disabledColor, instant ? 0f : colors.fadeDuration, true, true);
				}
				break;
			case SelectionState.Highlighted:
				targetGraphic.CrossFadeColor(colors.highlightedColor, instant ? 0f : colors.fadeDuration, true, true);
				foreach (MultiButtonChild child in buttonChildren)
				{
					child.CrossFadeColor(child.highlightedColor, instant ? 0f : colors.fadeDuration, true, true);
				}
				break;
			case SelectionState.Normal:
				targetGraphic.CrossFadeColor(colors.normalColor, instant ? 0f : colors.fadeDuration, true, true);
				foreach (MultiButtonChild child in buttonChildren)
				{
					child.CrossFadeColor(child.normalColor, instant ? 0f : colors.fadeDuration, true, true);
				}
				break;
			case SelectionState.Pressed:
				targetGraphic.CrossFadeColor(colors.pressedColor, instant ? 0f : colors.fadeDuration, true, true);
				foreach (MultiButtonChild child in buttonChildren)
				{
					child.CrossFadeColor(child.pressedColor, instant ? 0f : colors.fadeDuration, true, true);
				}
				break;
		}
	}
}