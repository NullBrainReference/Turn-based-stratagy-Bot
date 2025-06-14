using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReroutTacticMove : TacticMove
{
	public ReroutTacticMove()
	{
		move = BotTacticMove.Rerout;
	}

	public override void PosibilityCheck(BotStrategyModel strategyModel, int order)
	{
		isPosible = order != RevealLocalUtil.Instance.Order;
	}

	public override void CalcWeight(BotStrategyModel strategyModel)
	{
		weight = 15;

		if (HasEnemyAtItem(ItemType.FireDamage, strategyModel))
			weight += 8;
		else if (HasEnemyAtItem(ItemType.Rage, strategyModel))
			weight += 7;
	}

	public override int GetMoveWeight(BotTileModel tileFrom, BotTileModel tileTo, int order)
	{
		int result = 0;
		if (tileFrom.UnitModel.HasMoved == false)
			result += tileFrom.Unit.IsLeader ? 120 : 60;

		return result;
	}

	private bool HasEnemyAtItem(ItemType itemType, BotStrategyModel strategyModel)
	{
        foreach (var unit in strategyModel.PlayerLeader.Allies)
        {
            foreach (var tile in unit.Position.MoveTiles)
            {
                if (tile.GetItemType() == itemType)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
