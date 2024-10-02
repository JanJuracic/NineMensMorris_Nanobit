using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

public class LevelDataController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] LevelData data;

    [Header("Components")]
    [SerializeField] Toggle toggleDiagonals;
    [SerializeField] Toggle toggleCenterNode;
    [SerializeField] SliderController sliderRingNum;
    [SerializeField] SliderController sliderMillTokens;
    [SerializeField] SliderController sliderTokensPerPlayer;
    [SerializeField] SliderController sliderTokensForFlying;

    public void SetDiagonals(bool isOn)
    {
        UpdateData();
    }

    public void SetCenterNode(bool isOn)
    {
        int rings = sliderRingNum.GetSliderValue();
        int longestLineAcrossRings = isOn ? (rings * 2) + 1 : rings;
        int longestLine = Mathf.Max(3, longestLineAcrossRings);
        sliderMillTokens.SetSliderMax(longestLine);

        //Do not allow more tokens than nodes;
        int totalNodeNumber = toggleCenterNode.isOn ? (rings * 8) + 1 : rings * 8;
        int tokensPerPlayerMax = totalNodeNumber / 2;
        sliderTokensPerPlayer.SetSliderMax(tokensPerPlayerMax);

        UpdateData();
    }

    public void SetRings(int rings)
    {
        int longestLineAcrossRings = toggleCenterNode.isOn ? (rings * 2) + 1 : rings;
        int longestLine = Mathf.Max(3, longestLineAcrossRings);
        sliderMillTokens.SetSliderMax(longestLine);

        //Do not allow more tokens than nodes;
        int totalNodeNumber = toggleCenterNode.isOn ? (rings * 8) + 1 : rings * 8;
        int tokensPerPlayerMax = totalNodeNumber / 2;
        sliderTokensPerPlayer.SetSliderMax(tokensPerPlayerMax);

        UpdateData();
    }

    public void SetTokensForMill(int tokensMill)
    {
        sliderTokensPerPlayer.SetSliderMin(tokensMill);
        UpdateData();
    }

    public void SetTokensPerPlayer(int tokensTotal)
    {
        sliderTokensForFlying.SetSliderMax(tokensTotal);
        UpdateData();
    }

    public void SetTokensForFlying(int tokensFlying)
    {
        UpdateData();
    }

    public void UpdateData()
    {
        bool diagonals = toggleDiagonals.isOn;
        bool center = toggleCenterNode.isOn;
        int rings = sliderRingNum.GetSliderValue();
        int tokensMill = sliderMillTokens.GetSliderValue();
        int tokensTotal = sliderTokensPerPlayer.GetSliderValue();
        int tokensFlying = sliderTokensForFlying.GetSliderValue();

        data.Setup(tokensMill, tokensTotal, tokensFlying, rings, diagonals, center);
    }
}
