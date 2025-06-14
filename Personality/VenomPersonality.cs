using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VenomPersonality : BotUnitPersonality
{
    public VenomPersonality()
    {
        unitType = UnitType.Venomancer;
    }

    public override bool IsOfCenterAllowed()
    {
        return false;
    }

    public override List<MoveFilter> GetFilters(UnitModel unitModel, Field field = null)
    {
        var filters = base.GetFilters(unitModel, field);

        var tile = unitModel.Position.FindMostPopTile(unitModel);
        if (tile != null)
        {
            var filter = new MoveFilter(unitModel, x =>
                x.MoveType == MoveType.Move &&
                x.Tile == tile,
                tile
                );

            filter.Priority = 1;

            filters.Add(filter);
        }

        return filters;
    }

    public override List<MoveFilter> GetOpeningFilters(UnitModel unitModel, Field field)
    {
        return base.GetOpeningFilters(unitModel, field);
    }
}
