using UnityEngine;
using UnityEngine.UI;

public class MultiButtonChild : MonoBehaviour
{
	public Graphic targetGraphic;

	public Color normalColor = new Color(1, 1, 1, 1);
	public Color highlightedColor = new Color(0.9607843f, 0.9607843f, 0.9607843f, 1);
	public Color pressedColor = new Color(0.7843137f, 0.7843137f, 0.7843137f, 1);
	public Color disabledColor = new Color(0.7843137f, 0.7843137f, 0.7843137f, 0.5f);

	// ========================================================= Monobehaviour Methods =========================================================

	void Reset()
	{
		targetGraphic = GetComponent<Graphic>();
	}

	/// <summary>
	/// Simulate the color crossfading of a button.
	/// </summary>
	public void CrossFadeColor(Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha)
	{
		if (targetGraphic == null)
			return;

		targetGraphic.CrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha);
	}
}