using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawner gère l'apparition d'ennemis par vagues et utilise des ObjectPooler pour réutiliser des GameObject.
/// - Lit les vagues depuis des WaveData (type, intervalle, nombre)
/// - Déclenche l'événement statique OnWaveChanged lorsque la vague change
/// - Gère le timing d'apparition, le cooldown entre vagues et le comptage des ennemis retirés
/// </summary>
public class Spawner : MonoBehaviour
{
    // Timer utilisé pour espacer les spawns au sein d'une même vague
    private float _spawnTimer;

    // Événement publié quand la vague change. L'entier transmis est utilisé par le consommateur (index ou numéro de vague).
    public static event Action<int> OnWaveChanged;

    // Configuration des vagues (ScriptableObjects) définie depuis l'inspecteur
    [SerializeField] private WaveData[] waves;

    // Index courant dans le tableau waves
    private int _currentWaveIndex = 0;

    // Compteur de vagues jouées (différent de l'index si on boucle)
    private int _waveCounter = 0;

    // Propriété utilitaire pour accéder à la WaveData courante
    private WaveData currentWave => waves[_currentWaveIndex];

    // Poolers sérialisés pour chaque type d'ennemi (attribués dans l'inspecteur)
    [SerializeField] private ObjectPooler orcPooler;
    [SerializeField] private ObjectPooler dragonPooler;
    [SerializeField] private ObjectPooler kaijuPooler;

    // Dictionnaire construit à Awake pour mapper EnemyType -> ObjectPooler
    private Dictionary<EnemyType, ObjectPooler> _poolerDictionary;

    // Compteur d'ennemis spawnés dans la vague courante (float permet éventuellement des comportements fractionnaires)
    private float _spawnCounter;
    // Nombre d'ennemis "retirés" (ex : ont atteint la fin ou ont été comptabilisés comme morts)
    private int _enemiesRemoved;

    // Durée entre deux vagues (modifiable dans l'inspecteur)
    [SerializeField] private float _timeBetweenWaves = 0f;
    // Timer de cooldown entre vagues
    private float _waveCooldown;
    // État : true si on est dans la période entre deux vagues
    private bool _isBetweenWaves = false;

    private void Awake()
    {
        // Construire la map pour récupérer rapidement le pooler correspondant au type d'ennemi
        _poolerDictionary = new Dictionary<EnemyType, ObjectPooler>
        {
            { EnemyType.Orc, orcPooler },
            { EnemyType.Dragon, dragonPooler },
            { EnemyType.Kaiju, kaijuPooler }
        };
    }

    private void Start()
    {
        // Initialiser le timer de spawn à l'intervalle configuré pour la vague courante
        _spawnTimer = currentWave.SpawnInterval;
        // Initialiser le timer de cooldown entre vagues
        _waveCooldown = _timeBetweenWaves;
        // Notifier l'UI / autres systèmes de la vague courante au démarrage
        OnWaveChanged?.Invoke(_currentWaveIndex);
    }

    // S'abonner aux événements d'ennemi quand ce component est activé
    private void OnEnable()
    {
        Enemy.OnEnemyReachedEnd += HandleEnemyReachedEnd;
        Enemy.OnEnemyDestroyed += HandleEnemyDestroyed;
    }
    // Se désabonner pour éviter les références persistantes quand désactivé
    private void OnDisable()
    {
        Enemy.OnEnemyReachedEnd -= HandleEnemyReachedEnd;
        Enemy.OnEnemyDestroyed -= HandleEnemyDestroyed;
    }

    void Update()
    {
        if (_isBetweenWaves)
        {
            // Période de cooldown entre vagues : décrémenter le timer
            _waveCooldown -= Time.deltaTime;
            if (_waveCooldown <= 0)
            {
                // Passer à la vague suivante (boucle avec modulo), incrémenter le compteur logique de vagues
                _currentWaveIndex = (_currentWaveIndex + 1) % waves.Length;
                _waveCounter++;
                // Attention : ici OnWaveChanged est appelé avec _waveCounter (numéro de vague)
                OnWaveChanged?.Invoke(_waveCounter);
                // Réinitialiser compteurs pour la nouvelle vague
                _spawnCounter = 0;
                _enemiesRemoved = 0;
                _isBetweenWaves = false;
                // Réinitialiser le cooldown à la valeur configurée pour la prochaine fois
                _waveCooldown = _timeBetweenWaves;
            }
        }
        else
        {
            // Phase d'apparition d'ennemis : décrémenter le timer de spawn
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0 && _spawnCounter < currentWave.EnemyCountPerWave)
            {
                // Temps écoulé -> spawn d'un ennemi et réinitialisation du timer
                _spawnTimer = currentWave.SpawnInterval;
                SpawnEnemy();
                _spawnCounter++;
            }
            // Quand on a spawné le nombre d'ennemis prévu, on considère la vague terminée côté spawn
            // (le passage en état "between waves" ne dépend ici que du fait d'avoir spawné tous les ennemis)
            // Remarque : on ne vérifie pas ici que tous les ennemis sont "retirés" (la version commentée faisait cette vérif)
            else if (_spawnCounter >= currentWave.EnemyCountPerWave)
            {
                _isBetweenWaves = true;
            }
        }

    }

    private void SpawnEnemy()
    {
        // Récupérer le pooler correspondant au type d'ennemi de la vague courante
        if (_poolerDictionary.TryGetValue(currentWave.EnemyType, out ObjectPooler pooler))
        {
            GameObject spawnedObject = pooler.GetPooledObject();
            // Positionner l'ennemi au spawn et activer l'objet
            spawnedObject.transform.position = transform.position;

            float healthMultiplier = 1f + (0.1f * (_waveCounter)); // Augmente la santé de 10% par vague
            Enemy enemy = spawnedObject.GetComponent<Enemy>();
            enemy.Initialize(healthMultiplier);

            spawnedObject.SetActive(true);
        }
        // Sinon, si aucun pooler trouvé : rien n'est fait (utile d'ajouter un log d'erreur si nécessaire)
    }

    // Gestionnaire appelé quand un ennemi signale qu'il a atteint la fin (via Enemy.OnEnemyReachedEnd)
    private void HandleEnemyReachedEnd(EnemyData data)
    {
        // Incrémenter le compteur d'ennemis retirés pour le suivi (utile si l'on souhaite attendre que tous soient "retirés")
        _enemiesRemoved++;
    }
    private void HandleEnemyDestroyed(Enemy enemy)
    {
        // Incrémenter le compteur d'ennemis retirés pour le suivi (utile si l'on souhaite attendre que tous soient "retirés")
        _enemiesRemoved++;
    }
}