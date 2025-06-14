using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BotTacticsSelector: MonoBehaviour
{
    private BotStrategyModel strategyModel;

    [SerializeField] private List<BotTacticModel> tacticModels;

    public List<BotTacticModel> TacticModels { get { return tacticModels; } }

    public void SetStrategyModel(BotStrategyModel strategyModel)
    {
        this.strategyModel = strategyModel;
    }

    public void UpdateTactics()
    {
        foreach (BotTacticModel tacticModel in tacticModels) 
        {
            tacticModel.Init(strategyModel);
        }
    }

    public BotTacticModel GetBestTactic()
    {
        tacticModels.Sort((x, y) => y.Weight.CompareTo(x.Weight));

        foreach (BotTacticModel botTacticModel in tacticModels)
        {
            if (botTacticModel.IsPosible)
            {
                return botTacticModel;
            }
        }

        return null;
    }

    public BotTacticModel GetBestTactic(int bestOf)
    {
        tacticModels.Sort((x, y) => y.Weight.CompareTo(x.Weight));
        List<BotTacticModel> selection = new List<BotTacticModel>();

        foreach (BotTacticModel botTacticModel in tacticModels)
        {
            if (botTacticModel.IsPosible)
                selection.Add(botTacticModel);
        }

        if (selection.Count == 0)
            return null;

        int limit = bestOf >= selection.Count ? selection.Count : bestOf;
        int rnd = Random.Range(0, limit);

        if (rnd > 0)
        {
            if (selection[rnd].Weight == 0)
            {
                if (selection[0].Weight == 0)
                    return null;
                else
                    return selection[0];
            }

            if (selection[0].Weight - selection[rnd].Weight >= 15)
                return selection[0];
        }

        return selection[rnd];
    }

    public void InintLevelTactics()
    {
        int levelId = LevelTeam.GetIdFromPrefs();

        TacticsCollection tacticsCollection = TacticsCollection.GetCollection(levelId);
        tacticModels = tacticsCollection.Tactics;
    }
}
