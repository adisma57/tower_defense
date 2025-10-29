using UnityEngine;

[CreateAssetMenu(fileName = "WaveData", menuName = "Scriptable Objects/WaveData")]
public class WaveData : ScriptableObject
{
    public EnemyType EnemyType;
    public float SpawnInterval;
    public int EnemyCountPerWave;
}
