using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotActionMove : BotAction
{
    public BotActionMove(int order): base(order)
    {

    }

    public override void CalcWeight(
        BotTileModel tileTo, BotTileModel tileFrom,
        BotFieldModel fieldModel, BotStrategyModel strategyModel, 
        TacticMove tacticMove = null)
    {
        SetTiles(tileTo, tileFrom);

        action = tileTo.Tile.HasEnemy(tileFrom.Unit) ? MoveType.Attack : MoveType.Move;

        BotTacticMove moveType = BotTacticMove.Any;
        if (tacticMove != null)
        {
            moveType = tacticMove.Move;
            weight += tacticMove.GetMoveWeight(tileFrom, tileTo, order);
        }

        weight += GetSuddenDeathWeight(strategyModel);

        Debug.Log($"_Tactic : {moveType}");

        if (strategyModel.AwarenessModel != null)
        {
            int aw_diff = 
                strategyModel.AwarenessModel.GetDanger(tileFrom.Tile)
                - strategyModel.AwarenessModel.GetDanger(tileTo.Tile);

            if (aw_diff >= 32) //Value is too high bot behaves like a pussy
                aw_diff = 8;
            if (aw_diff >= 16)
                aw_diff = 4;
            else
                aw_diff = 2;

            if (strategyModel.Plan == GamePlan.Attack)
                aw_diff /= 2;

            if (tileFrom.Unit.IsLeader == false)
                aw_diff /= 2;

            if (tileFrom.Unit.Shield == false)
            {
                weight += aw_diff;

                aw_diff =
                        strategyModel.AwarenessModel.GetClosestDangerWeight(tileFrom) -
                        strategyModel.AwarenessModel.GetClosestDangerWeight(tileTo, tileFrom.Unit);

                if (strategyModel.Leader.Shield)
                    aw_diff /= 8;

                if (order <= 1)
                    weight += aw_diff;
                else
                    weight += aw_diff / 8;
            }
        }

        weight += GetItemWeight(tileFrom.Unit, strategyModel, fieldModel, moveType);

        weight += tileFrom.Tile.HasEnemyInRange(tileFrom.Unit) ? strategyModel.SafeBonus : 0;

        if (order > 1)
            weight += tileFrom.Unit.Health <= 1 ? -50 : 0;

        if (tileTo.Tile.HasEnemyInRange(tileFrom.Unit))
        {
            weight += tileFrom.HasNotAttackedInRange(tileFrom.Unit, fieldModel) ? -2 : 0;
            weight += tileTo.HasNotAttackedInRange(tileFrom.Unit, fieldModel) ? strategyModel.MoveToUnAttackedWeightBonus : 0;
        }

        if (tileFrom.Unit.IsLeader)
        {
            weight += tileTo.Tile.HasEnemyInRange(tileFrom.Unit) ? -12 : 4;
            weight += tileTo.Tile.IsPortal ? 4 : 0;
        }

        if (tileFrom.UnitModel != null)
        {
            weight += tileFrom.UnitModel.HasMoved ? -1 : 5;

            if (tileFrom.Unit.IsLeader)
            {
                weight += tileFrom.UnitModel.HasMoved ? -1 : 2;
                if (order == 3)
                    weight += tileFrom.UnitModel.HasMoved ? 0 : 50;
            }

            if (tileFrom.UnitModel.IsCharged == false)
            {
                weight += tileTo.Tile.IsCharged ? 3 : -5;

            }
            else
            {
                weight += tileFrom.Tile.HasEnemyInRange(tileFrom.Unit) ? -3 : 0;
                if (order >= 3)
                    weight += tileFrom.Tile.HasEnemyInRange(tileFrom.Unit) ? -5 : 0;
            }

            if (IsMeleeProfitable())
                weight += 8;
            else if (IsRangeProfitable())
                weight += 8;

            weight += tileFrom.UnitModel.UnitPersonality.GetMoveWeight(tileTo, tileFrom, order);

            if (tileFrom.Unit.IsLeader == false)
                if (tileFrom.Unit.UnitType == UnitType.Default)
                    weight += tileFrom.Unit.unitManager.GetSpawner(tileFrom.Unit.Name).HasTrueForm ? -16 : 0;
        }

        if (order == 3 && RevealLocalUtil.Instance.Order == order)
            if (tileFrom.Unit.IsLeader)
                weight -= tileTo.Tile.GetEnemyTiles(tileFrom.Unit, true).Count * 8;  


        weight += IsDeathThreat(tileFrom, tileFrom.Unit) ? 5 : 0;
        weight += IsDeathThreat(tileTo, tileFrom.Unit) ? -4 : 1;

        //weight += IsUnderAttack(tileFrom) ? 2 : 0;
        //weight += IsUnderAttack(tileTo) ? -2 : 0;

        weight += IsRevealedAndIntoAttack() ? -100 : 0;

        weight += SaveLeaderWeight(tileFrom.Unit);

        weight += tileTo.GetPosWeightValue();

        weight += GetMoveToRespawnWeight();

        weight += GetNoSleepWeight(fieldModel);

        if (moveType == BotTacticMove.ChaseLeader)
            weight += CanMoveToLeader(true) ? 40 : 0;
        else if (moveType == BotTacticMove.MoveToAttack)
            weight += tileTo.Tile.HasEnemyInRange(tileFrom.UnitModel.Unit) ? 50 : 0;
        else if (moveType == BotTacticMove.ChaseDancer)
            weight += CanMoveToDancer() ? 50 : 0;
        else if (moveType == BotTacticMove.MoveToCharge)
            weight += CanMoveToLeader() ? 100 : 0;
        else if (moveType == BotTacticMove.DistantLeader)
            weight += CanDistantLeader() ? 50 : 0;
        else if (moveType == BotTacticMove.ChargePosition)
            weight += CanMoveToChargePosition() ? 50 : 0;
        else if (moveType == BotTacticMove.PoisonLeader)
            weight += CanMoveToPoisonLeader() ? 25 : 0;
        else if (moveType == BotTacticMove.FakePosition)
            weight += CanMoveToChargePosition() ? 50 : 0;

        //DistantLeader second leader move
        if (moveType == BotTacticMove.DistantLeader)
        {
            weight += IsDistantLeader() ? 30 : 0;
            if (tileFrom.Tile.MeleeTiles.Contains(tileTo.Tile) == false)
                weight += 30;
            if (tileFrom.Tile.RangeTiles.Contains(tileTo.Tile) == false)
                weight += 30;
        }
    }

    public override void PushAction(ActionManager actionManager, BotStrategyModel strategyModel = null)
    {
        actionManager.CreateActionHidden(tileFrom.Unit, tileFrom.Tile, tileTo.Tile, order);

        if (tileFrom.UnitModel != null)
        {
            tileFrom.UnitModel.Move();
        }

        tileFrom.MoveToTile(tileTo);

        if (strategyModel != null)
            strategyModel.Move();
    }

    protected override int GetSuddenDeathWeight(BotStrategyModel strategyModel)
    {
        int result = 0;
        if (tileFrom.Tile.IsSuddenDeathTarget)
            result += tileFrom.Unit.IsLeader ? 16 : 8;
        if (tileTo.Tile.IsSuddenDeathTarget)
        {
            result += tileFrom.Unit.IsLeader ? -48 : -16;
            if (tileFrom.UnitModel.HasMoved)
                result += order == 3 ? -24 : 0;
        }


        if (tileFrom.Unit.Health <= 1)
        {
            result *= 4;
            if (strategyModel.Leader.Health <= 1)
                result *= 4;
        }

        if (strategyModel.Leader.Health <= 1) 
            result *= 2;

        return result;
    }

    private int GetNoSleepWeight(BotFieldModel fieldModel)
    {
        var center = fieldModel.Tiles.Find(x => x.Xcoord == 0 && x.Ycoord == 0);
        if (center == null)
            return 0;

        int result = 0;

        return result;
    }

    private bool IsDistantLeader()
    {
        if (tileFrom.Unit.IsLeader == false)
            return false;

        if (order == 2)
            return true;

        return false;
    }

    private bool IsRevealedAndIntoAttack()
    {
        if (order != RevealLocalUtil.Instance.Order)
            return false;

        foreach (var enemyTile in tileTo.Tile.GetEnemyTiles(tileFrom.Unit))
        {
            if (enemyTile.unitInTile.IsLeader)
                continue;

            //if (tileFrom.Unit.IsLeader)
            //    if (enemyTile.unitInTile.IsCharged)
            //        return true;

            if (enemyTile.unitInTile.DamageMelee == 1)
                if (tileTo.Tile.MeleeTiles.Contains(enemyTile))
                    continue;

            if (enemyTile.unitInTile.DamageRange == 1)
                if (tileTo.Tile.RangeTiles.Contains(enemyTile))
                    continue;

            if (enemyTile.unitInTile.IsCharged)
                return true;
        }

        return false;
    }

    private bool IsMeleeProfitable()
    {
        if (tileTo.Tile.HasEnemyInRange(tileFrom.UnitModel.Unit, false, true) == false)
            return false;

        if (tileFrom.UnitModel.MeleeDamage <= 1)
            return false;

        if (tileFrom.UnitModel.MeleeDamage < tileFrom.UnitModel.RangeDamage)
            return false;

        return true;
    }

    private bool IsRangeProfitable()
    {
        if (tileTo.Tile.HasEnemyInRange(tileFrom.UnitModel.Unit, true, false) == false)
            return false;

        if (tileFrom.UnitModel.RangeDamage <= 1)
            return false;

        if (tileFrom.UnitModel.MeleeDamage > tileFrom.UnitModel.RangeDamage)
            return false;

        return true;
    }

    private bool IsUnderAttack(BotTileModel tileModel)
    {
        return tileModel.Tile.HasEnemyInRange(tileFrom.Unit);
    }

    private bool CanDistantLeader()
    {
        if (order == 1)
            if (tileFrom.Unit.IsLeader == false)
                return false;
        if (tileTo.Tile.HasEnemyInRange(tileFrom.Unit))
            return false;
        if (tileFrom.UnitModel.HasMoved)
            return false;

        return true;
    }

    private bool CanMoveToLeader(bool chargeMatters = false)
    {
        if (chargeMatters)
            if (tileFrom.UnitModel.IsCharged == false)
                return false;

        if (order == RevealLocalUtil.Instance.Order && order > 1)
            return false;

        foreach (var tile in tileTo.Tile.GetEnemyTiles(tileFrom.Unit))
        {
            if (tile.unitInTile.IsLeader) 
            { 
                return true; 
            }
        }

        return false;
    }

    private bool CanMoveToDancer()
    {
        if (tileFrom.UnitModel.IsCharged == false)
            return false;

        if (order == RevealLocalUtil.Instance.Order)
            return false;

        foreach (var tile in tileTo.Tile.GetEnemyTiles(tileFrom.Unit))
        {
            if (tile.unitInTile.IsCharged == false)
                continue;

            if (tile.HasEnemyInRange(tileFrom.Unit) == false)
            {
                return true;
            }
        }

        return false;
    }
    
    private bool CanMoveToChargePosition()
    {
        if (tileFrom.UnitModel.MeleeDamage > 1)
        {
            if (tileTo.Tile.HasEnemyInRange(tileFrom.Unit, false, true))
            {
                return true;
            }
        }
        else if (tileFrom.UnitModel.RangeDamage > 1)
        {
            if (tileTo.Tile.HasEnemyInRange(tileFrom.Unit, true, false))
            {
                return true;
            }
        }

        return false;
    }

    private bool CanMoveToPoisonLeader()
    {
        if (tileFrom.Unit.UnitType != UnitType.Poison)
            return false;

        if (tileFrom.UnitModel.IsCharged == false)
            if (tileTo.Tile.content != null)
                return false;

        foreach (var enemyTile in tileTo.Tile.GetEnemyTiles(tileFrom.Unit))
        {
            if (enemyTile.unitInTile.IsLeader)
                return true;
        }

        return false;
    }

    private int SaveLeaderWeight(Unit unit)
    {
        int result = 0;

        if (unit.IsLeader)
        {
            if (unit.Health < 3) {
                result += IsUnderAttack(tileFrom) ? 7 : 0;
                result += IsDeathThreat(tileFrom, unit) ? 10 : 0;

                result += IsDeathThreat(tileTo, unit) ? -6 : 2;
            }
        }

        return result;
    }

    private int GetMoveToRespawnWeight()
    {
        if (tileFrom.UnitModel.Unit.IsLeader)
            return 0;
        if (tileFrom.UnitModel.Unit.UnitType != UnitType.Default)
            return 0;
        if (tileFrom.UnitModel.TriedToRespawn)
            return 0;
        if (tileFrom.Tile.IsCharged == true)
            return 0;
        if (tileTo.Tile.IsCharged == false)
            return 0;

        if (tileFrom.UnitModel.HasMoved)
            return 4;

        return 12;
    }

    private int GetItemWeight(Unit unit, BotStrategyModel botStrategy, BotFieldModel fieldModel, BotTacticMove tacticMove = BotTacticMove.Any)
    {
        int result = 0;

        int defaultMultiplier = tacticMove == BotTacticMove.CollectBerry ? 50 : 5;

        switch (tileTo.Tile.GetItemType())
        {
            case ItemType.Melee: 
                result = 5;
                if (tacticMove == BotTacticMove.CollectBuff)
                    result += 50;
                if (tacticMove == BotTacticMove.OverTake)
                    result += 50;
                result += botStrategy.IsAbleToAttack ? 0 : -4; 
                result += order < 3 ? 0 : -5; 
                break;
            case ItemType.Rage:
                result = 5;
                if (tacticMove == BotTacticMove.CollectBuff)
                    result += 50;
                if (tacticMove == BotTacticMove.OverTake)
                    result += 50;
                result += botStrategy.IsAbleToAttack ? 0 : -4;
                result += order < 3 ? 0 : -5;
                break;
            case ItemType.Range:
                result = 18;
                if (tacticMove == BotTacticMove.CollectBuff)
                    result += 50;
                if (tacticMove == BotTacticMove.OverTake)
                    result += 50;
                result += botStrategy.IsAbleToAttack ? 0 : -4;
                result += order < 3 ? 0 : -5;
                break;
            case ItemType.Dance:
                result = 2;
                break;
            case ItemType.Freeze:
                result = 5;
                break;
            case ItemType.Shield:
                result = 6;
                if (tileFrom.Unit.IsLeader)
                {
                    result += 4;
                    if (order == 1)
                    {
                        if (tacticMove == BotTacticMove.OverTake)
                            result += 50;
                    }
                    if (tacticMove == BotTacticMove.DistantLeader)
                    {
                        result += 50;
                    }
                }
                if (tileFrom.Unit.Shield == true)
                    result -= 55;
                break;
            case ItemType.Heal:
                result = 16;
                if (tacticMove == BotTacticMove.CollectHealth)
                    result += unit.IsLeader ? 260 : 260;
                //if (unit.IsLeader)
                //    result += unit.Health < unit.totem.MaxHealth ? 16 : 12;
                //else
                if (unit.totem.Leader.Health < unit.totem.MaxHealth)
                    result += 18;
                else
                    result += 12;

                if (unit.totem.Leader.Health <= 1)
                    result += 40;

                break;
            case ItemType.TeamHeal:
                result = 3 * 2;//GetTeamHealMultiplier(TileNature.Earth, unit);
                if (tacticMove == BotTacticMove.CollectHealth)
                    result += 50;
                if (unit.totem.Leader.Health < unit.totem.MaxHealth)
                    result += unit.Health < unit.totem.UnitMaxHealth ? 16 : 12;

                if (unit.totem.Leader.Health <= 1)
                    result += 40;

                result += unit.Health < unit.totem.UnitMaxHealth ? 4 : 0;

                break;

            case ItemType.FireDamage:
                if (tileTo.WasOccupied)
                    break;
                //result = defaultMultiplier * GetESMultiplier(TileNature.Fire, unit) - 1;
                result = 16;
                break;

            case ItemType.Charger:
                result = 20;
                break;

            case ItemType.IceDamage:
                if (tileTo.WasOccupied)
                    break;

                result = 1;
                foreach (Unit ally in unit.Allies)
                {
                    if (ally.IsCharged == false)
                        result++;
                }
                foreach (Unit enemy in unit.unitManager.GetEnemyLeader(unit).Allies)
                {
                    if (enemy.IsCharged == false)
                        result++;
                }

                result = defaultMultiplier * result - 1;
                
                break;
            case ItemType.AirDamage:
                if (tileTo.WasOccupied)
                    break;

                result = 1;
                foreach (Unit ally in unit.Allies)
                {
                    if (ally.IsCharged)
                        result++;
                }
                foreach (Unit enemy in unit.unitManager.GetEnemyLeader(unit).Allies)
                {
                    if (enemy.IsCharged)
                        result++;
                }

                result = defaultMultiplier * result - 1;

                break;
            case ItemType.EarthDamage:
                if (tileTo.WasOccupied)
                    break;
                
                result = defaultMultiplier * GetESMultiplier(unit, fieldModel) - 1;

                foreach (Tile enemyTile in tileTo.Tile.GetEnemyTiles(tileFrom.Unit))
                {
                    if (enemyTile.unitInTile.IsLeader)
                    {
                        result += 20;
                        if (enemyTile.unitInTile.Health <= 1)
                            result += 30;
                    }
                    else
                    {
                        if (enemyTile.unitInTile.Health <= 1)
                        {
                            result += 20;
                            if (botStrategy.PlayerLeader.Health <= 1)
                            {
                                result += 120;
                            }
                        }
                    }
                }
                
                break;
        }

        Debug.Log($"Unit: {unit.Name}, tactic: {tacticMove}, Item weight: {result}");

        return result;
    }

    private int GetTeamHealMultiplier(TileNature nature, Unit unit)
    {
        int result = 0;
        var allies = unit.unitManager.GetEnemysByNature(nature, unit);

        foreach (var u in allies)
        {
            result++;
        }

        return result;
    }

    private int GetESMultiplier(Unit unit, BotFieldModel fieldModel)
    {
        int result = 1;

        var enemyTiles = tileTo.Tile.GetEnemyTiles(unit, false);
        foreach (var tile in enemyTiles)
        {
            result++;
        }
        foreach (var tile in tileTo.Tile.MeleeTiles)
        {
            BotTileModel tileModel = fieldModel.GetTileModel(tile);

            if (tileModel.Unit == null)
                continue;
            if (tileModel.Unit.Team != unit.Team)
                continue;

            if (tileModel.Unit.IsLeader)
                result++;

            result++;
        }
        

        return result;
    }


    //Old method not actual for new rules
    private int GetESMultiplier(TileNature nature, Unit unit)
    {
        int result = 1;

        var enemies = unit.unitManager.GetEnemysByNature(nature, unit);
        var allies = unit.unitManager.GetAlliesByNature(nature, unit);

        foreach (var enemy in enemies)
        {
            result++;
        }

        if (order == 1)
        {
            foreach (var ally in allies)
            {
                result++;
            }
        }

        return result;
    }
}
