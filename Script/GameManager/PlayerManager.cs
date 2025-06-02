using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GameData;
using System.Threading.Tasks;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    public Player player;

    [Header("UI References")]
    [SerializeField] private Text textHP;
    [SerializeField] private Text CoinText;

    [Header("Player Data")]
    [SerializeField] private float currentCoin;
    [SerializeField] private int currentLevel = 1;
    private const string CoinKey = "CurrentCoin";
    private const string LevelKey = "CurrentLevel";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async void Start()
    {
        await LoadData();
        FindReferences();
        UpdateUI();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private async void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!scene.name.Contains("MainMenu"))
        {
            Debug.Log($"Scene mới được load: {scene.name}");
            FindReferences();
            await LoadData(); // Load lại dữ liệu khi chuyển scene
            UpdateUI();
        }
    }

    private async Task LoadData()
    {
        try
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.IsSignedIn())
            {
                Debug.Log("Đang tải dữ liệu từ Firebase...");
                // Load dữ liệu từ Firebase
                await PlayerDataManager.Instance.LoadPlayerData();
                
                // Lấy coin và level từ PlayerDataManager
                float firebaseCoin = PlayerDataManager.Instance.GetCachedCoin();
                int firebaseLevel = PlayerDataManager.Instance.GetCachedUnlockedLevel();
                
                Debug.Log($"Dữ liệu từ Firebase: Coin={firebaseCoin}, Level={firebaseLevel}");
                
                // Load dữ liệu local
                float localCoin = PlayerPrefs.GetFloat(CoinKey, 0);
                int localLevel = PlayerPrefs.GetInt(LevelKey, 1);
                
                Debug.Log($"Dữ liệu local: Coin={localCoin}, Level={localLevel}");
                
                // Sử dụng giá trị lớn nhất giữa Firebase và local
                currentCoin = Mathf.Max(firebaseCoin, localCoin);
                currentLevel = Mathf.Max(firebaseLevel, localLevel);
                
                Debug.Log($"Dữ liệu sau khi merge: Coin={currentCoin}, Level={currentLevel}");
                
                // Nếu dữ liệu local lớn hơn, cập nhật lên Firebase
                if (localCoin > firebaseCoin || localLevel > firebaseLevel)
                {
                    await PlayerDataManager.Instance.SavePlayerData();
                    Debug.Log("Đã cập nhật dữ liệu lên Firebase");
                }
            }
            else
            {
                // Nếu chưa đăng nhập, load từ PlayerPrefs
                currentCoin = PlayerPrefs.GetFloat(CoinKey, 0);
                currentLevel = PlayerPrefs.GetInt(LevelKey, 1);
                Debug.Log($"Chưa đăng nhập, load từ PlayerPrefs: Coin={currentCoin}, Level={currentLevel}");
            }
            
            // Cập nhật LevelManager
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.SetUnlockedLevel(currentLevel);
                Debug.Log($"Đã cập nhật level trong LevelManager: {currentLevel}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Lỗi khi load dữ liệu: {ex.Message}");
            // Nếu có lỗi, load từ PlayerPrefs
            currentCoin = PlayerPrefs.GetFloat(CoinKey, 0);
            currentLevel = PlayerPrefs.GetInt(LevelKey, 1);
        }
    }

    private void FindReferences()
    {
        try
        {
            // Tìm Player
            if (player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    player = playerObj.GetComponent<Player>();
                    Debug.Log("Đã tìm thấy Player");
                }
            }

        // Tìm UI Text
        if (textHP == null)
        {
            GameObject textHPObj = GameObject.Find("Text_HP");
            if (textHPObj != null)
            {
                textHP = textHPObj.GetComponent<Text>();
                if (textHP != null) Debug.Log("Đã tìm thấy HP Text");
            }
        }

        if (CoinText == null)
        {
            GameObject coinTextObj = GameObject.Find("Text_coin");
            if (coinTextObj != null)
            {
                CoinText = coinTextObj.GetComponent<Text>();
                if (CoinText != null) Debug.Log("Đã tìm thấy Coin Text");
            }
        }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Lỗi khi tìm references: {ex.Message}");
        }
    }

    private void UpdateUI()
    {
        try
        {
            UpdateCoin();
            UpdateHP();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Lỗi khi update UI: {ex.Message}");
        }
    }

    private void UpdateHP()
    {
        if (textHP != null && player != null)
        {
            float HP = player.GetCurentHp();
            textHP.text ="X" + HP.ToString();
        }
    }

    private void ResetData()
    {
        currentCoin = 0;
        currentLevel = 1;
        SaveData();
        
        // Cập nhật LevelManager
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.SetUnlockedLevel(1);
        }
        
        UpdateUI();
        Debug.Log("Đã reset dữ liệu");
    }

    #region Public Methods
    public void SetCoin(float coin)
    {
        currentCoin = coin;
        SaveData();
        UpdateCoin();
        // Cập nhật lên Firebase
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.IsSignedIn())
        {
            _ = PlayerDataManager.Instance.SavePlayerData();
            Debug.Log($"Đã gửi yêu cầu cập nhật coin lên Firebase: {coin}");
        }
    }

    public void AddCoin(float amount)
    {
        currentCoin += amount;
        SaveData();
        UpdateCoin();
        // Cập nhật lên Firebase
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.IsSignedIn())
        {
            _ = PlayerDataManager.Instance.SavePlayerData();
            Debug.Log($"Đã gửi yêu cầu cập nhật coin lên Firebase sau khi thêm {amount}, tổng: {currentCoin}");
        }
    }

    public void MinusCoin(float amount)
    {
        currentCoin = Mathf.Max(0, currentCoin - amount);
        SaveData();
        UpdateCoin();
        // Cập nhật lên Firebase
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.IsSignedIn())
        {
            _ = PlayerDataManager.Instance.SavePlayerData();
            Debug.Log($"Đã gửi yêu cầu cập nhật coin lên Firebase sau khi trừ {amount}, còn lại: {currentCoin}");
        }
    }

    public float GetCurrentCoin()
    {
        return currentCoin;
    }

    public Text GetHPText()
    {
        if (textHP == null) FindReferences();
        return textHP;
    }

    public Text TextHP()
    {
        if (textHP == null) FindReferences();
        return textHP;
    }

    public async void HandleLogin()
    {
        Debug.Log("Bắt đầu xử lý đăng nhập...");
        await LoadData();
        UpdateUI();
        Debug.Log($"Hoàn tất xử lý đăng nhập. Coin hiện tại: {currentCoin}");
    }

    public void HandleLogout()
    {
        ResetData(); // Reset dữ liệu khi đăng xuất
    }

    public void SetLevel(int level)
    {
        currentLevel = level;
        SaveData();
        
        // Cập nhật LevelManager
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.SetUnlockedLevel(level);
        }
        
        // Cập nhật lên Firebase
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.IsSignedIn())
        {
            _ = PlayerDataManager.Instance.SavePlayerData();
            Debug.Log($"Đã gửi yêu cầu cập nhật level lên Firebase: {level}");
        }
    }

    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    #endregion

    #region Private Methods
    private void SaveData()
    {
        // Lưu coin
        PlayerPrefs.SetFloat(CoinKey, currentCoin);
        // Lưu level
        PlayerPrefs.SetInt(LevelKey, currentLevel);
        PlayerPrefs.Save();
        Debug.Log($"Đã lưu dữ liệu vào PlayerPrefs: Coin={currentCoin}, Level={currentLevel}");
    }

    public void UpdateCoin()
    {
        if (CoinText != null)
        {
            CoinText.text = currentCoin.ToString();
            Debug.Log($"Đã cập nhật UI coin: {currentCoin}");
        }
    }
    #endregion

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }
        
        // Clear references
        player = null;
        textHP = null;
        CoinText = null;
    }

}
