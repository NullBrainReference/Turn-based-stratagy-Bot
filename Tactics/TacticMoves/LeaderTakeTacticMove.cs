using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderTakeTacticMove : CollectTacticMove
{
    public LeaderTakeTacticMove()
    {
        move = BotTacticMove.LeaderTake;
    }

    public override void PosibilityCheck(BotStrategyModel strategyModel)
    {
        if (HasLeaderAtItem(strategyModel))
            isPosible = true;
    }

    public override void CalcWeight(BotStrategyModel strategyModel)
    {
        weight = 0;

        if (HasLeaderAtItem(strategyModel) == false)
        {
            weight = 0;
            return;
        }

        ItemType itemType = ItemType.Empty;
        foreach (var tile in strategyModel.Leader.Position.MoveTiles)
        {
            itemType = tile.GetItemType();
            if (itemType != ItemType.Empty)
                break;
        }

        switch (itemType)
        {
            case ItemType.Rage:
                weight = 0;
                break;
            case ItemType.Venom:
                weight = 20;
                break;
            case ItemType.Range:
                weight = 0;
                break;
            case ItemType.Shield:
                weight = 35;
                break;
            case ItemType.Heal:
                weight = 40;
                break;
        }
    }

    public override int GetMoveWeight(BotTileModel tileFrom, BotTileModel tileTo, int order)
    {
        int weight = 0;

        weight += tileFrom.Unit.IsLeader ? 20 : -40;

        if (tileTo.Tile.content != null)
            weight += 55;

        return weight;
    }

    public override UnitType GetMover()
    {
        return UnitType.DefaultLeader;
    }

    public override bool Match(ShortMoveModel shortMove)
    {
        bool match = shortMove.Unit.IsLeader;

        if (shortMove.MoveType != MoveType.Move)
            return false;
        if (shortMove.Tile.ItemType == ItemType.Empty)
            return false;

        return match;
    }

    private bool HasLeaderAtItem(BotStrategyModel strategyModel)
    {
        bool hasItem = false;

        var leader = strategyModel.Leader;

        foreach (var tile in leader.Position.MoveTiles)
        {
            if (tile.content != null)
            {
                hasItem = true;
                return hasItem;
            }
        }

        return hasItem;
    }
}
