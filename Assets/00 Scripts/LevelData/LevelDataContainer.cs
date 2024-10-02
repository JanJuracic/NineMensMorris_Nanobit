using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "BoardAndRules/Container", fileName = "LevelDataContainer")]
public class LevelDataContainer : ScriptableObject
{
    [SerializeField] LevelData currentLevel;
    [SerializeField] List<LevelData> levelDatas;

    public LevelData CurrentLevel => currentLevel;

    public List<string> GetLevelDataNames()
    {
        List<string> result = new();
        foreach (var levelData in levelDatas)
        {
            result.Add(levelData.LevelName);
        }
        return result;
    }

    public void SetCurrentLevelData(int index)
    {
        currentLevel = levelDatas[index];
    }
}
