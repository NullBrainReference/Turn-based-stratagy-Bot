using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ShortTileModel
{
    //private TileOcupation _occupation;
    private bool _isCharged;
    private TileModel _tile;

    private short occupantsCount;

    public ShortTileModel(bool isSpace)
    {
        //_occupation = TileOcupation.Space;
        _isCharged = false;
        _tile = null;
        occupantsCount = 0;
    }

    public ShortTileModel(TileModel tile)
    {
        _isCharged = tile.IsCharged;
        _tile = tile;

        if (tile.UnitInTile == null)
        {
            //_occupation = TileOcupation.None;
            occupantsCount = 0;
        }
        else if (tile.UnitInTile.Team == Team.White)
        {
            //_occupation = TileOcupation.Bot;
            occupantsCount = 1;
        }
        else
        {
            //_occupation = TileOcupation.Player;
            occupantsCount = 1;
        }
    }

    public TileModel Tile => _tile;

    //public bool IsOccupied => _occupation != TileOcupation.None;
    public bool IsOccupied => occupantsCount > 0;
    public bool IsCharged => _isCharged;

    public void DropCharge()
    {
        _isCharged = false;
    }

    //public void Occupy(TileOcupation occupation)
    //{
    //    _occupation = occupation;
    //}

    //public bool IsOccupiedBy(Team team)
    //{
    //    if (_occupation == TileOcupation.None)
    //        return false;
    //    else if (_occupation == TileOcupation.Space)
    //        return true;
    //
    //    if (team == Team.White)
    //    {
    //        if (_occupation == TileOcupation.Bot)
    //            return true;
    //    }
    //    else
    //    {
    //        if (_occupation == TileOcupation.Player)
    //            return true;
    //    }
    //
    //    return false;
    //}

    public void Occupy(UnitModel unit)
    {
        //_occupation = unit.Team == Team.White ? TileOcupation.Bot : TileOcupation.Player;
        occupantsCount++;
    }
    public void Free()
    {
        //_occupation = TileOcupation.None;
        occupantsCount--;
    }
}
