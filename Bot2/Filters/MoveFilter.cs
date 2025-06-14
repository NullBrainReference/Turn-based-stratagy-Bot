using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveFilter
{
    private UnitModel executor;
    private Predicate<ShortMoveModel> filter;

    private string key;

    private int priority;

    private MoveFilter followup;

    private UnitModel uncharger;

    private TileModel moveTile;

    public MoveFilter(UnitModel executor, Predicate<ShortMoveModel> filter, UnitModel uncharger = null)
    {
        this.executor = executor;
        this.filter = filter;
        this.uncharger = uncharger;
    }

    public MoveFilter(UnitModel executor, Predicate<ShortMoveModel> filter, TileModel moveTile)
    {
        this.executor = executor;
        this.filter = filter;
        this.moveTile = moveTile;
    }

    public int Priority { get { return priority; } set { priority = value; } }
    public string Key { get { return key; } set { key = value; } }

    public MoveFilter Followup { get { return followup; } set { followup = value; } }

    public bool HasFollowUp => 
        followup != null;

    public UnitModel Executor =>
        executor;
    public UnitModel Uncharger =>
        uncharger;

    public bool IgnoreUncharge { get; set; } = false;

    public bool HasChargeCollision(MoveFilter filter)
    {
        if (Uncharger == null)
            return false;
        if (filter.Uncharger == null)
            return false;

        if (filter.Uncharger == Uncharger)
            return true;

        return false;
    }

    public bool HasEqualExecutor(MoveFilter filter)
    {
        if (executor == null)
            return false;
        if (filter.executor == null)
            return false;

        if (executor == filter.executor)
            return true;

        return false;
    }

    public bool IsUnreachibleFrom(MoveFilter filter)
    {
        if (HasEqualExecutor(filter) == false)
            return false;

        if (moveTile == null)
            return false;
        if (filter.moveTile == null)
            return false;

        if (filter.moveTile.MoveTiles.Contains(moveTile) == false)
            return true;
        else if (filter.moveTile == moveTile)
            return true;

        return false;
    }

    public bool IsExecutor(UnitModel unitModel) => 
        unitModel == executor;

    public bool Match(ShortMoveModel moveModel) => 
        filter(moveModel);
   
}
