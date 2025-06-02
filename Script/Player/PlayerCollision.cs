using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Key"))
        {
            Destroy(collision.gameObject);
            SoundManager.instance.TraloiDung();
            PlayerManager.instance.player.hasKey = true;
        }

        else if (collision.CompareTag("Trap"))
        {
            PlayerManager.instance.player.Takedamage(1);
        }
    }
}
