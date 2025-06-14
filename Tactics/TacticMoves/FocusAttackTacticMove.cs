using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusAttackTacticMove : TacticMove
{
	public FocusAttackTacticMove()
	{
		move = BotTacticMove.FocuseAttack;
	}

	public override void PosibilityCheck(BotStrategyModel strategyModel)
	{
		isPosible = HasFocus(strategyModel);
	}

	public override void CalcWeight(BotStrategyModel strategyModel)
	{
		weight = GetFocuseWeight(strategyModel);
    }

	private bool HasFocus(BotStrategyModel strategyModel)
    {
        bool hasPray = false;

        foreach (var model in strategyModel.UnitModels)
        {
            if (model.Unit.Position.HasEnemyInRange(model.Unit) == false)
                continue;

            if (model.IsCharged)
            {
                foreach (Tile tile in model.Unit.Position.GetEnemyTiles(model.Unit))
                {
                    int attackersCount = 0;
                    foreach (Tile pos in tile.GetEnemyTiles(tile.unitInTile))
                    {
                        if (pos.unitInTile.IsCharged)
                            attackersCount++;
                    }

                    if (attackersCount >= 2)
                        hasPray = true;
                }
            }
        }

        return hasPray;
    }

    private int GetFocuseWeight(BotStrategyModel strategyModel)
    {
        int weight = 20;

        foreach (var model in strategyModel.UnitModels)
        {
            if (model.Unit.Position.HasEnemyInRange(model.Unit) == false)
                continue;

            if (model.IsCharged)
            {
                foreach (Tile tile in model.Unit.Position.GetEnemyTiles(model.Unit))
                {
                    int attackersCount = 0;
                    foreach (Tile pos in tile.GetEnemyTiles(tile.unitInTile))
                    {
                        if (pos.unitInTile.IsCharged)
                            attackersCount++;
                    }

                    if (attackersCount >= 2)
                    {
                        if (tile.unitInTile.IsLeader)
                        {
                            if (tile.unitInTile.Health <= 2)
                                weight += 45;
                            else
                                weight += 15;

                            return weight;
                        }
                    }
                }
            }
        }

        return weight;
    }
}
