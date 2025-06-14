using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistantLeaderTacticMove : TacticMove
{
	public DistantLeaderTacticMove()
	{
		move = BotTacticMove.DistantLeader;
	}

	public override void PosibilityCheck(BotStrategyModel strategyModel)
	{
        if (strategyModel.Leader.Shield)
        {
            isPosible = false;
            return;
        }
        isPosible = CanDistantLeader(strategyModel);
	}

	public override void CalcWeight(BotStrategyModel strategyModel)
	{
        weight = 16;
        if (strategyModel.Leader.Health >= 5)
            weight -= 8;
    }

    public override UnitType GetMover()
    {
        return UnitType.DefaultLeader;
    }

    public override bool Match(ShortMoveModel shortMove)
    {
        bool match = base.Match(shortMove);

        if (shortMove.MoveType != MoveType.Move)
            return false;

        return match;
    }

    private bool CanDistantLeader(BotStrategyModel strategyModel)
    {
        bool result = false;

        //if (strategyModel.Leader.Position.HasEnemyInRange(strategyModel.Leader))
           //return false;

        foreach (var tile in strategyModel.Leader.Position.GetEnemyTiles(strategyModel.Leader))
        {
            if (tile.unitInTile.IsCharged)
                return true;
        }

        //foreach (var playerUnit in strategyModel.PlayerLeader.Allies)
        //{
        //    var buffTiles = playerUnit.Position.GetBuffTilesInRange();
        //    if (buffTiles.Count == 0)
        //        continue;
        //
        //    result = true;
        //}
        //
        //foreach (var playerUnit in strategyModel.PlayerLeader.Allies)
        //{
        //    var buffTiles = playerUnit.Position.GetBuffTilesInRange();
        //
        //    foreach (var tile in buffTiles)
        //    {
        //        if (tile.HasEnemyInMoveRange(strategyModel.PlayerLeader))
        //            result = false;
        //    }
        //}

        return result;
    }
}
