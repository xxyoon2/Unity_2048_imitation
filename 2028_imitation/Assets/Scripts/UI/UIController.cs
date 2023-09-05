using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    [SerializeField] private Button _resetButton;
    [SerializeField] private TextMeshProUGUI _currentScoreText;
    [SerializeField] private TextMeshProUGUI _bestScoreText;
    [SerializeField] private GameObject _gameOverUI;

    private void OnEnable()
    {
        GameManager.Instance.OnScoreChanged.AddListener(UpdateScoreText);
        GameManager.Instance.OnBestScoreChanged.AddListener(UpdateBestScoreText);
        GameManager.Instance.OnGameOver.AddListener(ActivateGameOverUI);
    }

    private void UpdateScoreText(int score)
    {
        _currentScoreText.text = $"{score}";
    }

    private void UpdateBestScoreText(int score)
    {
        _bestScoreText.text = $"{score}";
    }

    private void ActivateGameOverUI()
    {
        _gameOverUI.SetActive(true);
    }

    public void ResetGame()
    {
        GameManager.Instance.OnResetGame.Invoke();
        _gameOverUI.SetActive(false);
    }

    private void OnDisable()
    {
        GameManager.Instance.OnScoreChanged.RemoveListener(UpdateScoreText);
        GameManager.Instance.OnBestScoreChanged.RemoveListener(UpdateBestScoreText);
        GameManager.Instance.OnGameOver.RemoveListener(ActivateGameOverUI);
    }
}
