using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThornPersonality : BotUnitPersonality
{
    public ThornPersonality()
    {
        unitType = UnitType.Thorn;
    }

    public override int GetAttackWeight(BotTileModel tileTo)
    {
        return -15;
    }

    public override int GetMoveWeight(BotTileModel tileTo, BotTileModel tileFrom)
    {
        if (tileFrom.UnitModel.IsCharged)
            return 0;

        return 5;
    }
}
