using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileOcupation { None, Player, Bot, Space }

[System.Serializable] //Serialized to check in Unity Editor
public class BotTileModel
{
    [SerializeField] private Tile tile;
    [SerializeField] private Unit unit;
    [SerializeField] private TileOcupation ocupation;
    [SerializeField] private bool isAttacked;
    [SerializeField] private bool wasAttackedFrom;
    [SerializeField] private bool wasOccupied;

    [SerializeField] private BotUnitModel unitModel;

    public Tile Tile { get { return tile; } }
    public Unit Unit { get { return unit; } }
    public TileOcupation Ocupation{ get { return ocupation; } }
    public bool IsAttacked { get { return isAttacked; } }
    public bool WasAttackedFrom { get { return wasAttackedFrom; } }
    public bool WasOccupied { get { return wasOccupied; } }

    public BotUnitModel UnitModel { get { return unitModel; } }

    public BotTileModel(Tile tile)
    {
        this.tile = tile;
        this.isAttacked = false;
        this.wasOccupied = false;

        SetUnit(tile.unitInTile);
    }

    public void MoveToTile(BotTileModel tileModel)
    {
        if (tileModel.tile.IsCharged)
            UnitModel.Charge();

        tileModel.SetUnit(unit);
        tileModel.SetUnitModel(unitModel);
        tileModel.CollectItem(unitModel);
        this.SetUnit(null);
        this.SetUnitModel(null);
    }

    private void CollectItem(BotUnitModel unitModel)
    {
        if (wasOccupied)
            return;

        switch (tile.GetItemType())
        {
            case ItemType.Range:
                foreach (var model in unitModel.StrategyModel.UnitModels)
                    model.MakeRanger();
                break;
            case ItemType.Shield:
                unitModel.GiveShield();
                break;
            case ItemType.Charger:
                foreach (var model in unitModel.StrategyModel.UnitModels)
                    model.Charge();
                break;
        }
    }

    public void SetUnit(Unit unit)
    {
        this.unit = unit;

        if (unit == null)
        {
            ocupation = TileOcupation.None;
            return;
        }

        ocupation = unit.Team == BotController._botTeam ? TileOcupation.Bot : TileOcupation.Player;
        wasOccupied = true;
    }

    public void SetUnitModel(BotUnitModel unitModel)
    {
        this.unitModel = unitModel;

        SendDamageBuff(unitModel);
    }

    public List<Tile> GetTilesForAttack(BotUnitModel unitModel)
    {
        List<Tile> tiles = new List<Tile>();

        foreach (var t in tile.GetEnemyTiles(Unit, unitModel.IsRanger))
        {
            if (t.HasEnemy(Unit))
                tiles.Add(t);
        }
        return tiles;
    }

    public List<Tile> GetTilesForMove(BotFieldModel fieldModel)
    {
        List<Tile> tiles = new List<Tile>();

        foreach (var t in tile.MoveTiles)
        {
            if (fieldModel.GetTileModel(t).ocupation == TileOcupation.Bot)
                continue;
            if (t.IsEmpty())
                tiles.Add(t);
        }
        return tiles;
    }

    public bool HasNotAttackedInRange(Unit unit, BotFieldModel fieldModel)
    {
        bool result = false;

        foreach (Tile t in tile.GetEnemyTiles(unit))
        {
            if (!fieldModel.GetTileModel(t).IsAttacked)
                result = true;
        }

        return result;
    }

    public void Attack()
    {
        isAttacked = true;
    }

    public void DeactivateAttack()
    {
        wasAttackedFrom = true;
        unitModel.Move();
    }

    public int GetPosWeightValue()
    {
        int result = 3;

        result -= Math.Abs(tile.Xcoord);
        result -= Math.Abs(tile.Ycoord);

        return result;
    }

    public void SendDamageBuff(BotUnitModel unitModel)
    {
        if (unitModel == null) 
            return;

        switch (tile.GetItemType())
        {
            case ItemType.Rage:
                unitModel.AddDamageMelee(1, true);
                unitModel.AddDamageRange(1, true);
                break;
            case ItemType.Range:
                unitModel.AddDamageRange(2, true);
                break;
            case ItemType.Melee:
                unitModel.AddDamageMelee(2, true);
                break;
        }
    }
}
