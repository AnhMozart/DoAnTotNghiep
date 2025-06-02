using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using GameData;

public class LevelManager : MonoBehaviour
{
    private static LevelManager instance;
    public static LevelManager Instance => instance;

    [SerializeField] private List<Button> levelButtons;
    [SerializeField] private int currentUnlockedLevel = 1;
    private const string UNLOCKED_LEVEL_KEY = "UnlockedLevel";
    private const string LEVEL_SCENE_PREFIX = "Level";
    private bool isMainMenu = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadUnlockedLevels();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        isMainMenu = SceneManager.GetActiveScene().name == "MainMenu";
        if (isMainMenu)
        {
            FindLevelButtons();
            AddEventButton();
            UpdateLevelButtons();
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isMainMenu = scene.name == "MainMenu";
        if (isMainMenu)
        {
            // Reset lại danh sách button
            levelButtons = new List<Button>();
            // Tìm lại các button level trong scene MainMenu
            FindLevelButtons();
            AddEventButton();
            UpdateLevelButtons();
        }
    }

    private void FindLevelButtons()
    {
        if (!isMainMenu) return;

        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        
        foreach (GameObject rootObject in rootObjects)
        {
            Button[] buttons = rootObject.GetComponentsInChildren<Button>(true);
            foreach (Button button in buttons)
            {
                if (button.name.Contains("Level"))
                {
                    levelButtons.Add(button);
                }
            }
        }
    }

    private void AddEventButton()
    {
        if (!isMainMenu || levelButtons == null || levelButtons.Count == 0) return;

        for (int i = 0; i < levelButtons.Count; i++)
        {
            int levelNumber = i + 1; // Tạo biến cục bộ để tránh closure
            levelButtons[i].onClick.RemoveAllListeners();
            levelButtons[i].onClick.AddListener(() => PlayLevel(levelNumber));
        }
    }

    private void LoadUnlockedLevels()
    {
        currentUnlockedLevel = PlayerPrefs.GetInt(UNLOCKED_LEVEL_KEY, 1);
    }

    private void SaveUnlockedLevels()
    {
        PlayerPrefs.SetInt(UNLOCKED_LEVEL_KEY, currentUnlockedLevel);
        PlayerPrefs.Save();
    }

    private void UpdateLevelButtons()
    {
        if (!isMainMenu) return;
        if (levelButtons == null || levelButtons.Count == 0) return;

        for (int i = 0; i < levelButtons.Count; i++)
        {
            if (levelButtons[i] == null) continue;

            if (i + 1 <= currentUnlockedLevel)
            {
                levelButtons[i].interactable = true;
            }
            else
            {
                levelButtons[i].interactable = false;
            }
        }
    }

    public void SetUnlockedLevel(int level)
    {
        currentUnlockedLevel = level;
        SaveUnlockedLevels();
        if (isMainMenu)
        {
            UpdateLevelButtons();
        }
    }

    public void CompleteLevel(int levelNumber)
    {
        if (levelNumber == currentUnlockedLevel)
        {
            currentUnlockedLevel++;
            SaveUnlockedLevels();
            if (isMainMenu)
            {
                UpdateLevelButtons();
            }
            // Lưu lên Firebase
            if (PlayerDataManager.Instance != null)
            {
                _ = PlayerDataManager.Instance.SavePlayerData();
            }
        }
    }

    public void PlayLevel(int levelNumber)
    {
        if (levelNumber <= currentUnlockedLevel)
        {
            SceneManager.LoadScene(LEVEL_SCENE_PREFIX + levelNumber);
        }
    }

    public int GetCurrentUnlockedLevel()
    {
        return currentUnlockedLevel;
    }
} 