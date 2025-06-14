using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseAbilityTacticMove : TacticMove
{
	public UseAbilityTacticMove()
	{
		move = BotTacticMove.UseAbility;
	}

	public override void PosibilityCheck(BotStrategyModel strategyModel)
	{
        BotUnitModel caster = strategyModel.UnitModels.Find(x => x.UnitPersonality as ICasterPersonality != null);

        if (caster != null)
			isPosible = true;
    }

	public override void CalcWeight(BotStrategyModel strategyModel)
	{
        weight = 7;
        if (strategyModel.Leader.Shield == false)
            weight += 5;
        if (strategyModel.Leader.IsFull == false)
            weight += 3;

        BotUnitModel caster = strategyModel.UnitModels.Find(x => x.UnitPersonality as ICasterPersonality != null);
        if (caster != null)
        {
            if (caster.Unit.IsCharged)
                weight += 25;
        }

        if (hasItemAtLeader(ItemType.Shield, strategyModel))
        {
            weight -= 20;
            return;
        }

        foreach (Unit unit in strategyModel.Leader.Allies)
        {
            foreach (Tile tile in unit.Position.MoveTiles)
            {
                if (tile.GetItemType() == ItemType.FireDamage)
                {
                    weight -= 15;
                    return;
                }
                else if (tile.GetItemType() == ItemType.IceDamage)
                {
                    weight -= 5;
                    return;
                }
                else if (tile.GetItemType() == ItemType.AirDamage)
                {
                    weight -= 5;
                    return;
                }
            }
        }
    }

	public override int GetMoveWeight(BotTileModel tileFrom, BotTileModel tileTo, int order)
	{
        if (tileFrom.UnitModel.UnitPersonality == null)
            return 0;
        if (tileFrom.UnitModel.UnitPersonality as ICasterPersonality == null)
            return 0;

        int weight = 0;

        if (tileTo.UnitModel == null)
            return 0;

        if (tileTo.UnitModel.UnitPersonality != null)
        {
            if (tileTo.UnitModel.UnitPersonality.UnitType != UnitType.Default)
                weight += 10;
        }

        if (tileTo.UnitModel.HasShield == false)
        {
            weight += 5;
            if (tileTo.Unit.IsLeader)
                weight += 40;
        }

        return weight;
    }

    public override UnitType GetMover()
    {
        return UnitType.Shield;
    }

    public override bool Match(ShortMoveModel shortMove)
    {
        bool match = shortMove.Unit.UnitType == UnitType.Shield;

        if (shortMove.MoveType != MoveType.Ability)
            return false;
        if (shortMove.Target.UnitType == UnitType.Default && shortMove.Target.IsLeader == false)
            return false;

        if (shortMove.Target.Leader.HasShield == false)
            if (shortMove.Target.IsLeader == false)
                return false;

        return match;
    }

    private bool hasItemAtLeader(ItemType itemType, BotStrategyModel strategyModel)
    {
        Unit leader = strategyModel.Leader;
        foreach (var tile in leader.Position.MoveTiles)
        {
            if (tile.GetItemType() == itemType)
                return true;
        }

        return false;
    }
}
