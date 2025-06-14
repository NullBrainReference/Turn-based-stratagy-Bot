using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseLeaderTacticMove : TacticMove
{
	public ChaseLeaderTacticMove()
	{
		this.move = BotTacticMove.ChaseLeader;
	}

	public override void PosibilityCheck(BotStrategyModel strategyModel, int order)
	{
        if (order == RevealLocalUtil.Instance.Order)
        {
            isPosible = false;
            return;
        }
        isPosible = CanChaseLeader(strategyModel.UnitModels);
	}

    public override void CalcWeight(BotStrategyModel strategyModel)
    {
        weight = 14;

        //if (strategyModel.PlayerLeader.Health <= 3)
        //    weight += 20;
        if (strategyModel.Plan == GamePlan.Wild)
            weight += 10;

        if (strategyModel.PlayerLeader.Health <= 1)
            weight += 16;
    }

    private bool CanChaseLeader(List<BotUnitModel> models)
    {
        foreach (var model in models)
        {
            if (model.Unit.IsCharged == false)
                continue;

            foreach (var tile in model.Unit.Position.MoveTiles)
            {
                if (tile.IsEmpty() == false)
                    continue;

                foreach (var enemyTile in tile.GetEnemyTiles(model.Unit))
                {
                    if (enemyTile.unitInTile.IsCharged)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}
