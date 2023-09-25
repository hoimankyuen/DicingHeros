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

        public Sprite normalCursor;
        public Sprite iconCursor;

        [SerializeField]
        protected List<IconEntry> entries = new List<IconEntry>();
        [HideInInspector]
        public Dictionary<UICursor.IconType, Sprite> icons = new Dictionary<UICursor.IconType, Sprite>();

        protected void OnEnable()
        {
            icons = new Dictionary<UICursor.IconType, Sprite>();
            foreach (IconEntry entry in entries)
            {
                icons[entry.type] = entry.icon;
            }
        }
    }
}