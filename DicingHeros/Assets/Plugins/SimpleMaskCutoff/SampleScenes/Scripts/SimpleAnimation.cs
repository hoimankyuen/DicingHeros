using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleMaskCutoff;

public class SimpleAnimation : MonoBehaviour
{
    public enum Mode { linearSine, warpConstant }

    CutoffSpriteRenderer cutoffSpriteRenderer;
	CutoffImage cutoffImage;
    public Mode mode;

	// Use this for initialization
	void Start ()
    {
        // Retrieve references
        cutoffSpriteRenderer = GetComponent<CutoffSpriteRenderer>();
		cutoffImage = GetComponent<CutoffImage>();

	}
	
    void Update ()
    {
        // Display an animation with the cutoff value
        switch (mode)
        {   
            case Mode.linearSine:
				if (cutoffSpriteRenderer != null)
				{
					cutoffSpriteRenderer.CutoffTo(Mathf.Sin(Time.time) / 1.8f + 0.5f);
				}
				if (cutoffImage != null)
				{
					cutoffImage.CutoffTo(Mathf.Sin(Time.time) / 1.8f + 0.5f);
				}
				break;
            case Mode.warpConstant:
				if (cutoffSpriteRenderer != null)
				{
					cutoffSpriteRenderer.CutoffFrom(Time.time / 2 % 1);
					cutoffSpriteRenderer.CutoffTo((Time.time / 2 + 0.5f) % 1);
				}
				if (cutoffImage != null)
				{
					cutoffImage.CutoffFrom(Time.time / 2 % 1);
					cutoffImage.CutoffTo((Time.time / 2 + 0.5f) % 1);
				}
				break;
        }
    }
}
