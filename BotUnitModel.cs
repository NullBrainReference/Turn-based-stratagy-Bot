using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BotUnitModel
{
    private Unit unit;

    private int expectedMelee;
    private int expectedRange;

    private bool hasShield;
    private bool isCharged;
    private bool hasMoved;
    private bool triedToRespawn;
    private bool isRanger;

    private BotUnitPersonality unitPersonality;

    private BotStrategyModel strategyModel;

    public Unit Unit { get { return unit; } }
    public BotStrategyModel StrategyModel { get { return strategyModel; } }

    public int MeleeDamage { get { return expectedMelee; } }
    public int RangeDamage { get { return expectedRange; } }
    public bool HasShield { get { return hasShield; } }
    public bool IsCharged { get { return isCharged; } }
    public bool HasMoved { get { return hasMoved; } }
    public bool TriedToRespawn { get { return triedToRespawn; } }
    public bool IsRanger { get { return isRanger; } }


    public BotUnitPersonality UnitPersonality { get { return unitPersonality; } }

    public BotUnitModel(Unit unit, BotStrategyModel strategyModel)
    {
        this.strategyModel = strategyModel;
        this.unit = unit;
        this.isCharged = unit.IsCharged;
        this.expectedMelee = unit.DamageMelee;
        this.expectedRange = unit.DamageRange;
        this.isRanger = unit.IsRanger;

        this.unitPersonality = BotUnitPersonality.GetUnitPersonality(unit.UnitType);
    }

    public void ResetRange()
    {
        isRanger = unit.IsRanger;
    }

    public void MakeRanger()
    {
        isRanger = true;
    }

    public void ResetDamage()
    {
        expectedMelee = Unit.DamageMelee;
        expectedRange = Unit.DamageRange;
    }

    public void UpdateRoundState()
    {
        isCharged = Unit.IsCharged;
        hasShield = Unit.Shield;
        hasMoved = false;
        triedToRespawn = false;
    }

    public void Move()
    {
        hasMoved = true;
    }

    public void Respawn()
    {
        triedToRespawn = true;
    }

    public void GiveShield()
    {
        hasShield = true;
    }

    public void Charge()
    {
        isCharged = true;
    }

    public void DropCharge()
    {
        isCharged = false;
    }

    public void AddDamageMelee(int value, bool isTeamBuff = false)
    {
        if (isTeamBuff) 
        {
            foreach(var unitModel in strategyModel.UnitModels)
            {
                unitModel.AddDamageMelee(value);
            }
            return;
        }

        expectedMelee += value;
    }

    public void AddDamageRange(int value, bool isTeamBuff = false)
    {
        if (isTeamBuff)
        {
            foreach (var unitModel in strategyModel.UnitModels)
            {
                unitModel.AddDamageRange(value);
            }
            return;
        }

        expectedRange += value;
    }
}
