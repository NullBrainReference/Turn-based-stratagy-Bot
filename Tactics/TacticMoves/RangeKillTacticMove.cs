using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeKillTacticMove : CollectTacticMove
{
    public RangeKillTacticMove()
    {
        move = BotTacticMove.RangeKill;
    }

    public override void PosibilityCheck(BotStrategyModel strategyModel)
    {
        isPosible = false;

        if (HasUnitToKill(strategyModel) == false)
            return;
        //if (strategyModel.Leader.Health <= 2)
        //    return;
        if (HasAttacker(strategyModel) == false)
            return;

        if (HasUnitAtItem(ItemType.Range, strategyModel))
            isPosible = true;
    }

    public override void CalcWeight(BotStrategyModel strategyModel)
    {
        weight = 14;
        if (strategyModel.Leader.Health >= 3)
            weight += 2;
        if (strategyModel.PlayerLeader.Health <= 1)
            weight += 32;
    }

    public override int GetMoveWeight(BotTileModel tileFrom, BotTileModel tileTo, int order)
    {
        int weight = 0;

        if (order == 1)
        {
            if (tileTo.Unit == null)
            {
                if (tileTo.Tile.GetItemType() == ItemType.Range)
                    weight += 50;
                if (tileFrom.UnitModel.IsCharged)
                    weight += 50;
                if (tileFrom.Unit.UnitType == UnitType.Poison)
                    weight += 12;
            }

            return weight;
        }

        if (order == 2)
        {
            if (tileTo.Unit != null)
            {
                if (tileFrom.UnitModel.IsRanger)
                    weight += 50;
                if (tileTo.Unit.Health <= 1)
                    weight += 50;
                if (tileTo.Unit.Shield)
                    weight += -50;
                if (tileTo.Unit.IsLeader)
                    weight += 50;
                if (tileFrom.Tile.Xcoord == 0 && tileFrom.Tile.Ycoord == 0)
                    weight += 12;
                    
                if (tileFrom.Unit.UnitType == UnitType.Poison)
                    weight += 12;
            }

            return weight;
        }

        return weight;
    }

    private bool HasAttacker(BotStrategyModel strategyModel)
    {
        foreach (var unit in strategyModel.Leader.Allies)
        {
            if (unit.IsCharged == false)
                continue;

            //var enemyTiles = unit.Position.GetEnemyTiles(unit, true);

            if (unit.Position.MoveTiles.Find(x => x.Xcoord == 0 && x.Ycoord == 0) != null)
                return true;
        }

        return false;
    }

    private bool HasUnitToKill(BotStrategyModel strategyModel)
    {
        foreach (var unit in strategyModel.PlayerLeader.Allies)
        {
            if (unit.Health <= 1)
                return true;
            else if (unit.IsLeader)
                return true;
        }
        
        return false;
    }
}
