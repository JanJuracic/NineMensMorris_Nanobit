using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettingsController : MonoBehaviour
{
    [Header("Mixer")]
    [SerializeField] AudioMixer audioMixer;

    [Header("Sliders")]
    [SerializeField] Slider musicVolumeSlider;
    [SerializeField] Slider sfxVolumeSlider;

    private void Start()
    {
        LoadVolumes();
        UpdateMusicVolume();
        UpdateSFXVolume();
    }

    public void UpdateMusicVolume()
    {
        float vol = musicVolumeSlider.value;
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(vol) * 20);
        PlayerPrefs.SetFloat("MusicVolume", vol);
        PlayerPrefs.Save();
    }

    public void UpdateSFXVolume()
    {
        float vol = sfxVolumeSlider.value;
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(vol) * 20);
        PlayerPrefs.SetFloat("SFXVolume", vol);
        PlayerPrefs.Save();
    }

    private void LoadVolumes()
    {
        float musicVol = PlayerPrefs.HasKey("MusicVolume") ? PlayerPrefs.GetFloat("MusicVolume") : 1f;
        float sfxVol = PlayerPrefs.HasKey("SFXVolume") ? PlayerPrefs.GetFloat("SFXVolume") : 1f;

        musicVolumeSlider.value = musicVol;
        sfxVolumeSlider.value = sfxVol;
    }
}
