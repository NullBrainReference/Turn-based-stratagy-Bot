using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseDancerTacticMove : TacticMove
{
	public ChaseDancerTacticMove()
	{
		this.move = BotTacticMove.ChaseDancer;
	}

	public override void PosibilityCheck(BotStrategyModel strategyModel, int order)
	{
        if (order == RevealLocalUtil.Instance.Order)
        {
            isPosible = false;
            return;
        }
		isPosible = HasDancer(strategyModel.UnitModels);
	}

	public override void CalcWeight(BotStrategyModel strategyModel)
	{
        weight = 8;
        if (strategyModel.Plan == GamePlan.Wild)
            weight += 5;
        foreach (var unit in strategyModel.PlayerLeader.Allies)
            weight += unit.UnitType != UnitType.Default ? 3 : 0; 
	}

    public override int GetMoveWeight(BotTileModel tileFrom, BotTileModel tileTo, int order)
    {
        if (tileTo.Unit == null)
        {
            return 3 - Mathf.Abs(tileTo.Tile.Xcoord) + 3 - Mathf.Abs(tileTo.Tile.Xcoord);
        }

        return 0;
    }

    private bool HasDancer(List<BotUnitModel> models)
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
                    if (enemyTile.HasEnemyInRange(enemyTile.unitInTile))
                        continue;

                    return true;
                }
            }
        }

        return false;
    }
}
