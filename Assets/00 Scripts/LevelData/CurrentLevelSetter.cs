using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CurrentLevelSetter : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] TMP_Dropdown dropdown;

    [Header("Data")]
    [SerializeField] LevelDataContainer levelContainer;

    private void Awake()
    {
        PopulateDropdownWithLevelNames();
        SetNewCurrentLevel(0);
    }

    private void PopulateDropdownWithLevelNames()
    {
        dropdown.options.Clear();

        List<string> names = levelContainer.GetLevelDataNames();
        foreach (string name in names)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(name));
        }
    }

    public void SetNewCurrentLevel(int index)
    {
        levelContainer.SetCurrentLevelData(index);
    }

}
