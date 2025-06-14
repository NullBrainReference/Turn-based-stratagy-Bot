using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderAfterTakeTacticMove : TacticMove
{
	public LeaderAfterTakeTacticMove()
	{
		move = BotTacticMove.LeaderAfterTake;
	}

	public override void CalcWeight(BotStrategyModel strategyModel)
	{
		weight = 15;
	}

	public override void PosibilityCheck(BotStrategyModel strategyModel, int order)
	{
		if (order == RevealLocalUtil.Instance.Order)
			isPosible = false;
		else
			isPosible = true;
	}

	public override int GetMoveWeight(BotTileModel tileFrom, BotTileModel tileTo, int order)
	{
		int weight = 0;

		weight += tileFrom.Unit.IsLeader ? 20 : -40;

		if (tileTo.Tile.GetEnemyTiles(tileFrom.Unit).Count <= 0)
			weight += 30;

		return weight;
	}

    public override UnitType GetMover()
    {
        return UnitType.DefaultLeader;
    }

    public override bool Match(ShortMoveModel shortMove)
    {
        bool match = base.Match(shortMove);

        return match;
    }

}
