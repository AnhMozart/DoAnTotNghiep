using UnityEngine;

public class Arrows_Trap : Trap
{
    protected override void Start()
    {
        base.Start();
        rgb = GetComponentInChildren<Rigidbody2D>();
    }

    protected override void Update()
    {
        base.Update();
    }
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if(collision.CompareTag("Player"))
        {
            rgb.bodyType = RigidbodyType2D.Dynamic;
        }
    }
}
