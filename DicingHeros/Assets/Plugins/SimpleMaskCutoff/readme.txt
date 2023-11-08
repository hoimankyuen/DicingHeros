==== Simple Mask Cutoff ===

How to use:
1. Your can either create a "GameObject/2D Object/Cutoff Sprite" directly or add "cutoffSpriteRenderer.cs" to the gameObject.
2. Drag the sprite and cutoff mask(optional) in the inspector.
3. Either use the slider provided in the inspector or by script to change the cutoff values.
	3.1. By inspector: Just pull the slider or input a value from 0 to 1 in both "Cutoff From" and "Cutoff To".
	3.2. By script: Call cutoffSpriteRenderer.CutoffFrom(value) and cutoffSpriteRenderer.CutoffTo(value).

Note:
1. A sample scene has been provided to show the usage of this package.
2. The mask image only takes the alpha channel in order to function. Consult the provided default VerticalCutoff sprite for information.
3. Due to the lack of perrendererdata support in canvasrenderer, the apparence of cutoffImage will share the same material only in editor. This
   will not happen when in play mode or build. (Only applies to version 1.5)

Versions:
1.0: First release
1.1:   Add ontop material
1.2:   Fix nullPointerException issues
1.3:   Fix setting values directly by code do not alter parameters.
1.4:   Add a general ontop sprite, useful for most use cases.
1.4.1: Fix issue of not able to set the sprite to null.
1.4.2: Fix issue of crashes when changing sprite in runtime.
1.5:   Add canvas support.
	   Rename materials and shaders.