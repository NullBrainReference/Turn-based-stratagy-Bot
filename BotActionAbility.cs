using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotActionAbility : BotAction
{
    public BotActionAbility(int order) : base(order)
    {

    }

    public override void CalcWeight(
        BotTileModel tileTo, BotTileModel tileFrom,
        BotFieldModel fieldModel, BotStrategyModel strategyModel,
        TacticMove tacticMove = null)
    {
        SetTiles(tileTo, tileFrom);

        action = MoveType.Ability;

        BotTacticMove moveType = BotTacticMove.Any;
        if (tacticMove != null) 
        { 
            moveType = tacticMove.Move;
            weight += tacticMove.GetMoveWeight(tileFrom, tileTo, order);
        }

        ICasterPersonality casterPersonality = tileFrom.UnitModel.UnitPersonality as ICasterPersonality;
        if (casterPersonality != null)
        {
            weight += 8;
            weight += casterPersonality.GetAbilityWeight(tileFrom, tileTo);
        }

        if (moveType == BotTacticMove.Rerout)
            weight += 20;

        //if (tileFrom.Unit.IsLeader)
        //{
        //    weight += tileFrom.UnitModel.HasMoved ? -1 : 2;
        //    if (order == 3)
        //        weight += tileFrom.UnitModel.HasMoved ? 0 : 90;
        //}
    }

    public override void PushAction(ActionManager actionManager, BotStrategyModel strategyModel = null)
    {
        actionManager.CreateAbilityActionHidden(tileFrom.Unit, tileTo.Unit, order);
        tileFrom.UnitModel.Move();

        ICasterPersonality casterPersonality = tileFrom.UnitModel.UnitPersonality as ICasterPersonality;
        if (casterPersonality != null)
            casterPersonality.OnPush(tileFrom, tileTo);

        tileFrom.UnitModel.ResetDamage();
        tileFrom.UnitModel.DropCharge();

        if (strategyModel != null)
            strategyModel.Move();
    }
}
