using UnityEngine;

public class Collapsing_Floor : Trap
{
    private Vector3 CurrenPostion;
    protected override void Start()
    {
        base.Start();
        rgb = GetComponent<Rigidbody2D>();
        CurrenPostion = transform.position;
    }


    protected override void Update()
    {
        base.Update();
    }



    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);
        if(collision.gameObject.CompareTag("Player"))
        {
            Invoke("Set_Falling_Platform", 1f);
        }

        if(collision.gameObject.CompareTag("Ground"))
        {
            Reset_Falling_Platform();
        }    

        if(collision.gameObject.CompareTag("Trap"))
        {
            Reset_Falling_Platform();
        }    
    }


    protected override void OnCollisionExit2D(Collision2D collision)
    {
        base.OnCollisionExit2D(collision);
        Invoke("Reset_Falling_Platform", 3f);
    }



    private void Set_Falling_Platform()
    {
        rgb.bodyType = RigidbodyType2D.Dynamic;
    }


    private void Reset_Falling_Platform()
    {
        rgb.bodyType = RigidbodyType2D.Static;
        transform.position = CurrenPostion;
    }
}
