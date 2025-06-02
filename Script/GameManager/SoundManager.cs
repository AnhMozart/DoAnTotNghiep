using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField] private AudioClip jumpAudio;
    [SerializeField] private AudioClip AttackAudio;
    [SerializeField] private AudioClip NemGiaoAmThanh;
    [SerializeField] private AudioClip playerUnderAttack;
    [SerializeField] private AudioClip DAD_Audio;
    [SerializeField] private AudioClip DAS_Audio;
    [SerializeField] private AudioClip nextLevel;
    [SerializeField] private AudioClip openChest;
    private AudioSource ads;

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
        ads = GetComponent<AudioSource>();
        if (ads == null)
        {
            Debug.LogError("Không tìm thấy AudioSource component!");
        }
        if (jumpAudio == null)
        {
            Debug.LogError("Chưa gán AudioClip cho jumpAudio!");
        }
    }

    public void JumpSound()
    {
        if (ads != null && jumpAudio != null)
        {
            ads.PlayOneShot(jumpAudio);
            Debug.Log("Đã phát âm thanh nhảy");
        }
        else
        {
            Debug.LogError("Không thể phát âm thanh nhảy: AudioSource hoặc jumpAudio là null");
        }
    }


    public void AttackSounnd()
    {
        ads.PlayOneShot(AttackAudio);
    }


    public void NemGiaoSound()
    {
        ads?.PlayOneShot(NemGiaoAmThanh);
    }    


    public void TraloiDung()
    {
        ads.PlayOneShot(DAD_Audio);
    }

    public void TraloiSai()
    { 
        ads.PlayOneShot(DAS_Audio);
    }


    public void PlayerUnderAttack()
    {
        ads.PlayOneShot(playerUnderAttack);
    }

    public void NextLevel()
    {
        ads.PlayOneShot(nextLevel);
    }


    public void OpenChest()
    {
        ads.PlayOneShot(openChest);
    }
}
