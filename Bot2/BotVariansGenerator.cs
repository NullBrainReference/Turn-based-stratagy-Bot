using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class BotVariansGenerator
{
    //private List<ShortMoveModel> testLine;
    //private List<List<ShortMoveModel>> lines;
    //private ShortFieldModel[] fields;
    //private BotFieldModel _botFieldModel;

    private bool calculated = false;
    private bool pushed = false;
    private bool failed = false;

    private ShortMoveModel[] pickedLine;
    private int lineScore = int.MinValue;

    private int maxHealthLose = 0;

    private ShortFieldModel[] tmpFields;

    public bool Pushed => pushed;
    public bool Calculated => calculated;
    public bool Failed => failed;

    public int DiffScore => lineScore;

    public ShortMoveModel[] PickedLine => pickedLine;

    public int FieldsCount => tmpFields != null ? tmpFields.Length : 0;

    public void GenerateFilterLines(
        List<UnitModel> whiteUnits, List<UnitModel> redUnits,
        ShortFieldModel shortFieldModel, MoveFilter[] filters, ShortMoveModel[] redMoves = null, bool saveAllCost = false)
    {
        calculated = false;

        //ShortFieldModel shortFieldModel = new ShortFieldModel(whiteUnits, redUnits, botFieldModel.Models);

        //======================================= 1 

        GetNextFields(redMoves == null ? 
            shortFieldModel.GetMoves(Team.White, filters[0], saveAllCost) : 
            shortFieldModel.GetMoves(Team.White), shortFieldModel);
        Debug.Log($"_bot gen filter0 fields count = {tmpFields.Length}");
        if (tmpFields.Length == 0)
        {
            failed = true;
            return;
        }

        //======================================= 1-1
        if (redMoves == null)
            GetNextMoves(Team.Red);
        else
            GetNextMoves(Team.Red, redMoves[0]);

        maxHealthLose = LeaderMaxHealthLose(Team.White);

        Debug.Log($"_bot gen filter0_1 fields count = {tmpFields.Length}");
        //======================================= 2
        GetNextFilterMoves(Team.White, filters[1], redMoves != null, saveAllCost);
        if (tmpFields.Length == 0)
        {
            failed = true;
            return;
        }
        //======================================= 2-1
        if (redMoves == null)
            GetNextMoves(Team.Red);
        else
            GetNextMoves(Team.Red, redMoves[1]);
        //======================================= 3
        GetNextFilterMoves(Team.White, filters[2], redMoves != null, saveAllCost);
        if (tmpFields.Length == 0)
        {
            failed = true;
            return;
        }
        //======================================= 3-1
        if (redMoves == null)
            GetNextMoves(Team.Red, true);
        else
            GetNextMoves(Team.Red, redMoves[2], true);

        if (IsLeaderDead(Team.White))
        {
            if (HasLeaderMoved(Team.White) == false)
                maxHealthLose += 100;

            maxHealthLose *= 2;
        }

        Debug.Log($"_Bot gen filter fields count {tmpFields.Length}");

        calculated = true;

        //Console.Log($"_Bot gen item_takes fields count {tmpFields.Length}");

        //ShortMoveModel bestmove = PickupBestLine().Item1[0];
    }

    public (ShortMoveModel[], int) PickupBestLine(int bonus = 0, bool deathIsBad = false, bool saveAllCost = false)
    {
        //int maxW = -1000;
        //int maxR = -1000;
        //
        //int bestDiff = -1000;
        if (tmpFields.Length == 0)
        {
            return (new ShortMoveModel[6], int.MinValue);
        }

        var bestLine = tmpFields[0].Moves;

        //int bestIndex = 0;

        //Dictionary<string, List<ShortFieldModel>> whiteFields = new Dictionary<string, List<ShortFieldModel>>();
        Dictionary<string, ShortFieldModel> whiteFields = new Dictionary<string, ShortFieldModel>();

        Dictionary<string, int> rws = new Dictionary<string, int>();
        Dictionary<string, int> worstLeaderDiffs = new Dictionary<string, int>();

        string name = tmpFields[0].GetName();
        whiteFields.Add(name, tmpFields[0]);

        rws.Add(name, tmpFields[0].GetPosWeight(Team.Red));
        worstLeaderDiffs.Add(name, tmpFields[0].GetMajorDifference(deathIsBad, saveAllCost));

        for (int i = 1; i < tmpFields.Length; i++)
        {
            name = tmpFields[i].GetName();


            if (whiteFields.ContainsKey(name))
            {
                int rw = tmpFields[i].GetPosWeight(Team.Red);
                if (rws[name] < rw)
                {
                    rws[name] = rw;
                    whiteFields[name] = tmpFields[i];
                }

                int leaderDiff = tmpFields[i].GetMajorDifference(deathIsBad, saveAllCost);
                if (worstLeaderDiffs[name] > leaderDiff)
                {
                    worstLeaderDiffs[name] = leaderDiff;
                }
            }
            else
            {
                whiteFields.Add(name, tmpFields[i]);
                rws.Add(name ,tmpFields[i].GetPosWeight(Team.Red));
                worstLeaderDiffs.Add(name, tmpFields[i].GetMajorDifference(deathIsBad, saveAllCost));
            }
        }

        //Dictionary<string, int> diffs = new Dictionary<string, int>();
        string key = "";
        int maxDiff = -10000;
        int maxLeaderDiff = -1000;

        foreach (var field in whiteFields) 
        {
            int leaderDiff = worstLeaderDiffs[field.Key];
            if (leaderDiff > maxLeaderDiff)
            {
                //TODO uncomment weight comparasion
                int diff = field.Value.GetPosWeight(Team.White) - rws[field.Key] + field.Value.GetRealityMetric();
                
                maxLeaderDiff = leaderDiff;

                maxDiff = diff;
                key = field.Key;
            }
            else if (leaderDiff == maxLeaderDiff)
            {
                int diff = field.Value.GetPosWeight(Team.White) - rws[field.Key] + field.Value.GetRealityMetric();

                if (diff > maxDiff)
                {
                    maxDiff = diff;
                    key = field.Key;
                }
            }
        }

        bestLine = key == "" ? bestLine : whiteFields[key].Moves;
        int worstDiff = key == "" ? -1000 : worstLeaderDiffs[key]; 


        //Console.Log(
        //    $"_Bot line \n" +
        //    $"00={bestLine[0].Unit.Name} {bestLine[0].MoveType}: {bestLine[0].TileFrom.Name}-{bestLine[0].Tile.Name},\n" +
        //    $"01={bestLine[1].Unit.Name} {bestLine[1].MoveType}: {bestLine[1].TileFrom.Name}-{bestLine[1].Tile.Name},\n" +
        //    $"02={bestLine[2].Unit.Name} {bestLine[2].MoveType}: {bestLine[2].TileFrom.Name}-{bestLine[2].Tile.Name},\n" +
        //    $"03={bestLine[3].Unit.Name} {bestLine[3].MoveType}: {bestLine[3].TileFrom.Name}-{bestLine[3].Tile.Name},\n" +
        //    $"04={bestLine[4].Unit.Name} {bestLine[4].MoveType}: {bestLine[4].TileFrom.Name}-{bestLine[4].Tile.Name},\n" +
        //    $"05={bestLine[5].Unit.Name} {bestLine[5].MoveType}: {bestLine[5].TileFrom.Name}-{bestLine[5].Tile.Name}\n");

        if (key != "")
        {
            Debug.Log($"_Bot gen bestLine reality metric = {whiteFields[key].GetRealityMetric()}");
            Debug.Log($"_Bot gen bestLine worst leader diff = {whiteFields[key].GetMajorDifference(deathIsBad, saveAllCost)}");
            Debug.Log($"_Bot gen bestLine key = {key}");
        }
        else
        {
            Debug.Log($"_Bot bestLine no key");
        }
        if (worstDiff + bonus > lineScore)
        {
            pickedLine = bestLine;
            lineScore = worstDiff + bonus;
        }

        int leaderAbandonPunishment = maxHealthLose * 12;

        tmpFields = null;
 
        return (bestLine, worstDiff + bonus - leaderAbandonPunishment);
    }

    public void GetNextFields(List<ShortMoveModel> moves, ShortFieldModel shortFieldModel)
    {
        tmpFields = new ShortFieldModel[moves.Count];

        for (int i = 0; i < moves.Count; i++)
        {
            tmpFields[i] = shortFieldModel.GetNext(moves[i]);
            //if (tmpFields[i].HasMoved(shortFieldModel) == false)
            //    nonmoved++;
        }
    }

    public List<ShortMoveModel> GetNextFilterMoves(
        Team team, MoveFilter filter, bool ignore = false, bool allowOfcenter = false)
    {
        var result = new List<ShortMoveModel>();
        var newFields = new List<ShortFieldModel>();

        int variantsCount = 0;

        for (int i = 0; i < tmpFields.Length; i++)
        {
            var moves = ignore == false ? tmpFields[i].GetMoves(team, filter, allowOfcenter) : tmpFields[i].GetMoves(team);
            variantsCount += moves.Count;

            for (int j = 0; j < moves.Count; j++)
            {
                result.Add(moves[j]);
                var tmp = tmpFields[i].GetNext(moves[j]);

                newFields.Add(tmp);
                //newFields.Add(tmpFields[i].GetNext(move));
            }
        }

        tmpFields = new ShortFieldModel[variantsCount];

        for (int i = 0; i < variantsCount; i++)
        {
            tmpFields[i] = newFields[i];
        }

        return result;
    }

    public List<ShortMoveModel> GetNextMoves(
        Team team, bool afterMath = false, bool sort = false, int filter = 2, UnitModel mover = null)
    {
        var result = new List<ShortMoveModel>();
        var newFields = new List<ShortFieldModel>();

        int variantsCount = 0;

        for (int i = 0; i < tmpFields.Length; i++)
        {
            var moves = tmpFields[i].GetMoves(team, mover);
            variantsCount += moves.Count;
        
            for (int j = 0; j < moves.Count; j++)
            {
                result.Add(moves[j]);
                var tmp = tmpFields[i].GetNext(moves[j]);

                if (afterMath)
                    tmp.AfterMath();
        
                //if (tmp.HasMoved(tmpFields[i]) == false)
                //    nonmoved++;
        
                newFields.Add(tmp);
                //newFields.Add(tmpFields[i].GetNext(move));
            }
        }

        if (sort)
        {
            newFields.Sort((x, y) => 
                y.GetImportance().CompareTo(
                x.GetImportance()));
                //y.GetPosWeight(Team.White).CompareTo(
                //x.GetPosWeight(Team.White)));
            //
            variantsCount /= filter;
        }

        tmpFields = new ShortFieldModel[variantsCount];

        for (int i = 0; i < variantsCount; i++)
        {
            tmpFields[i] = newFields[i];
        }

        return result;
    }

    public List<ShortMoveModel> GetNextMoves(
        Team team, ShortMoveModel move, bool afterMath = false)
    {
        var result = new List<ShortMoveModel>() { move };

        for (int i = 0; i < tmpFields.Length; i++)
        {
            tmpFields[i] = tmpFields[i].GetNext(move);
            if (afterMath)
                tmpFields[i].AfterMath();
        }

        return result;
    }

    private bool IsLeaderDead(Team team)
    {
        foreach (var field in tmpFields)
        {
            if (field.IsLeaderDead(team))
                return true;
        }

        return false;
    }

    private bool HasLeaderMoved(Team team)
    {
        foreach (var field in tmpFields)
        {
            if (field.HasLeaderMoved(team))
                return true;
            else
                return false;
        }

        return false;
    }

    private int LeaderMaxHealthLose(Team team)
    {
        int max = 0;

        foreach (var field in tmpFields)
        {
            int value = field.LeaderHealthLose(team);

            if (value <= max)
                continue;

            max = value;
        }

        return max;
    }

    public static void Push(ActionManager actionManager, ShortMoveModel[] line)
    {
        line[0].Push(actionManager, 1);
        line[2].Push(actionManager, 2);
        line[4].Push(actionManager, 3);

        //pushed = true;
    }
}
