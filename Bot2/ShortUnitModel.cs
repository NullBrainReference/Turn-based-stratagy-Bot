using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

public struct ShortUnitModel
{
    private short health;
    private short damage;
    private short tookDamage;

    private bool isCharged;
    private bool isLeader;
    private bool hasShield;
    private bool isPoisoned;

    private bool isRanger;

    private Vector2Int pos;
    private bool moved;
    private bool respawned;

    private bool isPosOwner;

    private short chargeStage;
    private short chargeLimit;

    private BotUnitPersonality _personality;
    private UnitModel _unit;

    public ShortUnitModel(BotUnitPersonality personality, UnitModel unit)
    {
        _personality = personality;
        _unit = unit;

        health = (short) unit.Health;
        damage = (short) unit.Damage;
        isCharged = unit.IsCharged;
        hasShield = unit.HasShield;
        isPoisoned = unit.IsPoisoned;

        isRanger = unit.IsRanger;

        moved = false;
        respawned = false;
        tookDamage = 0;

        chargeLimit = (short)unit.ChargeLimit;
        chargeStage = (short)unit.ChargeStage;
        
        isLeader = unit.IsLeader;
        pos = BotFieldModel.GetPos04(unit.Position.Xcoord, unit.Position.Ycoord);

        isPosOwner = true;
    }


    /// <summary>
    /// Serves for respawn capability
    /// </summary>
    /// <param name="original"></param>
    public ShortUnitModel(ShortUnitModel original)
    {
        _unit = original.Unit;
        _personality = BotUnitPersonality.GetUnitPersonality(_unit.SpawnerUnitType);

        health = (short)original.Unit.TrueHealth;
        damage = 1; //TODO replace value with actual one
        tookDamage = 0;

        isCharged = false;
        hasShield = false;
        isPoisoned = false;
        isRanger = false;

        chargeLimit = 1;
        chargeStage = 0;

        moved = false;
        respawned = true;
        isLeader = _unit.IsLeader;
        pos = new Vector2Int(original.pos.x, original.pos.y);

        isPosOwner = original.isPosOwner;
    }


    /// <summary>
    /// Serves to present failed respawn
    /// </summary>
    /// <param name="original"></param>
    /// <param name="hp"></param>
    public ShortUnitModel(ShortUnitModel original, bool failed)
    {
        _unit = original.Unit;
        _personality = BotUnitPersonality.GetUnitPersonality(_unit.SpawnerUnitType);

        health = 0;
        damage = 1; 
        tookDamage = 0;

        isCharged = false;
        hasShield = false;
        isPoisoned = false;

        isRanger = false;

        chargeLimit = 1;
        chargeStage = 0;

        moved = false;
        respawned = true;
        isLeader = _unit.IsLeader;
        pos = new Vector2Int(original.pos.x, original.pos.y);

        isPosOwner = original.isPosOwner;
    }

    public bool IsDead => health <= 0;
    public short Health => health;
    public short Damage => damage;
    public bool IsCharged => isCharged;
    public bool IsLeader => isLeader;
    public bool IsRanger => isRanger;

    public bool IsPoisoned => isPoisoned;
    public bool HasShield => hasShield;

    //public bool IsRanger =>

    public int ChargeStage => chargeStage;
    public int ChargeLimit => chargeLimit;

    public bool Moved => moved;
    public bool Respawned { get => respawned; set => respawned = value; }
    public bool IsPosOwner { get => isPosOwner; set => isPosOwner = value; }

    public UnitModel Unit => _unit;

    public BotUnitPersonality Personality => _personality;

    public bool IsModelOf(UnitModel unit) => unit == _unit;

    public Vector2Int Pos => pos;

    public int Xofcenter => (pos.x - 2) ^ 2;
    public int YofCenter => (pos.y - 2) ^ 2;

    public void Charge()
    {
        chargeStage++;
        if (chargeStage >= chargeLimit)
            isCharged = true;
    }

    public void DropCharge()
    {
        isCharged = false;
        chargeStage = 0;
    }

    public void SetPos(int x, int y)
    {
        pos = new Vector2Int(x, y);
    }

    public void Move()
    {
        moved = true;
    }

    public void Heal(int value)
    {
        isPoisoned = false;
        health += (short)value;
    }

    public void SetShield(bool value)
    {
        hasShield = value;
    }

    public void TakeDamage(short value)
    {
        health -= value;
        tookDamage += value;
    }

    public void LeaderRespawnDamage(short value)
    {
        if (moved == false)
            health -= value;

        health -= value;
    }

    public void AfterDamage()
    {
        if (moved == false)
            health -= tookDamage;
    }

    public void Poison(bool poison = true)
    {
        isPoisoned = poison;
    }

    public void SetRanger(bool value)
    {
        isRanger = value;
    }
}
