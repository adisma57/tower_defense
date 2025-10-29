using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class Tower : MonoBehaviour
{
    [SerializeField] private TowerData towerData;
    private CircleCollider2D _rangeCollider;

    private List<Enemy> _enemiesInRange;
    private ObjectPooler _projectilePooler;

    private float _shootTimer = 0f;

    private void OnEnable()
    {
        Enemy.OnEnemyDestroyed += HandleEnemyDestroyed;
    }
    private void OnDisable()
    {
        Enemy.OnEnemyDestroyed -= HandleEnemyDestroyed;
    }
    private void Start()
    {
        // Set up range collider
        _rangeCollider = GetComponent<CircleCollider2D>();
        _rangeCollider.isTrigger = true;
        _rangeCollider.radius = towerData.range;

        _enemiesInRange = new List<Enemy>();

        _projectilePooler = GetComponent<ObjectPooler>();
        _shootTimer = towerData.fireRate;
    }

    private void Update()
    {
        _shootTimer -= Time.deltaTime;
        if (_shootTimer <= 0f)
        {
            Shoot();
            _shootTimer = towerData.fireRate;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.grey;
        Gizmos.DrawWireSphere(transform.position, towerData.range);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null && !_enemiesInRange.Contains(enemy))
            {
                _enemiesInRange.Add(enemy);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null && _enemiesInRange.Contains(enemy))
            {
                _enemiesInRange.Remove(enemy);
            }
        }
    }

    private void Shoot()
    {
        if (_enemiesInRange.Count == 0) return;

        _enemiesInRange.RemoveAll(enemy => enemy == null || !enemy.gameObject.activeInHierarchy);   

        Enemy targetEnemy = _enemiesInRange[0];
        GameObject projectileObj = _projectilePooler.GetPooledObject();

        if (projectileObj != null)
        {
            projectileObj.transform.position = transform.position;
            projectileObj.SetActive(true);
            Vector2 direction = (targetEnemy.transform.position - transform.position).normalized;
            projectileObj.GetComponent<Projectile>().Shoot(towerData, direction);
        }
    }

    private void HandleEnemyDestroyed(Enemy enemy)
    {
        if (_enemiesInRange.Contains(enemy))
        {
            _enemiesInRange.Remove(enemy);
        }
    }
}
