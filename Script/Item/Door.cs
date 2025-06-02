using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{
    [SerializeField] private float timeOpenDoor = 2f;
    private Animator arm;

    private void Start()
    {
        arm = GetComponent<Animator>();
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") && PlayerManager.instance.player.hasKey)
        {
            arm.SetTrigger("Open");
            PlayerManager.instance.player.HidPlayer();
            StartCoroutine(CloseDoor());
            PlayerManager.instance.player.hasKey = false;
        }
    }


    IEnumerator CloseDoor()
    {
        yield return new WaitForSeconds(timeOpenDoor);
        arm.SetTrigger("Close");
        SoundManager.instance.NextLevel();
        yield return new WaitForSeconds(2f);
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;
        // Lấy số level hiện tại từ tên scene
        string currentSceneName = SceneManager.GetActiveScene().name;
        int currentLevelNumber = int.Parse(currentSceneName.Replace("Level", ""));
        
        // Kiểm tra xem nextSceneIndex có vượt quá số lượng màn chơi hay không
        if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            // Nếu vượt quá, tải màn chơi đầu tiên (index 0)
            SceneManager.LoadScene(0);
        }
        else
        {
            // Nếu không vượt quá, mở khóa level tiếp theo và tải màn chơi tiếp theo
            LevelManager.Instance.CompleteLevel(currentLevelNumber);
            SceneManager.LoadScene(nextSceneIndex);
        }
    }    

}
