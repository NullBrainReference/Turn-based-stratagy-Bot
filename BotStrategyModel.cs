using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GamePlan { Balance, Defence, Attack, Wild }

[System.Serializable]
public class BotStrategyModel
{
    [SerializeField] private GamePlan plan;
    [SerializeField] private int unitsAttacked;
    [SerializeField] private int movesMade;
    [SerializeField] private int respawnsTried;

    [SerializeField] private Totem botTotem;
    [SerializeField] private Totem playerTotem;

    [NonSerialized] private BotAwarenessModel awarenessModel;

    [NonSerialized] private List<BotUnitModel> unitModels;

    private int AttacksFailed = 0;
    private int DefencesFailed = 0;

    public List<BotUnitModel> UnitModels { get { return unitModels; } set { unitModels = value; } }
    public Unit Leader { get { return botTotem.Leader; } }
    public Unit PlayerLeader { get { return playerTotem.Leader; } }

    public bool IsAbleToAttack
    {
        get
        {
            bool hasCharged = false;

            foreach (BotUnitModel model in unitModels)
            {
                if (model.IsCharged)
                    hasCharged = true;
            }

            return hasCharged;
        }
    }

    public int AttackWeightBonus 
    { 
        get 
        { 
            return 3 - unitsAttacked; 
        } 
    }
    public int MoveToUnAttackedWeightBonus 
    { 
        get 
        {
            return movesMade < 2 ? 2 - movesMade : -5; 
        } 
    }
    public int RespawnWeightBonus
    {
        get { return respawnsTried >= 1 ? -8 : 8; }
    }

    public int SafeBonus 
    { 
        get
        {
            switch (plan)
            {
                case GamePlan.Balance:
                    return 0;
                case GamePlan.Defence:
                    return 2;
                case GamePlan.Attack:
                    return -1;
            }

            return 0;
        } 
    }

    public GamePlan Plan { get { return plan; } }

    public BotAwarenessModel AwarenessModel => awarenessModel;

    public BotStrategyModel(Totem botTotem, Totem playerTotem, BotAwarenessModel awarenessModel)
    {
        plan = GamePlan.Balance;
        this.botTotem = botTotem;
        this.playerTotem = playerTotem;

        this.awarenessModel = awarenessModel;
    }

    /// <summary>
    /// Call on every new round (before bot moved) 
    /// </summary>
    public void Refresh()
    {
        movesMade = 0;
        respawnsTried = 0;
        unitsAttacked = 0;
    }

    public void UpdatePlan()
    {
        if (playerTotem.Health > botTotem.Health)
            plan = GamePlan.Defence;
        else
            plan = GamePlan.Attack;   
        
        if (Leader.Health <= 4 && Leader.Health < PlayerLeader.Health)
        {
            plan = GamePlan.Wild;
        }
    }

    /// <summary>
    /// Call after unic attack confirmed in model
    /// </summary>
    public void Attack()
    {
        unitsAttacked++;
        movesMade++;
    }

    /// <summary>
    /// Call after move confirmed in model
    /// </summary>
    public void Move()
    {
        movesMade++;
    }

    /// <summary>
    /// Call after move confirmed in model
    /// </summary>
    public void Respawn()
    {
        movesMade++;
    }
}
