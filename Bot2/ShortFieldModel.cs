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

        for (int i = 0; i < whiteModels.Length; i++)
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
            next.whiteModels[moverIndex].Move();
        else
            next.redModels[moverIndex].Move();

        return next;
    }

    private ShortUnitModel GetUnitModel(UnitModel unit)
    {
        var teamArray = unit.Team == Team.White ? whiteModels : redModels;

        foreach (var model in teamArray)
            if (model.IsModelOf(unit))
                return model;

        return teamArray[0];
    }

    public int GetPosWeight(Team team)
    {
        //int weight = 0;

        return 0; //TODO Test and if fine remove method;

        //ShortUnitModel leaderW = whiteModels[0];
        //ShortUnitModel leaderR = redModels[0];
        //
        //int leaderAfterDmgW = 0;
        //int leaderAfterDmgR = 0;
        //
        //for (int i = 0; i < whiteModels.Length; i++)
        //{
        //    if (whiteModels[i].IsLeader)
        //    {
        //        leaderW = whiteModels[i];
        //        if (whiteModels[i].IsDead)
        //            weight += team == Team.White ? -1000 : 1000;
        //        else
        //            weight += team == Team.White ? whiteModels[i].Health * 32 : -whiteModels[i].Health * 32;
        //
        //        if (whiteModels[i].HasShield) {
        //            weight += team == Team.White ? 40 : -4;
        //
        //            if (lastId <= 2)
        //            {
        //                weight += team == Team.White ? 40 : 0;
        //            }
        //        }
        //            
        //
        //        if (shortField[whiteModels[i].Pos.x, whiteModels[i].Pos.y].Tile.IsSuddenDeathTarget)
        //            weight += team == Team.White ? -16 : 8;
        //
        //        if (shortField[leaderW.Pos.x, leaderW.Pos.y].Tile.HasEnemyInRange(
        //            leaderW.Unit, UnitType.MeleeMaster, true))
        //            weight += team == Team.White ? -20 : 4;
        //        if (shortField[leaderW.Pos.x, leaderW.Pos.y].Tile.HasEnemyInRange(
        //            leaderW.Unit, UnitType.MeleeMaster, false))
        //        {
        //            if (leaderW.HasShield)
        //                weight += team == Team.White ? -4 : 4;
        //            else
        //                weight += team == Team.White ? -18 : 4;
        //        }
        //
        //    }
        //    else
        //    {
        //        if (whiteModels[i].IsDead)
        //        {
        //            weight += team == Team.White ? -28 : 28;
        //            leaderAfterDmgW++;
        //        }
        //        else
        //        {
        //            weight += team == Team.White ? whiteModels[i].Health * 18 : -whiteModels[i].Health * 18;
        //        }
        //
        //        if (whiteModels[i].Personality.UnitType != UnitType.Default)
        //        {
        //            weight += team == Team.White ? 24 : -3;
        //            if (whiteModels[i].Personality.UnitType == UnitType.Poison)
        //            {
        //                if (team == Team.White)
        //                    weight += 10;
        //
        //                if (whiteModels[i].HasShield)
        //                    weight += team == Team.White ? 2 : -3;
        //                if (whiteModels[i].IsCharged)
        //                    weight += team == Team.White ? 6 : -1;
        //                if (shortField[whiteModels[i].Pos.x, whiteModels[i].Pos.y].Tile.HasEnemyInRange(
        //                    whiteModels[i].Unit, UnitType.DefaultLeader, true))
        //                    weight += team == Team.White ? 8 : 0;
        //
        //                if (shortField[whiteModels[i].Pos.x, whiteModels[i].Pos.y].Tile.HasEnemyInRange(
        //                    whiteModels[i].Unit, false))
        //                {
        //                    if (whiteModels[i].IsCharged)
        //                        weight += team == Team.White ? 36 : 0;
        //                    else
        //                        weight += team == Team.White ? 2 : 0;
        //                }
        //
        //                if (lastId >= 5)
        //                {
        //                    if (shortField[whiteModels[i].Pos.x, whiteModels[i].Pos.y].Tile.IsPortal)
        //                        weight += team == Team.White ? 36 : 0;
        //                }
        //                else
        //                {
        //                    if (shortField[whiteModels[i].Pos.x, whiteModels[i].Pos.y].Tile.IsPortal)
        //                        weight += team == Team.White ? 8 : 0;
        //                }
        //            }
        //        }
        //
        //        if (tookItem == Team.White)
        //        {
        //            if (item == ItemType.SleepDmg)
        //            {
        //                if (whiteModels[i].Moved == false)
        //                    leaderAfterDmgR++;
        //            }
        //            else if (item == ItemType.SleepHP)
        //            {
        //                if (whiteModels[i].Moved == false)
        //                    leaderAfterDmgW--;
        //            }
        //        }
        //    }
        //
        //    weight += team == Team.White ? -whiteModels[i].Xofcenter - whiteModels[i].YofCenter : 0;
        //    if (whiteModels[i].Unit.UnitType == UnitType.Poison)
        //        weight += team == Team.White ? -whiteModels[i].Xofcenter - whiteModels[i].YofCenter : 0;
        //
        //    if (whiteModels[i].IsCharged)
        //        weight += team == Team.White ? 8 : -1;
        //    else if (shortField[whiteModels[i].Pos.x, whiteModels[i].Pos.y].Tile.ChargeStage == 1)
        //        weight += team == Team.White ? 2 : 0;
        //
        //    if (shortField[whiteModels[i].Pos.x, whiteModels[i].Pos.y].Tile.IsSuddenDeathTarget)
        //    {
        //        weight += team == Team.White ? -12 : 3;
        //        if (whiteModels[i].Health == 1)
        //            leaderAfterDmgW++;
        //    }
        //
        //    if (redModels[i].IsLeader)
        //    {
        //        leaderR = redModels[i];
        //        if (redModels[i].IsDead)
        //            weight += team == Team.Red ? -1000 : 1000;
        //        else
        //            weight += team == Team.Red ? redModels[i].Health * 32 : -redModels[i].Health * 32;
        //
        //        if (redModels[i].IsPoisoned)
        //        {
        //            weight += team == Team.Red ? -72 : 72;
        //            if (lastId <= 2)
        //            {
        //                weight += team == Team.Red ? 0 : 72;
        //            }
        //        }
        //
        //        if (redModels[i].HasShield)
        //            weight += team == Team.Red ? 40 : -4;
        //
        //        if (shortField[leaderR.Pos.x, leaderR.Pos.y].Tile.HasEnemyInRange(
        //            leaderR.Unit, UnitType.Poison, true))
        //            weight += team == Team.Red ? -20 : 2;
        //
        //    }
        //    else
        //    {
        //        if (redModels[i].IsDead)
        //        {
        //            weight += team == Team.Red ? -24 : 24;
        //            leaderAfterDmgR++;
        //        }
        //        else
        //        {
        //            weight += team == Team.Red ? redModels[i].Health * 18 : -redModels[i].Health * 18;
        //        }
        //
        //        if (redModels[i].IsPoisoned)
        //            weight += team == Team.Red ? -10 : 16;
        //
        //        if (redModels[i].Personality.UnitType != UnitType.Default)
        //            weight += team == Team.Red ? 16 : -4;
        //
        //        if (tookItem == Team.Red)
        //        {
        //            if (item == ItemType.SleepDmg)
        //            {
        //                if (redModels[i].Moved == false)
        //                    leaderAfterDmgW++;
        //            }
        //            else if (item == ItemType.SleepHP)
        //            {
        //                if (redModels[i].Moved == false)
        //                    leaderAfterDmgR--;
        //            }
        //        }
        //    }
        //
        //    weight += team == Team.Red ? -redModels[i].Xofcenter - redModels[i].YofCenter : 0;
        //    if (redModels[i].Unit.UnitType == UnitType.MeleeMaster)
        //    {
        //        weight += team == Team.Red ? -redModels[i].Xofcenter - redModels[i].YofCenter : 0;
        //        weight += team == Team.Red ? 4 : -2;
        //    }
        //
        //    if (redModels[i].IsCharged)
        //        weight += team == Team.Red ? 8 : -1;
        //    else if (shortField[redModels[i].Pos.x, redModels[i].Pos.y].Tile.ChargeStage == 1)
        //        weight += team == Team.Red ? 2 : 0;
        //
        //    if (shortField[redModels[i].Pos.x, redModels[i].Pos.y].Tile.IsSuddenDeathTarget)
        //    {
        //        weight += team == Team.Red ? -8 : 3;
        //        if (redModels[i].Health == 1)
        //            leaderAfterDmgR++;
        //    }
        //
        //}
        //
        ////rnd value Shift;
        ////weight += Random.Range(-1, 2);
        //
        //if (lastId >= 5)
        //{
        //    if (team == Team.White)
        //    {
        //        for (int i = 0; i < movesHistory.Length; i += 2)
        //        {
        //            if (movesHistory[i].MoveType == MoveType.Attack)
        //            {
        //                if (Mathf.Abs(movesHistory[i].Tile.Xcoord - movesHistory[i].Target.Position.Xcoord) >= 2 ||
        //                    Mathf.Abs(movesHistory[i].Tile.Ycoord - movesHistory[i].Target.Position.Ycoord) >= 2)
        //                    weight += -120;
        //
        //                else if (
        //                    Mathf.Abs(movesHistory[i].Tile.Xcoord - movesHistory[i].TileFrom.Xcoord) >= 2 ||
        //                    Mathf.Abs(movesHistory[i].Tile.Ycoord - movesHistory[i].TileFrom.Ycoord) >= 2)
        //                    if (item != ItemType.Range && tookItem != Team.White)
        //                        weight += -80;
        //
        //            }
        //        }
        //
        //        if (RevealLocalUtil.Instance.Order > 1)
        //        {
        //            if (RevealLocalUtil.Instance.Order == 2)
        //            {
        //
        //                if (movesHistory[2].MoveType == MoveType.Attack)
        //                {
        //                    //bool hasNoEscape = false;
        //                    //for (int i = 0; i < redModels.Length; i++)
        //                    //{
        //                    //    if (whiteModels[i].Unit.UnitType == UnitType.Poison)
        //                    //    {
        //                    //        if (HasNoEscape(leaderR.Unit, shortField[whiteModels[i].Pos.x, whiteModels[i].Pos.y].Tile))
        //                    //        {
        //                    //            hasNoEscape = true;
        //                    //            break;
        //                    //        }
        //                    //    }
        //                    //}
        //                    //
        //                    //if (hasNoEscape == false)
        //                    weight -= 20; //100;
        //                }
        //            }
        //            else if (RevealLocalUtil.Instance.Order == 3)
        //            {
        //                if (movesHistory[4].MoveType == MoveType.Attack)
        //                    weight -= 100;
        //
        //                if (movesHistory[4].Unit.IsLeader)
        //                    weight -= 60;
        //            }
        //
        //            //for (int i = 0; i <= 4; i += 2)
        //            //{
        //            //    if (movesHistory[i].MoveType == MoveType.Attack)
        //            //        if (RevealLocalUtil.Instance.Order == i / 2)
        //            //            weight -= 250;
        //            //}
        //        }
        //
        //        if (leaderW.Moved == false)
        //            weight -= 30;
        //    }
        //    else
        //    {
        //        if (leaderR.Moved == false)
        //            weight -= 30;
        //    }
        //}
        ////else if (lastId <= 1)
        ////{
        ////    if (movesHistory[0].MoveType == MoveType.Attack)
        ////    {
        ////        
        ////    }
        ////}
        //else if (lastId <= 2)//first Move Case
        //{
        //    if (movesHistory[0].MoveType == MoveType.Attack)
        //        weight += team == Team.White ? 10 : 0;
        //
        //    if (team == Team.White)
        //    {
        //        if (movesHistory[0].MoveType == MoveType.Ability)
        //        {
        //            if (movesHistory[0].Target.IsLeader)
        //            {
        //                if (movesHistory[0].Target.Health <= 2)
        //                    weight += 100;
        //                if (movesHistory[0].Target.Health <= 1)
        //                    weight += 200;
        //            }
        //        }
        //    }
        //
        //    for (int i = 0; i < whiteModels.Length; i++)
        //    {
        //        if (whiteModels[i].Unit.UnitType == UnitType.Poison)
        //        {
        //            if (HasNoEscape(leaderR.Unit, shortField[whiteModels[i].Pos.x, whiteModels[i].Pos.y].Tile))
        //            {
        //                weight += team == Team.White ? 180 : 0;
        //
        //                if (movesHistory[0].MoveType == MoveType.Attack)
        //                {
        //                    if (movesHistory[0].Target.IsLeader)
        //                        weight += team == Team.White ? 200 : 0;
        //                }
        //                else if (shortField[whiteModels[i].Pos.x, whiteModels[i].Pos.y].Tile != whiteModels[i].Unit.Position)
        //                {
        //
        //                    if (lastId <= 3)
        //                    {
        //                        if (HasNoEscape(leaderR.Unit, whiteModels[i].Unit.Position) == false)
        //                        {
        //                            if (whiteModels[i].IsCharged)
        //                                weight += 10000;
        //                        }
        //                    }
        //                }
        //            }
        //            else if (movesHistory[0].MoveType == MoveType.Attack)
        //            {
        //                weight += team == Team.White ? 10 : 0;
        //            }
        //
        //            weight += 
        //                shortField[whiteModels[i].Pos.x, whiteModels[i].Pos.y].Tile.GetEnemyTiles(whiteModels[i].Unit).Count * 10;
        //
        //
        //            if (item != ItemType.Empty)
        //            {
        //                if (item == ItemType.Range)
        //                {
        //                    if (whiteModels[i].IsCharged)
        //                    {
        //                        if (whiteModels[i].Pos.x == 2 && whiteModels[i].Pos.y == 2)
        //                            weight += team == Team.White ? 100 : 0;
        //                    }
        //                }
        //            }
        //        }
        //
        //        if (item != ItemType.Empty)
        //        {
        //            if (tookItem == Team.White)
        //                weight += team == Team.White ? 8 : 0;
        //        }
        //    }
        //}
        //
        //weight += team == Team.White ?
        //    ((leaderW.Health - leaderAfterDmgW) - (leaderR.Health - leaderAfterDmgR)) * 16 :
        //    ((leaderR.Health - leaderAfterDmgR) - (leaderW.Health - leaderAfterDmgW)) * 16;
        //
        //return weight;
    }


    public int LeaderHealthLose(Team team)
    {
        if (team == Team.White)
        {
            return whiteModels[2].Unit.Health - whiteModels[2].Health;
        }
        else
        {
            return redModels[2].Unit.Health - redModels[2].Health;
        }
    }

    public bool IsLeaderDead(Team team)
    {
        if (team == Team.White)
        {
            return whiteModels[2].IsDead;
        }
        else
        {
            return redModels[2].IsDead;
        }
    }

    public bool HasLeaderMoved(Team team)
    {
        if (team == Team.White)
        {
            return whiteModels[2].Moved;
        }
        else
        {
            return redModels[2].Moved;
        }
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
        var model = GetUnitModel(unit);

        bool wasFree = true;
        if (shortField[x, y].IsOccupied)
            wasFree = false;

        shortField[model.Pos.x, model.Pos.y].Free();
        if (shortField[model.Pos.x, model.Pos.y].IsOccupied)
        {
            var models = unit.Team == Team.White ? redModels : whiteModels;

            for (int i = 0; i < models.Length; i++)
            {
                if (models[i].Pos == model.Pos)
                {
                    models[i].IsPosOwner = true;

                    break;
                }
            }
        }

        shortField[x, y].Occupy(unit);

        int index = GetUnitModelIndex(unit);

        if (usedItem == false)
        {
            if (item == ItemType.Empty)
            {
                item = shortField[x, y].Tile.ItemType;
                tookItem = unit.Team;

                //if (model.IsDead == false)
                    TakeItemReacton(item, unit);
            }
            else if (item == ItemType.Teleport && tookItem == unit.Team)
            {
                item = ItemType.Empty;
                usedItem = true;
            }
        }

        if (unit.Team == Team.White)
        {
            whiteModels[index].SetPos(x, y);
            if (shortField[x, y].IsCharged)
                whiteModels[index].Charge();
            //if (wasFree)
            whiteModels[index].IsPosOwner = wasFree;
        }
        else
        {
            redModels[index].SetPos(x, y);
            if (shortField[x, y].IsCharged)
                redModels[index].Charge();

            redModels[index].IsPosOwner = wasFree;
        }

        shortField[x, y].DropCharge();
    }

    private void TakeItemReacton(ItemType item, UnitModel taker)
    {
        int index = 0;
        switch (item)
        {
            case ItemType.Heal:
                var takerModel = GetUnitModel(taker);
                if (takerModel.IsDead)
                {
                    break;
                }

                var leader = GetUnitModel(taker.Leader);
                index = GetUnitModelIndex(taker.Leader);

                leader.Heal(1);
                leader.Poison(false);

                if (taker.Team == Team.White)
                    whiteModels[index] = leader;
                else
                    redModels[index] = leader;

                usedItem = true;
                break;

            case ItemType.Shield:
                var model = GetUnitModel(taker);
                index = GetUnitModelIndex(taker);

                model.SetShield(true);

                if (taker.Team == Team.White)
                    whiteModels[index] = model;
                else
                    redModels[index] = model;

                usedItem = true;
                break;

            case ItemType.Charger:
                if (taker.Team == Team.White)
                {
                    for (int i = 0; i < whiteModels.Length; i++)
                        whiteModels[i].Charge();
                }
                else
                {
                    for (int i = 0; i < redModels.Length; i++)
                        redModels[i].Charge();
                }

                usedItem = true;
                break;
        }
    }

    public List<ShortMoveModel> GetMoves(Team team, UnitModel mover = null)
    {
        var movers = team == Team.White ? whiteModels : redModels;
        var targets = team == Team.Red ? whiteModels : redModels;
        var moveVariants = new List<ShortMoveModel>();

        for (int i = 0; i < movers.Length; i++)
        {
            if (mover != null)
            {
                if (movers[i].Unit != mover)
                    continue;
            }

            //Moves

            List<TileModel> tiles = shortField[movers[i].Pos.x, movers[i].Pos.y].Tile.MoveTiles;
            if (item == ItemType.Teleport && !usedItem && tookItem == movers[i].Unit.Team)
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

                if (shortField[movers[i].Pos.x, movers[i].Pos.y].Tile.IsPortal)
                {
                    if (tile.IsPortal)
                        continue;

                    if (movers[i].IsCharged == false)
                    {
                        if (tile.IsCharged == false)
                            if (tile.ItemType == ItemType.Empty)
                                continue;
                    }
                }

                if (movers[i].Personality.IsOfCenterAllowed() == false)
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
                        movers[i].Unit, 
                        tile, 
                        shortField[movers[i].Pos.x, movers[i].Pos.y].Tile));
            }

            //Respawn
            if (movers[i].Personality.UnitType == UnitType.Default && movers[i].Unit.IsRespawnable 
                || 
                movers[i].Unit.Health < movers[i].Unit.TrueHealth && movers[i].Respawned == false && movers[i].IsCharged)
            {
                //if (movers[i].Unit.IsRespawnable || movers[i].Unit.Health < movers[i].Unit.TrueHealth)
                //{
                    //if (movers[i].Personality.UnitType == UnitType.Default)
                moveVariants.Add(
                    new ShortMoveModel(
                        MoveType.Respawn,
                        movers[i].Unit,
                        movers[i].Unit.Position,
                        shortField[movers[i].Pos.x, movers[i].Pos.y].Tile));
                //}
            }

            //Attacks & Abilities
            if (movers[i].IsCharged)
            {
                //Attack
                for (int j = 0; j < targets.Length; j++)
                {
                    moveVariants.Add(
                        new ShortMoveModel(
                            MoveType.Attack,
                            movers[i].Unit,
                            shortField[targets[j].Pos.x, targets[j].Pos.y].Tile,
                            shortField[movers[i].Pos.x, movers[i].Pos.y].Tile,
                            targets[j].Unit));
                    
                }

                //Ability
                if (movers[i].Unit.UnitType == UnitType.Shield)
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
                                movers[i].Unit,
                                shortField[movers[j].Pos.x, movers[j].Pos.y].Tile,
                                shortField[movers[i].Pos.x, movers[i].Pos.y].Tile,
                                movers[j].Unit));
                    }
                }
            }
        }

        return moveVariants;
    }

    public List<ShortMoveModel> GetMoves(Team team, MoveFilter filter, bool allowOfCenter)
    {
        var movers = team == Team.White ? whiteModels : redModels;
        var targets = team == Team.Red ? whiteModels : redModels;
        var moveVariants = new List<ShortMoveModel>();

        for (int i = 0; i < movers.Length; i++)
        {
            if (filter.Executor != null)
            {
                if (filter.IsExecutor(movers[i].Unit) == false)
                {
                    continue;
                }
            }

            if (movers[i].Unit.Locked)
                continue;

            //Moves

            List<TileModel> tiles = shortField[movers[i].Pos.x, movers[i].Pos.y].Tile.MoveTiles;
            //if (item == ItemType.Teleport && !usedItem && tookItem == movers[i].Unit.Team)
            //    tiles = tiles[0].FieldTiles;
            if (shortField[movers[i].Pos.x, movers[i].Pos.y].Tile.IsPortal && team == Team.Red)
            {
                var currentTile = shortField[movers[i].Pos.x, movers[i].Pos.y].Tile;

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

                if (shortField[movers[i].Pos.x, movers[i].Pos.y].Tile.IsPortal)
                    if (tile.IsPortal)
                        continue;

                if (movers[i].Personality.IsOfCenterAllowed() == false)
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
                        movers[i].Unit,
                        tile,
                        shortField[movers[i].Pos.x, movers[i].Pos.y].Tile);

                if (filter.Match(moveVariant))
                    moveVariants.Add(moveVariant);
            }

            //Respawn
            if (movers[i].Personality.UnitType == UnitType.Default && movers[i].Unit.IsRespawnable
                ||
                movers[i].Unit.Health < movers[i].Unit.TrueHealth && movers[i].Respawned == false && movers[i].IsCharged)
            {
                //if (movers[i].Unit.IsRespawnable)
                //{
                    //if (movers[i].Personality.UnitType == UnitType.Default)
                var moveVariant = new ShortMoveModel(
                        MoveType.Respawn,
                        movers[i].Unit,
                        movers[i].Unit.Position,
                        shortField[movers[i].Pos.x, movers[i].Pos.y].Tile);

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
                            shortField[movers[i].Pos.x, movers[i].Pos.y].Tile,
                            targets[j].Unit);

                    if (filter.Match(moveVariant))
                        moveVariants.Add(moveVariant);

                }

                //Ability
                if (movers[i].Unit.UnitType == UnitType.Shield)
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
                                movers[i].Unit,
                                shortField[movers[j].Pos.x, movers[j].Pos.y].Tile,
                                shortField[movers[i].Pos.x, movers[i].Pos.y].Tile,
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

        int leaderIndexW = GetUnitModelIndex(whiteModels[0].Unit.Leader);
        int leaderIndexR = GetUnitModelIndex(redModels[0].Unit.Leader);

        //Collision Fight
        for (int i = 0; i < whiteModels.Length; i++)
        {
            for (int j = 0; j < whiteModels.Length; j++)
            {
                if (whiteModels[i].IsDead)
                    continue;
                if (redModels[j].IsDead)
                    continue;

                if (whiteModels[i].Pos.x == redModels[j].Pos.x && whiteModels[i].Pos.y == redModels[j].Pos.y)
                {
                    if (redModels[j].IsCharged)
                    {
                        if (whiteModels[i].HasShield)
                            whiteModels[i].SetShield(false);
                        else
                            whiteModels[i].TakeDamage(redModels[j].Damage);
                    }
                    if (whiteModels[i].IsCharged)
                    {
                        if (redModels[j].HasShield)
                            redModels[j].SetShield(false);
                        else
                            redModels[j].TakeDamage(whiteModels[i].Damage);
                    }

                    var models = redModels[j].IsPosOwner ? whiteModels : redModels;
                    int index = redModels[j].IsPosOwner ? i : j;
                    var pos = GetJumpPosClockwise(models[index].Pos);
                    var posFrom = models[index].Pos;
                    if (shortField[pos.x, pos.y].Tile.IsSuddenDeathTarget && 
                        shortField[posFrom.x, posFrom.y].Tile.IsSuddenDeathTarget == false)
                    {
                        if (models[index].HasShield)
                            models[index].SetShield(false);
                        else
                            models[index].TakeDamage(1);
                    }
                }
            }
        }

        //SuddenDeath Damage & afterdamage
        for (int i = 0; i < whiteModels.Length; i++)
        {
            whiteModels[i].AfterDamage();
            redModels[i].AfterDamage();

            if (shortField[whiteModels[i].Pos.x, whiteModels[i].Pos.y].Tile.IsSuddenDeathTarget)
            {
                if (whiteModels[i].HasShield)
                    redModels[i].SetShield(false);
                else
                    whiteModels[i].TakeDamage(1);
            }
            if (shortField[redModels[i].Pos.x, redModels[i].Pos.y].Tile.IsSuddenDeathTarget)
            {
                if (redModels[i].HasShield)
                    redModels[i].SetShield(false);
                else
                    redModels[i].TakeDamage(1);
            }

            if (whiteModels[i].Personality.UnitType == UnitType.Venomancer)
            {
                if (whiteModels[i].IsCharged || whiteModels[i].IsDead)
                {
                    for (int j = 0; j < redModels.Length; j++)
                    {
                        if (Mathf.Abs(redModels[j].Pos.x - whiteModels[i].Pos.x) <= 1 &&
                            Mathf.Abs(redModels[j].Pos.y - whiteModels[i].Pos.y) <= 1 &&
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
                if (redModels[i].IsCharged || redModels[i].IsDead)
                {
                    for (int j = 0; j < redModels.Length; j++)
                    {
                        if (Mathf.Abs(whiteModels[j].Pos.x - redModels[i].Pos.x) <= 1 &&
                            Mathf.Abs(whiteModels[j].Pos.y - redModels[i].Pos.y) <= 1 &&
                            whiteModels[j].Pos != redModels[i].Pos)
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
            if (whiteModels[i].Unit != whiteModels[leaderIndexW].Unit)
            {
                if (whiteModels[i].IsDead)
                {
                    whiteModels[leaderIndexW].LeaderRespawnDamage(1);
                }
            }
            if (redModels[i].Unit != redModels[leaderIndexR].Unit)
            {
                if (redModels[i].IsDead)
                {
                    redModels[leaderIndexR].LeaderRespawnDamage(1);
                }
            }
        }
    }

    private void UseAbility(UnitModel user, UnitModel targetUnit)
    {
        var model = GetUnitModel(user);
        var target = GetUnitModel(targetUnit);

        if (item != ItemType.Infinity || usedItem == true) //Causes shield not dropping charge
        {
            shortField[model.Pos.x, model.Pos.y].DropCharge();
            model.DropCharge();
        }
        else if (tookItem == user.Team)
        {
            usedItem = true;
        }
        else
        {
            shortField[model.Pos.x, model.Pos.y].DropCharge();
            model.DropCharge();
        }
        //shortField[model.Pos.x, model.Pos.y].DropCharge();
        //model.DropCharge();

        int index = GetUnitModelIndex(user);
        int targetIndex = GetUnitModelIndex(targetUnit);

        if (model.IsDead)
        {
            if (model.Unit.Team == Team.White)
                whiteModels[index] = model;
            else
                redModels[index] = model;

            return;
        }

        if (model.Unit.UnitType == UnitType.Shield)
        {
            target.SetShield(true);
        }

        if (target.Unit.Team == Team.White)
        {
            if (user == targetUnit)
            {
                target.DropCharge();
                whiteModels[targetIndex] = target;
            }
            else
            {
                whiteModels[index] = model;
                whiteModels[targetIndex] = target;
            }
        }
        else
        {
            if (user == targetUnit) 
            {
                target.DropCharge();
                redModels[targetIndex] = target;
            }
            else
            {
                redModels[index] = model;
                redModels[targetIndex] = target;
            }
        }
    }

    private void Respawn(UnitModel respawner)
    {
        var model = GetUnitModel(respawner);

        shortField[model.Pos.x, model.Pos.y].DropCharge();
        //model.DropCharge();

        int index = GetUnitModelIndex(respawner);

        if (model.Personality.UnitType == UnitType.Default && model.Unit.IsRespawnable)
        {
            if (respawner.Team == Team.White)
            {
                whiteModels[index] = model.IsDead ? new ShortUnitModel(model, true) : new ShortUnitModel(model);
            }
            else
            {
                redModels[index] = model.IsDead ? new ShortUnitModel(model, true) : new ShortUnitModel(model);
            }
        }
        else
        {
            if (model.IsDead == false)
            {
                model.Heal(1);
                model.Poison(false);
            }

            model.Respawned = true;

            shortField[model.Pos.x, model.Pos.y].DropCharge();
            model.DropCharge();

            if (respawner.Team == Team.White)
            {
                whiteModels[index] = model.IsDead ? new ShortUnitModel(model, true) : model;
            }
            else
            {
                redModels[index] = model.IsDead ? new ShortUnitModel(model, true) : model;
            }
        }
    }

    private void Attack(UnitModel attacker, UnitModel targetUnit)
    {
        var model = GetUnitModel(attacker);
        var target = GetUnitModel(targetUnit);

        shortField[model.Pos.x, model.Pos.y].DropCharge();
        model.DropCharge();

        int index = GetUnitModelIndex(attacker);
        int targetIndex = GetUnitModelIndex(target.Unit);


        if (model.IsDead)
        {
            if (model.Unit.Team == Team.White)
                whiteModels[index] = model;
            else
                redModels[index] = model;

            return;
        }
        else if (
            shortField[model.Pos.x, model.Pos.y].Tile.MeleeTiles.Contains(
                        shortField[target.Pos.x, target.Pos.y].Tile) == false &&
            //shortField[model.Pos.x, model.Pos.y].Tile.RangeTiles.Contains(
            //            shortField[target.Pos.x, target.Pos.y].Tile) == false &&
            shortField[model.Pos.x, model.Pos.y].Tile !=
                        shortField[target.Pos.x, target.Pos.y].Tile)
        {
            if (shortField[model.Pos.x, model.Pos.y].Tile.RangeTiles.Contains(
                        shortField[target.Pos.x, target.Pos.y].Tile) == false)
            {
                if (model.Unit.Team == Team.White)
                    whiteModels[index] = model;
                else
                    redModels[index] = model;

                return;
            }
            else if (
                model.IsRanger == false &&
                (item == ItemType.Range && !usedItem && tookItem == attacker.Team) == false)
            {
                if (model.Unit.Team == Team.White)
                    whiteModels[index] = model;
                else
                    redModels[index] = model;

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
            for (int i = 0; i < whiteModels.Length; i++)
            {
                if (i == targetIndex)
                    continue;

                if (target.Unit.Team == Team.White)
                {
                    if (whiteModels[i].HasShield)
                        whiteModels[i].SetShield(false);
                    else
                        whiteModels[i].TakeDamage(1);
                }
                else
                {
                    if (redModels[i].HasShield)
                        redModels[i].SetShield(false);
                    else
                        redModels[i].TakeDamage(1);
                }
            }
            usedItem = true;
        }

        if (item == ItemType.Range && !usedItem && tookItem == attacker.Team)
            usedItem = true;

        if (target.Unit.Team == Team.White)
        {
            whiteModels[targetIndex] = target;
            redModels[index] = model;
        }
        else
        {
            redModels[targetIndex] = target;
            whiteModels[index] = model;
        }
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
