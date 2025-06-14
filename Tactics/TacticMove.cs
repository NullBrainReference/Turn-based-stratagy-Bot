using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TacticMove
{
    [SerializeField] protected BotTacticMove move;
    [SerializeField] protected bool isPosible;
    [SerializeField] protected int weight;

    public BotTacticMove Move => move;
    public bool IsPosible => isPosible;
    public int Weight => weight;

    public TacticMove(bool isPosible = false)
    {
        move = BotTacticMove.Any;
        this.isPosible = isPosible;
        weight = 0;
    }

    public void Init(BotStrategyModel strategyModel, int order)
    {
        PosibilityCheck(strategyModel, order);
        if (isPosible)
            CalcWeight(strategyModel);
        else
            weight = -500;
    }

    public virtual void PosibilityCheck(BotStrategyModel strategyModel, int order)
    {
        PosibilityCheck(strategyModel);
    }

    public virtual void PosibilityCheck(BotStrategyModel strategyModel)
    {

    }

    public virtual void CalcWeight(BotStrategyModel strategyModel)
    {
        weight = 10;
    }

    public virtual int GetMoveWeight(BotTileModel tileFrom, BotTileModel tileTo, int order)
    {
        return 0;
    }

    public virtual UnitType GetMover()
    {
        return UnitType.Default;
    }

    public virtual bool Match(ShortMoveModel shortMove)
    {
        bool match = shortMove.Unit.UnitType == GetMover();

        //if (move == BotTacticMove.Any)
        //    return true;

        if (GetMover() != UnitType.Default)
        {
            return match;
        }

        return true;
    }

    public static TacticMove GetTacticMove(BotTacticMove botTacticMove)
    {
        switch (botTacticMove)
        {
            case BotTacticMove.ChargePosition:
                return new PositionTacticMove();
            case BotTacticMove.ChaseLeader:
                return new ChaseLeaderTacticMove();
            case BotTacticMove.PoisonLeader:
                return new PoisonLeaderTacticMove();
            case BotTacticMove.ChaseDancer:
                return new ChaseDancerTacticMove();
            case BotTacticMove.Respawn:
                return new RespawnTacticMove();
            case BotTacticMove.Collect:
                return new CollectTacticMove();
            case BotTacticMove.CollectBuff:
                return new CollectBuffTacticMove();
            case BotTacticMove.CollectBerry:
                return new CollectBerryTacticMove();
            case BotTacticMove.CollectHealth:
                return new CollectHealthTacticMove();
            case BotTacticMove.OverTake:
                return new OverTakeTacticMove();
            case BotTacticMove.MoveToCharge:
                return new ChargeTacticMove();
            case BotTacticMove.FocuseAttack:
                return new FocusAttackTacticMove();
            case BotTacticMove.DistantLeader:
                return new DistantLeaderTacticMove();
            case BotTacticMove.Attack:
                return new AttackTacticMove();
            case BotTacticMove.MoveToAttack:
                return new MoveToAttackTacticMove();
            case BotTacticMove.FakePosition:
                return new FakePositionTacticMove();
            case BotTacticMove.Rerout:
                return new ReroutTacticMove();

            case BotTacticMove.LeaderTake:
                return new LeaderTakeTacticMove();
            case BotTacticMove.LeaderAfterTake:
                return new LeaderAfterTakeTacticMove();
            case BotTacticMove.LeaderAfterTakeAgro:
                return new LeaderAfterTakeAgroTacticMove();

            case BotTacticMove.ChargeAbility:
                return new ChargeAbilityTacticMove();
            case BotTacticMove.UseAbility:
                return new UseAbilityTacticMove();

            case BotTacticMove.SpaceTroop:
                return new SpaceTroopTacticMove();
            case BotTacticMove.SaveAllCost:
                return new SaveAllCostTacticMove();
            case BotTacticMove.RangeKill:
                return new RangeKillTacticMove();
            case BotTacticMove.RangePoison:
                return new RangePoisonTacticMove();
        }

        return new TacticMove(true);
    }
}
