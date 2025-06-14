using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectHealthTacticMove : CollectTacticMove
{
    public CollectHealthTacticMove()
    {
        move = BotTacticMove.CollectHealth;
    }

    public override void PosibilityCheck(BotStrategyModel strategyModel)
    {
        isPosible = HasUnitAtItem(ItemType.Heal, strategyModel);
        if (isPosible == false)
            isPosible = HasUnitAtItem(ItemType.TeamHeal, strategyModel);
    }

    public override void CalcWeight(BotStrategyModel strategyModel)
    {
        weight = GetHealWeight(strategyModel);
    }

    public override bool Match(ShortMoveModel shortMove)
    {
        return shortMove.Tile.ItemType == ItemType.Heal;
    }

    private int GetHealWeight(BotStrategyModel strategyModel)
    {
        int weight = 10;

        Tile itemPos = null;

        if (strategyModel.Plan == GamePlan.Wild)
            weight += 10;

        foreach (var tile in strategyModel.Leader.Position.MoveTiles)
        {
            if (tile.GetItemType() == ItemType.Heal)
            {
                weight += 20;
                itemPos = tile;

                if (strategyModel.Leader.Health <= 3)
                    weight += 35;

                break;
            }
        }

        if (itemPos != null)
        {
            if (itemPos.HasEnemyInRange(strategyModel.Leader))
                weight += 10;

            foreach (var enemyTile in itemPos.GetEnemyTiles(strategyModel.Leader))
            {
                if (enemyTile.unitInTile.IsLeader)
                {
                    weight += 20;
                }
            }
        }
        else
        {
            foreach (var model in strategyModel.UnitModels)
            {
                foreach (var tile in model.Unit.Position.MoveTiles)
                {
                    if (tile.GetItemType() != ItemType.Heal)
                        continue;

                    if (tile.HasEnemyInRange(strategyModel.Leader))
                    {
                        if (tile.GetEnemyTiles(strategyModel.Leader).Contains(strategyModel.PlayerLeader.Position))
                        {
                            weight += 20;
                        }

                        weight += 10;
                        return weight;
                    }
                }
            }
        }

        return weight;
    }
}
