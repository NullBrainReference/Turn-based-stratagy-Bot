using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonerPersonality : BotUnitPersonality
{
    public PoisonerPersonality()
    {
        unitType = UnitType.Poison;
    }

    public override bool IsOfCenterAllowed()
        => false;

    public override int GetAttackWeight(BotTileModel tileFrom, BotTileModel tileTo, int order)
    {
        int result = 0;

        GamePlan gamePlan = tileFrom.UnitModel.StrategyModel.Plan;

        if (gamePlan == GamePlan.Wild)
            result += 8;

        if (order > 1)
        {
            result += -25;
            if (order == RevealLocalUtil.Instance.Order)
                return gamePlan == GamePlan.Wild ? -25 : -50;
        }
        else
        {
            if (tileTo.Unit.Shield == false)
                result += 20;
            if (tileTo.Unit.UnitType != UnitType.Default)
                result += 10;
        }

        if (tileFrom.UnitModel.IsRanger)
            result += 50;

        if (tileTo.Unit.Poison == false)
        {
            result += 20;

            if (tileTo.Unit.UnitType == UnitType.Coward)
            {
                if (tileTo.IsAttacked == false)
                {
                    result -= 15;
                }
            }

            if (tileTo.Unit.IsLeader)
            {
                result += 200;
                if (order == 2)
                    result -= 60;
                if (order == 3)
                    result -= 120;
            }
            else if (tileTo.Unit.UnitType != UnitType.Default)
            {
                result += 24;
                if (tileTo.Unit.DamageMelee > 1)
                    result += 8;

                if (order == 1)
                    result *= 2;
            }
        }
        else if (tileTo.Unit.IsLeader == false)
        {
            return -4;
        }

        return result;
    }

    public override int GetMoveWeight(BotTileModel tileTo, BotTileModel tileFrom, int order)
    {
        int result = 0;

        if (tileFrom.UnitModel.HasMoved)
            result -= 10;

        if (tileTo.Tile.IsPortal)
            result += 6;

        if (tileFrom.UnitModel.StrategyModel.Plan == GamePlan.Wild)
            result += 5;

        if (tileFrom.Unit.Health <= 1 && tileFrom.Tile.GetEnemyTiles(tileFrom.Unit).Count == 0)
        {
            if (order != RevealLocalUtil.Instance.Order)
                result += 40;
        }
        else if (tileFrom.Unit.Health > 1)
        {
            if (tileFrom.Tile.GetEnemyTiles(tileFrom.Unit).Count <= 0)
                result += tileTo.Tile.GetEnemyTiles(tileFrom.Unit).Count * 8;
        }

        if (tileFrom.UnitModel.IsCharged == false)
        {
            if (tileTo.Tile.IsCharged)
            {
                result += 2;

                if (tileTo.Tile.HasEnemyInRange(tileFrom.Unit, false, true))
                    result += 8;

                if (MathF.Abs(tileTo.Tile.Xcoord) - 2 < MathF.Abs(tileFrom.Tile.Xcoord) - 2)
                    result += 2;
                if (MathF.Abs(tileTo.Tile.Ycoord) - 2 < MathF.Abs(tileFrom.Tile.Ycoord) - 2)
                    result += 2;

                if (tileFrom.Tile.HasEnemyInRange(tileFrom.Unit) == false)
                    result += 8;

                foreach (var tile in tileTo.Tile.GetEnemyTiles(tileFrom.Unit))
                {
                    if (tile.unitInTile.IsLeader)
                        result += 10;
                    if (tile.unitInTile.UnitType != UnitType.Default)
                        result += 10;
                }

                return result;
            }
        }
        else
        {
            if (order == 1)
                if (tileTo.Tile.GetItemType() == ItemType.Range)
                    result += 50;

        }

        foreach (var tile in tileTo.Tile.GetEnemyTiles(tileFrom.Unit))
        {
            if (tile.unitInTile.IsLeader)
                result += 4;
        }

        if (MathF.Abs(tileTo.Tile.Xcoord) - 2 < MathF.Abs(tileFrom.Tile.Xcoord) - 2)
            result += 2;
        if (MathF.Abs(tileTo.Tile.Ycoord) - 2 < MathF.Abs(tileFrom.Tile.Ycoord) - 2)
            result += 2;

        if (tileFrom.Tile.HasEnemyInRange(tileFrom.Unit) == false)
            result += 8;

        return base.GetMoveWeight(tileTo, tileFrom) + result;
    }

    public override int GetDanger()
    {
        return 5;
    }

    public override List<MoveFilter> GetFilters(UnitModel unitModel, Field field = null)
    {
        List<MoveFilter> filters = base.GetFilters(unitModel, field);

        if (unitModel.Position.MoveTiles.Find(x => x.IsPortal) != null)
        {
            var filter = new MoveFilter(unitModel, x => 
                x.MoveType == MoveType.Move &&
                x.Tile.IsPortal
                );

            filters.Add(filter);
        }

        if (RevealLocalUtil.Instance.IsSwitchable)
        {
            var moveTile = unitModel.Position.MoveTiles.Find(x =>
                x.IsCharged &&
                x.HasEnemyInRange(unitModel, true) == false &&
                (Mathf.Abs(x.Xcoord) >= 2 || Mathf.Abs(x.Ycoord) >= 2)
                );

            if (moveTile != null)
            {
                var filter = new MoveFilter(unitModel, x =>
                    x.MoveType == MoveType.Move &&
                    x.Tile == moveTile,
                    moveTile
                    );

                filter.Priority = 1;

                filters.Add(filter);
            }
        }

        if (unitModel.Position.IsPortal)
        {
            TileModel leaderTile = null;

            foreach (var tile in unitModel.Position.MoveTiles)
            {
                if (tile.IsEmpty())
                    continue;
                if (tile.UnitInTile.Team == Team.White)
                    continue;

                if (tile.UnitInTile.IsLeader)
                {
                    leaderTile = tile;
                    break;
                }
            }

            if (leaderTile != null)
            {
                var threatTile = leaderTile.MeleeTiles.Find(x => x.IsEmpty() && x.IsCharged);

                if (threatTile != null)
                {
                    var filter = new MoveFilter(unitModel, x =>
                        x.MoveType == MoveType.Move &&
                        x.Tile.MeleeTiles.Contains(threatTile)
                        );

                    filters.Add(filter);
                }
            }
        }

        //No escape chase
        if (unitModel.IsCharged)
        {
            var noEscapeTile = unitModel.Position.MoveTiles.Find(x => x.HasNoEscape(unitModel));

            //if (unitModel.Position.MoveTiles.Find(x => x.HasNoEscape(unitModel)) != null)
            if (noEscapeTile != null)
            {
                var filter = new MoveFilter(unitModel, x =>
                    x.MoveType == MoveType.Move &&
                    x.Tile == noEscapeTile //x.Tile.HasNoEscape(unitModel)
                    ,
                    noEscapeTile
                    );

                filters.Add(filter);

                var followup = new MoveFilter(unitModel, x =>
                    x.MoveType == MoveType.Attack &&
                    x.Target.IsLeader,
                    unitModel
                    );

                filter.Followup = followup;

                filter.Priority = -1;
            }
        }
        else
        {
            var noEscapeTile = unitModel.Position.MoveTiles.Find(x => x.HasNoEscape(unitModel) && x.IsCharged);
            //bool tileCharged = noEscapeTile != null ? noEscapeTile.IsCharged : false;
            //if (unitModel.Position.MoveTiles.Find(x => x.HasNoEscape(unitModel)) != null)
            if (noEscapeTile != null)
            {
                var filter = new MoveFilter(unitModel, x =>
                    x.MoveType == MoveType.Move &&
                    x.Tile.HasNoEscape(unitModel) &&
                    x.Tile.IsCharged
                    ,
                    noEscapeTile
                    );

                filters.Add(filter);

                var followup = new MoveFilter(unitModel, x =>
                    x.MoveType == MoveType.Attack &&
                    x.Target.IsLeader,
                    unitModel
                    );

                filter.Followup = followup;

                filter.Priority = -1;
            }
        }

        return filters;
    }

    public override List<MoveFilter> GetOpeningFilters(UnitModel unitModel, Field field)
    {
        var filters = base.GetOpeningFilters(unitModel, field);

        var filter = new MoveFilter(unitModel, x =>
                x.MoveType == MoveType.Move &&
                x.Tile.IsBestChargeOption(x.TileFrom)
                );

        filter.Priority = 1;

        filters.Add(filter);

        if (unitModel.Position.MoveTiles.Find(x =>
                x.ItemType == ItemType.Shield ||
                x.ItemType == ItemType.Heal ||
                x.ItemType == ItemType.Charger) != null)
        {
            filter = new MoveFilter(unitModel, x =>
                x.MoveType == MoveType.Move &&
                x.Tile.ItemType != ItemType.Empty
                );

            filter.Priority = -1;

            filters.Add(filter);
        }

        if (RevealLocalUtil.Instance.IsSwitchable && unitModel.Position.HasEnemyInRange(unitModel, UnitType.MeleeMaster, true))
        {
            var moveTile = unitModel.Position.MoveTiles.Find(x =>
                x.IsCharged &&
                x.HasEnemyInRange(unitModel, true) == false &&
                (Mathf.Abs(x.Xcoord) >= 2 || Mathf.Abs(x.Ycoord) >= 2)
                );

            if (moveTile != null) {
                filter = new MoveFilter(unitModel, x =>
                    x.MoveType == MoveType.Move &&
                    x.Tile == moveTile,
                    moveTile
                    );

                filter.Priority = 1;

                filters.Add(filter); 
            }
        }

        return filters;
    }
}
