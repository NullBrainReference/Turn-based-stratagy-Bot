using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderAfterTakeAgroTacticMove : TacticMove
{
	public LeaderAfterTakeAgroTacticMove()
	{
		move = BotTacticMove.LeaderAfterTakeAgro;
	}

    public override void CalcWeight(BotStrategyModel strategyModel)
    {
        weight = 16;
    }

    public override void PosibilityCheck(BotStrategyModel strategyModel, int order)
    {
        if (HasEnemyLeaderNearBuff(strategyModel))
            isPosible = true;

        if (order == RevealLocalUtil.Instance.Order)
            isPosible = false;

        if (strategyModel.Leader.Health < strategyModel.Leader.totem.MaxHealth)
            if (strategyModel.Plan != GamePlan.Wild)
                isPosible = false;
    }

    public override int GetMoveWeight(BotTileModel tileFrom, BotTileModel tileTo, int order)
    {
        int weight = 0;

        weight += tileFrom.Unit.IsLeader ? 20 : -40;

        if (tileTo.Tile.unitInTile != null) //Attack case, probably should have it's own method
        {
            if (tileTo.Tile.unitInTile.IsLeader)
            {
                weight += 30;
                return weight;
            }
        }    

        if (tileFrom.UnitModel.IsCharged == false)
            weight += tileTo.Tile.IsCharged ? 10 : -20;

        if (tileTo.Tile.GetEnemyTiles(tileFrom.Unit).Find(x => x.unitInTile.IsLeader) != null)
            weight += 30;

        return weight;
    }

    private bool HasEnemyLeaderNearBuff(BotStrategyModel strategyModel)
    {
        foreach (var tile in strategyModel.PlayerLeader.Position.MoveTiles)
        {
            if (tile.GetItemType() == ItemType.Rage)
                return true;
            if (tile.GetItemType() == ItemType.Venom)
                return true;
            if (tile.GetItemType() == ItemType.FireDamage)
                return true;
        }

        return false;
    }
}
