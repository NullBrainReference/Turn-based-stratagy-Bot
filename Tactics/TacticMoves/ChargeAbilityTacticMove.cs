using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeAbilityTacticMove : TacticMove
{
	public ChargeAbilityTacticMove()
	{
		move = BotTacticMove.ChargeAbility;
	}

	public override void PosibilityCheck(BotStrategyModel strategyModel, int order)
	{
		BotUnitModel caster = strategyModel.UnitModels.Find(x => x.UnitPersonality as ICasterPersonality != null);

		if (order == RevealLocalUtil.Instance.Order)
		{
            isPosible = false;
            return;
        }

		if (caster != null)
			if (caster.IsCharged == false)
				isPosible = true;

        if (hasItemAtLeader(ItemType.Shield, strategyModel))
		{
			isPosible = false;
			return;
		}

    }

	public override void CalcWeight(BotStrategyModel strategyModel)
	{
		weight = 5;
		if (strategyModel.Leader.Shield == false)
			weight += 5;
        if (strategyModel.Leader.IsFull == false)
            weight += 2;
		if (hasItemAtLeader(ItemType.Shield, strategyModel))
			weight -= 100;
        if (strategyModel.Plan == GamePlan.Wild)
            weight -= 30;
        else
			weight += 3;

    }

	public override UnitType GetMover()
	{
		return UnitType.Shield;
	}

	public override bool Match(ShortMoveModel shortMove)
	{
		bool match = base.Match(shortMove);

		if (shortMove.MoveType != MoveType.Move)
			return false;
		if (shortMove.Tile.IsCharged == false)
			return false;

		return match;
	}

	public override int GetMoveWeight(BotTileModel tileFrom, BotTileModel tileTo, int order)
	{
		if (tileFrom.UnitModel.UnitPersonality == null)
			return 0;
		if (tileFrom.UnitModel.UnitPersonality as ICasterPersonality == null)
			return 0;

		if (tileTo.Tile.IsCharged)
			return 50;

		return 0;
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
