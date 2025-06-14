using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangePoisonTacticMove : CollectTacticMove
{
	public RangePoisonTacticMove()
	{
		move = BotTacticMove.RangePoison;
	}

	public override void PosibilityCheck(BotStrategyModel strategyModel)
	{
		isPosible = false;

		if (HasUnitAtItem(ItemType.Range, strategyModel) == false)
		{
			isPosible = false;
			return;
		}

		foreach (var unit in strategyModel.Leader.Allies)
		{
			if (unit.UnitType != UnitType.Poison)
				continue;
			if (unit.IsCharged == false)
				continue;

			isPosible = true;
			break;
		}
	}

    public override UnitType GetMover()
    {
        return UnitType.Poison;
    }

    public override void CalcWeight(BotStrategyModel strategyModel)
	{
		weight = 10;

		if (strategyModel.PlayerLeader.Shield == false)
			weight += 15;

		if (strategyModel.PlayerLeader.Allies.Find(x => x.UnitType == UnitType.MeleeMaster) == null)
			weight += 10;
        else if (strategyModel.PlayerLeader.Allies.Find(x => x.UnitType == UnitType.MeleeMaster).IsCharged == false)
            weight += 10;
    }

	public override int GetMoveWeight(BotTileModel tileFrom, BotTileModel tileTo, int order)
	{
        int weight = 0;

        if (order == 1)
        {
            if (tileTo.Unit == null)
            {
                if (tileTo.Tile.GetItemType() == ItemType.Range)
                    weight += 100;
                if (tileFrom.UnitModel.IsCharged)
                {
                    weight += 70;
                    if (tileFrom.Unit.UnitType == UnitType.Poison)
                        weight += 80;
                }
            }

            return weight;
        }

        if (order == 2)
        {
            if (tileTo.Unit != null)
            {
                if (tileFrom.UnitModel.IsRanger)
                    weight += 30;
                if (tileFrom.Tile.Xcoord == 0 && tileFrom.Tile.Ycoord == 0)
                    weight += 12;

                if (tileFrom.Unit.UnitType == UnitType.Poison)
                    weight += 100;
                if (tileTo.Unit.IsLeader)
                    weight += 150;
            }

            return weight;
        }

        return weight;
    }
}
