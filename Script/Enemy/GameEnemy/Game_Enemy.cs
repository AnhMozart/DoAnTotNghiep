using UnityEngine;

public class Game_Enemy : Enemy
{
    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 1f;
    private float nextFireTime;

    protected override void Die()
    {
        base.Die();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        ShootGun();
    }

    private void ShootGun()
    {
        if (canSeePlayer)
        {
            if (Time.time >= nextFireTime)
            {
                FireBullet();
                nextFireTime = Time.time + fireRate;
            }
            arm.SetBool("Attack", true);
        }    
        else
        {
            arm.SetBool("Attack", false);
        }    
    }

    private void FireBullet()
    {
        if (firePoint == null || bulletPrefab == null) return;

        // Tạo đạn từ prefab
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        
        // Tính hướng bắn từ enemy đến player
        Vector2 directionToPlayer = (player.transform.position - transform.position).normalized;
        
        // Gán hướng cho đạn
        Bullet_Enemy bulletScript = bullet.GetComponent<Bullet_Enemy>();
        if (bulletScript != null)
        {
            bulletScript.SetDirection(directionToPlayer);
        }
    }
}
