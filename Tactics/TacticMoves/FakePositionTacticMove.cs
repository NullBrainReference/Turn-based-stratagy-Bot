using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakePositionTacticMove : PositionTacticMove
{
    public FakePositionTacticMove()
    {
        move = BotTacticMove.FakePosition;
    }

    public override void PosibilityCheck(BotStrategyModel strategyModel, int order)
    {
        if (order != RevealLocalUtil.Instance.Order)
        {
            isPosible = false;
            return;
        }

        var models = strategyModel.UnitModels;

        if (HasAttack(models))
        {
            isPosible = false;
            return;
        }
        if (HasLeaderToAttack(models))
        {
            isPosible = false;
            return;
        }

        if (HasMove(models))
        {
            isPosible = true;
        }
    }

    public override void CalcWeight(BotStrategyModel strategyModel)
    {
        weight = 55;
        
        foreach (var model in strategyModel.UnitModels)
        {
            weight += model.IsCharged ? -20 : 0;
        }
    }

    protected override bool HasMove(List<BotUnitModel> models)
    {
        foreach (var model in models)
        {
            foreach (var pos in model.Unit.Position.MoveTiles)
            {
                if (pos.IsEmpty() == false)
                    continue;

                if (model.RangeDamage >= 1)
                {
                    if (pos.HasEnemyInRange(model.Unit, true, false))
                    {
                        return true;
                    }
                }
                else if (model.MeleeDamage >= 1)
                {
                    if (pos.HasEnemyInRange(model.Unit, false, true))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private bool HasLeaderToAttack(List<BotUnitModel> models)
    {
        foreach (BotUnitModel model in models)
        {
            if (model.IsCharged == false)
                continue;

            foreach (Tile tile in model.Unit.Position.GetEnemyTiles(model.Unit))
            {
                if (tile.unitInTile.IsLeader)
                    return true;
            }
        }

        return false;
    }
}
