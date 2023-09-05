using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : SingletonBehaviour<GameManager>
{
    public UnityEvent<int> OnScoreChanged = new UnityEvent<int>();
    public UnityEvent<int> OnBestScoreChanged = new UnityEvent<int>();
    public UnityEvent OnGameOver = new UnityEvent();
    public UnityEvent OnResetGame = new UnityEvent();

    private int _currentScore = 0;
    public int CurrentScore
    {
        get
        {
            return _currentScore;
        }
        set
        {
            _currentScore = value;

            OnScoreChanged.Invoke(_currentScore);
            OnScoreChanged?.Invoke(_currentScore);
        }
    }

    private readonly string BestScoreKeyName = "2048_BestScore";
    private int _bestScore;
    public int BestScore
    {
        get
        {
            _bestScore = PlayerPrefs.GetInt(BestScoreKeyName, 0);
            return _bestScore;
        }
        set
        {
            if (CurrentScore > BestScore)
            {
                _bestScore = value;

                PlayerPrefs.SetInt(BestScoreKeyName, _bestScore);
                OnBestScoreChanged.Invoke(_bestScore);
                OnBestScoreChanged?.Invoke(_bestScore);
            }
        }
    }

    private void OnEnable()
    {
        //OnBestScoreChanged?.Invoke(BestScore);
        OnResetGame.AddListener(ResetScore);
    }

    public void AddScore(int score)
    {
        CurrentScore += score;
        BestScore = CurrentScore;
    }

    public void ResetScore()
    {
        _currentScore = 0;
    }

    private void OnDisable()
    {
        OnResetGame.RemoveListener(ResetScore);
    }
}
