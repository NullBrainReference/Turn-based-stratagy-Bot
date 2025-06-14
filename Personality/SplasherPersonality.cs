using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplasherPersonality : BotUnitPersonality
{
    public SplasherPersonality()
    {
        unitType = UnitType.Splasher;
    }

    public override int GetAttackWeight(BotTileModel tileTo)
    {
        int result = 0;

        result += 5;

        if (tileTo.Unit.UnitType == UnitType.Coward)
        {
            if (tileTo.IsAttacked == false)
            {
                result -= 12;
            }
        }
        else
        {
            foreach (var enemy in tileTo.Unit.Allies)
            {
                if (enemy.Health <= 1)
                    result += 10;
            }
        }

        return result;
    }
}
