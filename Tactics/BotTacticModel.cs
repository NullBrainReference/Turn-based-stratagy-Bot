using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BotTacticMove { 
    Move,
    MoveToCharge,
    MoveToAttack,
    MoveToRetreat,

    Collect,
    CollectHealth,
    CollectBuff,
    CollectBerry,

    Attack,

    Any,

    FocuseAttack,
    ChaseLeader,
    ChaseDancer,

    OverTake,
    DistantLeader,
    Respawn,

    ChargePosition,
    PoisonLeader,
    FakePosition,
    Rerout,

    LeaderTake,
    LeaderAfterTake,
    LeaderAfterTakeAgro,

    ChargeAbility,
    UseAbility,

    SpaceTroop,
    SaveAllCost,
    RangeKill,
    RangePoison
}

[System.Serializable]
public class BotTacticModel
{
    [SerializeField] protected string name;

    [SerializeField] protected BotTacticMove[] moves;
    [SerializeField] protected TacticMove[] tacticMoves;

    protected BotStrategyModel strategyModel;

    protected bool isPosible;
    protected int weight;

    public string Name { get { return name; } }
    public bool IsPosible { get { return isPosible; } }
    public int Weight { get { return weight; } }

    protected List<BotUnitModel> botUnitModels { get { return strategyModel.UnitModels; } }

    public void Init(BotStrategyModel strategyModel)
    {
        name = "";
        isPosible = false;
        weight = 0;

        this.strategyModel = strategyModel;

        foreach (var move in moves)
            name += move.ToString();

        InitTacticMoves();
    }

    protected void InitTacticMoves()
    {
        tacticMoves = new TacticMove[moves.Length];

        for (int i = 0; i < moves.Length; i++)
        {
            tacticMoves[i] = TacticMove.GetTacticMove(moves[i]);
            tacticMoves[i].Init(strategyModel, i  + 1);

            weight += tacticMoves[i].Weight;
        }

        isPosible = true;

        foreach (var move in tacticMoves)
        {
            if (move.IsPosible)
                continue;

            isPosible = false;
            weight -= 500;
            return;
        }
    }

    public TacticMove GetTacticMove(int order)
    {
        return tacticMoves[order - 1];
    }

    public BotTacticMove GetMoveType(int order)
    {
        return moves[order - 1];
    }
}
