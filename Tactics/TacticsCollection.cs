using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using System.IO;
#endif

using UnityEngine;

[System.Serializable]
public class TacticsCollection
{
    [SerializeField] private List<BotTacticModel> tactics;
    [SerializeField] private int id;

    public List<BotTacticModel> Tactics => tactics;
    public int Id => id;

    public void SetTactics(List<BotTacticModel> tacticModels)
    {
        tactics = tacticModels;
    }

#if UNITY_EDITOR
    public void SaveToFIle()
    {
        string collectionString = JsonUtility.ToJson(this);

        string path = Application.dataPath + @"/Resources/Levels/tactics_" + id + ".txt";

        Debug.Log(path);

        using (StreamWriter sw = new StreamWriter(path, false))
        {
            sw.WriteLine(collectionString);
        }
    }

    public void SaveToFIle(int id)
    {
        this.id = id;

        string collectionString = JsonUtility.ToJson(this);

        string path = Application.dataPath + @"/Resources/Levels/tactics_" + id + ".txt";

        Debug.Log(path);

        using (StreamWriter sw = new StreamWriter(path, false))
        {
            sw.WriteLine(collectionString);
        }
    }
#endif

    public static TacticsCollection GetCollection(int id)
    {
        TacticsCollection collection = new TacticsCollection();

        var textFile = Resources.Load<TextAsset>(@"Levels\tactics_" + id.ToString());

        collection = JsonUtility.FromJson<TacticsCollection>(textFile.text);

        return collection;
    }
}
