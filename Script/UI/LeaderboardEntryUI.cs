using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardEntryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Image background;

    [Header("Colors")]
    [SerializeField] private Color firstPlaceColor = new Color(1f, 0.84f, 0f); // Vàng
    [SerializeField] private Color secondPlaceColor = new Color(0.75f, 0.75f, 0.75f); // Bạc
    [SerializeField] private Color thirdPlaceColor = new Color(0.8f, 0.5f, 0.2f); // Đồng
    [SerializeField] private Color normalColor = Color.white;

    private void Awake()
    {
        ValidateComponents();
    }

    private void ValidateComponents()
    {
        if (rankText == null)
            Debug.LogError($"[{gameObject.name}] rankText is missing!");
        if (nameText == null)
            Debug.LogError($"[{gameObject.name}] nameText is missing!");
        if (scoreText == null)
            Debug.LogError($"[{gameObject.name}] scoreText is missing!");
        if (background == null)
            Debug.LogError($"[{gameObject.name}] background is missing!");
    }

    public void SetData(int rank, string playerName, int score)
    {
        Debug.Log($"Setting data - Rank: {rank}, Name: {playerName}, Score: {score}");
        
        if (rankText != null)
        {
            rankText.text = $"#{rank}";
            Debug.Log($"Set rank text: {rankText.text}");
        }

        if (nameText != null)
        {
            nameText.text = playerName;
            Debug.Log($"Set name text: {nameText.text}");
        }

        if (scoreText != null)
        {
            scoreText.text = score.ToString("N0");
            Debug.Log($"Set score text: {scoreText.text}");
        }

        if (background != null)
        {
            Color newColor = rank switch
            {
                1 => firstPlaceColor,
                2 => secondPlaceColor,
                3 => thirdPlaceColor,
                _ => normalColor
            };
            background.color = newColor;
            Debug.Log($"Set background color for rank {rank}");
        }
    }
} 