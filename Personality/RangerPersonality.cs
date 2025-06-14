using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangerPersonality : BotUnitPersonality
{
	public RangerPersonality()
	{
        unitType = UnitType.Ranger;
    }

    public override int GetDanger()
    {
        return 1;
    }

    public override int GetRadius()
    {
        return 2;
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

        //No escape chase
        if (unitModel.IsCharged)
        {
            if (unitModel.Position.MoveTiles.Find(x => x.HasNoEscape(unitModel)) != null)
            {
                var filter = new MoveFilter(unitModel, x =>
                    x.MoveType == MoveType.Move &&
                    x.Tile.HasNoEscape(unitModel)
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
            if (unitModel.Position.MoveTiles.Find(x => x.HasNoEscape(unitModel)) != null)
            {
                var filter = new MoveFilter(unitModel, x =>
                    x.MoveType == MoveType.Move &&
                    x.Tile.HasNoEscape(unitModel) &&
                    x.Tile.IsCharged
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
            else
            {
                var filter = new MoveFilter(unitModel, x =>
                    x.MoveType == MoveType.Move &&
                    x.Tile.HasEnemyInRange(unitModel) &&
                    x.Tile.IsCharged
                    );

                filters.Add(filter);

                var followup = new MoveFilter(unitModel, x =>
                    x.MoveType == MoveType.Attack,
                    unitModel
                    );
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

        filter = new MoveFilter(unitModel, x =>
            x.MoveType == MoveType.Move &&
            x.Tile.IsBestChargeOption(x.TileFrom)
            );

        filter.Priority = -1;

        var followup = new MoveFilter(unitModel, x =>
            x.MoveType == MoveType.Attack &&
            x.Target.IsLeader, 
            unitModel
            );
        filter.Followup = followup;

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

        return filters;
    }
}
