using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveAllCostTacticMove : TacticMove
{
	public SaveAllCostTacticMove()
	{
		move = BotTacticMove.SaveAllCost;
	}

	public override void PosibilityCheck(BotStrategyModel strategyModel, int order)
	{
		Unit leader = strategyModel.Leader;

		if (order > 1)
		{
            isPosible = false;
            return;
        }

        if (leader.Health > 1)
		{
			isPosible = false;
			return;
		}
		else if (leader.Shield)
		{
            isPosible = false;
            return;
        }

		foreach (var tile in leader.Position.GetEnemyTiles(leader))
		{
			if (tile.unitInTile.IsCharged)
			{
				isPosible = true;
                return;
            }
		}

		foreach (var tile in leader.Position.GetEnemyTiles(leader, true))
		{
			if (tile.unitInTile.IsRanger)
			{
				isPosible = true;
				return;
			}
		}
	}

	public override void CalcWeight(BotStrategyModel strategyModel)
	{
		weight = 30;
	}

	public override int GetMoveWeight(BotTileModel tileFrom, BotTileModel tileTo, int order)
	{
		int weight = 24;

		if (tileFrom.Unit.IsLeader == false)
			return 0;

		if (tileTo.Tile.HasEnemy(tileFrom.Unit))
			return tileTo.Unit.Health <= 1 ? 10 : -20;
		else if (tileTo.Unit != null)
			return tileTo.Unit.IsLeader ? 20 : -20;

		foreach (var tile in tileTo.Tile.GetEnemyTiles(tileFrom.Unit))
			weight += tile.unitInTile.IsCharged ? -20 : 0;

		foreach (var tile in tileTo.Tile.RangeTiles)
		{
			if (tile.HasEnemy(tileFrom.Unit) == false)
				continue;
			if (tile.unitInTile.IsCharged == false)
				continue;
			if (tile.unitInTile.IsRanger == false)
				continue;

			weight += tile.unitInTile.IsCharged ? -20 : 0;
		}

        return weight;
	}

}
