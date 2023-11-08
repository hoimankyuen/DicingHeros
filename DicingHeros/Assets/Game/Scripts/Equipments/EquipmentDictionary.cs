using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DicingHeros
{
    public class EquipmentDictionary
    {
        [System.Serializable]
        public enum Name
        { 
            SimpleShoes,    
            ShortSword,
            SmallShield,

            QuickmansBoot,
            GreatSword,
            LongBow,

            MagesRing,
            Fireball,
            Thunderbolt,
        }

        public static Equipment NewEquipment(Name name, Unit owner)
        {
            switch (name)
            {
                case Name.SimpleShoes:
                    return new SimpleShoes(owner);
                case Name.ShortSword:
                    return new ShortSword(owner);
                case Name.SmallShield:
                    return new SmallShield(owner);

                case Name.QuickmansBoot:
                    return new QuickmansBoot(owner);
                case Name.GreatSword:
                    return new GreatSword(owner);
                case Name.LongBow:
                    return new LongBow(owner);

                case Name.MagesRing:
                    return new MagesRing(owner);
                case Name.Fireball:
                    return new Fireball(owner);
                case Name.Thunderbolt:
                    return new Thunderbolt(owner);

            }
            Debug.LogError("Equipment Not Found");
            return null;
        }
    }
}