using UnityEngine;

public class Humberger_Enemy : Enemy
{
    [SerializeField] private GameObject BookPrefab;
    [SerializeField] private bool canBook = false;


    protected override void Start()
    {
        base.Start();
    }


    protected override void Update()
    {
        base.Update();
        Time_off();
    }


    protected override void Die()
    {
        base.Die();
        if(!canBook)
        {
            CreateBook();
            canBook = true;

        }    
    }


    private void CreateBook()
    {
        float RandomCreate = Random.Range(0, 10);
        Debug.Log(RandomCreate);
        if(RandomCreate >= 0 && RandomCreate <=5)
        {
            Instantiate(BookPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.Log("Chúc bạn may mắn lần sau");
        }
    }    



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerManager.instance.player.Takedamage(1);
            canAttack = true;
        }
    }

}
