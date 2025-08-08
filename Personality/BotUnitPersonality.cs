using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotUnitPersonality: IBotDanger, IMoveFilerSource
{
    protected UnitType unitType;

    public BotUnitPersonality()
    {
        unitType = UnitType.Default;
    }

    public UnitType UnitType => unitType;

    public virtual bool IsOfCenterAllowed() => true;
    

    public virtual int GetMoveWeight(BotTileModel tileTo, BotTileModel tileFrom, int order)
    {
        return GetMoveWeight(tileTo, tileFrom);
    }

    public virtual int GetMoveWeight(BotTileModel tileTo, BotTileModel tileFrom) 
    { 
        if (tileFrom.UnitModel.Unit.IsLeader)
        {
            if (tileFrom.UnitModel.HasMoved == false)
            {
                return 4;
            }
        }

        return 0;
    }

    public virtual int GetAttackWeight(BotTileModel tileFrom, BotTileModel tileTo, int order)
    {
        return GetAttackWeight(tileTo, order);
    }

    public virtual int GetAttackWeight(BotTileModel tileTo, int order)
    {
        return GetAttackWeight(tileTo);
    }

    public virtual int GetAttackWeight(BotTileModel tileTo) 
    { 
        return 0; 
    }

    public static BotUnitPersonality GetUnitPersonality(UnitType unitType)
    {
        switch (unitType)
        {
            case UnitType.Poison:
                return new PoisonerPersonality();
            case UnitType.TNT:
                return new TNTPersonality();
            case UnitType.Spore:
                return new SporePersonality();
            case UnitType.Thorn:
                return new ThornPersonality();
            case UnitType.Vampire:
                return new VampirePersonality();
            case UnitType.Splasher:
                return new SplasherPersonality();
            case UnitType.Shield:
                return new ShieldPersonality();
            case UnitType.MeleeMaster:
                return new MeleePersonality();
            case UnitType.Venomancer:
                return new VenomPersonality();
            case UnitType.SuperMelee:
                return new SuperMeleePersonality();
            case UnitType.PoisonRange:
                return new PoisonRangePersonality();
        }


        return new BotUnitPersonality();
    }

    public virtual int GetDanger()
    {
        return 1;
    }

    public virtual int GetRadius()
    {
        return 1;
    }

    public virtual List<MoveFilter> GetFilters(UnitModel unitModel, Field field = null)
    {
        List<MoveFilter> moveFilters = new List<MoveFilter>();

        if (unitModel.IsRespawnable)
        {
            var filter = new MoveFilter(unitModel, x => x.MoveType == MoveType.Respawn, unitModel);
            moveFilters.Add(filter);

            filter.Priority = 2;
        }
        else if (unitModel.Health < unitModel.TrueHealth && unitModel.IsCharged) //TODO: Uncomment if charge required to heal
        {
            var filter = new MoveFilter(unitModel, x => x.MoveType == MoveType.Respawn, unitModel);
            moveFilters.Add(filter);

            filter.Priority = 2;
        }
        //TODO: Uncomment if charge required to heal
        if (unitModel.Health < unitModel.TrueHealth && unitModel.IsLeader && unitModel.IsCharged)
        {
            var moveTile = unitModel.Position.MoveTiles.Find(x => x.IsBestChargeOption(unitModel.Position));
        
            var filter = new MoveFilter(unitModel, x => 
                x.MoveType == MoveType.Move &&
                x.Tile == moveTile, 
                moveTile);
        
            moveFilters.Add(filter);
        
            filter.Priority = -1;
        
            var followup = new MoveFilter(unitModel, x => x.MoveType == MoveType.Respawn, unitModel);
            filter.Followup = followup;
        }


        //Charge case
        else if (unitModel.IsCharged == false)
        {
            bool ofCenterAllowed = GetUnitPersonality(unitModel.UnitType).IsOfCenterAllowed();

            //TODO: check if bad, remove if so and make free charge tile option for filter
            var chargeTile = unitModel.Position.MoveTiles.Find(x => 
                x.IsCharged && 
                x.IsBestChargeOption(unitModel.Position) &&
                ((x.Xcoord < 2 && x.Xcoord > -2 && x.Ycoord < 2 && x.Ycoord > -2) || ofCenterAllowed)
                );

            var filter = new MoveFilter(unitModel, x =>
                x.MoveType == MoveType.Move &&
                x.Tile == chargeTile//TODO: check if bad, uncomment old if
                //x.Tile.IsCharged &&
                //x.Tile.IsBestChargeOption(x.TileFrom) &&
                //((x.Tile.Xcoord < 2 && x.Tile.Xcoord > -2 && x.Tile.Ycoord < 2 && x.Tile.Ycoord > -2) || ofCenterAllowed)
                ,
                chargeTile //TODO: check if bad, remove if so and make free charge tile option for filter
                );

            moveFilters.Add(filter);

            filter.Priority = 1;
        }

        if (unitModel.IsLeader)
        {
            TileModel escapeFrom = null;
            int topDmg = 0;

            foreach (var tile in unitModel.Position.MeleeTiles)
            {
                if (tile.IsEmpty())
                    continue;

                if (tile.HasEnemy(unitModel) == false)
                    continue;

                if (tile.UnitInTile.IsCharged)
                {
                    if (tile.UnitInTile.Damage > topDmg)
                    {
                        topDmg = tile.UnitInTile.Damage;
                        escapeFrom = tile;
                    }
                }
            }

            TileModel escapeFromRange = null;

            foreach (var tile in unitModel.Position.RangeTiles)
            {
                if (tile.IsEmpty())
                    continue;

                if (tile.HasEnemy(unitModel) == false)
                    continue;

                if (tile.UnitInTile.IsCharged)
                {
                    if (tile.UnitInTile.IsRanger)
                    {
                        escapeFromRange = tile;
                        break;
                    }
                }
            }


            if (escapeFrom != null)
            {
                var safeTile = unitModel.Position.MoveTiles.Find(x => x.MeleeTiles.Contains(escapeFrom) == false);

                var filter = new MoveFilter(unitModel, x => 
                    x.MoveType == MoveType.Move &&
                    x.Tile == safeTile,
                    safeTile
                    );

                filter.Priority = -1;

                moveFilters.Add(filter);

                var retreatTile = safeTile.MoveTiles.Find(x =>
                    x.IsEmpty() &&
                    x.HasEnemyInRange(unitModel, true) == false
                    );

                if (retreatTile != null)
                {
                    filter = new MoveFilter(unitModel, x =>
                        x.MoveType == MoveType.Move &&
                        x.Tile == safeTile,
                        safeTile
                        );

                    var followUp = new MoveFilter(unitModel, x =>
                        x.MoveType == MoveType.Move &&
                        x.Tile == retreatTile
                        ,
                        retreatTile //check if bad, remove if so and make free tile option for filter
                        );

                    filter.Followup = followUp;
                    filter.Priority = -1;

                    moveFilters.Add(filter);
                }

                if (escapeFromRange != null)
                {
                    safeTile = unitModel.Position.MoveTiles.Find(x => escapeFromRange.RangeTiles.Contains(x) == false);

                    filter = new MoveFilter(unitModel, x =>
                        x.MoveType == MoveType.Move &&
                        x.Tile == safeTile,
                        safeTile
                        );
                    filter.Priority = -1;

                    moveFilters.Add(filter);
                }
            }
            else if (escapeFromRange != null)
            {
                var safeTile = unitModel.Position.MoveTiles.Find(x => escapeFromRange.RangeTiles.Contains(x) == false);

                var filter = new MoveFilter(unitModel, x =>
                    x.MoveType == MoveType.Move &&
                    x.Tile == safeTile,
                    safeTile
                    );

                filter.Priority = -1;

                moveFilters.Add(filter);
            }
            
        }

        if (unitModel.Position.HasEnemyInRange(unitModel))
        {
            if (unitModel.IsCharged)
            {
                //TODO switched to test performance
                foreach (var enemyTile in unitModel.Position.GetEnemyTiles(unitModel))
                {
                    var filter = new MoveFilter(unitModel, x => 
                        x.MoveType == MoveType.Attack &&
                        x.Target == enemyTile.UnitInTile
                        ,
                        unitModel);
                    moveFilters.Add(filter);
                
                    filter.Priority = 0;
                }

                //var filter = new MoveFilter(unitModel, x => x.MoveType == MoveType.Attack, unitModel);
                //moveFilters.Add(filter);
                //
                //filter.Priority = 0;
            }
        }

        if (unitModel.Position.MoveTiles.Find(x => x.ItemType != ItemType.Empty) != null)
        {
            var filter = new MoveFilter(unitModel, x => 
                x.MoveType == MoveType.Move &&
                x.Tile.ItemType != ItemType.Empty,
                unitModel.Position.MoveTiles.Find(x => x.ItemType != ItemType.Empty)
                );

            moveFilters.Add(filter);

            filter.Priority = -1;
        }

        if (unitModel.Position.MoveTiles.Find(x => x.ItemType == ItemType.Range) != null)
        {
            if (unitModel.IsCharged)
            {
                var filter = new MoveFilter(unitModel, x =>
                    x.MoveType == MoveType.Move &&
                    x.Tile.ItemType != ItemType.Empty,
                    unitModel.Position.MoveTiles.Find(x => x.ItemType != ItemType.Empty)
                    );

                moveFilters.Add(filter);

                filter.Priority = -1;

                var followup = new MoveFilter(unitModel, x =>
                    x.MoveType == MoveType.Attack &&
                    x.Target.IsLeader,
                    unitModel
                    );

                filter.Followup = followup;

                filter.Key = "Range hit";
            }
        }

        if (unitModel.Position.MoveTiles.Find(x => x.ItemType == ItemType.Charger) != null)
        {
            if (unitModel.IsCharged)
            {
                var filter = new MoveFilter(unitModel, x =>
                    x.MoveType == MoveType.Move &&
                    x.Tile.ItemType != ItemType.Empty,
                    unitModel.Position.MoveTiles.Find(x => x.ItemType != ItemType.Empty)
                    );

                moveFilters.Add(filter);

                filter.Priority = -1;

                var followup = new MoveFilter(null, x =>
                    x.MoveType == MoveType.Attack &&
                    x.Unit.Position.MeleeTiles.Contains(x.Target.Position) &&
                    x.Target.HasShield == false,
                    unitModel
                    );

                filter.Followup = followup;

                filter.Key = "Charger hit";
            }
        }


        return moveFilters;
    }

    public virtual List<MoveFilter> GetOpeningFilters(UnitModel unitModel, Field field)
    {
        List<MoveFilter> moveFilters = new List<MoveFilter>();

        if (unitModel.IsLeader)
        {
            if (unitModel.Position.MoveTiles.Find(x => 
                x.ItemType == ItemType.Shield || 
                x.ItemType == ItemType.Heal || 
                x.ItemType == ItemType.Charger) != null)
            {
                var filter = new MoveFilter(unitModel, x =>
                    x.MoveType == MoveType.Move &&
                    x.Tile.ItemType != ItemType.Empty,
                    field.TilesLibrary["C3"].TileModel
                    );

                filter.Priority = -1;

                moveFilters.Add(filter);
            }
            else
            {
                var filter = new MoveFilter(unitModel, x =>
                    x.MoveType == MoveType.Move &&
                    x.Tile.HasEnemyInRange(unitModel, true) == false
                    );

                filter.Priority = 0;

                moveFilters.Add(filter);

                if (RevealLocalUtil.Instance.IsSwitchable)
                {
                    var moveTile = unitModel.Position.MoveTiles.Find(x =>
                        x.IsCharged &&
                        x.HasEnemyInRange(unitModel, true) == false &&
                        (Mathf.Abs(x.Xcoord) >= 2 || Mathf.Abs(x.Ycoord) >= 2)
                        );

                    if (moveTile != null)
                    {
                        filter = new MoveFilter(unitModel, x =>
                            x.MoveType == MoveType.Move &&
                            x.Tile == moveTile,
                            moveTile
                            );

                        filter.Priority = 1;

                        moveFilters.Add(filter);
                    }
                }
            }


        }
        else
        {
            if (unitModel.Position.MoveTiles.Find(x =>
                x.ItemType == ItemType.Heal ||
                x.ItemType == ItemType.Charger) != null)
            {
                var filter = new MoveFilter(unitModel, x =>
                    x.MoveType == MoveType.Move &&
                    x.Tile.ItemType != ItemType.Empty,
                    field.TilesLibrary["C3"].TileModel
                    );

                filter.Priority = -1;

                moveFilters.Add(filter);
            }
            else if (
                unitModel.Position.MoveTiles.Find(x => x.ItemType == ItemType.Shield) != null &&
                unitModel.Leader.Position.MoveTiles.Find(x => x.ItemType == ItemType.Shield) == null)
            {
                var filter = new MoveFilter(unitModel, x =>
                    x.MoveType == MoveType.Move &&
                    x.Tile.ItemType != ItemType.Empty,
                    field.TilesLibrary["C3"].TileModel
                    );

                var followup = new MoveFilter(null, x =>
                    x.MoveType == MoveType.Move &&
                    x.Unit.IsLeader &&
                    x.Tile.IsCharged &&
                    x.Tile.HasEnemyInRange(unitModel, true) == false
                    );

                filter.Followup = followup;
                filter.Priority = -1;

                moveFilters.Add(filter);
            }
        }

        return moveFilters;
    }
}
