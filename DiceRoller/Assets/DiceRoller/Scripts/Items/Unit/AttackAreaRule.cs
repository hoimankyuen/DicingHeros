using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceRoller
{
    public class AttackAreaRule : IEquatable<AttackAreaRule>
    {
        private Func<Tile, Tile, bool> rule = null; // target, starting, result
        private int range = 0;
        public AttackAreaRule(Func<Tile, Tile, bool> rule, int range)
        {
            this.rule = rule;
            this.range = range;
        }

        public bool Equals(AttackAreaRule other)
        {
            return this == other;
        }

        public bool Evaulate(Tile target, Tile starting)
        {
            return rule(target, starting);
        }

        public int GetRange()
        {
            return range;
        }
    }
}