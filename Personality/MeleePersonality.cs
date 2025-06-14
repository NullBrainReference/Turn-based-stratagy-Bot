using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleePersonality : BotUnitPersonality
{
	public MeleePersonality()
	{
        unitType = UnitType.MeleeMaster;
    }

    public override bool IsOfCenterAllowed()
        => false;
    

    public override int GetAttackWeight(BotTileModel tileTo, int order)
    {
        if (order == 1)
            return 20;

        if (order == RevealLocalUtil.Instance.Order)
            return -4;

        return 20;
    }

    public override int GetDanger()
    {
        return 3;
    }

    //public override List<MoveFilter> GetFilters(UnitModel unitModel)
    //{
    //    List<MoveFilter> filters = base.GetFilters(unitModel);
    //
    //    if (unitModel.IsCharged)
    //    {
    //        if (unitModel.Position.MoveTiles.Find(x => x.HasNoEscape(unitModel)) != null)
    //        {
    //            var filter = new MoveFilter(unitModel, x =>
    //                x.MoveType == MoveType.Move &&
    //                x.Tile.HasNoEscape(unitModel)
    //                );
    //
    //            filters.Add(filter);
    //
    //            var followup = new MoveFilter(unitModel, x =>
    //                x.MoveType == MoveType.Attack &&
    //                x.Target.IsLeader,
    //                unitModel
    //                );
    //
    //            filter.Followup = followup;
    //
    //            filter.Priority = 0;
    //        }
    //    }
    //    else
    //    {
    //        if (unitModel.Position.MoveTiles.Find(x => x.HasNoEscape(unitModel)) != null)
    //        {
    //            var filter = new MoveFilter(unitModel, x =>
    //                x.MoveType == MoveType.Move &&
    //                x.Tile.HasNoEscape(unitModel) &&
    //                x.Tile.IsCharged
    //                );
    //
    //            filters.Add(filter);
    //
    //            var followup = new MoveFilter(unitModel, x =>
    //                x.MoveType == MoveType.Attack &&
    //                x.Target.IsLeader,
    //                unitModel
    //                );
    //
    //            filter.Followup = followup;
    //
    //            filter.Priority = 0;
    //        }
    //    }
    //
    //    return filters;
    //}

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
        else
        {
            filter = new MoveFilter(unitModel, x =>
                x.MoveType == MoveType.Move &&
                x.Tile.IsCharged
                );

            filters.Add(filter);
        }

        return filters;
    }
}
