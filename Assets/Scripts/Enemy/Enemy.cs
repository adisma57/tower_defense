using Unity.VisualScripting;
using UnityEngine;
using System;

public class Enemy : MonoBehaviour
{


    [SerializeField] private EnemyData data;
    public EnemyData Data => data;

    private Path _currentPath;

    private Vector3 _targetPosition;
    private int _currentWaypointIndex = 0;

    public static event Action<EnemyData> OnEnemyReachedEnd;
    public static event Action<Enemy> OnEnemyDestroyed;

    private float _currentHealth;
    private float _maxHealth;

    [SerializeField] private Transform _healthBar;
    private Vector3 _healthBarOriginalScale;

    private bool _hasBeenCounted = false;

    private void Awake()
    {
        _currentPath = GameObject.Find("Path").GetComponent<Path>();
        _healthBarOriginalScale = _healthBar.localScale;
    }
    private void OnEnable()
    {
        _currentWaypointIndex = 0;
        _targetPosition = _currentPath.GetWaypointPosition(_currentWaypointIndex); 
    }
    
    void Update()
    {
        if (_hasBeenCounted)  return;

        //move enemy towards target position
        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, data.MoveSpeed * Time.deltaTime);

        //when target position is reached, update to next waypoint
        float relativeDistance = Vector3.Distance(transform.position, _targetPosition);
        if(relativeDistance < 0.1f)
        {
            _currentWaypointIndex++;
            if (_currentWaypointIndex < _currentPath.Waypoints.Length)
            {
                _targetPosition = _currentPath.GetWaypointPosition(_currentWaypointIndex);
            }
            else
            {
                // Reached the end of the path
                _hasBeenCounted = true;
                OnEnemyReachedEnd?.Invoke(data);
                gameObject.SetActive(false); // Deactivate enemy or handle as needed

            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (_hasBeenCounted) return;

        _currentHealth -= damage;
        _currentHealth = Mathf.Max(_currentHealth, 0f);

        updateHealthBar();
        if (_currentHealth <= 0f)
        {
            _hasBeenCounted = true;
            OnEnemyDestroyed?.Invoke(this);
            gameObject.SetActive(false); // Deactivate enemy or handle death as needed
        }
    }

    private void updateHealthBar()
    {
        float healthRatio = _currentHealth / _maxHealth;
        _healthBar.localScale = new Vector3(_healthBarOriginalScale.x * healthRatio, _healthBarOriginalScale.y, _healthBarOriginalScale.z);
    }

    public void Initialize(float healthMultiplier)
    {
        _hasBeenCounted = false;
        // Initialize enemy stats based on multiplier
        _maxHealth = data.Health * healthMultiplier;
        _currentHealth = _maxHealth;
        updateHealthBar();
    }
}
