using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class SliderController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] Slider slider;
    [SerializeField] TextMeshProUGUI currentValueDisplay;
    [SerializeField] TextMeshProUGUI minValueDisplay;
    [SerializeField] TextMeshProUGUI maxValueDisplay;

    [Header("UnityEvents")]
    public UnityEvent<int> OnValueChanged;

    private void Awake()
    {
        currentValueDisplay.text = slider.value.ToString();
        minValueDisplay.text = slider.minValue.ToString();
        maxValueDisplay.text = slider.maxValue.ToString();
    }

    public void SetValue(float value)
    {
        int intValue = (int)value;
        currentValueDisplay.text = intValue.ToString();
        OnValueChanged.Invoke(intValue);
    }

    public int GetSliderValue()
    {
        return (int)slider.value;
    }

    public void SetSliderMin(int min)
    {
        slider.minValue = min;
        if (slider.maxValue < min) slider.maxValue = min;
        minValueDisplay.text = slider.minValue.ToString();
        maxValueDisplay.text = slider.maxValue.ToString();
        currentValueDisplay.text = slider.value.ToString();
    }
    public void SetSliderMax(int max)
    {
        slider.maxValue = max;
        if (slider.minValue > max) slider.minValue = max;
        minValueDisplay.text = slider.minValue.ToString();
        maxValueDisplay.text = slider.maxValue.ToString();
        currentValueDisplay.text = slider.value.ToString();
    }
}
