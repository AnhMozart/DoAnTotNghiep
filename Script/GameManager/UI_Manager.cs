using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class UI_Manager : MonoBehaviour
{
    private static UI_Manager instance;
    public static UI_Manager Instance => instance;

    [SerializeField] private GameObject playGame;
    [SerializeField] private GameObject level;
    [SerializeField] private GameObject DH_Panel;
    [SerializeField] private GameObject pause_Game;
    [SerializeField] private GameObject login_UI;
    [SerializeField] private GameObject logout_UI;

    // Button PauseGame
    private Button pauseButton;
    private Button restartButton;
    private Button homeButton;
    private Button resumeButton;
    // Button MainMenu
    [SerializeField] private Button playGameButton;
    [SerializeField] private Button DHButton;
    [SerializeField] private Button[] hideButtons;
    [SerializeField] private Button exitButton;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        FindAllPanels();
        InitializeUI();
        FindAndSetupButtons();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Time.timeScale = 1f;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            // Reset các tham chiếu
            playGame = null;
            level = null;
            DH_Panel = null;
            
            // Tìm lại các panel và button
            FindAllPanels();
            if (playGame != null && level != null && DH_Panel != null)
            {
                InitializeUI();
                FindAndSetupMainMenuButtons();
            }
        }
        else
        {
            FindPauseGame();
            FindAndSetupButtons();
        }
    }

    private void FindAllPanels()
    {
        // Tìm tất cả GameObject trong scene MainMenu
        Scene mainMenuScene = SceneManager.GetSceneByName("MainMenu");
        if (!mainMenuScene.IsValid()) return;

        GameObject[] rootObjects = mainMenuScene.GetRootGameObjects();
        foreach (GameObject rootObject in rootObjects)
        {
            // Tìm Canvas
            Canvas canvas = rootObject.GetComponent<Canvas>();
            if (canvas != null)
            {
                // Tìm các panel trong Canvas
                foreach (Transform child in canvas.transform)
                {
                    if (child.name == "GameMenu")
                    {
                        playGame = child.gameObject;
                    }
                    else if (child.name == "Level")
                    {
                        level = child.gameObject;
                    }
                    else if (child.name == "DH_Panel")
                    {
                        DH_Panel = child.gameObject;
                    }
                    else if(child.name == "Login_UI")
                    {
                        login_UI = child.gameObject;
                    }    
                    else if(child.name == "Logout_UI")
                    {
                        logout_UI = child.gameObject;
                    }
                }
                break;
            }
        }
    }

    private void FindPauseGame()
    {
        // Tìm tất cả GameObject trong scene, kể cả những cái đang inactive
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "Pause_Game" && obj.scene == SceneManager.GetActiveScene())
            {
                pause_Game = obj;
                break;
            }
        }
    }

    private void FindAndSetupMainMenuButtons()
    {
        if (playGame == null || level == null || DH_Panel == null)
        {
            Debug.LogError("Không tìm thấy các panel trong MainMenu!");
            return;
        }

        // Tìm các button trong GameMenu
        Button[] gameMenuButtons = playGame.GetComponentsInChildren<Button>(true);
        foreach (Button button in gameMenuButtons)
        {
            if (button.name == "PlayGame")
            {
                playGameButton = button;
                playGameButton.onClick.RemoveAllListeners();
                playGameButton.onClick.AddListener(PlayGame);
            }
            else if (button.name == "DH")
            {
                DHButton = button;
                DHButton.onClick.RemoveAllListeners();
                DHButton.onClick.AddListener(DH);
            }
            else if (button.name == "Exit")
            {
                exitButton = button;
                exitButton.onClick.RemoveAllListeners();
                exitButton.onClick.AddListener(QuitGame);
            }
        }

        // Tìm các button Hide trong các panel
        List<Button> hideButtonList = new List<Button>();
        
        // Tìm trong Level panel
        Button[] levelButtons = level.GetComponentsInChildren<Button>(true);
        foreach (Button button in levelButtons)
        {
            if (button.name == "Hide")
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(Hide);
                hideButtonList.Add(button);
            }
        }

        // Tìm trong DH_Panel
        Button[] dhButtons = DH_Panel.GetComponentsInChildren<Button>(true);
        foreach (Button button in dhButtons)
        {
            if (button.name == "Hide")
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(Hide);
                hideButtonList.Add(button);
            }
        }

        Button[] loginButton = login_UI.GetComponentsInChildren<Button>(true);
        foreach (Button button in loginButton)
        {
            if(button.name == "Hide")
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(Hide);
                hideButtonList.Add(button);
            }    
        }


        Button[] logOutButton = logout_UI.GetComponentsInChildren<Button>(true);
        foreach (Button button in logOutButton)
        {
            if(button.name == "Hide")
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(Hide);
                hideButtonList.Add(button);
            }    
        }    

        hideButtons = hideButtonList.ToArray();
    }

    private void FindAndSetupButtons()
    {
        // Tìm tất cả GameObject trong scene
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.scene != SceneManager.GetActiveScene()) continue;

            // Tìm button Pause
            if (obj.name == "Pause")
            {
                pauseButton = obj.GetComponent<Button>();
                if (pauseButton != null)
                {
                    pauseButton.onClick.RemoveAllListeners();
                    pauseButton.onClick.AddListener(PauseGame);
                }
            }
            // Tìm các button trong pause_Game
            else if (pause_Game != null && obj.transform.parent == pause_Game.transform)
            {
                if (obj.name == "Resume")
                {
                    resumeButton = obj.GetComponent<Button>();
                    if (resumeButton != null)
                    {
                        resumeButton.onClick.RemoveAllListeners();
                        resumeButton.onClick.AddListener(Resume);
                    }
                }
                else if (obj.name == "Restart")
                {
                    restartButton = obj.GetComponent<Button>();
                    if (restartButton != null)
                    {
                        restartButton.onClick.RemoveAllListeners();
                        restartButton.onClick.AddListener(Restart);
                    }
                }
                else if (obj.name == "Home")
                {
                    homeButton = obj.GetComponent<Button>();
                    if (homeButton != null)
                    {
                        homeButton.onClick.RemoveAllListeners();
                        homeButton.onClick.AddListener(Home);
                    }
                }
            }
        }
    }

    private void InitializeUI()
    {
        // Kiểm tra và khởi tạo các panel
        if (pause_Game != null) pause_Game.SetActive(false);
        if (level != null) level.SetActive(false);
        if (DH_Panel != null) DH_Panel.SetActive(false);
        if (playGame != null) playGame.SetActive(true);
    }

    public void PlayGame()
    {
        if (playGame == null || level == null) return;
        
        playGame.SetActive(false);
        level.SetActive(true);
    }    

    public void DH()
    {
        if (DH_Panel == null || playGame == null) return;
        
        DH_Panel.SetActive(true);
        playGame.SetActive(false);
    }    

    public void Hide()
    {
        if (level != null)
        {
            level.SetActive(false);
            if (playGame != null) playGame.SetActive(true);
        }
        
        if (DH_Panel != null)
        {
            DH_Panel.SetActive(false);
            if (playGame != null) playGame.SetActive(true);
        }

        if(login_UI != null)
        {
            login_UI.SetActive(false);
            if(playGame != null) playGame.SetActive(true);
        }    

        if(logout_UI != null)
        {
            logout_UI.SetActive(false);
            if (playGame != null) playGame.SetActive(true);
        }    
    }


    public void PauseGame()
    {
        if (pause_Game == null) return;
        
        Time.timeScale = 0f;
        pause_Game.SetActive(true);
    }    


    public void Home()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }    

    public void Resume()
    {
        if (pause_Game == null) return;
        
        Time.timeScale = 1f;
        pause_Game.SetActive(false);

    }   

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }    


    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Thoát Game thành công");
    }    
    
}
