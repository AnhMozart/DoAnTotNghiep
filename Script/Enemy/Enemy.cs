using System.Collections;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    protected Player player;
    protected virtual Rigidbody2D rgb {  get; set; }
    protected virtual Animator arm { get; set; }

    [Header("Check")]
    [SerializeField] protected Transform groundCheck;
    [SerializeField] protected float groundCheckDistance;
    [SerializeField] protected Transform wallCheck;
    [SerializeField] protected float wallCheckDistance;
    [SerializeField] protected LayerMask whatIsGround;

    [Header("Player Detection")]
    [SerializeField] protected float detectionRange = 5f;
    [SerializeField] protected LayerMask playerLayer;
    protected bool canSeePlayer = false;

    [SerializeField] protected Vector3 Push;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float Direction;
    [SerializeField] protected float maxHP = 100;
    [SerializeField] protected float currentHP;


    private bool IsRatation = false;
    public bool canAttack = false;

    public enum State { move,idle}
    [SerializeField] protected State state;


    protected virtual void Start()
    {
        rgb = GetComponent<Rigidbody2D>();
        arm = GetComponentInChildren<Animator>();
        player = FindAnyObjectByType<Player>();
        currentHP = maxHP;
    }

    protected virtual void Update()
    {
        MoveSpeed();
        CanFlip();
        CheckPlayerDetection();
    }

    protected virtual void CheckPlayerDetection()
    {
        if (player == null) return;

        // Tính hướng nhìn của enemy
        Vector2 facingDirection = new Vector2(transform.localScale.x, 0).normalized;
        
        // Tạo tia ray từ vị trí enemy theo hướng nhìn
        RaycastHit2D hit = Physics2D.Raycast(transform.position, facingDirection, detectionRange, playerLayer);

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            canSeePlayer = true;
            Debug.Log("Can see player");
        }
        else
        {
            canSeePlayer = false;
            Debug.DrawRay(transform.position, facingDirection * detectionRange, Color.yellow);
        }
    }


    public bool CheckGround() => Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);
    public bool CheckWall() => Physics2D.Raycast(wallCheck.position, Vector2.left, groundCheckDistance, whatIsGround);


    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(groundCheck.position, Vector2.down * groundCheckDistance);
        Gizmos.DrawRay(wallCheck.position, Vector2.left * wallCheckDistance);

        if (Application.isPlaying)
        {
            // Vẽ tia ray phát hiện player
            Vector2 facingDirection = new Vector2(transform.localScale.x, 0).normalized;
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, facingDirection * detectionRange);
        }
    }


    public virtual void TakeDamage(float _damage)
    {
        currentHP -= _damage;
        currentHP = Mathf.Max(currentHP, 0);
        if(currentHP <= 0)
        {
            Die();
        } 
    }    


    protected virtual void Die()
    {
        arm.SetTrigger("Die");
        Invoke(nameof(DestroySelf), 0.5f);
    }


    private void DestroySelf()
    {
        Destroy(gameObject);
    }


    public void PushUp()
    {
        rgb.transform.position += Push;
    }


    protected virtual void MoveSpeed()
    {
        switch(state)
        {
            case State.move:
                rgb.linearVelocity = new Vector2(moveSpeed * Direction, rgb.linearVelocity.y);
                arm.SetBool("Run", true);
                break;
            case State.idle:
                rgb.linearVelocity = new Vector3(0, 0, 0);
                break;

        }
    }


    private void Flip()
    {
        if (CheckGround() && !CheckWall())
        {
            transform.localScale = new Vector2(transform.localScale.x, 1);
        }
        else
        {
            transform.localScale = new Vector2(-transform.localScale.x, 1);
            IsRatation = true;
            moveSpeed = -moveSpeed;
        }
    }

    private void CanFlip()
    {
        Flip();
        Time_off();
    }

    public void Time_off()
    {
        if (IsRatation || canAttack)
        {
            rgb.linearVelocity = Vector2.zero;
            arm.SetBool("Run", false);
            StartCoroutine(Timer_off());
        }
        else
        {
            MoveSpeed();
            arm.SetBool("Run", true);
        }
    }

    IEnumerator Timer_off()
    {
        yield return new WaitForSeconds(1.5f);
        IsRatation = false;
        canAttack = false;
    }
}
