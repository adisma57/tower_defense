using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class TowerCard : MonoBehaviour
{
    [SerializeField] private Image towerImage;
    [SerializeField] private TMP_Text towerCostText;

    private TowerData _towerData;
    public static event Action<TowerData> onTowerSelected;
    public void Initialize(TowerData data)
    {
        _towerData = data;
        towerImage.sprite = data.sprite;
        towerCostText.text = data.cost.ToString(); 
    }

    public void PlaceTower()
    {
        onTowerSelected?.Invoke(_towerData);
    }
}
