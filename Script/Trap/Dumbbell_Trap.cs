using UnityEngine;

public class Dumbbell_Trap : Trap
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
}
