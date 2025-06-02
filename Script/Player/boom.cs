using System.Collections;
using UnityEngine;


public class boom : MonoBehaviour
{
    [SerializeField] private float Damage_Enemy = 50f;
    [SerializeField] private float Damage_Player = 1f;
    [SerializeField] private float TimerBoom = 5f;
    private Animator arm;
    private CircleCollider2D cr;


    private void Awake()
    {
        arm = GetComponent<Animator>();
        cr = GetComponent<CircleCollider2D>();
    }


    private void OnEnable() // Gọi khi object được kích hoạt từ pool
    {
        cr.enabled = false;
        StartCoroutine(Boooom());
    }


    IEnumerator Boooom()
    {
        yield return new WaitForSeconds(TimerBoom);
        arm.SetTrigger("Boom");
        cr.enabled=true;
    }    

    public void DestroyBoom() // ham nay duoc goi trong enven animation booom no
    {
        FindFirstObjectByType<ObjectPooling>().ReturnGameObject(gameObject);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if(enemy != null)
            {
                enemy.TakeDamage(Damage_Enemy);
                enemy.PushUp();
                Debug.Log("Kẻ địch bị gây sát thương");

            }

        }   
        

        else if(collision.CompareTag("Player"))
        {
            if(PlayerManager.instance.player != null)
            {
                PlayerManager.instance.player.Takedamage(Damage_Player);
                Debug.Log("Người chơi bị gây sát thương");
            }    
        }    

    }

}
