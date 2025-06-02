using Unity.VisualScripting;
using UnityEngine;

public class Ceiling : MonoBehaviour
{
    public virtual void ChangeLayer(int layerint)
    {
        gameObject.layer = layerint;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            gameObject.layer = 8;
        }    
    }

}
