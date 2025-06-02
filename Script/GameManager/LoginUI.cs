using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using GameData;
public class LoginUI : MonoBehaviour
{
    [SerializeField] private GameObject login_UI;
    [SerializeField] private GameObject logout_UI;
    [SerializeField] private Button logout_bnt;

    [Header("Login Panel")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private TMP_InputField loginEmailInput;
    [SerializeField] private TMP_InputField loginPasswordInput;
    [SerializeField] private Button loginButton;
    //[SerializeField] private Button forgotPasswordButton;
    [SerializeField] private Button switchToRegisterButton;

    [Header("Register Panel")]
    [SerializeField] private GameObject registerPanel;
    [SerializeField] private TMP_InputField registerEmailInput;
    [SerializeField] private TMP_InputField registerPasswordInput;
    [SerializeField] private TMP_InputField confirmPasswordInput;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button switchToLoginButton;

    [Header("Message")]
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("Setting")]
    [SerializeField] private Button DN_button;
    [SerializeField] private Button XH_button;

    [Header("References")]
    [SerializeField] private LeaderboardUI leaderboardUI;

    private void Start()
    {
        // Đăng ký sự kiện cho các nút
        loginButton.onClick.AddListener(OnLoginButtonClicked);
        registerButton.onClick.AddListener(OnRegisterButtonClicked);
        DN_button.onClick.AddListener(OnDNClick);
        XH_button.onClick.AddListener(OnXHClick);
        logout_bnt.onClick.AddListener(OnLogOutClick);
        //forgotPasswordButton.onClick.AddListener(OnForgotPasswordClicked);
        switchToRegisterButton.onClick.AddListener(() => SwitchToPanel(false));
        switchToLoginButton.onClick.AddListener(() => SwitchToPanel(true));

        // Tìm LeaderboardUI nếu chưa được gán
        if (leaderboardUI == null)
        {
            leaderboardUI = FindObjectOfType<LeaderboardUI>();
            if (leaderboardUI == null)
            {
                Debug.LogError("Không tìm thấy LeaderboardUI trong scene!");
            }
        }

        // Ẩn message panel
        messagePanel.SetActive(false);
        logout_UI.SetActive(false);
        // Mặc định hiển thị panel đăng nhập
        SwitchToPanel(true);
    }



    private async void OnLoginButtonClicked()
    {
        string email = loginEmailInput.text;
        string password = loginPasswordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowMessage("Vui lòng nhập đầy đủ thông tin!");
            return;
        }

        FirebaseAuthManager.Instance.SignIn(email, password, async (success, message) =>
        {
            ShowMessage(message);
            if (success)
            {
                try
                {
                    // Đảm bảo dữ liệu hiện tại được lưu trước khi chuyển scene
                    if (LevelManager.Instance != null)
                    {
                        int currentLevel = LevelManager.Instance.GetCurrentUnlockedLevel();
                        PlayerPrefs.SetInt("UnlockedLevel", currentLevel);
                        Debug.Log($"Đã lưu level hiện tại: {currentLevel}");
                    }

                    if (PlayerManager.instance != null)
                    {
                        float currentCoin = PlayerManager.instance.GetCurrentCoin();
                        PlayerPrefs.SetFloat("CurrentCoin", currentCoin);
                        Debug.Log($"Đã lưu coin hiện tại: {currentCoin}");
                    }

                    PlayerPrefs.Save();

                    // Đợi PlayerDataManager load và đồng bộ dữ liệu
                    if (PlayerDataManager.Instance != null)
                    {
                        await PlayerDataManager.Instance.LoadPlayerData();
                        Debug.Log("Đã load dữ liệu từ Firebase sau đăng nhập");
                    }

                    // Ẩn các UI panel
                    loginPanel.SetActive(false);
                    login_UI.SetActive(false);

                    Debug.Log("Đăng nhập thành công và đã đồng bộ dữ liệu");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Lỗi khi xử lý đăng nhập: {ex.Message}");
                    ShowMessage("Có lỗi xảy ra khi đồng bộ dữ liệu!");
                }
            }
        });
    }

    private void OnRegisterButtonClicked()
    {
        string email = registerEmailInput.text;
        string password = registerPasswordInput.text;
        string confirmPassword = confirmPasswordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
        {
            ShowMessage("Vui lòng nhập đầy đủ thông tin!");
            return;
        }

        if (password != confirmPassword)
        {
            ShowMessage("Mật khẩu không khớp!");
            return;
        }

        FirebaseAuthManager.Instance.SignUp(email, password, (success, message) =>
        {
            ShowMessage(message);
            if (success)
            {
                // Đăng ký thành công, chuyển sang panel đăng nhập
                SwitchToPanel(true);
            }
        });
    }

    // private void OnForgotPasswordClicked()
    // {
    //     string email = loginEmailInput.text;

    //     if (string.IsNullOrEmpty(email))
    //     {
    //         ShowMessage("Vui lòng nhập email!");
    //         return;
    //     }

    //     FirebaseAuthManager.Instance.ResetPassword(email, (success, message) =>
    //     {
    //         ShowMessage(message);
    //     });
    //}

    private void SwitchToPanel(bool showLogin)
    {
        loginPanel.SetActive(showLogin);
        registerPanel.SetActive(!showLogin);
    }

    private void ShowMessage(string message)
    {
        messageText.text = message;
        messagePanel.SetActive(true);
        Invoke(nameof(HideMessage), 3f);
    }

    private void HideMessage()
    {
        messagePanel.SetActive(false);
    }

    private void OnDNClick()
    {
        if(PlayerDataManager.Instance.IsSignedIn())
        {
            login_UI.SetActive(false);
            logout_UI.SetActive(true);
        }    
        else
        {
            login_UI.SetActive(true);
            logout_UI.SetActive(false);
        }    
    }    


    private void OnLogOutClick()
    {
        FirebaseAuthManager.Instance.SignOut();
        logout_UI?.SetActive(false);
    }    

    private void OnXHClick()
    {
        if (leaderboardUI != null)
        {
            leaderboardUI.ShowLeaderboard();
        }
        else
        {
            Debug.LogError("LeaderboardUI chưa được gán!");
            ShowMessage("Không thể hiển thị bảng xếp hạng!");
        }
    }    
} 