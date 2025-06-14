using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotActionAttack : BotAction
{
    public BotActionAttack(int order) : base(order)
    {
        
    }

    public override void CalcWeight(BotTileModel tileTo, BotTileModel tileFrom, 
        BotFieldModel fieldModel, BotStrategyModel strategyModel, 
        TacticMove tacticMove = null)
    {
        SetTiles(tileTo, tileFrom);

        action = tileTo.Tile.HasEnemy(tileFrom.Unit) ? MoveType.Attack : MoveType.Move;

        if (tileFrom.UnitModel.IsCharged == false)
        {
            weight -= 500;
            return;
        }

        BotTacticMove moveType = BotTacticMove.Any;
        if (tacticMove != null)
        {
            moveType = tacticMove.Move;
            weight += tacticMove.GetMoveWeight(tileFrom, tileTo, order);
        }

        //TODO romove or accept 1 move atk only test block 
        if (order <= 1)
        {
            weight += tileTo.Unit.IsLeader ? 250 : 20;
        }
        else
        {
            weight -= 150;
        }

        //weight += tileFrom.Unit.Health <= 3 ? -2 : 1;
        weight += IsDeathThreat(tileFrom, tileFrom.Unit) ? -2 : 1;

        weight += strategyModel.AttackWeightBonus;

        if (strategyModel.Plan == GamePlan.Wild)
            weight += 10;

        weight += tileTo.IsAttacked ? -3 : 1;
        weight += order == 1 ? 9 : 0;
        //weight += IsPosibleKill() ? 4 : 0;
        weight += IsSecuredKill() ? 150 : 0;

        //if (tileTo.Unit)
        if (order == 1)
        {
            if (strategyModel.AwarenessModel.HasToDieNow(tileTo))
                weight += 200;
            else
                weight += tileTo.Unit.Poison ? -180 : 0;
        }
        else
        {
            weight += tileTo.Unit.Poison ? -180 : 0;
        }

        if (IsPosibleKill())
        {
            weight += 4;
            if (tileTo.Unit.IsLeader)
            {
                if (tileTo.Unit.Health <= 1)
                    weight += 20;
                else if (tileTo.Unit.Health <= 2)
                    weight += 10;
            }
        }

        if (order == 1)
        {
            weight += tileTo.Unit.IsLeader ? 6 : 0;
        }
        else
        {
            weight += tileFrom.Unit.Health <= 1 ? -50 : 0;

            if (order == RevealLocalUtil.Instance.Order)
                weight += -100;
        }

        if (order == 3)
        {
            if (tileTo.Unit.IsLeader)
                weight += 4;
        }

        weight += GetTacticWeight(moveType);

        weight += tileTo.Unit.IsLeader ? 4 : 0;

        weight += CanKillLeader() ? 300 : 0;

        if (tileFrom.UnitModel != null)
        {
            weight += tileFrom.UnitModel.HasMoved ? 0 : 5;

            if (tileFrom.Tile.MeleeTiles.Contains(tileTo.Tile))
            {
                weight += tileFrom.UnitModel.MeleeDamage > 1 ? 8 : 0;
                if (tileFrom.UnitModel.MeleeDamage < tileTo.Unit.Health)
                    if (tileTo.Unit.IsLeader == false)
                        weight -= 4;
            }
            else if (tileFrom.Tile.RangeTiles.Contains(tileTo.Tile))
            {
                weight += tileFrom.UnitModel.RangeDamage > 1 ? 8 : 0;
                if (tileFrom.UnitModel.RangeDamage < tileTo.Unit.Health)
                    if (tileTo.Unit.IsLeader == false)
                        weight -= 4;
            }

            weight += tileFrom.UnitModel.UnitPersonality.GetAttackWeight(tileFrom ,tileTo, order);
            if (tileFrom.UnitModel.IsRanger)
                weight += 5;
        }
    }

    private int GetTacticWeight(BotTacticMove tacticMove)
    {
        int result = 0;

        switch (tacticMove)
        {
            case BotTacticMove.Attack:
                return 50;

            case BotTacticMove.FocuseAttack:
                result = GetAttackersCount() * 50;
                if (tileTo.Unit.IsLeader)
                {
                    if (result >= 100)
                    {
                        weight += 20;
                        if (tileTo.Unit.Health <= 2)
                            weight += 50;
                    }
                }
                result += tileTo.IsAttacked ? 100 : 0;
                return result;

            case BotTacticMove.ChaseLeader:
                if (tileTo.Unit.IsLeader)
                    result = 80;
                return result;

            case BotTacticMove.ChaseDancer:
                result += CanAttackDancer() ? 70 : 0;
                result += tileTo.Unit.IsCharged ? 50 : 0;
                return result;

            case BotTacticMove.ChargePosition:
                if (CanPositionAttack())
                {
                    result += 100;
                    if (tileTo.Unit.IsLeader)
                        result += 30;
                }
                return result;

            case BotTacticMove.PoisonLeader:
                result += CanPoisonLeader()? 100 : 0;
                return result;

        }

        return result;
    }

    private int GetAttackersCount()
    {
        int attackersCount = 0;
        foreach (Tile pos in tileTo.Tile.GetEnemyTiles(tileFrom.Unit))
        {
            if (pos.unitInTile.IsCharged)
                attackersCount++;
        }

        return attackersCount;
    }

    private bool CanPositionAttack()
    {
        if (tileFrom.UnitModel.MeleeDamage > 1)
        {
            if (tileFrom.Tile.MeleeTiles.Contains(tileTo.Tile))
                return true;
        }

        if (tileFrom.UnitModel.RangeDamage > 1)
        {
            if (tileFrom.Tile.RangeTiles.Contains(tileTo.Tile))
                return true;
        }

        return false;
    }

    private bool CanPoisonLeader()
    {
        if (tileFrom.Unit.UnitType != UnitType.Poison)
            return false;

        if (tileTo.Unit.IsLeader)
            return true;

        return false;
    }

    private bool CanKillLeader()
    {
        if (tileTo.Unit.IsLeader)
        {
            if (tileTo.Unit.Health <= tileFrom.UnitModel.MeleeDamage)
                return true;
            if (tileTo.Unit.Health <= tileFrom.UnitModel.RangeDamage)
                return true;
        }

        return false;
    }

    private bool IsPosibleKill()
    {
        return tileTo.Unit.Health <= 1;
    }

    private bool IsSecuredKill()
    {
        if (order > 1)
            return false;

        if (tileFrom.UnitModel != null)
        {
            if (tileFrom.Tile.MeleeTiles.Contains(tileTo.Tile))
            {
                return tileFrom.UnitModel.MeleeDamage >= tileTo.Unit.Health;
            }
            else if (tileFrom.Tile.RangeTiles.Contains(tileTo.Tile))
            {
                return tileFrom.UnitModel.RangeDamage >= tileTo.Unit.Health;
            }
        }
        else
        {
            return 1 >= tileTo.Unit.Health;
        }

        return false;
    }

    private bool CanAttackDancer()
    {
        if (tileFrom.UnitModel.IsCharged == false)
            return false;

        if (tileTo.Unit.IsCharged == false)
            return false;

        if (tileFrom.UnitModel.HasMoved)
        {
            return true;
        }

        return false;
    }

    public override void PushAction(ActionManager actionManager, BotStrategyModel strategyModel = null)
    {
        //actionManager.CreateAction(tileFrom.Unit, tileTo.Unit);
        actionManager.CreateActionHidden(tileFrom.Unit, tileTo.Tile.unitInTile, tileFrom.Tile, order);
        tileTo.Attack();
        tileFrom.DeactivateAttack();

        foreach (var model in strategyModel.UnitModels)
            tileFrom.UnitModel.ResetDamage();

        foreach (var model in strategyModel.UnitModels)
            tileFrom.UnitModel.ResetRange();

        tileFrom.UnitModel.DropCharge();

        if (strategyModel != null)
            strategyModel.Attack();
    }
}
