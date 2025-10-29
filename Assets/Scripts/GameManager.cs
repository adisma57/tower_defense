using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event Action<int> OnLivesChanged;
    public static event Action<int> OnResourcesChanged;

    private int _lives = 20;
    private int _resources = 175;

    public int Resources => _resources;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    private void OnEnable()
    {
        Enemy.OnEnemyReachedEnd += HandleEnemyReachedEnd;
        Enemy.OnEnemyDestroyed += HandleEnemyDestroyed;
    }
    private void OnDisable()
    {
        Enemy.OnEnemyReachedEnd -= HandleEnemyReachedEnd;
        Enemy.OnEnemyDestroyed -= HandleEnemyDestroyed;
    }
    private void Start()
    {
        OnLivesChanged?.Invoke(_lives);
        OnResourcesChanged?.Invoke(_resources);
    }

    private void HandleEnemyReachedEnd(EnemyData enemyData)
    {
        _lives = Math.Max(_lives - enemyData.Damage, 0);
        OnLivesChanged?.Invoke(_lives);

        Debug.Log($"Lives remaining: {_lives}");
        if (_lives <= 0)
        {
            Debug.Log("Game Over!");
            // Implement game over logic here
        }
    }

    private void addResources(int amount)
    {
        _resources += amount;
        OnResourcesChanged?.Invoke(_resources);
    }

    public void SpendResources(int amount)
    {
        if(_resources > amount)
        {
            _resources -= amount;
            OnResourcesChanged?.Invoke(_resources);
        }
            
    }
    private void HandleEnemyDestroyed(Enemy enemy)
    {
        addResources(Mathf.RoundToInt(enemy.Data.resourceReward));
    }

    public void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
    }
}
