using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectBuffTacticMove : CollectTacticMove
{
	public CollectBuffTacticMove()
	{
		move = BotTacticMove.CollectBuff;
	}

	public override void PosibilityCheck(BotStrategyModel strategyModel)
	{
        if (HasUnitAtItem(ItemType.Rage, strategyModel))
            isPosible = true;
        if (HasUnitAtItem(ItemType.Range, strategyModel))
            isPosible = true;
        if (HasUnitAtItem(ItemType.Melee, strategyModel))
            isPosible = true;
        if (HasUnitAtItem(ItemType.FireDamage, strategyModel))
            isPosible = true;
    }

	public override void CalcWeight(BotStrategyModel strategyModel)
	{
        weight = GetBuffWeight(strategyModel.UnitModels);
    }

	private int GetBuffWeight(List<BotUnitModel> models)
    {
        int weight = 0;

        foreach (var model in models)
        {
            if (model.Unit.IsCharged)
            {
                weight += 10;

                if (model.Unit.Position.HasEnemyInRange(model.Unit))
                {
                    weight += 10;
                }
            }
        }

        return weight;
    }
}
