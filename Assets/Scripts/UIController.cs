using NUnit.Framework;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;


public class UIController : MonoBehaviour
{
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text LivesText;
    [SerializeField] private TMP_Text ResourcesText;
    [SerializeField] private GameObject noResourcesText;


    [SerializeField] private GameObject TowerPanel;
    [SerializeField] private GameObject towerCardPrefab;
    [SerializeField] private Transform cardsContainer;

    [SerializeField] private TowerData[] towers;
    private List<GameObject> activeCards = new List<GameObject>();

    private Platform _currentPlatform;

    private void OnEnable()
    {
        Spawner.OnWaveChanged += UpdateWaveText;
        GameManager.OnLivesChanged += UpdateLivesText;
        GameManager.OnResourcesChanged += UpdateResourcesText;
        Platform.OnPlatformClicked += HandlePlatformClicked;
        TowerCard.onTowerSelected += handleTowerSelected;
    }
    private void OnDisable()
    {
        Spawner.OnWaveChanged -= UpdateWaveText;
        GameManager.OnLivesChanged -= UpdateLivesText;
        GameManager.OnResourcesChanged -= UpdateResourcesText;
        Platform.OnPlatformClicked -= HandlePlatformClicked;
        TowerCard.onTowerSelected -= handleTowerSelected;
    }

    private void UpdateWaveText(int currentWave)
    {
        waveText.text = $"Wave: {currentWave + 1}";
    }

    private void UpdateLivesText(int currentLives)
    {
        LivesText.text = $"Lives: {currentLives}";
    }
    private void UpdateResourcesText(int currentResources)
    {
        ResourcesText.text = $"Resources: {currentResources}";
    }

    private void ShowTowerPanel()
    {
        TowerPanel.SetActive(true);
        Platform.towerPanelOpen = true;
        GameManager.Instance.SetTimeScale(0f);
        populateTowerCards();
    }

    public void HideTowerPanel()
    {
        TowerPanel.SetActive(false);
        Platform.towerPanelOpen = false;
        GameManager.Instance.SetTimeScale(1f);
    }

    private void HandlePlatformClicked(Platform platform)
    {
        _currentPlatform = platform;
        if (!_currentPlatform.hasTowerOn)
        {
            ShowTowerPanel();
        }
    }

    private void populateTowerCards()
    {
        foreach(var card in activeCards)
        {
            Destroy(card);
        }
        activeCards.Clear();

        foreach(var data in towers)
        {
            GameObject cardGameObject = Instantiate(towerCardPrefab, cardsContainer);
            TowerCard card = cardGameObject.GetComponent<TowerCard>();
            card.Initialize(data);
            activeCards.Add(cardGameObject);
        }
    }

    private void handleTowerSelected(TowerData towerData)
    {
        if(GameManager.Instance.Resources >= towerData.cost)
        {
            GameManager.Instance.SpendResources(towerData.cost);
            _currentPlatform.PlaceTower(towerData);
        }
        else
        {
            StartCoroutine(ShowNoResourcesMessage());
        }
            HideTowerPanel();

    }

    private IEnumerator ShowNoResourcesMessage()
    {
        noResourcesText.SetActive(true);
        yield return new WaitForSecondsRealtime(3f);
        noResourcesText.SetActive(false);
    }
}
