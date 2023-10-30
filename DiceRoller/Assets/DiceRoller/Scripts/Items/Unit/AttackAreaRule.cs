using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceRoller
{
    public class AttackAreaRule : IEquatable<AttackAreaRule>
    {
        private Func<Tile, Tile, int, bool> rule = null; // target, starting, range, result
        public AttackAreaRule(Func<Tile, Tile, int, bool> rule)
        {
            this.rule = rule;
        }
        public bool Equals(AttackAreaRule other)
        {
            return this == other;
        }
        public bool Evaulate(Tile target, Tile starting, int range)
        {
            return rule(target, starting, range);
        }

        public static readonly AttackAreaRule Adjacent = new AttackAreaRule((target, starting, range) => Int2.GridDistance(target.BoardPos, starting.BoardPos) <= range);
    }
}