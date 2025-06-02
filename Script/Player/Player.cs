using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class Player : MonoBehaviour
{
    [Header("PlayerController")]
    [SerializeField] private float MoveSpeed = 5f;
    [SerializeField] private float Jump = 10f;
    private Vector3 startPosition;


    [Header("PlayerComponents")]
    private Rigidbody2D rgb;
    private Animator arm;
    private SpriteRenderer sr;
    private CapsuleCollider2D capsule;


    [Header("Check")]
    [SerializeField] private Transform groundcheck;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private LayerMask groundLayer;
    public Transform AttackCheck;
    public float AttackRadius;
    private bool CanDoubleJump;
    public bool hasKey = false;
    public bool isLive = true;
    private bool ceilingLayerChanged = false;



    [Header("Staten")]
    [SerializeField] private float maxHP;
    [SerializeField] private float currentHP;
    [SerializeField] private float Damage = 100;


    [Header("Bullet")]
    [SerializeField] private ObjectPooling BulletPool;
    [SerializeField] private GameObject shootPos; // vị trí bắn đạn
    [SerializeField] private float WaitTime = 3f;
    [SerializeField] private float nextTime;


    [Space]
    [Header("Fx")]
    [SerializeField] private Material hitMat;
    private Material originalMat;


    [Header("Controller")]
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private Button jumpButton;  // Thêm button nhảy
    [SerializeField] private float xInput;


    private void Start()
    {
        rgb = GetComponent<Rigidbody2D>();
        arm = GetComponentInChildren<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();
        capsule = GetComponent<CapsuleCollider2D>();
        currentHP = maxHP;
        originalMat = sr.material;
        UpdateHP();

        startPosition = transform.position; // Lưu vị trí ban đầu khi game bắt đầu

        // Thêm sự kiện cho button nhảy
        if(jumpButton != null)
        {
            jumpButton.onClick.RemoveAllListeners();
            jumpButton.onClick.AddListener(Player_Jump);
        }
    }


    private void Update()
    {
        CheckLayer();
        Player_Jump_test();
    }


    private void FixedUpdate() 
    {
        Player_Move();
        //Player_Move_Test();
        arm.SetBool("Isground", IsGround());
    }

    #region PlayerController
    private void Player_Move()
    {
        if(!isLive) { return; }

        //float xInput = Input.GetAxis("Horizontal");
        rgb.linearVelocity = new Vector2(xInput * MoveSpeed, rgb.linearVelocity.y);
        bool playerhasHorizontalSpeed = Mathf.Abs(rgb.linearVelocity.x) > Mathf.Epsilon;
        if(IsGround())
        {
            arm.SetBool("Run", playerhasHorizontalSpeed);
        }
          
        else
        {
            arm.SetBool("Run", false);
        }

        Flip();
    }    



    public void Player_Jump()
    {
        if(IsGround())
        {
            SoundManager.instance.JumpSound();
            rgb.linearVelocity = new Vector2(rgb.linearVelocity.x, Jump);
            arm.SetTrigger("Jump");
            arm.SetBool("Run",false);
            CanDoubleJump = true;
        }
        else if(CanDoubleJump)
        {
            SoundManager.instance.JumpSound();
            rgb.linearVelocity = new Vector2(rgb.linearVelocity.x, Jump);
            CanDoubleJump = false;
            arm.SetTrigger("Jump");
            arm.SetBool("Run", false);
        }
    }



    private void Player_Move_Test()
    {
        if (!isLive) { return; }

        float xInput_test = Input.GetAxis("Horizontal");
        rgb.linearVelocity = new Vector2(xInput_test * MoveSpeed, rgb.linearVelocity.y);
        bool playerhasHorizontalSpeed = Mathf.Abs(rgb.linearVelocity.x) > Mathf.Epsilon;
        if (IsGround())
        {
            arm.SetBool("Run", playerhasHorizontalSpeed);
        }

        else
        {
            arm.SetBool("Run", false);
        }

        Flip();
    }



    public void Player_Jump_test()
    {
        if (IsGround() && Input.GetKeyDown(KeyCode.Space))
        {
            rgb.linearVelocity = new Vector2(rgb.linearVelocity.x, Jump);
            arm.SetTrigger("Jump");
            arm.SetBool("Run", false);
            CanDoubleJump = true;
        }
        else if (CanDoubleJump && Input.GetKeyDown(KeyCode.Space))
        {
            rgb.linearVelocity = new Vector2(rgb.linearVelocity.x, Jump);
            CanDoubleJump = false;
            arm.SetTrigger("Jump");
            arm.SetBool("Run", false);
        }
    }


    public void OnLeftButton()
    {
        xInput = -1;
    }
    
    public void OnRightButton()
    {
        xInput = 1;
    }

    public void OnButtonUp()
    {
        xInput = 0;
    }
        
    #endregion

    #region FlipPlayer
    private void Flip()
    {
        bool playerhasHorizontalSpeed = Mathf.Abs(rgb.linearVelocity.x) > Mathf.Epsilon;
        if (playerhasHorizontalSpeed)
        {
            transform.localScale = new Vector2(Mathf.Sign(rgb.linearVelocity.x), 1f);
        } 
            
    }
    #endregion


    #region Check
    private bool IsGround() => Physics2D.Raycast(groundcheck.position, Vector2.down, groundCheckRadius, groundLayer);


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(groundcheck.position, Vector2.down * groundCheckRadius);
        Gizmos.DrawWireSphere(AttackCheck.position, AttackRadius);
    }
    #endregion

    #region TakeDamage

    public void Takedamage(float _damage)
    {
        currentHP -= _damage;
        currentHP = Mathf.Max(currentHP, 0);
        StartCoroutine(FlashFX());
        UpdateHP();
        SoundManager.instance.PlayerUnderAttack();
        if(currentHP <= 0)
        {
            Die();
        }
        else
        {
            // Thay vì load scene, chỉ reset position của player
            StartCoroutine(ResetPosition());
        }
    }

    private IEnumerator ResetPosition()
    {
        yield return new WaitForSeconds(0.3f); // Đợi hiệu ứng flash
        rgb.linearVelocity = Vector2.zero; // Dừng mọi chuyển động
        transform.position = startPosition; // Reset về vị trí ban đầu
    }

    private IEnumerator FlashFX() //Hàm chuyển màu khi bị đánh
    {
        sr.material = hitMat;
        yield return new WaitForSeconds(0.3f);
        sr.material = originalMat;
    }


    private void Die()
    {
        // Vô hiệu hóa collider và set trạng thái chết
        capsule.enabled = false;
        isLive = false;
        
        // Tìm enemy nếu có
        GameObject enemyObject = GameObject.FindWithTag("Enemy");
        if (enemyObject != null)
        {
            Enemy enemy = enemyObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Nếu có enemy, đẩy nhân vật ra xa
                transform.Translate(transform.position.x < enemy.transform.position.x ? -2 : 2, 2f, 0f);
            }
        }

        // Load lại scene sau 1 giây
        StartCoroutine(ReloadScene());
    }

    private IEnumerator ReloadScene()
    {
        yield return new WaitForSeconds(1f);
        // Load lại scene hiện tại, tất cả sẽ reset về ban đầu
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }




    #endregion



    #region DoDamageAndCheck
    public void ShootBullet()
    {
        if (isLive && PlayerManager.instance.GetCurrentCoin() >= 100 && Time.time > nextTime)
        {
            nextTime = Time.time + WaitTime;
            PlayerManager.instance.MinusCoin(100);
            PlayerManager.instance.UpdateCoin();
            GameObject bullet = BulletPool.GetObject();
            bullet.transform.position = shootPos.transform.position;
            float direction = transform.localScale.x;
            bullet.GetComponent<Bullet>().DirectionPlayer(direction);
            SoundManager.instance.NemGiaoSound();
        }
        //Debug.Log("Đã ấn bắn");
    }



    private void CheckLayer() // kiểm tra chấn thương đầu và các vẫn đề liên quan đến layer
    {
        Collider2D[] hit = Physics2D.OverlapCircleAll(AttackCheck.position, AttackRadius);
        bool ceilingHit = false; //biến kiểm tra chạm trần
        foreach (var collider in hit)
        {
            if (collider.GetComponent<Enemy>() != null && isLive)
            {
                if (transform.position.y > collider.transform.position.y && rgb.linearVelocity.y < 0)
                {
                    SoundManager.instance.JumpSound();
                    rgb.linearVelocity = new Vector2(rgb.linearVelocity.x, 10f);
                    Enemy enemy = collider.GetComponent<Enemy>();
                    enemy.TakeDamage(Damage);
                }
            }

            if (collider.CompareTag("Ceiling"))
            {
                ceilingHit = true; //đã chạm trần
                Ceiling ceiling = collider.GetComponent<Ceiling>();
                if (!ceilingLayerChanged)
                {
                    StartCoroutine(Duration(ceiling));
                }
                if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                {

                    ceiling.ChangeLayer(8);
                    ceilingLayerChanged = true;
                }
            }
        }

        if (!ceilingHit) //nếu không chạm trần, reset biến trạng thái.
        {
            ceilingLayerChanged = false;
        }
    }


    IEnumerator Duration(Ceiling _ceiling)
    {
        yield return new WaitForSeconds(0.15f);
        _ceiling.ChangeLayer(3);
    }
    #endregion


    #region Effect
    public void HidPlayer()
    {
        StartCoroutine(Hide_Presently());
    }    

    IEnumerator Hide_Presently()
    {
        yield return new WaitForSeconds(2f);
        sr.color = new Color(1, 1, 1, 0);
        MoveSpeed = 0;
        yield return new WaitForSeconds(4f);
        sr.color = new Color(1, 1, 1, 1);
        MoveSpeed = 5;
    }
    #endregion


    #region UI_HP

    public void Heal(float _heal)
    {
        currentHP += _heal;

        float bonuscoin;

        if(currentHP > 3)
        {
            float bonus = Random.Range(1, 5);

            if(bonus == 2)
            {
                bonuscoin = 20f;
            }    
            else
            {
                bonuscoin = 10f;
            }    

            PlayerManager.instance.AddCoin(bonuscoin);   
            currentHP = Mathf.Min(currentHP, 3);
        }

        UpdateHP();
    }

    public void UpdateHP()
    {
        PlayerManager.instance.TextHP().text = "X " + currentHP.ToString();
    }    

    //public int GetCurrentScore()
    //{
    //    return PlayerManager.instance.GetCurrentCoin();
    //}
    
    public float GetCurentHp()
    {
        return currentHP;
    }    
    #endregion

}
