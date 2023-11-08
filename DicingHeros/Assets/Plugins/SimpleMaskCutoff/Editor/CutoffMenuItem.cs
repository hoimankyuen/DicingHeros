using UnityEngine;
using UnityEditor;

namespace SimpleMaskCutoff
{
    public class CutoffMenuItem
    {
        [MenuItem("GameObject/2D Object/Cutoff Sprite")]
        private static void CreateNewCutoffSprite()
        {
            GameObject newCutoffSprite = new GameObject("New Cutoff Sprite");
            newCutoffSprite.AddComponent<CutoffSpriteRenderer>();
        }
    }
}
