using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoveType { Move, Attack, Respawn, Ability }

public abstract class BotAction
{
	protected int order;
	protected MoveType action;
	protected int weight;

	protected BotTileModel tileFrom;
	protected BotTileModel tileTo;

	protected bool confirmed;

	public int Weight { get { return weight; } }
	public MoveType MoveType { get { return action; } }

	public BotAction(int order)
	{
		this.order = order;
		this.weight = 0;
		this.confirmed = false;
	}

	public void ConfirmAction()
	{
		confirmed = true;
	}

	public virtual void CalcWeight(
		BotTileModel tileTo, BotTileModel tileFrom,
		BotFieldModel fieldModel, BotStrategyModel strategyModel,
		TacticMove tacticMove = null)
	{
		Debug.Log("BotAction virtual");
	}

	protected bool IsDeathThreat(BotTileModel tileModel, Unit unit)
	{
		int damage = 0;

		foreach (var tile in tileModel.Tile.GetEnemyTiles(unit)) 
		{
			if (tile.HasEnemy(unit) == false)
				continue;

			if (tile.unitInTile.IsCharged)
				damage += 1; //TODO reaplace with real damage;
		}

		if (unit.Health <= damage)
			return true;

		return false;
	}

	protected virtual int GetSuddenDeathWeight(BotStrategyModel strategyModel)
    {
        int result = 0;
        if (tileFrom.Tile.IsSuddenDeathTarget)
            result += tileFrom.Unit.IsLeader ? 16 : 8;
        if (tileTo.Tile.IsSuddenDeathTarget)
            result += tileFrom.Unit.IsLeader ? -48 : -16;


        if (tileFrom.Unit.Health <= 1)
        {
            result *= 4;
            if (strategyModel.Leader.Health <= 1)
                result *= 4;
        }

        if (strategyModel.Leader.Health <= 1)
            result *= 2;
        if (tileFrom.UnitModel.HasMoved)
            result /= 2;


        return result;
    }

    protected void SetTiles(BotTileModel tileTo, BotTileModel tileFrom)
	{
		this.tileTo = tileTo;
		this.tileFrom = tileFrom;
	}

	public virtual void PushAction(ActionManager actionManager, BotStrategyModel strategyModel = null)
	{
        Debug.Log("BotAction virtual");
    }
}
