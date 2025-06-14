using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeTacticMove : TacticMove
{
	public ChargeTacticMove()
	{
		move = BotTacticMove.MoveToCharge;
	}

	public override void PosibilityCheck(BotStrategyModel strategyModel)
	{
        isPosible = CanCharge(strategyModel.UnitModels);
	}

	public override void CalcWeight(BotStrategyModel strategyModel)
	{
        weight += GetChargeWeight(strategyModel);
    }

	private bool CanCharge(List<BotUnitModel> models)
    {
        foreach (var model in models)
            if (model.Unit.IsCharged)
                return false;

        foreach (var model in models)
        {
            foreach (var tile in model.Unit.Position.MoveTiles)
            {
                if (tile.IsEmpty() == false)
                    continue;

                foreach (var enemyTile in model.Unit.Position.GetEnemyTiles(model.Unit))
                {
                    if (enemyTile.unitInTile.IsLeader)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private int GetChargeWeight(BotStrategyModel strategyModel)
    {
        if (strategyModel.PlayerLeader.Health <= 1)
        {
            return 55;
        }
        else if (strategyModel.PlayerLeader.Health <= 3)
        {
            return 35;
        }

        return 5;
    }
}
