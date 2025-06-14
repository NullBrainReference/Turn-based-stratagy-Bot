using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TNTPersonality : BotUnitPersonality
{
    public TNTPersonality()
    {
        unitType = UnitType.TNT;
    }

    public override int GetMoveWeight(BotTileModel tileTo, BotTileModel tileFrom)
    {
        int result = 10;

        int fromCount = tileFrom.Tile.GetEnemyTiles(tileFrom.UnitModel.Unit).Count;
        int toCount = tileTo.Tile.GetEnemyTiles(tileFrom.UnitModel.Unit).Count;

        bool isStayBetter = toCount < fromCount;

        if (tileFrom.Unit.Health <= 1)
            result += 50;

        if (tileFrom.UnitModel.HasMoved)
        {
            result -= 100;
        }
        var aw_model = tileFrom.UnitModel.StrategyModel.AwarenessModel;
        result +=
            aw_model.GetClosestDangerWeight(tileTo, tileFrom.Unit) -
            aw_model.GetClosestDangerWeight(tileFrom);

        if (toCount <= 0 && fromCount <= 0)
            result += 10;

        if (isStayBetter)
            result -= tileFrom.Tile.GetEnemyTiles(tileFrom.UnitModel.Unit).Count * 4;
        else
            result += tileTo.Tile.GetEnemyTiles(tileFrom.UnitModel.Unit).Count * 4;

        return result;
    }
}
