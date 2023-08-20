using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceRoller
{
    [CreateAssetMenu(fileName = "NewDieIcons", menuName = "DataDieIcons", order = 1)]
    public class UIDieIcons : ScriptableObject
    {
        [System.Serializable]
        public class DefaultIconEntry
        {
            public Die.Type type;
            public Sprite icon;
        }

        [SerializeField]
        protected List<DefaultIconEntry> entries = new List<DefaultIconEntry>();
        [HideInInspector]
        public Dictionary<Die.Type, Sprite> dieIcons = new Dictionary<Die.Type, Sprite>();

        protected void OnEnable()
        {
            dieIcons = new Dictionary<Die.Type, Sprite>();
            foreach(DefaultIconEntry entry in entries)
            {
                dieIcons[entry.type] = entry.icon;
            }
        }
    }
}