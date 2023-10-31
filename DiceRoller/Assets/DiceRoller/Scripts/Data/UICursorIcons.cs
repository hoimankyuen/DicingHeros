using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DiceRoller
{
    [CreateAssetMenu(fileName = "newCursorIcons", menuName = "Data/CursorIcons", order = 1)]
    public class UICursorIcons : ScriptableObject
    {
        [System.Serializable]
        public class IconEntry
        {
            public UICursor.IconType type;
            public Sprite icon;
        }

        [Header("Cursor")]
        public Sprite normalCursor;
        public Sprite iconCursor;

        [Header("Icon")]
        public Sprite action;
        public Sprite inspect;
        public Sprite pick;
        public Sprite throws;
        public Sprite movement;
        public Sprite physicalAttack;
        public Sprite rangedAttack;
        public Sprite magicalAttack;

        [HideInInspector]
        public Dictionary<UICursor.IconType, Sprite> icons = new Dictionary<UICursor.IconType, Sprite>();

        private void OnEnable()
        {
            icons = new Dictionary<UICursor.IconType, Sprite>();
            icons[UICursor.IconType.None] = null;
            icons[UICursor.IconType.Action] = action;
            icons[UICursor.IconType.Inspect] = inspect;
            icons[UICursor.IconType.Pick] = pick;
            icons[UICursor.IconType.Throw] = throws;
            icons[UICursor.IconType.Movement] = movement;
            icons[UICursor.IconType.PhysicalAttack] = physicalAttack;
            icons[UICursor.IconType.RangedAttack] = rangedAttack;
            icons[UICursor.IconType.MagicalAttack] = magicalAttack;
            
        }
    }
}