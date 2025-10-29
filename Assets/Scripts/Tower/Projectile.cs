using UnityEngine;

public class Projectile : MonoBehaviour
{

    private TowerData _data;
    private Vector3 _shootDirection;
    private float _projectileDuration;

    private void Start()
    {
        transform.localScale = Vector3.one * _data.projectileSize;
    }
    void Update()
    {
        if(_projectileDuration <= 0f)
        {
            gameObject.SetActive(false);
        }
        else
        {
            _projectileDuration -= Time.deltaTime;
            transform.position += new Vector3(_shootDirection.x, _shootDirection.y, 0f) * _data.projectileSpeed * Time.deltaTime;
        }
    }

    public void Shoot(TowerData data, Vector3 shootDirection)
    {
        _data = data;
        _shootDirection = shootDirection.normalized;
        _projectileDuration = data.projectileDuration;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if(enemy != null)
            {
                enemy.TakeDamage(_data.damage);
                gameObject.SetActive(false);
            }
        }
    }
}
