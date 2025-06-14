using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectTacticMove : TacticMove
{
	public CollectTacticMove()
	{
		move = BotTacticMove.Collect;
	}

	public override void PosibilityCheck(BotStrategyModel strategyModel)
	{
		isPosible = HasUnitAtItem(strategyModel);
	}

	public override void CalcWeight(BotStrategyModel strategyModel)
	{
        weight = 10;
    }

    private bool HasUnitAtItem(BotStrategyModel strategyModel)
    {
        bool hasItem = false;

        foreach (var model in strategyModel.UnitModels)
        {
            foreach (var tile in model.Unit.Position.MoveTiles)
            {
                if (tile.content != null)
                {
                    hasItem = true;
                    return hasItem;
                }
            }
        }

        return hasItem;
    }

    protected bool HasUnitAtItem(ItemType itemType, BotStrategyModel strategyModel)
    {
        bool hasItem = false;

        foreach (var model in strategyModel.UnitModels)
        {
            foreach (var tile in model.Unit.Position.MoveTiles)
            {
                if (tile.GetItemType() == itemType)
                {
                    hasItem = true;
                    return hasItem;
                }
            }
        }

        return hasItem;
    }

    public override bool Match(ShortMoveModel shortMove)
    {
        return shortMove.Tile.ItemType != ItemType.Empty;
    }
}
