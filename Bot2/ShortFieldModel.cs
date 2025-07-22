using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ShortFieldModel
{
    private ShortTileModel[,] shortField;
    private ShortUnitModel[] whiteModels;
    private ShortUnitModel[] redModels;

    private ShortMoveModel[] movesHistory;
    private int lastId;

    private ItemType item;
    private Team tookItem;
    private bool usedItem;

    public ShortFieldModel(List<UnitModel> whiteUnits, List<UnitModel> redUnits, BotTileModel[,] tilesGrid)
    {
        whiteModels = new ShortUnitModel[whiteUnits.Count];
        redModels = new ShortUnitModel[redUnits.Count];

        for (int i = 0; i < whiteUnits.Count; i++)
        {
            whiteModels[i] = new ShortUnitModel(
                BotUnitPersonality.GetUnitPersonality(whiteUnits[i].UnitType),
                whiteUnits[i]);
        
            redModels[i] = new ShortUnitModel(
                BotUnitPersonality.GetUnitPersonality(redUnits[i].UnitType),
                redUnits[i]);
        }

        shortField = new ShortTileModel[5, 5];
        movesHistory = new ShortMoveModel[6];
        lastId = 0;

        item = ItemType.Empty;
        tookItem = Team.White;
        usedItem = false;

        FillTilesByGrid(tilesGrid);
    }

    public ShortFieldModel(ShortFieldModel model)
    {
        whiteModels = new ShortUnitModel[] { model.whiteModels[0], model.whiteModels[1], model.whiteModels[2] };
        redModels = new ShortUnitModel[] { model.redModels[0], model.redModels[1], model.redModels[2] };

        shortField = new ShortTileModel[5, 5];
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                shortField[i, j] = model.shortField[i, j];
            }
        }

        //shortField = model.shortField;
        movesHistory = new ShortMoveModel[6];
        for (int i = 0; i < 6; i++)
        {
            movesHistory[i] = model.movesHistory[i];
        }

        lastId = model.lastId;

        item = model.item;
        tookItem = model.tookItem;
        usedItem = model.usedItem;
    }

    public int LastId => lastId;
    public ShortTileModel[,] ShortField => shortField;

    public ShortMoveModel[] Moves => movesHistory;

    public ShortFieldModel GetNext(ShortMoveModel move)
    {
        ShortFieldModel next = new ShortFieldModel(this);

        //next.movesHistory[lastId] = move;
        //next.lastId++;

        switch (move.MoveType)
        {
            case MoveType.Move:
                next.Move(move.Unit, move.X, move.Y);
                break;
            case MoveType.Attack:
                next.Attack(move.Unit, move.Target);
                break;
            case MoveType.Ability:
                next.UseAbility(move.Unit, move.Target);
                break;
            case MoveType.Respawn:
                next.Respawn(move.Unit);
                break;
        }


        //if (move.Unit.Team == Team.Red)
        //    move.LeadsInDanger = LeaderInDanger();

        next.movesHistory[lastId] = move;
        next.lastId++;

        int moverIndex = GetUnitModelIndex(move.Unit);

        if (move.Unit.Team == Team.White)
        {
            ref ShortUnitModel mover = ref next.whiteModels[moverIndex];
            mover.Move();
            //next.whiteModels[moverIndex].Move();
        }
        else
        {
            ref ShortUnitModel mover = ref next.redModels[moverIndex];
            mover.Move();
            //next.redModels[moverIndex].Move();
        }

        return next;
    }

    private ref ShortUnitModel GetUnitModel(UnitModel unit)
    {
        var teamArray = unit.Team == Team.White ? whiteModels : redModels;

        for (int i = 0; i < teamArray.Length; i++)
        {
            ref ShortUnitModel model = ref teamArray[i];
            if (model.IsModelOf(unit))
                return ref model;
        }
        //foreach (var model in teamArray)
        //    if (model.IsModelOf(unit))
        //        return ref model;

        return ref teamArray[0];
    }

    public int GetPosWeight(Team team)
    {
        //int weight = 0;

        return 0; //TODO Test and if fine remove method;

        
    }


    public int LeaderHealthLose(Team team)
    {
        var models = team == Team.White ? whiteModels : redModels;
        ref ShortUnitModel leader = ref models[2];

        return leader.Unit.Health - leader.Health;

        //if (team == Team.White)
        //{
        //    return whiteModels[2].Unit.Health - whiteModels[2].Health;
        //}
        //else
        //{
        //    return redModels[2].Unit.Health - redModels[2].Health;
        //}
    }

    public bool IsLeaderDead(Team team)
    {
        var models = team == Team.White ? whiteModels : redModels;
        ref ShortUnitModel leader = ref models[2];

        return leader.IsDead;
    }

    public bool HasLeaderMoved(Team team)
    {
        var models = team == Team.White ? whiteModels : redModels;
        ref ShortUnitModel leader = ref models[2];

        return leader.Moved;
    }

    private int GetUnitModelIndex(UnitModel unit)
    {
        var teamArray = unit.Team == Team.White ? whiteModels : redModels;

        for (int i = 0; i < teamArray.Length; i++)
        {
            if (teamArray[i].IsModelOf(unit))
                return i;
        }

        return 0;
    }

    private void Move(UnitModel unit, int x, int y)
    {
        ref ShortUnitModel model = ref GetUnitModel(unit);

        bool wasFree = true;
        if (shortField[x, y].IsOccupied)
            wasFree = false;

        ref ShortTileModel prevTile = ref shortField[model.Pos.x, model.Pos.y];

        //shortField[model.Pos.x, model.Pos.y].Free();
        prevTile.Free();
        if (prevTile.IsOccupied)
        {
            var models = unit.Team == Team.White ? redModels : whiteModels;

            for (int i = 0; i < models.Length; i++)
            {
                ref ShortUnitModel otherModel = ref models[i];

                if (otherModel.Pos == model.Pos)
                {
                    otherModel.IsPosOwner = true;

                    break;
                }
            }
        }

        ref ShortTileModel nextTile = ref shortField[x, y];
        nextTile.Occupy(unit);
        //shortField[x, y].Occupy(unit);

        int index = GetUnitModelIndex(unit);

        if (usedItem == false)
        {
            if (item == ItemType.Empty)
            {
                item = nextTile.Tile.ItemType;
                tookItem = unit.Team;

                //if (model.IsDead == false)
                if (item != ItemType.Empty)
                    TakeItemReacton(item, unit);
            }
            else if (item == ItemType.Teleport && tookItem == unit.Team)
            {
                item = ItemType.Empty;
                usedItem = true;
            }
        }

        model.SetPos(x, y);

        if (nextTile.IsCharged)
            model.Charge();

        model.IsPosOwner = wasFree;

        nextTile.DropCharge();
    }

    private void TakeItemReacton(ItemType item, UnitModel taker)
    {
        //int index = 0;
        ref ShortUnitModel takerModel = ref GetUnitModel(taker);

        switch (item)
        {
            case ItemType.Heal:
                if (takerModel.IsDead)
                    break;

                ref ShortUnitModel leader = ref GetUnitModel(taker.Leader);
                leader.Heal(1);
                leader.Poison(false);
                usedItem = true;
                break;

            case ItemType.Shield:
                takerModel.SetShield(true);
                usedItem = true;
                break;

            case ItemType.Charger:
                var models = taker.Team == Team.White ? whiteModels : redModels;

                for (int i = 0; i < models.Length; i++)
                {
                    ref ShortUnitModel model = ref models[i];
                    model.Charge();
                }

                usedItem = true;
                break;
        }
    }

    public List<ShortMoveModel> GetMoves(Team team, List<ShortMoveModel> moveVariants, UnitModel executor = null)
    {
        var movers = team == Team.White ? whiteModels : redModels;
        var targets = team == Team.Red ? whiteModels : redModels;
        //var moveVariants = new List<ShortMoveModel>();
        moveVariants.Clear();

        for (int i = 0; i < movers.Length; i++)
        {
            if (executor != null)
            {
                if (movers[i].Unit != executor)
                    continue;
            }

            ref ShortUnitModel mover = ref movers[i];

            //Moves

            List<TileModel> tiles = shortField[mover.Pos.x, mover.Pos.y].Tile.MoveTiles;
            if (item == ItemType.Teleport && !usedItem && tookItem == mover.Unit.Team)
                tiles = tiles[0].FieldTiles;
            foreach (var tile in tiles) 
            {
                int x = BotFieldModel.GetX(tile.Xcoord);
                int y = BotFieldModel.GetY(tile.Ycoord);

                if (shortField[x, y].IsOccupied)
                {
                    bool hasAlly = false;
                    for (int j = 0; j < movers.Length; j++)
                    {
                        //if (i == j)
                        //    continue;

                        if (x == movers[j].Pos.x && y == movers[j].Pos.y)
                        {
                            hasAlly = true;
                            break;
                        }
                    }

                    if (hasAlly)
                        continue;
                }
                


                if (tile.IsEmpty() == false)
                {
                    if (tile.UnitInTile.Team != team)
                        continue;
                }

                if (shortField[mover.Pos.x, mover.Pos.y].Tile.IsPortal)
                {
                    if (tile.IsPortal)
                        continue;

                    if (mover.IsCharged == false)
                    {
                        if (tile.IsCharged == false)
                            if (tile.ItemType == ItemType.Empty)
                                continue;
                    }
                }

                if (mover.Personality.IsOfCenterAllowed() == false)
                {
                    if (tile.IsPortal == false)
                    {
                        if (Mathf.Abs(tile.Xcoord) >= 2)
                            continue;
                        if (Mathf.Abs(tile.Ycoord) >= 2)
                            continue;
                    }
                }

                moveVariants.Add(
                    new ShortMoveModel(
                        MoveType.Move, 
                        mover.Unit, 
                        tile, 
                        shortField[mover.Pos.x, mover.Pos.y].Tile));
            }

            //Respawn
            if (mover.Personality.UnitType == UnitType.Default && mover.Unit.IsRespawnable 
                || 
                mover.Unit.Health < mover.Unit.TrueHealth && mover.Respawned == false && mover.IsCharged) //TODO: Uncomment if charge required to heal
            {
                //if (movers[i].Unit.IsRespawnable || movers[i].Unit.Health < movers[i].Unit.TrueHealth)
                //{
                    //if (movers[i].Personality.UnitType == UnitType.Default)
                moveVariants.Add(
                    new ShortMoveModel(
                        MoveType.Respawn,
                        mover.Unit,
                        mover.Unit.Position,
                        shortField[mover.Pos.x, mover.Pos.y].Tile));
                //}
            }

            //Attacks & Abilities
            if (mover.IsCharged)
            {
                //Attack
                for (int j = 0; j < targets.Length; j++)
                {
                    moveVariants.Add(
                        new ShortMoveModel(
                            MoveType.Attack,
                            mover.Unit,
                            shortField[targets[j].Pos.x, targets[j].Pos.y].Tile,
                            shortField[mover.Pos.x, mover.Pos.y].Tile,
                            targets[j].Unit));
                    
                }

                //Ability
                if (mover.Unit.UnitType == UnitType.Shield)
                {
                    for (int j = 0; j < movers.Length; j++)
                    {
                        if (movers[j].Unit.UnitType == UnitType.Default)
                            if (movers[j].IsLeader == false)
                                continue;
                        if (movers[j].Unit.HasShield)
                            continue;

                        //if (movers[j].HasShield)
                        //    continue;
                        //if (movers[j].IsDead)
                        //    continue;

                        moveVariants.Add(
                            new ShortMoveModel(
                                MoveType.Ability,
                                mover.Unit,
                                shortField[movers[j].Pos.x, movers[j].Pos.y].Tile,
                                shortField[mover.Pos.x, mover.Pos.y].Tile,
                                movers[j].Unit));
                    }
                }
            }
        }

        return moveVariants;
    }

    public List<ShortMoveModel> GetMoves(Team team, MoveFilter filter, bool allowOfCenter, List<ShortMoveModel> moveVariants)
    {
        var movers = team == Team.White ? whiteModels : redModels;
        var targets = team == Team.Red ? whiteModels : redModels;

        moveVariants.Clear();
        //var moveVariants = new List<ShortMoveModel>();

        for (int i = 0; i < movers.Length; i++)
        {
            ref ShortUnitModel mover = ref movers[i];

            if (filter.Executor != null)
            {
                if (filter.IsExecutor(mover.Unit) == false)
                {
                    continue;
                }
            }

            if (mover.Unit.Locked)
                continue;

            //Moves

            List<TileModel> tiles = shortField[mover.Pos.x, mover.Pos.y].Tile.MoveTiles;
            //if (item == ItemType.Teleport && !usedItem && tookItem == movers[i].Unit.Team)
            //    tiles = tiles[0].FieldTiles;
            if (shortField[mover.Pos.x, mover.Pos.y].Tile.IsPortal && team == Team.Red)
            {
                var currentTile = shortField[mover.Pos.x, mover.Pos.y].Tile;

                tiles = new List<TileModel>();
                tiles.AddRange(currentTile.MeleeTiles);

                if (currentTile.MostDistantTile != null)
                    tiles.Add(currentTile.MostDistantTile);
                if (currentTile.MostThreatningtTile != null)
                    tiles.Add(currentTile.MostThreatningtTile);

                var itemTile = currentTile.MoveTiles.Find(x => x.ItemType != ItemType.Empty);
                if (itemTile != null)
                {
                    if (currentTile.MeleeTiles.Contains(itemTile) == false)
                        tiles.Add(itemTile);
                }
            }

            foreach (var tile in tiles)
            {
                int x = BotFieldModel.GetX(tile.Xcoord);
                int y = BotFieldModel.GetY(tile.Ycoord);

                if (shortField[x, y].IsOccupied)
                {
                    bool hasAlly = false;
                    for (int j = 0; j < movers.Length; j++)
                    {
                        //if (i == j)
                        //    continue;

                        if (x == movers[j].Pos.x && y == movers[j].Pos.y)
                        {
                            hasAlly = true;
                            break;
                        }
                    }

                    if (hasAlly)
                        continue;
                }

                if (tile.IsEmpty() == false)
                {
                    if (tile.UnitInTile.Team != team)
                        continue;
                }

                ref ShortTileModel moverTile = ref shortField[mover.Pos.x, mover.Pos.y];

                if (moverTile.Tile.IsPortal)
                    if (tile.IsPortal)
                        continue;

                if (mover.Personality.IsOfCenterAllowed() == false)
                {
                    if (allowOfCenter == false && tile.IsPortal == false)
                    {
                        if (Mathf.Abs(tile.Xcoord) >= 2)
                            continue;
                        if (Mathf.Abs(tile.Ycoord) >= 2)
                            continue;
                    }
                }

                var moveVariant = new ShortMoveModel(
                        MoveType.Move,
                        mover.Unit,
                        tile,
                        shortField[mover.Pos.x, mover.Pos.y].Tile);

                if (filter.Match(moveVariant))
                    moveVariants.Add(moveVariant);
            }

            //Respawn
            if (mover.Personality.UnitType == UnitType.Default && mover.Unit.IsRespawnable
                ||
                mover.Unit.Health < mover.Unit.TrueHealth && mover.Respawned == false && mover.IsCharged) //TODO: Uncomment if charge required to heal
            {
                //if (movers[i].Unit.IsRespawnable)
                //{
                    //if (movers[i].Personality.UnitType == UnitType.Default)
                var moveVariant = new ShortMoveModel(
                        MoveType.Respawn,
                        mover.Unit,
                        mover.Unit.Position,
                        shortField[mover.Pos.x, mover.Pos.y].Tile);

                if (filter.Match(moveVariant))
                    moveVariants.Add(moveVariant);
                //}
            }

            //Attacks & Abilities
            if (movers[i].IsCharged)
            {
                //Attack
                for (int j = 0; j < targets.Length; j++)
                {
                    var moveVariant = new ShortMoveModel(
                            MoveType.Attack,
                            movers[i].Unit,
                            shortField[targets[j].Pos.x, targets[j].Pos.y].Tile,
                            shortField[mover.Pos.x, mover.Pos.y].Tile,
                            targets[j].Unit);

                    if (filter.Match(moveVariant))
                        moveVariants.Add(moveVariant);

                }

                //Ability
                if (mover.Unit.UnitType == UnitType.Shield)
                {
                    for (int j = 0; j < movers.Length; j++)
                    {
                        if (movers[j].Unit.UnitType == UnitType.Default)
                            if (movers[j].IsLeader == false)
                                continue;
                        if (movers[j].Unit.HasShield)
                            continue;

                        var moveVariant = new ShortMoveModel(
                                MoveType.Ability,
                                mover.Unit,
                                shortField[movers[j].Pos.x, movers[j].Pos.y].Tile,
                                shortField[mover.Pos.x, mover.Pos.y].Tile,
                                movers[j].Unit);

                        if (filter.Match(moveVariant))
                            moveVariants.Add(moveVariant);
                    }
                }
            }
        }

        return moveVariants;
    }

    public void AfterMath()
    {
        //var leaderW = GetUnitModel(whiteModels[0].Unit.totem.Leader);
        //var leaderR = GetUnitModel(redModels[0].Unit.totem.Leader);

        int leaderIndexW = 2;
        int leaderIndexR = 2;

        ref ShortUnitModel leaderW = ref whiteModels[leaderIndexW];
        ref ShortUnitModel leaderR = ref redModels[leaderIndexR];

        //Collision Fight
        for (int i = 0; i < whiteModels.Length; i++)
        {
            ref ShortUnitModel wm = ref whiteModels[i];
            if (wm.IsDead)
                continue;

            for (int j = 0; j < whiteModels.Length; j++)
            {
                ref ShortUnitModel rm = ref redModels[j];
                if (rm.IsDead)
                    continue;

                if (wm.Pos.x == rm.Pos.x && wm.Pos.y == rm.Pos.y)
                {
                    if (rm.IsCharged)
                    {
                        if (wm.HasShield)
                            wm.SetShield(false);
                        else
                            wm.TakeDamage(rm.Damage);
                    }
                    if (wm.IsCharged)
                    {
                        if (rm.HasShield)
                            rm.SetShield(false);
                        else
                           rm.TakeDamage(wm.Damage);
                    }

                    var models = rm.IsPosOwner ? whiteModels : redModels;
                    int index = rm.IsPosOwner ? i : j;
                    ref ShortUnitModel refModel = ref models[index];

                    var pos = GetJumpPosClockwise(refModel.Pos);
                    var posFrom = refModel.Pos;

                    if (shortField[pos.x, pos.y].Tile.IsSuddenDeathTarget && 
                        shortField[posFrom.x, posFrom.y].Tile.IsSuddenDeathTarget == false)
                    {
                        if (refModel.HasShield)
                            refModel.SetShield(false);
                        else
                            refModel.TakeDamage(1);
                    }
                }
            }
        }

        //SuddenDeath Damage & afterdamage
        for (int i = 0; i < whiteModels.Length; i++)
        {
            ref ShortUnitModel wm = ref whiteModels[i];
            ref ShortUnitModel rm = ref redModels[i];

            wm.AfterDamage();
            rm.AfterDamage();

            if (shortField[wm.Pos.x, wm.Pos.y].Tile.IsSuddenDeathTarget)
            {
                if (wm.HasShield)
                    wm.SetShield(false);
                else
                    wm.TakeDamage(1);
            }
            if (shortField[rm.Pos.x, rm.Pos.y].Tile.IsSuddenDeathTarget)
            {
                if (rm.HasShield)
                    rm.SetShield(false);
                else
                    rm.TakeDamage(1);
            }

            if (wm.Personality.UnitType == UnitType.Venomancer)
            {
                if (wm.IsCharged || wm.IsDead)
                {
                    for (int j = 0; j < redModels.Length; j++)
                    {
                        if (Mathf.Abs(redModels[j].Pos.x - wm.Pos.x) <= 1 &&
                            Mathf.Abs(redModels[j].Pos.y - wm.Pos.y) <= 1 &&
                            redModels[j].Pos != whiteModels[i].Pos)
                        {
                            redModels[j].Poison(true);
                        }
                    }
                }
                whiteModels[i].TakeDamage(1);
            }
            if (redModels[i].Personality.UnitType == UnitType.Venomancer)
            {
                if (rm.IsCharged || rm.IsDead)
                {
                    for (int j = 0; j < redModels.Length; j++)
                    {
                        if (Mathf.Abs(whiteModels[j].Pos.x - rm.Pos.x) <= 1 &&
                            Mathf.Abs(whiteModels[j].Pos.y - rm.Pos.y) <= 1 &&
                            whiteModels[j].Pos != rm.Pos)
                        {
                            whiteModels[j].Poison(true);
                        }
                    }
                }
                redModels[i].TakeDamage(1);
            }

        }

        //Respawn Damage
        for (int i = 0; i < whiteModels.Length; i++)
        {

            ref ShortUnitModel wm = ref whiteModels[i];
            if (wm.Unit != leaderW.Unit && wm.IsDead)
            {
                leaderW.LeaderRespawnDamage(1);
            }

            ref ShortUnitModel rm = ref redModels[i];
            if (rm.Unit != leaderR.Unit && rm.IsDead)
            {
                leaderR.LeaderRespawnDamage(1);
            }

            //if (whiteModels[i].Unit != whiteModels[2].Unit)
            //{
            //    if (whiteModels[i].IsDead)
            //    {
            //        whiteModels[leaderIndexW].LeaderRespawnDamage(1);
            //    }
            //}
            //if (redModels[i].Unit != redModels[leaderIndexR].Unit)
            //{
            //    if (redModels[i].IsDead)
            //    {
            //        redModels[leaderIndexR].LeaderRespawnDamage(1);
            //    }
            //}
        }
    }

    private void UseAbility(UnitModel user, UnitModel targetUnit)
    {
        ref ShortUnitModel model = ref GetUnitModel(user);
        ref ShortUnitModel target = ref GetUnitModel(targetUnit);

        //ref ShortTileModel targetTile = ref shortField[target.Pos.x, target.Pos.y];
        ref ShortTileModel modelTile = ref shortField[model.Pos.x, model.Pos.y];

        if (item != ItemType.Infinity || usedItem == true) //Causes shield not dropping charge
        {
            modelTile.DropCharge();
            model.DropCharge();
        }
        else if (tookItem == user.Team)
        {
            usedItem = true;
        }
        else
        {
            modelTile.DropCharge();
            model.DropCharge();
        }
        //shortField[model.Pos.x, model.Pos.y].DropCharge();
        //model.DropCharge();

        int index = GetUnitModelIndex(user);
        int targetIndex = GetUnitModelIndex(targetUnit);

        if (model.IsDead)
        {
            return;
        }

        if (model.Unit.UnitType == UnitType.Shield)
        {
            target.SetShield(true);
        }

        if (user == targetUnit)
        {
            target.DropCharge();
        }
    }

    private void Respawn(UnitModel respawner)
    {
        ref ShortUnitModel model = ref GetUnitModel(respawner);
        ref ShortTileModel tile = ref shortField[model.Pos.x, model.Pos.y];

        tile.DropCharge();
        //shortField[model.Pos.x, model.Pos.y].DropCharge();
        //model.DropCharge();

        ShortUnitModel[] models = respawner.Team == Team.White ? whiteModels : redModels;
        int index = GetUnitModelIndex(respawner);

        if (model.Personality.UnitType == UnitType.Default && model.Unit.IsRespawnable)
        {
            models[index] = model.IsDead ? new ShortUnitModel(model, true) : new ShortUnitModel(model);
        }
        else
        {
            if (model.IsDead == false)
            {
                model.Heal(1);
                model.Poison(false);
            }

            model.Respawned = true;

            model.DropCharge();

            models[index] = model.IsDead ? new ShortUnitModel(model, true) : model;
        }
    }

    private void Attack(UnitModel attacker, UnitModel targetUnit)
    {
        ref ShortUnitModel model = ref GetUnitModel(attacker);
        ref ShortUnitModel target = ref GetUnitModel(targetUnit);

        ref ShortTileModel attackerTile = ref shortField[model.Pos.x, model.Pos.y];
        ref ShortTileModel targetTile = ref shortField[target.Pos.x, target.Pos.y];

        //shortField[model.Pos.x, model.Pos.y].DropCharge(); actually not needed (tile discharged on unit enter)
        model.DropCharge();

        //int index = GetUnitModelIndex(attacker);
        //int targetIndex = GetUnitModelIndex(target.Unit);


        if (model.IsDead)
        {
            return;
        }
        else if (
            attackerTile.Tile.MeleeTiles.Contains(
                        targetTile.Tile) == false &&
            attackerTile.Tile !=
                        targetTile.Tile)
        {
            if (attackerTile.Tile.RangeTiles.Contains(
                        targetTile.Tile) == false)
            {
                return;
            }
            else if (
                model.IsRanger == false &&
                (item == ItemType.Range && !usedItem && tookItem == attacker.Team) == false)
            {
                return;
            }
        }


        if (target.HasShield == false)
        {
            if (item == ItemType.Rage && attacker.Team == tookItem && usedItem == false)
            {
                target.TakeDamage((short)(model.Damage + 1));
            }
            else
            {
                target.TakeDamage(model.Damage);
            }

            //target.TakeDamage(model.Damage);
            if (attacker.UnitType == UnitType.Poison)
                target.Poison();
        }
        else
        {
            target.SetShield(false);
        }

        if (HasHitBack(targetUnit.Team))
        {
            model.TakeDamage(2);
        }        

        if (item == ItemType.FireDamage && attacker.Team == tookItem && usedItem == false)
        {
            var targets = target.Unit.Team == Team.White ? whiteModels : redModels;
            for (int i = 0; i < whiteModels.Length; i++)
            {
                //if (i == targetIndex)
                //    continue;
                if (targets[i].Unit == target.Unit)
                    continue;

                ref ShortUnitModel splashTarget = ref targets[i];

                if (splashTarget.HasShield)
                    splashTarget.SetShield(false);
                else
                    splashTarget.TakeDamage(1);
            }
            usedItem = true;
        }

        if (item == ItemType.Range && !usedItem && tookItem == attacker.Team)
            usedItem = true;
    }

    //TODO: temp solution rework for all team effects
    private bool HasHitBack(Team team)
    {
        var models = team == Team.White ? whiteModels : redModels;

        for (int i = 0; i < models.Length; i++)
        {
            if (models[i].Personality.UnitType == UnitType.Thorn && models[i].IsCharged)
                return true;
        }

        return false;
    }

    public string GetName()
    {
        return
            movesHistory[0].GetName() +
            movesHistory[2].GetName() +
            movesHistory[4].GetName();
    }

    /// <summary>
    /// Returns negative value of possition unlikeliness
    /// </summary>
    /// <returns></returns>
    public int GetRealityMetric()
    {
        int metric = 0;

        int repeats = 0;
        int attacks = 0;

        bool poisonMoved = false;

        var lastMover = movesHistory[0].Unit;

        for (int i = 1; i < movesHistory.Length; i += 2)
        {
            if (movesHistory[i].Unit == null)
                continue;

            if (lastMover == movesHistory[i].Unit)
                repeats++;

            lastMover = movesHistory[i].Unit;

            if (lastMover.IsLeader)
            {
                metric += 5;
            }
            else
            {

            }

            if (movesHistory[i].MoveType == MoveType.Attack)
            {
                attacks++;

                metric += 2;
                if (movesHistory[i].Target.IsLeader)
                    metric += 10;

                if (movesHistory[i].Target.IsLeader)
                {
                    if (movesHistory[0].Unit.IsLeader == false)
                        metric -= 2;
                }
            }
        }

        for (int i = 0; i < movesHistory.Length; i += 2) 
        {
            if (movesHistory[i].Unit == null)
                continue;

            if (movesHistory[i].MoveType == MoveType.Ability)
            {
                if (movesHistory[i].Target.HasShield == true)
                    metric -= 80;
            }
            else if (movesHistory[i].MoveType == MoveType.Attack)
            {
                if (Mathf.Abs(movesHistory[i].Tile.Xcoord - movesHistory[i].Target.Position.Xcoord) >= 2 ||
                    Mathf.Abs(movesHistory[i].Tile.Ycoord - movesHistory[i].Target.Position.Ycoord) >= 2) 
                    metric -= 400;

                if (i > 0)
                    metric -= 4;
                if (i == 4)
                    metric -= 8;

            }

            if (movesHistory[i].Unit.UnitType == UnitType.Poison)
            {
                if (poisonMoved == false)
                    poisonMoved = true;
            }

            if (item == ItemType.Range && tookItem == Team.White)
            {
                if (movesHistory[i].MoveType == MoveType.Attack)
                {
                    if (movesHistory[i].Target != movesHistory[0].Unit.Leader)
                    {
                        if (shortField[2, 2].Tile.ItemType != ItemType.Empty)
                            metric -= 8;//250;
                        else
                            metric -= 2;//40;
                    }
                }
            }
        }

        if (GetUnitModel(movesHistory[0].Unit.Leader).Moved == false)
        {
            if (lastId >= 5)
            {
                metric -= 24;
                if (movesHistory[0].Unit.Leader.Position.HasEnemyInRange(movesHistory[0].Unit, true))
                {
                    metric -= 60;
                }
            }
        }

        if (poisonMoved == false)
            metric -= movesHistory[5].Unit == null ? 4 : 16;

        int leaderIndexW = GetUnitModelIndex(whiteModels[0].Unit.Leader);

        if (lastId >= 5)
        {
            if (whiteModels[leaderIndexW].Moved == false)
            {
                if (whiteModels[leaderIndexW].HasShield)
                    metric -= 50;
                else
                    metric -= 150;
            }
        }

        if (lastId >= 6)
        {
            metric -= repeats == 2 ? 36 : 0;
            metric -= repeats > 2 ? 120 : 0;
            metric -= attacks < 1 ? 8 : 0;
            metric -= attacks >= 2 ? 16 : 0;
        }

        return metric;
    }

    /// <summary>
    /// Serves to evaluate white's move with out cutting red
    /// </summary>
    /// <returns></returns>
    public int GetImportance()
    {
        int metric = 0;
        var leader = whiteModels[0].Unit.Leader;

        for (int i = 2; i < movesHistory.Length; i += 2)
        {
            if (movesHistory[i].Unit == null)
                continue;

            if (movesHistory[i].Unit == leader)
                metric += 10;
            else if (movesHistory[i].Unit.UnitType != UnitType.Default)
                metric += 10;
            else if (movesHistory[i].MoveType == MoveType.Respawn)
                metric += 10;

            if (movesHistory[i].Tile.ItemType != ItemType.Empty)
                metric += 10;

            if (movesHistory[i].MoveType == MoveType.Attack)
            {
                if (RevealLocalUtil.Instance.Order == i / 2 + 1)
                    metric -= 2;
                if (movesHistory[i].Target.IsLeader)
                    metric += 2;
            }

        }

        if (RevealLocalUtil.Instance.Order == 3)
        {
            if (movesHistory[4].Unit == leader)
                metric -= 5;
        }

        return metric;
    }

    public int GetMajorDifference(bool deathIsBad = false, bool saveAllCost = false)
    {
        int hplost = 0;
        int unitsDiff = 0;
        int unreasonable = 0;

        for (int i = 0; i < whiteModels.Length; i++)
        {
            if (whiteModels[i].IsDead)
            {
                hplost++;
                if (saveAllCost)
                    unreasonable += 500;

                //continue;
            }
            else if (whiteModels[i].IsLeader == false)
            {
                //if (whiteModels[i].IsLeader == false)
                unitsDiff -= whiteModels[i].Health * 2;

                if (whiteModels[i].Personality.UnitType != UnitType.Default)
                {
                    unitsDiff -= 2;
                    if (saveAllCost)
                        unreasonable -= 50;
                }

                if (whiteModels[i].IsPoisoned)
                    hplost += 1;
            }
            else
            {
                if (whiteModels[i].IsPoisoned)
                    hplost += 2;
            }

            if (redModels[i].IsDead || redModels[i].IsPoisoned)
            {
                hplost--;
                if (redModels[i].IsLeader)
                {
                    hplost -= redModels[i].IsDead ? 10000 : 8000;
                }
                else
                {
                    hplost--;
                }
                //continue;
            }
            else if (redModels[i].IsLeader == false)
            {
                unitsDiff += redModels[i].Health * 2;
                if (whiteModels[i].Personality.UnitType != UnitType.Default)
                    unitsDiff += 2;
            }

            if (redModels[i].HasShield)
                unitsDiff += redModels[i].IsLeader ? 2 : 1;

            if (whiteModels[i].HasShield)
                unitsDiff -= whiteModels[i].IsLeader ? 2 : 1;

            if (whiteModels[i].IsCharged)
            {
                unitsDiff--;
                if (whiteModels[i].ChargeLimit > 1)
                    unitsDiff -= whiteModels[i].ChargeStage;
            }

            if (tookItem == Team.White) {
                //if (item == ItemType.FireDamage || item == ItemType.Rage || item == ItemType.Range)
                
                if (movesHistory[2].MoveType != MoveType.Attack && movesHistory[4].MoveType != MoveType.Attack)
                {
                    switch (item)
                    {
                        case ItemType.FireDamage:
                            unreasonable += 3;
                            break;

                        case ItemType.Rage:
                            unreasonable += 1;
                            break;

                        case ItemType.Range:
                            unreasonable += 3;
                            break;
                    }
                }
                
            }

            //if (whiteModels[i].IsLeader == false)
            //    unitsDiff -= whiteModels[i].Health;
            //if (redModels[i].IsLeader == false)
            //    unitsDiff += redModels[i].Health;
        }

        if (RevealLocalUtil.Instance.Order > 1) {
            for (int i = 0; i < movesHistory.Length; i += 2) {
                if (i / 2 + 1 == RevealLocalUtil.Instance.Order)
                {
                    if (movesHistory[i].Unit.IsLeader)
                        unreasonable += 6;
                    else if (movesHistory[i].MoveType == MoveType.Attack)
                        unreasonable += item == ItemType.Range ? 0 : 3;
                }
            } 
        }

        for (int i = 0; i < movesHistory.Length; i += 2)
        {
            if (movesHistory[i].MoveType == MoveType.Ability)
            {
                if (movesHistory[i].Target.Leader.HasShield == false)
                    if (movesHistory[i].Target.IsLeader == false)
                        unreasonable += 12;
            }

            if (movesHistory[i].MoveType != MoveType.Attack)
                continue;

            if (!movesHistory[i].TileFrom.MeleeTiles.Contains(movesHistory[i].Target.Position))
                if (tookItem != Team.White || item != ItemType.Range)
                    unreasonable += 6;

            if (saveAllCost)
            {
                if (movesHistory[i].MoveType == MoveType.Respawn)
                    unreasonable -= 450;

                if (movesHistory[i].MoveType == MoveType.Ability &&
                    movesHistory[i].Target.UnitType == UnitType.Poison)
                    unreasonable -= 650;
            }
        }

        int whiteLeaderIndex = GetUnitModelIndex(movesHistory[0].Unit.Leader);
        int redLeaderIndex = GetUnitModelIndex(movesHistory[1].Unit.Leader);

        if (saveAllCost)
        {
            if (whiteModels[whiteLeaderIndex].Moved == false)
                unreasonable += 200;
        }

        if (whiteModels[whiteLeaderIndex].IsDead)
        {
            hplost += 500;
            hplost += -whiteModels[whiteLeaderIndex].Health * 100;

            if (deathIsBad)
            {
                hplost += 20000;
            }

            if (shortField[whiteModels[whiteLeaderIndex].Pos.x, whiteModels[whiteLeaderIndex].Pos.y].Tile.
                IsSuddenDeathTarget)
                unreasonable += 500;

            for (int i = 0; i < whiteModels.Length; i++)
            {
                //if (whiteModels[i].IsDead == false)
                //    continue;

                if (shortField[whiteModels[i].Pos.x, whiteModels[i].Pos.y].Tile.IsSuddenDeathTarget)
                {
                    unreasonable += 500;
                }
            }

            if (whiteModels[whiteLeaderIndex].Unit.HasShield == false)
            {
                for (int i = 0; i <= 4; i += 2) 
                {
                    if (movesHistory[i].MoveType != MoveType.Ability)
                    {
                        unreasonable += 200;
                    }
                    else if (movesHistory[i].Target.IsLeader == false)
                    {
                        unreasonable += 200;
                    }
                }
            }
        }

        if (redModels[redLeaderIndex].IsPoisoned)
            hplost -= 4;

        return
            (whiteModels[whiteLeaderIndex].Health -
            redModels[redLeaderIndex].Health - hplost) * 6 - unitsDiff - unreasonable;
    }

    private Vector2Int GetJumpPosClockwise(Vector2Int posFrom)
    {
        var pattern = UnitModel.JumpPattern;
        int length = shortField.GetLength(0);

        foreach (var pos in pattern)
        {
            if (pos.x + posFrom.x >= length || pos.y + posFrom.y >= length)
                continue;
            if (pos.x + posFrom.x < 0 || pos.y + posFrom.y < 0)
                continue;

            var shortTile = shortField[pos.x + posFrom.x, pos.y + posFrom.y];
            if (shortTile.IsOccupied || shortTile.Tile == null)
                continue;

            return posFrom + pos;
        }

        return posFrom;
    }

    private void FillTilesByGrid(BotTileModel[,] tilesGrid)
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (tilesGrid[i, j] == null)
                {
                    shortField[i, j] = new ShortTileModel(true);
                    continue;
                }
                
                shortField[i, j] = new ShortTileModel(tilesGrid[i, j].Tile.TileModel);
            }
        }
    }
}
