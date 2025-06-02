using System.Collections;
using UnityEngine;

public class Jumper : Trap
{

    protected override void Start()
    {
        base.Start();
        rgb = GetComponent<Rigidbody2D>();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);
        if (collision.gameObject.CompareTag("Player"))
        {
            arm.SetBool("Jump", true);
        }
    }

    protected override void OnCollisionExit2D(Collision2D collision)
    {
        base.OnCollisionExit2D(collision);
        StartCoroutine(Reset_Jumper());
    }


    IEnumerator Reset_Jumper()
    {
        yield return new WaitForSeconds(1);
        arm.SetBool("Jump", false);
    }

}
