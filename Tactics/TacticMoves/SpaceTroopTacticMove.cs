using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceTroopTacticMove : TacticMove
{
	public SpaceTroopTacticMove()
	{
		move = BotTacticMove.SpaceTroop;
	}

	public override void PosibilityCheck(BotStrategyModel strategyModel, int order)
	{
		var models = strategyModel.UnitModels;

		if (strategyModel.Leader.Position.GetEnemyTiles(strategyModel.Leader).Count >= 1)
		{
			isPosible = false;
			return;
		}

		if (HasUnitInPortal(models))
		{
            if (order == RevealLocalUtil.Instance.Order)
            {
                isPosible = false;
                return;
            }

            isPosible = true;
			return;
		}

		if (order == RevealLocalUtil.Instance.Order && order > 1)
        {
            isPosible = false;
            return;
        }

        if (HasUnitAtPortal(models))
        {
            isPosible = true;
            return;
        }
    }

	public override void CalcWeight(BotStrategyModel strategyModel)
	{
        if (strategyModel.Plan == GamePlan.Wild)
            weight += 10;

        if (HasUnitInPortal(strategyModel.UnitModels))
		{
			weight += 17;
			return;
		}

        if (HasUnitAtPortal(strategyModel.UnitModels))
        {
            weight += 15;
            return;
        }
    }

	public override int GetMoveWeight(BotTileModel tileFrom, BotTileModel tileTo, int order)
	{
		int weight = 0;

		if (tileTo.Tile.unitInTile != null)
		{
			if (order == RevealLocalUtil.Instance.Order && order > 1)
				weight -= 30;

			weight += 10;

			weight += tileTo.Tile.unitInTile.IsLeader ? 30 : 10;

			return weight;
        }
		else
		{
			if (tileFrom.Tile.IsPortal)
			{
				var leaderTile = tileTo.Tile.GetEnemyTiles(tileFrom.Unit).Find(x => x.unitInTile.IsLeader);

                weight += leaderTile != null ? 40 : 0;
				weight += 18 - Mathf.Abs(tileTo.Tile.Xcoord) * 9;
                weight += 18 - Mathf.Abs(tileTo.Tile.Ycoord) * 9;

                weight += tileFrom.UnitModel.StrategyModel.AwarenessModel.GetClosestDangerWeight(tileTo) / 2;

				if (leaderTile != null)
				{
					foreach (var tile in tileTo.Tile.MeleeTiles)
					{
						if (tile.IsPortal)
						{
							weight += 8;
							break;
						}
					}
				}

                if (order == RevealLocalUtil.Instance.Order)
					weight -= 25;
			}
			else if (tileTo.Tile.IsPortal)
			{
				weight += 20;
            }
		}

		return weight;
	}

    public override UnitType GetMover()
    {
        return UnitType.Poison;
    }

    public override bool Match(ShortMoveModel shortMove)
    {
        bool match = base.Match(shortMove);

		if (shortMove.MoveType == MoveType.Move)
		{
			if (shortMove.Tile.IsPortal)
				return match;
			else if (shortMove.Tile.HasEnemyInRange(shortMove.Unit, UnitType.DefaultLeader, false))
				return match;
		}
		else if (shortMove.MoveType == MoveType.Attack)
		{
			if (shortMove.Target.IsLeader)
				return match;
		}

        return false;
    }

    private bool HasUnitInPortal(List<BotUnitModel> models)
	{
		foreach (var model in models)
		{
			if (model.Unit.Position.IsPortal)
				return true;
		}

		return false;
	}

    private bool HasUnitAtPortal(List<BotUnitModel> models)
    {
        foreach (var model in models)
        {
            if (model.Unit.Position.MeleeTiles.Find(x => x.IsPortal) != null)
                return true;
        }

        return false;
    }
}
