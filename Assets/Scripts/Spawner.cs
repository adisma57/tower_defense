using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawner g�re l'apparition d'ennemis par vagues et utilise des ObjectPooler pour r�utiliser des GameObject.
/// - Lit les vagues depuis des WaveData (type, intervalle, nombre)
/// - D�clenche l'�v�nement statique OnWaveChanged lorsque la vague change
/// - G�re le timing d'apparition, le cooldown entre vagues et le comptage des ennemis retir�s
/// </summary>
public class Spawner : MonoBehaviour
{
    // Timer utilis� pour espacer les spawns au sein d'une m�me vague
    private float _spawnTimer;

    // �v�nement publi� quand la vague change. L'entier transmis est utilis� par le consommateur (index ou num�ro de vague).
    public static event Action<int> OnWaveChanged;

    // Configuration des vagues (ScriptableObjects) d�finie depuis l'inspecteur
    [SerializeField] private WaveData[] waves;

    // Index courant dans le tableau waves
    private int _currentWaveIndex = 0;

    // Compteur de vagues jou�es (diff�rent de l'index si on boucle)
    private int _waveCounter = 0;

    // Propri�t� utilitaire pour acc�der � la WaveData courante
    private WaveData currentWave => waves[_currentWaveIndex];

    // Poolers s�rialis�s pour chaque type d'ennemi (attribu�s dans l'inspecteur)
    [SerializeField] private ObjectPooler orcPooler;
    [SerializeField] private ObjectPooler dragonPooler;
    [SerializeField] private ObjectPooler kaijuPooler;

    // Dictionnaire construit � Awake pour mapper EnemyType -> ObjectPooler
    private Dictionary<EnemyType, ObjectPooler> _poolerDictionary;

    // Compteur d'ennemis spawn�s dans la vague courante (float permet �ventuellement des comportements fractionnaires)
    private float _spawnCounter;
    // Nombre d'ennemis "retir�s" (ex : ont atteint la fin ou ont �t� comptabilis�s comme morts)
    private int _enemiesRemoved;

    // Dur�e entre deux vagues (modifiable dans l'inspecteur)
    [SerializeField] private float _timeBetweenWaves = 0f;
    // Timer de cooldown entre vagues
    private float _waveCooldown;
    // �tat : true si on est dans la p�riode entre deux vagues
    private bool _isBetweenWaves = false;

    private void Awake()
    {
        // Construire la map pour r�cup�rer rapidement le pooler correspondant au type d'ennemi
        _poolerDictionary = new Dictionary<EnemyType, ObjectPooler>
        {
            { EnemyType.Orc, orcPooler },
            { EnemyType.Dragon, dragonPooler },
            { EnemyType.Kaiju, kaijuPooler }
        };
    }

    private void Start()
    {
        // Initialiser le timer de spawn � l'intervalle configur� pour la vague courante
        _spawnTimer = currentWave.SpawnInterval;
        // Initialiser le timer de cooldown entre vagues
        _waveCooldown = _timeBetweenWaves;
        // Notifier l'UI / autres syst�mes de la vague courante au d�marrage
        OnWaveChanged?.Invoke(_currentWaveIndex);
    }

    // S'abonner aux �v�nements d'ennemi quand ce component est activ�
    private void OnEnable()
    {
        Enemy.OnEnemyReachedEnd += HandleEnemyReachedEnd;
        Enemy.OnEnemyDestroyed += HandleEnemyDestroyed;
    }
    // Se d�sabonner pour �viter les r�f�rences persistantes quand d�sactiv�
    private void OnDisable()
    {
        Enemy.OnEnemyReachedEnd -= HandleEnemyReachedEnd;
        Enemy.OnEnemyDestroyed -= HandleEnemyDestroyed;
    }

    void Update()
    {
        if (_isBetweenWaves)
        {
            // P�riode de cooldown entre vagues : d�cr�menter le timer
            _waveCooldown -= Time.deltaTime;
            if (_waveCooldown <= 0)
            {
                // Passer � la vague suivante (boucle avec modulo), incr�menter le compteur logique de vagues
                _currentWaveIndex = (_currentWaveIndex + 1) % waves.Length;
                _waveCounter++;
                // Attention : ici OnWaveChanged est appel� avec _waveCounter (num�ro de vague)
                OnWaveChanged?.Invoke(_waveCounter);
                // R�initialiser compteurs pour la nouvelle vague
                _spawnCounter = 0;
                _enemiesRemoved = 0;
                _isBetweenWaves = false;
                // R�initialiser le cooldown � la valeur configur�e pour la prochaine fois
                _waveCooldown = _timeBetweenWaves;
            }
        }
        else
        {
            // Phase d'apparition d'ennemis : d�cr�menter le timer de spawn
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0 && _spawnCounter < currentWave.EnemyCountPerWave)
            {
                // Temps �coul� -> spawn d'un ennemi et r�initialisation du timer
                _spawnTimer = currentWave.SpawnInterval;
                SpawnEnemy();
                _spawnCounter++;
            }
            // Quand on a spawn� le nombre d'ennemis pr�vu, on consid�re la vague termin�e c�t� spawn
            // (le passage en �tat "between waves" ne d�pend ici que du fait d'avoir spawn� tous les ennemis)
            // Remarque : on ne v�rifie pas ici que tous les ennemis sont "retir�s" (la version comment�e faisait cette v�rif)
            else if (_spawnCounter >= currentWave.EnemyCountPerWave)
            {
                _isBetweenWaves = true;
            }
        }

    }

    private void SpawnEnemy()
    {
        // R�cup�rer le pooler correspondant au type d'ennemi de la vague courante
        if (_poolerDictionary.TryGetValue(currentWave.EnemyType, out ObjectPooler pooler))
        {
            GameObject spawnedObject = pooler.GetPooledObject();
            // Positionner l'ennemi au spawn et activer l'objet
            spawnedObject.transform.position = transform.position;

            float healthMultiplier = 1f + (0.1f * (_waveCounter)); // Augmente la sant� de 10% par vague
            Enemy enemy = spawnedObject.GetComponent<Enemy>();
            enemy.Initialize(healthMultiplier);

            spawnedObject.SetActive(true);
        }
        // Sinon, si aucun pooler trouv� : rien n'est fait (utile d'ajouter un log d'erreur si n�cessaire)
    }

    // Gestionnaire appel� quand un ennemi signale qu'il a atteint la fin (via Enemy.OnEnemyReachedEnd)
    private void HandleEnemyReachedEnd(EnemyData data)
    {
        // Incr�menter le compteur d'ennemis retir�s pour le suivi (utile si l'on souhaite attendre que tous soient "retir�s")
        _enemiesRemoved++;
    }
    private void HandleEnemyDestroyed(Enemy enemy)
    {
        // Incr�menter le compteur d'ennemis retir�s pour le suivi (utile si l'on souhaite attendre que tous soient "retir�s")
        _enemiesRemoved++;
    }
}