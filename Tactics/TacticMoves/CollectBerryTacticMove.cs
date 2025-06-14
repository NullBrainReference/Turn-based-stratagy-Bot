using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectBerryTacticMove : CollectTacticMove
{
	public CollectBerryTacticMove()
	{
		move = BotTacticMove.CollectBerry;
	}

	public override void PosibilityCheck(BotStrategyModel strategyModel)
	{
        if (HasUnitAtItem(ItemType.AirDamage, strategyModel))
            isPosible = true;
        if (HasUnitAtItem(ItemType.EarthDamage, strategyModel))
            isPosible = true;
        //if (HasUnitAtItem(ItemType.FireDamage, strategyModel))
        //    isPosible = true;
        if (HasUnitAtItem(ItemType.IceDamage, strategyModel))
            isPosible = true;
    }

	public override void CalcWeight(BotStrategyModel strategyModel)
	{
		weight = GetBerryWeight(strategyModel);
    }

    private int GetBerryWeight(BotStrategyModel strategyModel)
    {
        int weight = 0;
        List<Tile> berryTiles = new List<Tile>();

        foreach (var model in strategyModel.UnitModels)
        {
            var tiles = model.Unit.Position.GetBerryTilesInRange();

            foreach (var berryTile in tiles)
            {
                if (berryTiles.Contains(berryTile))
                    continue;

                berryTiles.Add(berryTile);
            }
        }

        foreach (var berryTile in berryTiles)
        {
            List<Unit> enemies = null;

            switch (berryTile.GetItemType())
            {
                case ItemType.IceDamage:
                    weight = GetBerryWeight(strategyModel, false);
                    break;
                case ItemType.AirDamage:
                    weight = GetBerryWeight(strategyModel, true);
                    break;
                case ItemType.EarthDamage:
                    weight = GetGreenBerryWeight(berryTile, strategyModel);
                    
                    break;
            }

            

        }

        Debug.Log("_Bot Berry weight: " + weight);

        return weight;
    }

    private int GetBerryWeight(BotStrategyModel strategyModel, bool charged)
    {
        int result = 0;

        foreach (Unit unit in strategyModel.PlayerLeader.Allies)
        {
            if (unit.IsCharged == charged)
            {
                result += 5;
                if (unit.IsLeader)
                {
                    result += 5;
                }
                if (unit.Health <= 1) 
                {
                    result += 10;
                    if (strategyModel.PlayerLeader.Health <= 1)
                        result += 80;
                }
            }
        }

        foreach (Unit unit in strategyModel.Leader.Allies)
        {
            if (unit.IsCharged == charged)
            {
                result += 5;
                if (unit.IsLeader)
                {
                    result += 5;
                }
                if (unit.Health <= 1)
                {
                    result += 10;
                    if (strategyModel.Leader.Health <= 1)
                        result += 80;
                }
            }
        }

        return result;
    }

    private int GetGreenBerryWeight(Tile berryTile, BotStrategyModel strategyModel)
    {
        int result = 0;

        foreach (Tile tile in berryTile.MoveTiles)
        {
            if (tile.unitInTile != null)
            {
                result += 5;
                if (tile.unitInTile.IsLeader)
                {
                    result += 5;
                }
                if (tile.unitInTile.Health <= 1)
                {
                    result += 40;
                    if (strategyModel.PlayerLeader.Health <= 1)
                    {
                        result += 80;
                    }
                }
            }
        }

        return result;
    }
}
