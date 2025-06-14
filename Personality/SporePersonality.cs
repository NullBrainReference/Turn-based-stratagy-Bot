using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SporePersonality : BotUnitPersonality
{
    public SporePersonality()
    {
        unitType = UnitType.Spore;
    }

    public override int GetAttackWeight(BotTileModel tileTo)
    {
        return -120;
    }

    public override int GetMoveWeight(BotTileModel tileTo, BotTileModel tileFrom)
    {
        int result = 0;

        result += tileTo.Tile.GetEnemyTiles(tileFrom.UnitModel.Unit).Count * 4;

        result += tileFrom.UnitModel.HasMoved ? -8 : 2; 

        return result;
    }
}
