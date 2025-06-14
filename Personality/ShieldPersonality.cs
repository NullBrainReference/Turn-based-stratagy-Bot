using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldPersonality : BotUnitPersonality, ICasterPersonality
{
    public int GetAbilityWeight(BotTileModel tileFrom, BotTileModel tileTo)
    {
        int result = 0;

        if (tileTo.UnitModel.HasShield == false)
        {
            result += tileTo.UnitModel.Unit.IsLeader ? 40 : 20;
            result += tileTo.UnitModel.Unit.UnitType == UnitType.Poison ? 20 : 0;
            if (tileTo.UnitModel.Unit.Poison)
                result -= 4;
        }
        else
        {
            if (tileTo.Unit.Shield)
                result -= 25;

            result -= 25;
        }

        return result;
    }

    public override int GetAttackWeight(BotTileModel tileTo)
    {
        return -5;
    }

    public override int GetMoveWeight(BotTileModel tileTo, BotTileModel tileFrom)
    {
        if (tileFrom.UnitModel.IsCharged == false)
        {
            int result = tileTo.Tile.IsPortal ? -8 : 0;

            if (tileTo.Tile.IsCharged)
            {
                result += 
                    4 + 
                    Mathf.Abs(tileTo.Tile.Xcoord) * 2 + Mathf.Abs(tileTo.Tile.Ycoord) * 2 
                    - tileTo.Tile.GetEnemyTiles(tileFrom.Unit).Count;

                return result;
            }

            return result;
        }

        return base.GetMoveWeight(tileTo, tileFrom);
    }

    public void OnPush(BotTileModel tileFrom, BotTileModel tileTo)
    {
        tileTo.UnitModel.GiveShield();
    }


    public override List<MoveFilter> GetFilters(UnitModel unitModel, Field field = null)
    {
        List<MoveFilter> filters = base.GetFilters(unitModel, field);

        if (unitModel.IsCharged)
        {
            if (unitModel.Leader.HasShield == false) {

                var filter = new MoveFilter(unitModel, x =>
                    x.MoveType == MoveType.Ability &&
                    x.Target.IsLeader,
                    unitModel
                    );

                filters.Add(filter);

                filter.Priority = 0;
            }
            else
            {
                var filter = new MoveFilter(unitModel, x =>
                    x.MoveType == MoveType.Ability &&
                    x.Unit.UnitType != UnitType.Poison &&
                    x.Target.HasShield == false,
                    unitModel
                    );

                filters.Add(filter);

                filter.Priority = 0;
            }

            if (field.mapManager.unitManager.WhiteUnits.Find(x => x.UnitType == UnitType.Poison) != null)
            {
                var filter = new MoveFilter(unitModel, x =>
                    x.MoveType == MoveType.Ability &&
                    x.Unit.UnitType == UnitType.Poison &&
                    x.Target.HasShield == false,
                    unitModel
                    );

                filters.Add(filter);

                filter.Priority = 0;
            }
        }
        else if (unitModel.Leader.HasShield == false)
        {
            if (unitModel.Position.MoveTiles.Find(x => x.ItemType == ItemType.Charger) != null)
            {
                var filter = new MoveFilter(null, x =>
                x.MoveType == MoveType.Move &&
                x.Tile.ItemType == ItemType.Charger,
                unitModel.Position.MoveTiles.Find(x => x.ItemType == ItemType.Charger)
                );

                filters.Add(filter);

                filter.Priority = -1;

                var followup = new MoveFilter(unitModel, x =>
                    x.MoveType == MoveType.Ability &&
                    x.Target.IsLeader,
                    unitModel
                    );

                filter.Followup = followup;

                if (field.mapManager.unitManager.WhiteUnits.Find(x => x.UnitType == UnitType.Poison) != null)
                {
                    filter = new MoveFilter(null, x =>
                        x.MoveType == MoveType.Move &&
                        x.Tile.ItemType == ItemType.Charger,
                        unitModel.Position.MoveTiles.Find(x => x.ItemType == ItemType.Charger)
                        );

                    filters.Add(filter);

                    filter.Priority = -1;

                    followup = new MoveFilter(unitModel, x =>
                        x.MoveType == MoveType.Ability &&
                        x.Target.UnitType == UnitType.Poison,
                        unitModel
                        );

                    filter.Followup = followup;
                }
            }
            else
            {
                var filter = new MoveFilter(unitModel, x =>
                    x.MoveType == MoveType.Move &&
                    x.Tile.IsCharged
                    );

                filters.Add(filter);

                filter.Priority = 1;

                var followup = new MoveFilter(unitModel, x =>
                    x.MoveType == MoveType.Ability &&
                    x.Target.IsLeader,
                    unitModel
                    );

                filter.Followup = followup;

                if (field.mapManager.unitManager.WhiteUnits.Find(x => x.UnitType == UnitType.Poison) != null)
                {
                    filter = new MoveFilter(unitModel, x =>
                        x.MoveType == MoveType.Move &&
                        x.Tile.IsCharged
                        );

                    filters.Add(filter);

                    filter.Priority = -1;

                    followup = new MoveFilter(unitModel, x =>
                        x.MoveType == MoveType.Ability &&
                        x.Target.UnitType == UnitType.Poison,
                        unitModel
                        );

                    filter.Followup = followup;
                }
            }
        }

        return filters;
    }

    public override List<MoveFilter> GetOpeningFilters(UnitModel unitModel, Field field)
    {
        var filters = new List<MoveFilter>();

        var filter = new MoveFilter(unitModel, x =>
                x.MoveType == MoveType.Move &&
                x.Tile.IsCharged &&
                Mathf.Abs(x.Tile.Ycoord) == 2 ||
                Mathf.Abs(x.Tile.Xcoord) == 2
                );

        if (field.mapManager.LastItemType != ItemType.Heal &&
            field.mapManager.LastItemType != ItemType.Charger &&
            field.mapManager.LastItemType != ItemType.Shield)
        {



            if (unitModel.Position.MoveTiles.Find(x =>
                x.ItemType == ItemType.Charger) != null)
            {
                filter = new MoveFilter(unitModel, x =>
                    x.MoveType == MoveType.Move &&
                    x.Tile.ItemType != ItemType.Empty
                    );
            }

            var followup = new MoveFilter(unitModel, x =>
                    x.MoveType == MoveType.Ability &&
                    (x.Target.IsLeader || x.Target.UnitType == UnitType.Poison),
                    unitModel
                    );

            filter.Followup = followup;
            filter.Priority = -1;

        }
        else if (unitModel.Position.MoveTiles.Find(x => x.ItemType != ItemType.Empty) != null)
        {
            var itemTakeFilter = new MoveFilter(unitModel, x =>
                x.MoveType == MoveType.Move &&
                x.Tile.ItemType != ItemType.Empty
                );

            var followup = new MoveFilter(null, x =>
                x.MoveType == MoveType.Move &&
                x.Unit.IsLeader &&
                x.Tile.IsCharged &&
                x.Tile.HasEnemyInRange(unitModel, true) == false
                );

            itemTakeFilter.Followup = followup;

            filters.Add(itemTakeFilter);
            itemTakeFilter.Priority = -1;
        }

        filters.Add(filter);

        if (unitModel.Position.MoveTiles.Find(x =>
                x.ItemType == ItemType.Charger) != null)
        {
            filter = new MoveFilter(unitModel, x =>
                x.MoveType == MoveType.Move &&
                x.Tile.ItemType != ItemType.Empty
                );

            var followup = new MoveFilter(unitModel, x =>
                x.MoveType == MoveType.Ability &&
                (x.Target.IsLeader || x.Target.UnitType == UnitType.Poison),
                unitModel
                );

            filter.Followup = followup;
            filter.Priority = -1;

            filters.Add(filter);
        }

        if (field.mapManager.LastItemType == ItemType.Charger)
        {
            filter = new MoveFilter(unitModel, x =>
                x.MoveType == MoveType.Ability &&
                (x.Target.IsLeader || x.Target.UnitType == UnitType.Poison),
                unitModel
                );

            filter.Priority = 101;
            filters.Add(filter);
        }

        return filters;
    }
}
