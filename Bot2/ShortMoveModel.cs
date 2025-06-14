using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ShortMoveModel
{
    private UnitModel _unit;
    private UnitModel _target;
    private TileModel _tileTo;
    private TileModel _tileFrom;
    private MoveType _moveType;

    //private bool leadsInDanger; //No need, remove

    public ShortMoveModel(MoveType moveType, UnitModel unit, TileModel tileTo, TileModel tileFrom, UnitModel target = null)
    {
        _moveType = moveType;
        _unit = unit;
        _tileTo = tileTo;
        _tileFrom = tileFrom;
        _target = target;

        //leadsInDanger = false;
    }

    public ShortMoveModel(ActionSlot actionSlot)
    {
        _moveType = actionSlot.MoveType;
        _unit = actionSlot.ActionPerformer.UnitModel;
        _tileFrom = actionSlot.StartPosition != null ? actionSlot.StartPosition.TileModel : _unit.Position;

        switch (_moveType)
        {
            case MoveType.Move:
                _tileTo = actionSlot.MoveTarget.TileModel;
                _target = null;
                break;
            case MoveType.Attack:
                _tileTo = actionSlot.AttackTarget.Position.TileModel;
                _target = actionSlot.AttackTarget.UnitModel;
                break;
            case MoveType.Respawn:
                _tileTo = _unit.Position;
                _target = null;
                break;
            case MoveType.Ability:
                _tileTo = actionSlot.AttackTarget.Position.TileModel;
                _target = actionSlot.AttackTarget.UnitModel;
                break;

            default:
                _tileTo = null;
                _target = null;
                break;
        }
        //_tileTo = actionSlot.MoveTarget != null ? actionSlot.MoveTarget.TileModel : _unit.Position;


        //leadsInDanger = false;
    }

    public MoveType MoveType => _moveType;
    public UnitModel Unit => _unit;

    public UnitModel Target => _target;

    public TileModel Tile => _tileTo;
    public TileModel TileFrom => _tileFrom;


    public int X => BotFieldModel.GetX(_tileTo.Xcoord);
    public int Y => BotFieldModel.GetY(_tileTo.Ycoord);

    //public bool LeadsInDanger { get { return leadsInDanger; } set { leadsInDanger = value; } }

    public void Push(ActionManager actionManager, int order)
    {
        Unit unit = actionManager.unitManager.Units[_unit.Key];
        Unit target = null;
        if (_target != null)
            target = actionManager.unitManager.Units[_target.Key];
        Tile tileTo = actionManager.roundManager.mapManager.field.TilesLibrary[_tileTo.Key];
        Tile tileFrom = actionManager.roundManager.mapManager.field.TilesLibrary[_tileFrom.Key];

        if (MoveType == MoveType.Move)
            actionManager.CreateActionHidden(unit, tileFrom, tileTo, order);
        else if (MoveType == MoveType.Attack)
            actionManager.CreateActionHidden(unit, target, tileFrom, order);
        else if (MoveType == MoveType.Respawn)
            actionManager.CreateRespawnActionHidden(unit, order);
        else if (MoveType == MoveType.Ability)
            actionManager.CreateAbilityActionHidden(unit, target, order);
    }

    public static bool operator ==(ShortMoveModel a, ShortMoveModel b)
        => a.GetName() == b.GetName();

    public static bool operator !=(ShortMoveModel a, ShortMoveModel b)
        => a.GetName() == b.GetName();

    public override bool Equals(object obj)
    {
        if (obj is ShortMoveModel other)
        {
            return other == this;
        }
        return false;
    }

    public override int GetHashCode()
    {
        int hash = 17;
        hash = hash * 31 + (_unit != null ? _unit.GetHashCode() : 0);
        hash = hash * 31 + (_target != null ? _target.GetHashCode() : 0);
        hash = hash * 31 + (_tileTo != null ? _tileTo.GetHashCode() : 0);
        hash = hash * 31 + (_tileFrom != null ? _tileFrom.GetHashCode() : 0);
        hash = hash * 31 + _moveType.GetHashCode();
        return hash;
    }

    public string GetName()
    {
        switch (_moveType)
        {
            case MoveType.Move:
                return
                    //_unit.Name + _moveType;
                    _unit.Key + _moveType +
                    _tileTo.Key + _tileFrom.Key;

            case MoveType.Attack:
                return
                    //_unit.Name + _moveType;
                    _unit.Key + _moveType +
                    _target.Key;

            case MoveType.Ability:
                return
                    _unit.Key + _moveType + _target.Key;
                    //_unit.Name + _moveType +
                    //_tileFrom.Name + _target.Name;

            case MoveType.Respawn:
                return
                    _unit.Key + _moveType;
                    //_unit.Name + _moveType +
                    //_tileFrom.Name;

        }

        return
            _unit.Key + _moveType +
            _tileTo.Key + _tileFrom.Key;
            //target;
    }
}
