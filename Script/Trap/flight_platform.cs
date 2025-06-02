using UnityEngine;

public class flight_platform : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float maxPositionAround = 5f;
    [SerializeField] private float maxPostionUpDown = 10f;
    private Vector3 startingPosition;
    private bool movingRight = true;
    private bool movingUp = true;
    [SerializeField] private bool movingAround;
    [SerializeField] private bool movingUpdown;


    private void Start()
    {
        startingPosition = this.transform.position;
    }


    private void Update()
    {
        MoveAround();
    }



    private void MoveAround()
    {
        if(movingAround)
        {
            if (movingRight)
            {
                this.transform.Translate(Vector3.right * speed * Time.deltaTime);
                if (transform.position.x > startingPosition.x + maxPositionAround)
                {
                    movingRight = false;
                }
            }
            else
            {
                this.transform.Translate(Vector3.left * speed * Time.deltaTime);
                if (transform.position.x < startingPosition.x)
                {
                    movingRight = true;
                }
            }
        }

        else if(movingUpdown)
        {
            if (movingUp)
            {
                this.transform.Translate(Vector3.up * speed * Time.deltaTime);
                if (transform.position.y > startingPosition.y + maxPostionUpDown)
                {
                    movingUp = false;
                }
            }
            else
            {
                this.transform.Translate(Vector3.down * speed * Time.deltaTime);
                if (transform.position.y < startingPosition.y)
                {
                    movingUp = true;
                }
            }
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.parent = this.transform;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.parent = null;
        }
    }
}
