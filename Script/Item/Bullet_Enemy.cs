using UnityEngine;

public class Bullet_Enemy : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    private Vector2 direction;


    // Update is called once per frame
    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("đã va chạm vào player");
            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                PlayerManager.instance.player.Takedamage(1);
                Destroy(gameObject);
            }
        }

        else if(collision.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }    
    }
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }
}
