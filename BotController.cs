using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class BotController : MonoBehaviour
{
    [SerializeField] private bool IsActive;
    [SerializeField] private Field field;
    [SerializeField] private ActionManager actionManager;
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private BotStrategyModel strategyModel;

    [SerializeField] private Totem playerTotem;
    [SerializeField] private Totem botTotem;

    [SerializeField] private List<BotUnitModel> unitModels;

    [SerializeField] private BotTacticsSelector tacticsSelector;

    [SerializeField] private GameObject botScreen;
    [SerializeField] private TextMesh botScreenText;

    private List<BotAction> actions;
    private BotFieldModel fieldModel;

    private BotAwarenessModel awarenessModel;

    public static readonly Team _botTeam = Team.White;

    private bool isInitOver = false;

    private int order = 1;

    private int rounds = 0;

    private string[] messages;
    private int linesDone = 0;
    private int linesFailed = 0;

    //private Task task = null;
    private BotVariansGenerator[] variansGenerator = null;
    private Task[] tasks;

    private (ShortMoveModel[], int)[] bestLines;

    private readonly object lockObject = new object();

    private void Start()
    {
        unitModels = new List<BotUnitModel>();

        actions = new List<BotAction>();
        awarenessModel = new BotAwarenessModel();
        strategyModel = new BotStrategyModel(botTotem, playerTotem, awarenessModel);

        tacticsSelector.SetStrategyModel(strategyModel);
        tacticsSelector.InintLevelTactics();
        //StartCoroutine(InitFieldModel());
    }

    //TODO: this should not be here 
    private void UpdateModels()
    {
        foreach (var tile in field.Tiles)
        {
            tile.TileModel.Update(tile);
        }

        for (int i = 0; i < networkManager.unitManager.WhiteUnits.Count; i++)
        {
            networkManager.unitManager.WhiteUnits[i].UnitModel.Update(networkManager.unitManager.WhiteUnits[i]);
            networkManager.unitManager.RedUnits[i].UnitModel.Update(networkManager.unitManager.RedUnits[i]);
        }
    }

    private IEnumerator InitFieldModel()
    {
        yield return new WaitUntil(() => networkManager.IsSpawnOver);
        yield return new WaitUntil(() => networkManager.unitManager.WhiteUnits.Count >= 3);
        yield return new WaitUntil(() => networkManager.unitManager.RedUnits.Count >= 3);

        UpdateModels();

        fieldModel = new BotFieldModel(field);

        //if (unitModels.Count <= 0)
        //{
        unitModels = new List<BotUnitModel>();

        foreach (var unit in field.unitManager.WhiteUnits)
        {
            unitModels.Add(new BotUnitModel(unit, strategyModel));
        }

        strategyModel.UnitModels = unitModels;
        awarenessModel.Init(strategyModel.PlayerLeader.Allies);
        //}

        foreach (var unit in unitModels)
        {
            fieldModel.PutUnitModel(unit);
        }

        strategyModel.UpdatePlan();
        strategyModel.Refresh();

        isInitOver = true;
    }

    private IEnumerator RunMove()
    {
        yield return new WaitUntil(() => isInitOver);
        yield return new WaitUntil(() => unitModels.Count >= 3);

        actionManager.roundManager.SelectPlayerLocal(Team.White);

        foreach (var unit in unitModels)
        {
            unit.UpdateRoundState();
        }

        tacticsSelector.UpdateTactics();
        var tactic = tacticsSelector.GetBestTactic(9);

        Debug.Log(tactic != null ? $"_Bot got tactic: {tactic.Name}" : "_Bot moves by weight only");

        for (int i = 0; i < 3; i++)
        {
            List<BotAction> actionVariants = new List<BotAction>();

            TacticMove tacticMove = null;
            if (tactic != null)
                tacticMove = tactic.GetTacticMove(order);

            actionVariants.AddRange(GetAttackActions(order, tacticMove));
            actionVariants.AddRange(GetMoveActions(order, tacticMove));
            actionVariants.AddRange(GetRespawnActions(order, tacticMove));
            actionVariants.AddRange(GetAbilityActions(order, tacticMove));

            actions.Add(SelectAction(actionVariants));

            order++;

            actions[i].PushAction(actionManager, strategyModel);
        }


        ClearActions();
        CallPostMoveActions();

        isInitOver = false;
    }

    public IEnumerator RunMultiLinesCalculation(bool redActionsAssigned = false)
    {
        yield return new WaitUntil(() => isInitOver);
        yield return new WaitUntil(() => unitModels.Count >= 3);

        actionManager.roundManager.SelectPlayerLocal(Team.White);

        foreach (var unit in unitModels)
        {
            unit.UpdateRoundState();
        }

        tacticsSelector.UpdateTactics();
        var tactic = tacticsSelector.GetBestTactic(3);

        TacticMove[] tacticMoves = new TacticMove[3];
        if (tactic != null)
        {
            tacticMoves[0] = tactic.GetTacticMove(1);
            tacticMoves[1] = tactic.GetTacticMove(2);
            tacticMoves[2] = tactic.GetTacticMove(3);
        }

        messages = new string[2];

        linesFailed = 0;
        linesDone = 0;

        StartCoroutine(AnimateMoveCalculation());
        //variansGenerator = new BotVariansGenerator[2];


        List<UnitModel> whiteModels = new List<UnitModel>();
        List<UnitModel> redModels = new List<UnitModel>();

        foreach (var unit in networkManager.unitManager.WhiteUnits)
            whiteModels.Add(unit.UnitModel);
        foreach (var unit in networkManager.unitManager.RedUnits)
            redModels.Add(unit.UnitModel);

        //if (rounds <= 0)
        //{
        //    //tactic = new BotTacticModel();
        //    //tacticMoves = GetDebute(whiteModels);
        //}
        if (tacticMoves == null)
        {
            tactic = null;
        }

        Debug.Log("_Bot gen Started ========================");

        ShortFieldModel shortFieldModel = new ShortFieldModel(whiteModels, redModels, fieldModel.Models);

        var lines = rounds <= 0 ?
            MoveFilterCarousel.GetOpeningLines(new UnitModel[] { whiteModels[0], whiteModels[1], whiteModels[2] }, field) :
            MoveFilterCarousel.GetLinesFromUnits(new UnitModel[] { whiteModels[0], whiteModels[1], whiteModels[2] }, field);

        variansGenerator = new BotVariansGenerator[lines.Count];
        bestLines = new(ShortMoveModel[], int)[lines.Count];

        ShortMoveModel[] redMoves = null;

        if (redActionsAssigned)
        {   //Should be counted by single line
            redMoves = GetPlayerMoves(actionManager);
            variansGenerator = new BotVariansGenerator[1];
            bestLines = new (ShortMoveModel[], int)[1];
            if (lines.Count > 0)
                lines = new List<MoveFilter[]> { lines[0] };
            else 
                lines = new List<MoveFilter[]> { new MoveFilter[] { 
                    new MoveFilter(null, null),
                    new MoveFilter(null, null),
                    new MoveFilter(null, null)
                } };
        }

        for (int i = 0; i < lines.Count; i++)
        {
            variansGenerator[i] = new BotVariansGenerator();
        }

        tasks = new Task[lines.Count];

        //var semaphore = new SemaphoreSlim(16);

        for (int i = 0; i < lines.Count; i++)
        {
            int i_0 = i;
            bool surviveUntilReveal = false;
            if (RevealLocalUtil.Instance.IsSwitchable && redMoves == null)
                surviveUntilReveal = true;

            tasks[i] = Task.Run(//async 
                () =>
            {
                //await semaphore.WaitAsync();
                int index = i_0;
                bool saveAllCost = surviveUntilReveal;

                try
                {
                    if (lines[index].Length < 3) 
                    {
                        Debug.LogError("Filter lenght error");
                        Interlocked.Increment(ref linesFailed);
                        return;
                    }
                    else if (lines[index][0] == null || lines[index][1] == null || lines[index][2] == null)
                    {
                        Debug.LogError("Filter filling error");
                        Interlocked.Increment(ref linesFailed);
                        return;
                    }

                    variansGenerator[index].GenerateFilterLines(whiteModels, redModels, shortFieldModel, lines[index],
                        redMoves, saveAllCost);
                    if (variansGenerator[index].FieldsCount > 0)
                        bestLines[index] = variansGenerator[index].PickupBestLine(0, redActionsAssigned, saveAllCost);
                    else
                        Interlocked.Increment(ref linesFailed);

                    //var line = variansGenerator[index].GetBestLine(
                    //    whiteModels,
                    //    redModels,
                    //    shortFieldModel,
                    //    lines[index],
                    //    redMoves,
                    //    redActionsAssigned,
                    //    saveAllCost
                    //    );

                    //Debug.Log($"_bot picked value: {line.Item2}");

                    //if (variansGenerator[index].Failed == false)
                    //    bestLines[index] = line;
                    //else
                    //    Interlocked.Increment(ref linesFailed);


                }
                catch (Exception e)
                {
                    Interlocked.Increment(ref linesFailed);
                    Debug.LogError($"index = {index}; " +  e.ToString());
                }
                finally
                {
                    //semaphore.Release();
                    Interlocked.Increment(ref linesDone);
                }

                //Interlocked.Increment(ref linesDone);
            });
        }


        yield return new WaitUntil(() => linesDone >= lines.Count);

        (ShortMoveModel[], int) bestLine = new (null, int.MinValue);
        for (int i = 0; i < lines.Count; i++)
        {
            if (variansGenerator[i].Failed)
                continue;

            if (bestLines[i].Item2 > bestLine.Item2)
            {
                bestLine = bestLines[i];
            }
        }

        //var bestLine = variansGenerator[0].PickedLine;
        //if (variansGenerator[1].DiffScore > variansGenerator[0].DiffScore)
        //{
        //    bestLine = variansGenerator[1].PickedLine;
        //}

        ShortMoveModel[] finalLine = bestLine.Item1;

        if (finalLine == null)
        {
            Console.Log("No filter line passed");
            StartCoroutine(RunMove());
            yield break;
        }

        Debug.Log(
            $"_Bot line \n" +
            $"00={finalLine[0].Unit.Key} {finalLine[0].MoveType}: {finalLine[0].TileFrom.Key}-{finalLine[0].Tile.Key},\n" +
            $"01={finalLine[1].Unit.Key} {finalLine[1].MoveType}: {finalLine[1].TileFrom.Key}-{finalLine[1].Tile.Key},\n" +
            $"02={finalLine[2].Unit.Key} {finalLine[2].MoveType}: {finalLine[2].TileFrom.Key}-{finalLine[2].Tile.Key},\n" +
            $"03={finalLine[3].Unit.Key} {finalLine[3].MoveType}: {finalLine[3].TileFrom.Key}-{finalLine[3].Tile.Key},\n" +
            $"04={finalLine[4].Unit.Key} {finalLine[4].MoveType}: {finalLine[4].TileFrom.Key}-{finalLine[4].Tile.Key},\n" +
            $"05={finalLine[5].Unit.Key} {finalLine[5].MoveType}: {finalLine[5].TileFrom.Key}-{finalLine[5].Tile.Key}\n");

        BotVariansGenerator.Push(actionManager, bestLine.Item1);

        //yield return new WaitUntil(() => variansGenerator[0].Pushed);

        ClearActions();
        CallPostMoveActions(!redActionsAssigned);

        isInitOver = false;

        rounds++;
    }


    //TODO: place it somewhere else
    private bool HasToDefend(List<UnitModel> units)
    {
        bool result = false;

        foreach (var unit in units)
        {
            //Looking for leader safety
            if (unit.IsLeader)
            {
                if (unit.HasShield)
                    return false;

                foreach (var tile in unit.Position.GetEnemyTiles(unit))
                {
                    if (tile.UnitInTile.IsCharged)
                        result = true;
                }
            }

            //Looking for win in one move
            if (unit.IsCharged)
            {
                foreach (var tile in unit.Position.GetEnemyTiles(unit))
                {
                    if (tile.UnitInTile.Health <= 1)
                    {
                        if (tile.UnitInTile.HasShield)
                            continue;

                        if (tile.UnitInTile.Leader.Health <= 1)
                            result = false;
                    }
                }
            }
        }

        return result;
    }

    private bool HasToAttack(List<UnitModel> units)
    {
        bool result = false;

        foreach (var unit in units)
        {
            if (unit.IsCharged == false)
                continue;

            //Looking for leader safety, avoiding traids
            if (unit.IsLeader)
            {
                foreach (var tile in unit.Position.GetEnemyTiles(unit))
                {
                    if (tile.UnitInTile.IsCharged)
                    {
                        if (tile.UnitInTile.IsLeader)
                            result = true;
                        else
                            result = false;
                    }
                }
            }

            //Looking for win in one move
            foreach (var tile in unit.Position.GetEnemyTiles(unit))
            {
                if (tile.UnitInTile.Health <= 1)
                {
                    if (tile.UnitInTile.HasShield)
                        continue;

                    if (tile.UnitInTile.Leader.Health <= 1)
                        return true;

                    result = true;
                }
            }
        }

        return result;
    }

    private ShortMoveModel[] GetPlayerMoves(ActionManager actionManager)
    {
        ShortMoveModel[] shortMoves = new ShortMoveModel[3];

        for (int i = 0; i < shortMoves.Length; i++)
        {
            shortMoves[i] = new ShortMoveModel(actionManager.RedPlayerActionsList[i]);
        }

        return shortMoves;
    }

    private IEnumerator AnimateMoveCalculation()
    {
        int counter = 0;
        botScreen.SetActive(true);

        while (isInitOver)
        {
            string message = $"Bot is processing: {counter}";
            //message += $"\n {messages[0]}";
            //message += $"\n {messages[1]}";
            if (variansGenerator != null)
            {
                message += $"\n Lines done: {linesDone} / {variansGenerator.Length}";
                if (linesFailed > 0)
                    message += $"\n Lines failed: {linesFailed}";
            }

            counter += 1;

            botScreenText.text = message;

            if (counter >= 40)
                message = "timeOut";

            //CreateText.Flying(false, Message, networkManager.roundManager. textPrefab, 1f, Color.white);

            yield return new WaitForSeconds(1);
        }

        botScreen.SetActive(false);
    }

    public void MakeMove(bool tacticVariant = true)
    {
        StartCoroutine(InitFieldModel());

        bool hasToBeCalced = true;

        if (hasToBeCalced == false)
        {
            //StartCoroutine(RunMove());
        }
        else
        {
            //StartCoroutine(RunMoveCalculation());
            StartCoroutine(RunMultiLinesCalculation(actionManager.roundManager.InvertBotCall));
        }

        //rounds++;
    }

    private void CallPostMoveActions(bool switchToPlayer = true)
    {
        
        actionManager.roundManager.EndTurnLocal(Team.White);

        if (switchToPlayer)
        {
            actionManager.roundManager.SelectPlayerLocal(Team.Red);

            RevealLocalUtil.Instance.RevealAction();

            if (ConsoleScenarioPlayer.Instance != null)
            {
                if (!ConsoleScenarioPlayer.Instance.PlayBefore)
                    ConsoleScenarioPlayer.Instance.Play();
            }
        }
        else if (RevealLocalUtil.Instance.IsSwitchable)
        {
            actionManager.roundManager.InvertBotCall = false;

            EventCollector.Instance.Invoke(EventType.Reveal);
        }
    } 

    private void ClearActions()
    {
        actions.Clear();
        order = 1;

        fieldModel = null;
    }

    private BotAction SelectAction(List<BotAction> actions)
    {
        actions.Sort((x, y) => y.Weight.CompareTo(x.Weight));

        return actions[0];
    }

    private List<BotAction> GetAttackActions(int order, TacticMove tacticMove = null)
    {
        List<BotAction> attacksVariants = new List<BotAction>();

        List<BotTileModel> tileModels = fieldModel.GetAttackModels();

        foreach (var model in tileModels) 
        {
            List<Tile> tiles = model.GetTilesForAttack(model.UnitModel);
            foreach (var tile in tiles)
            {
                BotActionAttack botAction = new BotActionAttack(order);
                botAction.CalcWeight(fieldModel.GetTileModel(tile), model, fieldModel, strategyModel, tacticMove);

                attacksVariants.Add(botAction);
            }
        }

        return attacksVariants;
    }

    private List<BotAction> GetMoveActions(int order, TacticMove tacticMove = null)
    {
        List<BotAction> moveVariants = new List<BotAction>();

        List<BotTileModel> tileModels = fieldModel.GetMoveModels();

        foreach (var model in tileModels)
        {
            List<Tile> tiles = model.GetTilesForMove(fieldModel);
            foreach (var tile in tiles)
            {
                BotActionMove botAction = new BotActionMove(order);
                botAction.CalcWeight(fieldModel.GetTileModel(tile), model, fieldModel, strategyModel, tacticMove);

                moveVariants.Add(botAction);
            }
        }

        return moveVariants;
    }

    private List<BotAction> GetRespawnActions(int order, TacticMove tacticMove = null)
    {
        List<BotAction> respawnVariants = new List<BotAction>();

        List<BotTileModel> tileModels = fieldModel.GetMoveModels();

        foreach (var model in tileModels)
        {
            if (model.Ocupation != TileOcupation.Bot)
                continue;
            if (model.Unit.Locked)
                continue;

            if (model.UnitModel.Unit.IsLeader)
                continue;
            if (model.UnitModel.Unit.UnitType != UnitType.Default)
                continue;
            if (model.UnitModel.TriedToRespawn)
                continue;
            //if (model.UnitModel.IsCharged == false)
            //    continue;

            if (model.Unit.unitManager.GetSpawner(model.Unit.Name).HasTrueForm == false)
                continue;

            BotActionRespawn botAction = new BotActionRespawn(order);
            botAction.CalcWeight(model, model, fieldModel, strategyModel, tacticMove);
            
            respawnVariants.Add(botAction);
        }

        return respawnVariants;
    }

    private List<BotAction> GetAbilityActions(int order, TacticMove tacticMove = null)
    {
        List<BotAction> abilityVariants = new List<BotAction>();

        List<BotTileModel> tileModels = fieldModel.GetMoveModels();

        foreach (var model in tileModels)
        {
            if (model.Ocupation != TileOcupation.Bot)
                continue;

            if (model.Unit.Locked)
                continue;

            ICasterPersonality casterPersonality = model.UnitModel.UnitPersonality as ICasterPersonality; //TODO UNBOXING!!! Remove!
            if (casterPersonality == null)
                continue;

            if (model.UnitModel.IsCharged == false)
                continue;
            if (model.UnitModel.HasShield)
                continue;

            foreach (var tileTo in tileModels)
            {
                BotActionAbility botAction = new BotActionAbility(order);
                botAction.CalcWeight(tileTo, model, fieldModel, strategyModel, tacticMove);

                abilityVariants.Add(botAction);
            }
        }

        return abilityVariants;
    }

    public void SkipOpening()
    {
        rounds++;
    }
}
