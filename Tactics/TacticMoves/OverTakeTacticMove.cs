using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverTakeTacticMove : TacticMove
{
	public OverTakeTacticMove()
	{
		this.move = BotTacticMove.OverTake;
	}

	public override void PosibilityCheck(BotStrategyModel strategyModel)
	{
		isPosible = CanOvertake(strategyModel);
	}

	public override void CalcWeight(BotStrategyModel strategyModel)
	{
        weight = GetOvertakeWeight(strategyModel);
	}

	private bool CanOvertake(BotStrategyModel strategyModel)
    {
        foreach (var playerUnit in strategyModel.PlayerLeader.Allies)
        {
            var buffTiles = playerUnit.Position.GetBuffTilesInRange();

            foreach (var tile in buffTiles)
            {
                if (tile.HasEnemyInMoveRange(strategyModel.PlayerLeader))
                    return true;
            }
        }

        return false;
    }

	private int GetOvertakeWeight(BotStrategyModel strategyModel)
    {
        int weight = 5;

        foreach (var enemyTile in strategyModel.Leader.Position.GetEnemyTiles(strategyModel.Leader))
        {
            if (enemyTile.unitInTile.IsCharged)
                weight += 20;
        }

        foreach (var model in strategyModel.UnitModels)
        {
            if (model.Unit.IsCharged)
                weight += 5;
        }

        foreach (var enemy in strategyModel.PlayerLeader.Allies)
        {
            if (enemy.IsCharged)
                weight += 5;
        }

        return weight;
    }
}
