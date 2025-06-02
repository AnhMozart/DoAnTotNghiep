using UnityEngine;

public class Book_Heal : MonoBehaviour
{
    [SerializeField] private float Heal = 1f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            SoundManager.instance.TraloiDung();
            PlayerManager.instance.player.Heal(Heal);
            Destroy(gameObject);
        }    
    }
}
