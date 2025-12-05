using UnityEngine;

[System.Serializable]
public class QuestItemData
{
    public string questID;
    public string displayName;
    public string prerequisiteID;
}

public enum QuestDisplayMode
{
    AllAtOnce,
    Sequential
}
