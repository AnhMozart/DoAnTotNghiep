using UnityEngine;

public class Arrow_Trap_Collision : MonoBehaviour
{
    private Rigidbody2D rgb;
    private Vector3 DiemBatdau;

    private void Start()
    {
        rgb = GetComponent<Rigidbody2D>();
        DiemBatdau = transform.position;
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Ground"))
        {
            rgb.bodyType = RigidbodyType2D.Static;
            SoundManager.instance.AttackSounnd();
            Invoke("ResetPostion", 1f);
        }
    }


    private void ResetPostion()
    {
        transform.position = DiemBatdau;
    }
}
