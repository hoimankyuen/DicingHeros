using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DiceRoller
{
    [CreateAssetMenu(fileName = "NewItemEffectStyle", menuName = "Data/ItemEffectStyle", order = 1)]
    public class ItemEffectStyle : ScriptableObject
    {
        [System.Serializable]
        public class ItemEffect
        {
            public Item.EffectType effectType;
            public Color outlineColor;
            public Color overlayColor;
        }

        public List<ItemEffect> itemEffectStack = new List<ItemEffect>();


        public void ResolveEffect(IReadOnlyCollection<Item.EffectType> effectList, out Color outlineColor, out Color overlayColor)
        {
            // resolve outline color
            outlineColor = new Color(0, 0, 0, 0);
            for (int i = itemEffectStack.Count - 1; i>= 0; i--)
            {
                if (itemEffectStack[i].outlineColor.a != 0 && effectList.Contains(itemEffectStack[i].effectType))
                {
                    outlineColor = itemEffectStack[i].outlineColor;
                    break;
                }
            }

            // resolve overlay color
            overlayColor = new Color(0, 0, 0, 0);
            for (int i = itemEffectStack.Count - 1; i >= 0; i--)
            {
                if (itemEffectStack[i].overlayColor.a != 0 && effectList.Contains(itemEffectStack[i].effectType))
                {
                    overlayColor = itemEffectStack[i].overlayColor;
                    break;
                }
            }
        }
    }
}