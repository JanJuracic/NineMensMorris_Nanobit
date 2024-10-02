using UnityEngine;

namespace NineMensMorris
{
    [CreateAssetMenu(fileName = "Player", menuName = "Players/Player")]
    public class PlayerData : ScriptableObject
    {

        [Header("Player Defaults")]
        [SerializeField] string defaultName;
        [SerializeField] int defaultColorIndex;

        [Header("Player Details")]
        [SerializeField] string playerName;
        [SerializeField] Color color;
        [SerializeField] int colorIndex;

        [Header("PlayerPreference Settings")]
        [SerializeField] string ID;

        public string Name => playerName;
        public Color Color => color;
        public int ColorIndex => colorIndex;
        public PlayerTokensManager TokenManager { get; set; }
        public PlayerInfoDisplayer PlayerInfo { get; set; }

        public void LoadPlayerDataFromPreferences()
        {
            playerName = PlayerPrefs.HasKey(ID + "Name") ? PlayerPrefs.GetString(ID + "Name") : defaultName;
            colorIndex = PlayerPrefs.HasKey(ID + "Color") ? PlayerPrefs.GetInt(ID + "Color") : defaultColorIndex;
        }

        public void UpdateName(string newName)
        {
            playerName = newName;
            PlayerPrefs.SetString(ID + "Name", playerName);
            PlayerPrefs.Save();
        }

        public void UpdateColor(Color newColor, int colorIndex)
        {
            color = newColor;
            PlayerPrefs.SetInt(ID + "Color", colorIndex);
            PlayerPrefs.Save();
        }

        public override string ToString()
        {
            string hexColor = ColorUtility.ToHtmlStringRGB(color);
            return $"<color=#{hexColor}>{playerName}</color>";
        }
    }
}



