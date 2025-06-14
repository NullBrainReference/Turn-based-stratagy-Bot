using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticsCreator : MonoBehaviour
{
    [SerializeField] private TacticsCollection tacticsCollection;

#if UNITY_EDITOR
    public void SaveCollection()
    {
        tacticsCollection.SaveToFIle();
    }
#endif

    public void LoadCollection()
    {
        tacticsCollection = TacticsCollection.GetCollection(tacticsCollection.Id);
    }
}
