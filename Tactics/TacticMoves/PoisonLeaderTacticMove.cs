using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonLeaderTacticMove : TacticMove
{
    public PoisonLeaderTacticMove()
    {
        move = BotTacticMove.PoisonLeader;
    }

    public override void PosibilityCheck(BotStrategyModel strategyModel, int order)
    {
        var models = strategyModel.UnitModels;

        if (RevealLocalUtil.Instance.Order > 1) //Avoid revealing
        {
            if (RevealLocalUtil.Instance.Order == order)
            {
                isPosible = false;
                return;
            }
        }

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
        weight = 40;

        if (strategyModel.Plan == GamePlan.Wild)
            weight += 20;

        if (strategyModel.PlayerLeader.Poison)
            weight -= 30;
    }

    public override UnitType GetMover()
    {
        return UnitType.Poison;
    }

    public override bool Match(ShortMoveModel shortMove)
    {
        bool match = base.Match(shortMove);

        if (shortMove.MoveType == MoveType.Attack)
        {
            if (shortMove.Unit.IsLeader == false)
                return false;
        }
        else if (shortMove.MoveType == MoveType.Move)
        {
            if (shortMove.Tile.HasEnemyInRange(shortMove.Unit, UnitType.DefaultLeader, false) == false)
                return false;
        }

        return match;
    }

    private bool HasMove(List<BotUnitModel> models)
    {
        foreach (var model in models)
        {
            if (model.Unit.UnitType != UnitType.Poison)
                continue;

            foreach (var pos in model.Unit.Position.MoveTiles)
            {
                if (pos.IsEmpty() == false)
                    continue;
                
                if (model.IsCharged == false)
                {
                    if (pos.IsCharged == false)
                        continue;
                }

                if (pos.HasEnemyInRange(model.Unit, true, false))
                {
                    foreach (var enemyPos in pos.GetEnemyTiles(model.Unit))
                    {
                        if (enemyPos.unitInTile.IsLeader)
                            return true;
                    }
                }
            }
        }
        return false;
    }

    private bool HasAttack(List<BotUnitModel> models)
    {
        foreach (var model in models)
        {
            if (model.Unit.UnitType != UnitType.Poison)
                continue;
            if (model.IsCharged == false)
                continue;

            if (model.Unit.Position.HasEnemyInRange(model.Unit))
            {
                foreach (var enemyPos in model.Unit.Position.GetEnemyTiles(model.Unit))
                {
                    if (enemyPos.unitInTile.IsLeader)
                        return true;
                }
            }
        }

        return false;
    }
}
