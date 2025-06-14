using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotAwarenessModel
{
    private int[,] dangerLevels;

	private List<IBotDanger> personalities;
	private List<Unit> units;

    private readonly int xOffset = 2;
    private readonly int yOffset = -2;

    public BotAwarenessModel()
	{

	}

	public void Init(List<Unit> units)
	{
        dangerLevels = new int[5, 5];
        personalities = new List<IBotDanger>();

        this.units = units;

        foreach (var unit in units)
        {
            personalities.Add(BotUnitPersonality.GetUnitPersonality(unit.UnitType));
        }

        for ( int i = 0; i < personalities.Count; i++)
		{
			FIllDangerLevels(personalities[i], units[i]);
		}
	}

	private void SetDanger(Tile tile, int value)
	{
        dangerLevels[tile.Xcoord + xOffset, -(tile.Ycoord + yOffset)] += value;
        //dangerLevels[Xcoord, Ycoord] = value;
	}
	public int GetDanger(Tile tile)
	{
		return dangerLevels[tile.Xcoord + xOffset, -(tile.Ycoord + yOffset)];
    }

    private void SetDanger(int Xcoord, int Ycoord, int value)
    {
        dangerLevels[Xcoord + xOffset, -(Ycoord + yOffset)] += value;
    }

    //Was recursive
    private List<Tile> FillDangerLevels(Tile Pos, Unit unit, bool unitCharged, int levelBase)
	{
        List<Tile> nextTiles = new List<Tile>();

        foreach (Tile tile in Pos.MoveTiles)
        {
            if (tile.IsCharged)
            {
                unitCharged = true;
                SetDanger(tile, levelBase);

                foreach (Tile t in tile.MeleeTiles)
                    SetDanger(t, levelBase);

                if (unit.IsRanger)
                    foreach (Tile t in tile.RangeTiles)
                        SetDanger(t, levelBase);
            }
            else if (unitCharged)
            {
                SetDanger(tile, levelBase);

                foreach (Tile t in tile.MeleeTiles)
                    SetDanger(t, levelBase);

                if (unit.IsRanger)
                    foreach (Tile t in tile.RangeTiles)
                        SetDanger(t, levelBase);
            }

            nextTiles = Pos.MoveTiles;
        }

        return nextTiles;
    }

    private void FIllDangerLevels(IBotDanger personality, Unit unit)
	{
		int levelBase = personality.GetDanger();
		if (unit.IsCharged)
		{
			levelBase *= 2;
			foreach (Tile tile in unit.Position.MeleeTiles)
				SetDanger(tile, levelBase);

			if (unit.IsRanger)
                foreach (Tile tile in unit.Position.RangeTiles)
                    SetDanger(tile, levelBase);
        }

		bool unitCharged = unit.IsCharged;

        try //I do belive stack overflow is posible here
        {
			var tiles = FillDangerLevels(unit.Position, unit, unitCharged, levelBase);
		}
		catch (Exception e)
		{
			Debug.Log("_Bot Error caused by FillDangerLevels, " + e.Message);
		}
	}

    /// <summary>
    /// Returns danger weight for unit in tileFrom, works only for tile model with Unit in it.
    /// </summary>
    /// <param name="tileFrom"></param>
    /// <returns></returns>
    public int GetClosestDangerWeight(BotTileModel tileFrom)
    {
        int weight = 0;

        if (tileFrom.Unit == null)
            return weight;

        foreach (var tile in tileFrom.Tile.GetEnemyTiles(tileFrom.Unit))
        {
            if (tile.unitInTile.IsCharged)
            {
                weight += 5;
                if (tile.unitInTile.DamageMelee >= 2)
                    weight += 10;

                if (tileFrom.Unit.IsLeader)
                    weight *= 2;
            }
        }

        foreach (var tile in tileFrom.Tile.RangeTiles)
        {
            if (tile.HasEnemy(tileFrom.Unit) == false)
                continue;
            if (tile.unitInTile.IsRanger == false)
                continue;

            //weight += 3;
            if (tile.unitInTile.IsCharged)
                weight += 7;
            if (tileFrom.Unit.IsLeader)
                weight *= 2;
        }

        int chargedCount = 0;
        int unchargedDanger = 0;

        foreach (var tile in tileFrom.Tile.MeleeTiles)
        {
            if (tile.IsCharged)
                chargedCount++;

            if (tile.HasEnemy(tileFrom.Unit))
            {
                unchargedDanger += 2;
                if (tile.unitInTile.DamageMelee >= 2)
                    unchargedDanger += 7;
            }
        }

        weight += unchargedDanger * chargedCount;

        return weight;
    }

    public int GetClosestDangerWeight(BotTileModel tileTo, Unit unit)
    {
        int weight = 0;

        foreach (var tile in tileTo.Tile.GetEnemyTiles(unit))
        {
            if (tile.unitInTile.IsCharged)
            {
                weight += 5;
                if (tile.unitInTile.DamageMelee >= 2)
                    weight += 15;
            }
        }

        foreach (var tile in tileTo.Tile.RangeTiles)
        {
            if (tile.HasEnemy(unit) == false)
                continue;
            if (tile.unitInTile.IsRanger == false)
                continue;

            //weight += 3;
            if (tile.unitInTile.IsCharged)
                weight += 7;
            if (unit.IsLeader)
                weight *= 2;
        }

        int chargedCount = 0;
        int unchargedDanger = 0;

        foreach (var tile in tileTo.Tile.MeleeTiles)
        {
            if (tile.IsCharged)
                chargedCount++;

            if (tile.HasEnemy(unit))
            {
                unchargedDanger += 2;
                if (tile.unitInTile.DamageMelee >= 2)
                    unchargedDanger += 7;
            }
        }

        weight += unchargedDanger * chargedCount;

        return weight;
    }

    /// <summary>
    /// Returns true if Unit in tile has hight danger and can be killed, use for attack actions only
    /// </summary>
    /// <param name="tileTo"></param>
    /// <returns></returns>
    public bool HasToDieNow(BotTileModel tileTo)
    {
        if (tileTo.Unit == null)
            return false;

        var unit = tileTo.Unit;

        switch (unit.UnitType)
        {
            case UnitType.MeleeMaster:
                return unit.Health <= 1 ? true : false;
            case UnitType.Splasher:
                return unit.Health <= 1 ? true : false;
        }

        if (unit.IsLeader)
            return unit.Health <= 1 ? true : false;

        return false;
    } 
}
