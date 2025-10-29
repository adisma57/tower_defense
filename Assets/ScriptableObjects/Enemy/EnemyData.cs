using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    public float MoveSpeed = 3f;
    public float Health = 100f;
    public int Damage = 10;
    public float resourceReward = 5f;
}
