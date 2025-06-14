using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionTacticMove : TacticMove
{
    public PositionTacticMove()
    {
        move = BotTacticMove.ChargePosition;
    }

    public override void PosibilityCheck(BotStrategyModel strategyModel)
    {
        var models = strategyModel.UnitModels;

        if (HasAttack(models))
        {
            isPosible = true;
            return;
        }

        if (HasMove(models))
        {
            isPosible = true;
            return;
        }
    }

    public override void CalcWeight(BotStrategyModel strategyModel)
    {
        weight = 70;
    }

    public override UnitType GetMover()
    {
        return UnitType.Poison;
    }

    public override bool Match(ShortMoveModel shortMove)
    {
        bool match = base.Match(shortMove);

        return match;
    }

    protected bool HasAttack(List<BotUnitModel> models)
    {
        foreach (var model in models)
        {
            if (model.IsCharged == false)
                continue;

            if (model.RangeDamage > 1)
            {
                if (model.Unit.Position.HasEnemyInRange(model.Unit, true, false))
                {
                    return true;
                }
            }
            else if (model.MeleeDamage > 1)
            {
                if (model.Unit.Position.HasEnemyInRange(model.Unit, false, true))
                {
                    return true;
                }
            }
            
        }

        return false;
    }

    protected virtual bool HasMove(List<BotUnitModel> models)
    {
        foreach (var model in models)
        {
            foreach (var pos in model.Unit.Position.MoveTiles)
            {
                if (pos.IsEmpty() == false)
                    continue;

                if (model.RangeDamage > 1)
                {
                    if (pos.HasEnemyInRange(model.Unit, true, false))
                    {
                        return true;
                    }
                }
                else if (model.MeleeDamage > 1)
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
}
