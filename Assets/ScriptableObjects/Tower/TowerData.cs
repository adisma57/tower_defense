using UnityEngine;

[CreateAssetMenu(fileName = "TowerData", menuName = "Scriptable Objects/TowerData")]
public class TowerData : ScriptableObject
{
    public float range;
    public float fireRate;
    public float projectileSpeed;
    public float projectileDuration;
    public float damage;
    public float projectileSize;

    public int cost;
    public Sprite sprite;

    public GameObject prefab;
}
