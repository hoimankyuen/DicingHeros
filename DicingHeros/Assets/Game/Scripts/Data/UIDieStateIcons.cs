using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DicingHeros
{
    [CreateAssetMenu(fileName = "NewDieStateIcons", menuName = "Data/DieStateIcons", order = 1)]
    public class UIDieStateIcons : ScriptableObject
    {
        [System.Serializable]
        public class StateIconEntry
        {
            public Die.DieState state;
            public Sprite icon;
        }

        [SerializeField]
        protected List<StateIconEntry> entries = new List<StateIconEntry>();
        [HideInInspector]
        public Dictionary<Die.DieState, Sprite> stateIcons = new Dictionary<Die.DieState, Sprite>();

        protected void OnEnable()
        {
            stateIcons = new Dictionary<Die.DieState, Sprite>();
            foreach(StateIconEntry entry in entries)
            {
                stateIcons[entry.state] = entry.icon;
            }
        }
    }
}