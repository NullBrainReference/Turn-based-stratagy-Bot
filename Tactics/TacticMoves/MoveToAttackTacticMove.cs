using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToAttackTacticMove : TacticMove
{
	public MoveToAttackTacticMove()
	{
		move = BotTacticMove.MoveToAttack;
	}

	public override void PosibilityCheck(BotStrategyModel strategyModel)
	{
		isPosible = HasAttackOnNextMove(strategyModel.UnitModels);
	}

	public override void CalcWeight(BotStrategyModel strategyModel)
	{
		weight = 10;
	}

	private bool HasAttackOnNextMove(List<BotUnitModel> models)
    {
        // is not correct for all situations
        foreach (var model in models)
            if (model.IsCharged)
                return true;

        return false;
    }
}
