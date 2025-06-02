using UnityEngine;

public class Phone_Explosion : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            PlayerManager.instance.player.Takedamage(1);
        }   
    }

    public void DestroyBoom()
    {
        // Lấy tham chiếu đến GameObject cha
        GameObject parentObject = transform.parent?.gameObject;

        // Kiểm tra xem GameObject cha có tồn tại hay không
        if (parentObject != null)
        {
            // Xóa GameObject cha
            Destroy(parentObject);
        }
    }    
}
