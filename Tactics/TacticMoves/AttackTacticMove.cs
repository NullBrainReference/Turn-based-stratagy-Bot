using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTacticMove : TacticMove
{
	public AttackTacticMove()
	{
		move = BotTacticMove.Attack;
	}

	public override void PosibilityCheck(BotStrategyModel strategyModel)
	{
		isPosible = HasAttacker(strategyModel);
    }

	public override void CalcWeight(BotStrategyModel strategyModel)
	{
        weight = GetAttackWeight(strategyModel);
    }

	private bool HasAttacker(BotStrategyModel strategyModel)
    {
        bool hasAttacker = false;

        foreach (var model in strategyModel.UnitModels)
        {
            if (model.Unit.Position.HasEnemyInRange(model.Unit))
                if (model.IsCharged)
                    hasAttacker = true;
        }

        return hasAttacker;
    }

	private int GetAttackWeight(BotStrategyModel strategyModel)
    {
        int weight = 0;

        foreach (var model in strategyModel.UnitModels)
        {
            if (model.Unit.Position.HasEnemyInRange(model.Unit) == false)
                continue;

            if (model.IsCharged == false)
                continue;

            foreach (var tile in model.Unit.Position.GetEnemyTiles(model.Unit))
            {
                if (tile.unitInTile == null)
                    continue;

                if (tile.unitInTile.Health <= 1)
                {
                    weight += 40;

                    if (tile.unitInTile.IsCharged)
                        weight += 20;
                }


                if (tile.unitInTile.IsLeader)
                {
                    if (tile.unitInTile.Health <= 1)
                        weight += 80;

                    weight += 10;
                }
            }
        }

        return weight;
    }
}
