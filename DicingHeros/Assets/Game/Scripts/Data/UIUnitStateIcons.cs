using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DicingHeros
{
    [CreateAssetMenu(fileName = "NewUnitStateIcons", menuName = "Data/UnitStateIcons", order = 1)]
    public class UIUnitStateIcons : ScriptableObject
    {
        [System.Serializable]
        public class StateIconEntry
        {
            public Unit.UnitState state;
            public Sprite icon;
        }

        [SerializeField]
        protected List<StateIconEntry> entries = new List<StateIconEntry>();
        [HideInInspector]
        public Dictionary<Unit.UnitState, Sprite> stateIcons = new Dictionary<Unit.UnitState, Sprite>();

        protected void OnEnable()
        {
            stateIcons = new Dictionary<Unit.UnitState, Sprite>();
            foreach(StateIconEntry entry in entries)
            {
                stateIcons[entry.state] = entry.icon;
            }
        }
    }
}