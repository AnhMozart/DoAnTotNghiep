using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float Damage_Enemy = 50f;
    [SerializeField] private float Speed = 5f;
    private float Direction;

    private void Update()
    {
        transform.position += new Vector3(Speed * Direction * Time.deltaTime, 0f, 0f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(Damage_Enemy);
                enemy.PushUp();
                Debug.Log("ke dich bi gay sat thuong");
                SoundManager.instance.AttackSounnd();
                FindFirstObjectByType<ObjectPooling>().ReturnGameObject(gameObject);
            }

        }

        if (collision.CompareTag("Ground"))
        {
            SoundManager.instance.AttackSounnd();
            FindFirstObjectByType<ObjectPooling>().ReturnGameObject(gameObject);
        }    
    }


    public void DirectionPlayer(float _Direc)
    {
        Direction = Mathf.Sign(_Direc); // chỉ nhận -1 hoặc 1
        if (Direction == 1)
        {
            // Xoay viên đạn về góc 43 độ
            transform.rotation = Quaternion.Euler(0, 0, -43);
        }
        else if (Direction == -1)
        {
            // Xoay viên đạn về góc 137 độ
            transform.rotation = Quaternion.Euler(0, 0, 137);
        }
    }    

}
