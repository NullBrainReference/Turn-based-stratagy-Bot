using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotActionRespawn : BotAction
{
    public BotActionRespawn(int order) : base(order)
    {

    }

    public override void CalcWeight(
        BotTileModel tileTo, BotTileModel tileFrom,
        BotFieldModel fieldModel, BotStrategyModel strategyModel,
        TacticMove tacticMove = null)
    {
        SetTiles(tileTo, tileFrom);

        action = MoveType.Respawn;

        BotTacticMove moveType = BotTacticMove.Any;
        if (tacticMove != null)
            moveType = tacticMove.Move;

        if (order > 1)
            weight += tileFrom.Unit.Health <= 1 ? -55 : 0;
        else
            weight += tileFrom.Unit.Health <= 1 ? 20 : 5;

        if (tileFrom.Tile.IsSuddenDeathTarget)
            weight += tileFrom.Unit.Health <= 1 ? 20 : 0;

        if (strategyModel.Plan == GamePlan.Wild)
        {
            weight += -30;
            if (tileFrom.Unit.unitManager.GetSpawner(tileFrom.Unit.Name).UnitType == UnitType.Shield)
                weight += -50;
            else if (tileFrom.Unit.unitManager.GetSpawner(tileFrom.Unit.Name).UnitType == UnitType.Poison)
                weight += 35;
        }

        if (tileFrom.UnitModel.HasShield)
            weight -= 16;

        if (tileFrom.Unit.unitManager.GetSpawner(tileFrom.Unit.Name).UnitType == UnitType.Shield)
            weight += -10;

        if (moveType == BotTacticMove.Rerout)
            weight += 50;
        if (moveType == BotTacticMove.Respawn)
            weight += 55;

        weight -= strategyModel.AwarenessModel.GetClosestDangerWeight(tileFrom) / 4;

        weight += strategyModel.RespawnWeightBonus;

        weight += 12;
    }

    public override void PushAction(ActionManager actionManager, BotStrategyModel strategyModel = null)
    {
        actionManager.CreateRespawnActionHidden(tileFrom.Unit, order);

        tileFrom.UnitModel.Move();
        tileFrom.UnitModel.Respawn();
        tileFrom.UnitModel.ResetDamage();
        tileFrom.UnitModel.DropCharge();

        if (strategyModel != null)
        {
            strategyModel.Move();
            strategyModel.Respawn();
        }
    }
}
