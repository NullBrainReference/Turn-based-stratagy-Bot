using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnTacticMove : TacticMove
{
    public RespawnTacticMove()
    {
        this.move = BotTacticMove.Respawn;
    }

    public override void PosibilityCheck(BotStrategyModel strategyModel)
    {
        isPosible = CanRespawn(strategyModel.UnitModels);
    }

    public override void CalcWeight(BotStrategyModel strategyModel)
    {
        weight = 35;
    }

    private bool CanRespawn(List<BotUnitModel> models)
    {
        foreach (var model in models)
        {
            if (model.Unit.IsLeader)
                continue;
            if (model.IsCharged == false)
                continue;

            if (model.Unit.UnitType == UnitType.Default)
                return true;
        }

        return false;
    }
}
