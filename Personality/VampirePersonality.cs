using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VampirePersonality : BotUnitPersonality
{
    public VampirePersonality()
    {
        unitType = UnitType.Vampire;
    }

    public override int GetAttackWeight(BotTileModel tileTo)
    {
        return 4;
    }
}
