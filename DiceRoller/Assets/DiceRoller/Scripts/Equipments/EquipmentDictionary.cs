using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceRoller
{
    public class EquipmentDictionary
    {
        [System.Serializable]
        public enum Name
        { 
            SimpleShoe,
            SimpleKnife,
            Fireball,
        }

        public static Equipment NewEquipment(Name name, Unit owner)
        {
            switch (name)
            {
                case Name.SimpleShoe:
                    return new SimpleShoe(owner);
                case Name.SimpleKnife:
                    return new SimpleKnife(owner);
                case Name.Fireball:
                    return new Fireball(owner);
            }
            Debug.LogError("Equipment Not Found");
            return null;
        }

        public static GameObject GetUIPrefab(Name name)
        {
            switch (name)
            {
                case Name.SimpleShoe:
                    return Resources.Load("UISimpleKnife") as GameObject;
                case Name.SimpleKnife:
                    return Resources.Load("UISimpleKnife") as GameObject;
                case Name.Fireball:
                    return Resources.Load("UIFireball") as GameObject;
            }
            Debug.LogError("Equipment Not Found");
            return null;
        }
    }
}