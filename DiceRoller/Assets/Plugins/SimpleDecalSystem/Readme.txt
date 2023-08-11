==== Simple Decal System ===

This is a simple implementation of shader based decal system, with both unlit and lit variation.
Outline and overlay effect are also included as normal outline and overlay solution would not work with
the decal system.

Note: 
1. The lit version only workss when in play mode. It will display wrongly when in the editor.
2. If the active cameras are not main cameras or are dynamically generated in runtime, but you still need
   to render lit decal, please attach DecalRenderEnabler component onto the camera in question.
3. The lit version will only works on geometry queue, thus will not work when projecting on transparent surfaces.
   The unlit version though works on transparent queue and beyond, thus works fine on transparent surfaces.

Versions:
1.0:   First release
1.1:   Add outline and overlay effects
1.2:   Added DecalRenderEnabler
       Added preview image and projector line on the decal handle
1.3:   Added normal strength settings to the lit shader
	   Lit shader now correctly recieves shadow
	   Modify lit shader to make the lighting better