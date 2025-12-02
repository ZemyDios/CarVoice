using System;
using TMPro;
using UnityEngine;

public enum LevelState
{
    PreStart,   
    Countdown,  
    Running,    
    Paused,     
    Finished    
}

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public LevelState CurrentState { get; private set; }

    // Public events to reacto to new State
    public event Action OnCountdownStarted;
    public event Action OnLevelStarted;
    public event Action OnLevelPaused;
    public event Action OnLevelFinished;

    private float time = 0;
    [SerializeField] private TextMeshProUGUI timerText;

    private void Awake()
    {
        Instance = this;
        ChangeState(LevelState.Running);
    }

    private void Update()
    {
        if (CurrentState == LevelState.Running)
        {
            time += Time.deltaTime;
            // Update timer UI
            if (timerText) timerText.text = (time / 60).ToString("00") + ":" + (time % 60).ToString("00");
        }
    }

    public void ChangeState(LevelState newState)
    {
        if (CurrentState == newState) return;
        CurrentState = newState;

        switch (newState)
        {
            case LevelState.PreStart:

                break;

            case LevelState.Countdown:
                OnCountdownStarted?.Invoke();
                break;

            case LevelState.Running:
                OnLevelStarted?.Invoke();
                break;

            case LevelState.Paused:
                OnLevelPaused?.Invoke();
                break;

            case LevelState.Finished:
                OnLevelFinished?.Invoke();
                break;
        }
    }

    // Public methods to change State
    public void StartCountdown() => ChangeState(LevelState.Countdown);
    public void StartLevel() => ChangeState(LevelState.Running);
    public void PauseLevel() => ChangeState(LevelState.Paused);
    public void FinishLevel() => ChangeState(LevelState.Finished);
}
