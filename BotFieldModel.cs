using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class BotFieldModel
{
    private static readonly int xOffset = 2;
    private static readonly int yOffset = -2;

    private List<Tile> tiles;
    private BotTileModel [,] tileModels;

    public List<Tile> Tiles => tiles;

    public BotTileModel[,] Models => tileModels;

    public BotFieldModel(Field field)
    {
        //tiles = new List<Tile> {
        //    field.A1, field.A2, field.A4, field.A5,
        //    field.B1, field.B3, field.B5,
        //    field.C2, field.C4,
        //    field.D1, field.D3, field.D5,
        //    field.E1, field.E2, field.E4, field.E5
        //};
        tiles = field.TilesLibrary.Values.ToList();

        tileModels = new BotTileModel[5, 5];
        FillModels();
    }

    public static Vector2Int GetPos04(int x, int y) 
    {
        x = x + xOffset;
        y = -(y + yOffset);
        return new Vector2Int(x, y);
    }

    public static int GetX(int x) => x + xOffset;
    public static int GetY(int y) => -(y + yOffset);

    private void FillModels()
    {
        foreach (var tile in tiles)
        {
            int x = tile.Xcoord + xOffset;
            int y = -(tile.Ycoord + yOffset);

            tileModels[x, y] = new BotTileModel(tile);
        }
    }

    public void PutUnitModel(BotUnitModel unitModel)
    {
        var tile = GetTileModel(unitModel.Unit.Position);
        tile.SetUnitModel(unitModel);

        Debug.Log($"_Bot unitModel: {unitModel.Unit.Name} was put to tileModel {tile.Tile.Name}");
        //GetTileModel(unitModel.Unit.Position).SetUnitModel(unitModel);
    }

    public BotTileModel GetTileModel(Tile tile)
    {
        return tileModels[tile.Xcoord + xOffset, -(tile.Ycoord + yOffset)];
    }

    public List<Tile> GetAttackableTiles()
    {
        List<Tile> result= new List<Tile>();

        foreach (var model in tileModels)
        {
            if (model == null)
                continue;
            if (model.Ocupation == TileOcupation.None)
                continue;

            if (model.Ocupation == TileOcupation.Bot)
                if(model.Tile.HasEnemyInRange(model.Unit))
                    result.Add(model.Tile);
        }

        return result;
    }

    public List<BotTileModel> GetAttackModels()
    {
        List<BotTileModel> result = new List<BotTileModel>();

        foreach (var model in tileModels)
        {
            if (model == null)
                continue;
            if (model.Ocupation != TileOcupation.Bot)
                continue;

            if (model.Unit.Locked)
                continue;

            if (model.WasAttackedFrom)
                continue;

            if (model.UnitModel == null)
                continue;

            //if (model.UnitModel != null)
            if (model.UnitModel.IsCharged == false)
                continue;

            if (model.Tile.HasEnemyInRange(model.Unit, model.UnitModel.IsRanger))
                result.Add(model);
        }

        return result;
    }

    public List<BotTileModel> GetMoveModels()
    {
        List<BotTileModel> result = new List<BotTileModel>();

        foreach (var model in tileModels)
        {
            if (model == null)
                continue;
            if (model.Ocupation == TileOcupation.None)
                continue;

            if (model.Ocupation == TileOcupation.Bot)
            {
                if (model.Unit.Locked)
                    continue;

                result.Add(model);
            }
        }

        return result;
    }
}
