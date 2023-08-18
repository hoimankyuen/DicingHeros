using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceRoller
{
    [CreateAssetMenu(fileName = "DefaultDieIconEntries", menuName = "Data/DefaultDieIconEntries", order = 1)]
    public class UIDefaultDieIconEntries : ScriptableObject
    {
        [System.Serializable]
        public class DefaultIconEntry
        {
            public Die.Type type;
            public Sprite icon;
        }

        public List<DefaultIconEntry> entries = new List<DefaultIconEntry>();
    }
}