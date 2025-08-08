using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BotVariansGenerator
{
    private bool calculated = false;
    private bool pushed = false;
    private bool failed = false;

    private ShortMoveModel[] pickedLine;
    private int lineScore = int.MinValue;

    private int maxHealthLose = 0;

    private ShortFieldModel[] tmpFields;
    private List<ShortFieldModel> newFields;
    private List<ShortMoveModel> moveVariants;

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
        newFields = new List<ShortFieldModel>();
        moveVariants = new List<ShortMoveModel>();

        //ShortFieldModel shortFieldModel = new ShortFieldModel(whiteUnits, redUnits, botFieldModel.Models);

        //======================================= 1 

        GetNextFields(redMoves == null ? 
            shortFieldModel.GetMoves(Team.White, filters[0], saveAllCost, moveVariants) : 
            shortFieldModel.GetMoves(Team.White, moveVariants), shortFieldModel);
        //Debug.Log($"_bot gen filter0 fields count = {tmpFields.Length}");
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

        //Debug.Log($"_bot gen filter0_1 fields count = {tmpFields.Length}");
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

        //Debug.Log($"_Bot gen filter fields count {tmpFields.Length}");

        calculated = true;
        newFields = null;
        moveVariants.Clear();
        moveVariants = null;

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


    /// <summary>
    /// recursive gen with evaluation
    /// </summary>
    /// <param name="whiteUnits"></param>
    /// <param name="redUnits"></param>
    /// <param name="shortFieldModel"></param>
    /// <param name="filters"></param>
    /// <param name="redMoves"></param>
    /// <param name="deathIsBad"></param>
    /// <param name="saveAllCost"></param>
    /// <returns></returns>
    public (ShortMoveModel[], int) GetBestLine(
        List<UnitModel> whiteUnits, List<UnitModel> redUnits,
        ShortFieldModel shortFieldModel, 
        MoveFilter[] filters, 
        ShortMoveModel[] redMoves = null,
        bool deathIsBad = false,
        bool saveAllCost = false)
    {
        var worstFields = new Dictionary<string, ShortFieldModel>();
        var worstDiffs = new Dictionary<string, int>();
        var rms = new Dictionary<string, int>();

        var moveVariants = new List<ShortMoveModel>();
        //Gen part

        if (redMoves == null)
        {
            ExploreRecursive(
                shortFieldModel,
                0,
                6,
                filters,
                saveAllCost,
                deathIsBad,
                moveVariants,
                worstFields,
                worstDiffs,
                rms);
        }
        else
        {
            ExploreRecursiveRedKnown(
                shortFieldModel,
                0,
                6,
                redMoves,
                saveAllCost,
                deathIsBad,
                moveVariants,
                worstFields,
                worstDiffs,
                rms
            );
        }

        if (worstDiffs.Count <= 0)
        {
            failed = true;
            pickedLine = null;
            return (null, int.MinValue);
        }

        //Get best part
        string bestKey = worstDiffs.Keys.First();

        foreach (var pair in worstDiffs)
        {
            //Debug.Log($"_bot worst diff {pair.Key}: {worstDiffs[pair.Key]}");
            if (worstDiffs[bestKey] > pair.Value)
                continue;

            //Debug.Log($"_bot passed: {bestKey}, value {worstDiffs[bestKey]}");
            bestKey = pair.Key;
        }

        //Debug.Log($"_bot final in gen: {bestKey}, value {worstDiffs[bestKey]}");

        int leaderAbandonPunishment = maxHealthLose * 12;

        pickedLine = worstFields[bestKey].Moves;
        return (worstFields[bestKey].Moves, worstDiffs[bestKey] - leaderAbandonPunishment);
    }

    private void ExploreRecursive(
        ShortFieldModel current,
        int depth,
        int maxDepth,
        MoveFilter[] filters,
        bool saveAllCost,
        bool deathIsBad,
        List<ShortMoveModel> moveVariants,
        Dictionary<string, ShortFieldModel> worstFields,
        Dictionary<string, int> worstDiffs,
        Dictionary<string, int> rms)
    {
        // Exit
        if (depth >= maxDepth)
        {
            current.AfterMath();

            string name = current.GetName();
            int diff = current.GetMajorDifference(deathIsBad, saveAllCost);

            if (!worstDiffs.ContainsKey(name) || worstDiffs[name] > diff)
            {
                worstDiffs[name] = diff;
                worstFields[name] = current;
                rms[name] = current.GetRealityMetric();
            }
            else if (worstDiffs[name] == diff)
            {
                int c_rm = current.GetRealityMetric();
                if (rms[name] < c_rm)
                {
                    worstDiffs[name] = diff;
                    worstFields[name] = current;
                    rms[name] = c_rm;
                }
            }
            return;
        }

        var team = depth % 2 == 0 ? Team.White : Team.Red;

        List<ShortMoveModel> moves;
        if (team == Team.White)
        {
            moves = current.GetMoves(
                team,
                filters[depth / 2],
                allowOfCenter: saveAllCost,
                moveVariants
            );

            if (depth == 2) //punishment for leader hp lose on move 1 after red response here its yet field 1-1
            {
                int healthLose = LeaderHealthLose(Team.White, ref current);
                if (maxHealthLose <= healthLose)
                    maxHealthLose = healthLose;
            }
        }
        else
        {
            moves = current.GetMoves(team, moveVariants, null);
        }

        foreach (var move in moves)
        {
            ShortFieldModel nextField = current.GetNext(move);
            ExploreRecursive(
                nextField,
                depth + 1,
                maxDepth,
                filters,
                saveAllCost,
                deathIsBad,
                new List<ShortMoveModel>(),
                worstFields,
                worstDiffs,
                rms
            );
        }
    }

    private void ExploreRecursiveRedKnown(
        ShortFieldModel current,
        int depth,
        int maxDepth,
        ShortMoveModel[] redMoves,
        bool saveAllCost,
        bool deathIsBad,
        List<ShortMoveModel> moveVariants,
        Dictionary<string, ShortFieldModel> worstFields,
        Dictionary<string, int> worstDiffs,
        Dictionary<string, int> rms)
        {
        // Exit
        if (depth >= maxDepth)
        {
            current.AfterMath();

            string name = current.GetName();
            int diff = current.GetMajorDifference(deathIsBad, saveAllCost);

            if (!worstDiffs.ContainsKey(name) || worstDiffs[name] < diff)
            {
                worstDiffs[name] = diff;
                worstFields[name] = current;
                rms[name] = current.GetRealityMetric();
            }

            //if (!worstDiffs.ContainsKey(name) || worstDiffs[name] > diff)
            //{
            //    worstDiffs[name] = diff;
            //    worstFields[name] = current;
            //    rms[name] = current.GetRealityMetric();
            //}
            //else if (worstDiffs[name] == diff)
            //{
            //    int c_rm = current.GetRealityMetric();
            //    if (rms[name] < c_rm)
            //    {
            //        worstDiffs[name] = diff;
            //        worstFields[name] = current;
            //        rms[name] = c_rm;
            //    }
            //}
            return;
        }

        var team = depth % 2 == 0 ? Team.White : Team.Red;

        if (team == Team.White)
        {
            List<ShortMoveModel> moves = current.GetMoves(
                team,
                moveVariants,
                null
            );

            foreach (var move in moves)
            {
                ShortFieldModel nextField = current.GetNext(move);
                ExploreRecursiveRedKnown(
                    nextField,
                    depth + 1,
                    maxDepth,
                    redMoves,
                    saveAllCost,
                    deathIsBad,
                    new List<ShortMoveModel>(),
                    worstFields,
                    worstDiffs,
                    rms
                );
            }
        }
        else
        {
            var move = redMoves[depth / 2]; //d: 1/2 => 0; 3/2 => 1; 5/2 => 2
            //moves = current.GetMoves(team, moveVariants, null);

            //Debug.Log($"_bot depth: {depth}, d: {depth / 2}");

            ShortFieldModel nextField = current.GetNext(move);
            ExploreRecursiveRedKnown(
                nextField,
                depth + 1,
                maxDepth,
                redMoves,
                saveAllCost,
                deathIsBad,
                new List<ShortMoveModel>(),
                worstFields,
                worstDiffs,
                rms
            );
        }
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

    public void GetNextFilterMoves(
        Team team, MoveFilter filter, bool ignore = false, bool allowOfcenter = false)
    {
        //var newFields = new List<ShortFieldModel>();
        newFields.Clear();

        int variantsCount = 0;

        for (int i = 0; i < tmpFields.Length; i++)
        {
            var moves = ignore == false ? 
                tmpFields[i].GetMoves(team, filter, allowOfcenter, moveVariants) : 
                tmpFields[i].GetMoves(team, moveVariants);
            variantsCount += moves.Count;

            for (int j = 0; j < moves.Count; j++)
            {
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
    }

    public void GetNextMoves(
        Team team, bool afterMath = false, bool sort = false, int filter = 2, UnitModel mover = null)
    {
        newFields.Clear();
        //var newFields = new List<ShortFieldModel>();

        int variantsCount = 0;

        for (int i = 0; i < tmpFields.Length; i++)
        {
            var moves = tmpFields[i].GetMoves(team, moveVariants, mover);
            variantsCount += moves.Count;
        
            for (int j = 0; j < moves.Count; j++)
            {
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
    }

    public void GetNextMoves(
        Team team, ShortMoveModel move, bool afterMath = false)
    {

        for (int i = 0; i < tmpFields.Length; i++)
        {
            tmpFields[i] = tmpFields[i].GetNext(move);
            if (afterMath)
                tmpFields[i].AfterMath();
        }
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

    private int LeaderHealthLose(Team team, ref ShortFieldModel field)
    {
        return field.LeaderHealthLose(team);
    }

    public static void Push(ActionManager actionManager, ShortMoveModel[] line)
    {
        line[0].Push(actionManager, 1);
        line[2].Push(actionManager, 2);
        line[4].Push(actionManager, 3);

        //pushed = true;
    }
}
