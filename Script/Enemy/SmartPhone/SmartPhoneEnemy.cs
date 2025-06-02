using UnityEngine;

public class SmartPhoneEnemy : Enemy
{
    [SerializeField] private GameObject Boom;


    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void Die()
    {
        base.Die();
        Invoke("CreateBoom", 0.5f);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Die();
        }
    }

    private void CreateBoom()
    {
        Instantiate(Boom, transform.position, Quaternion.identity);
    }
}
