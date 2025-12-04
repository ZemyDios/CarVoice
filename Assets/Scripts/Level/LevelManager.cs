using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    private GameObject car;
    private float time = 0;

    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject carPrefab;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject winMenu;

    private void Awake()
    {
        Instance = this;
        CurrentState = LevelState.Countdown;
        ChangeState(LevelState.PreStart);
    }

    private void OnEnable()
    {
        // Subscribe to VoiceCommands events
        if (VoiceCommands.Instance != null)
            VoiceCommands.Instance.OnCommandDetected += OnVoiceCommand;
    }

    private void OnDisable()
    {
        // Unsubscribe
        if (VoiceCommands.Instance != null)
            VoiceCommands.Instance.OnCommandDetected -= OnVoiceCommand;
    }

    private void Update()
    {
        if (CurrentState == LevelState.Running)
        {
            time += Time.deltaTime;
            // Update timer UI
            if (timerText) timerText.text = ((int)time / 60).ToString("00") + ":" + ((int)time % 60).ToString("00");
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (CurrentState == LevelState.Running) PauseLevel();
            else if (CurrentState == LevelState.Paused) UnPause();
        }
    }

    public void ChangeState(LevelState newState)
    {
        if (CurrentState == newState) return;
        CurrentState = newState;

        switch (newState)
        {
            case LevelState.PreStart:
                PreStart();
                break;

            case LevelState.Countdown:
                OnCountdownStarted?.Invoke();
                StartCoroutine(StartCountdownAnimation());
                break;

            case LevelState.Running:
                OnLevelStarted?.Invoke();
                if (timerText) timerText.gameObject.SetActive(true);
                car.GetComponent<CarController>().enabled = true; // Activate car controller
                break;

            case LevelState.Paused:
                OnLevelPaused?.Invoke();
                if (timerText) timerText.gameObject.SetActive(false);
                pauseMenu.SetActive(true);
                Time.timeScale = 0;
                break;

            case LevelState.Finished:
                OnLevelFinished?.Invoke();
                if (timerText) timerText.gameObject.SetActive(false);
                car.GetComponent<CarController>().enabled = false; // Deactivate car controller
                winMenu.SetActive(true);
                if (timerText) winMenu.transform.Find("Time").GetComponent<TextMeshProUGUI>().text = "Time: " + timerText.text;
                break;
        }
    }

    private void PreStart()
    {
        car = Instantiate(carPrefab, spawnPoint.position, Quaternion.identity);
        car.GetComponent<CarController>().enabled = false; // Deactivate car controller

        ChangeState(LevelState.Countdown);
    }

    /// <summary>
    /// Displays a countdown sequence: 3, 2, 1, GO!
    /// with scale + fade animation.
    /// </summary>
    private IEnumerator StartCountdownAnimation()
    {
        yield return StartCoroutine(AnimateCount("3"));
        yield return StartCoroutine(AnimateCount("2"));
        yield return StartCoroutine(AnimateCount("1"));
        yield return StartCoroutine(AnimateCount("GO!"));

        countdownText.gameObject.SetActive(false);

        StartLevel();
    }

    /// <summary>
    /// Animates a single countdown number:
    /// - Fade in
    /// - Scale pop
    /// - Fade out
    /// </summary>
    private IEnumerator AnimateCount(string text)
    {
        countdownText.text = text;
        countdownText.alpha = 0f;
        countdownText.transform.localScale = Vector3.one * 0.2f;

        countdownText.gameObject.SetActive(true);

        float t = 0f;

        // Fade + scale in
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            float p = t / 0.3f;

            countdownText.alpha = p;
            countdownText.transform.localScale = Vector3.Lerp(Vector3.one * 0.2f, Vector3.one, p);

            yield return null;
        }

        // Stay visible for a moment
        yield return new WaitForSeconds(0.4f);

        // Fade out
        t = 0f;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            float p = t / 0.3f;

            countdownText.alpha = 1f - p;

            yield return null;
        }
    }

    public void UnPause()
    {
        if (CurrentState != LevelState.Paused) return;
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        ChangeState(LevelState.Running);
    }

    private void OnVoiceCommand(VoiceCommandType command, float intensity)
    {
        if (command == VoiceCommandType.Pause)
        {
            if (CurrentState == LevelState.Running) PauseLevel();
            else if (CurrentState == LevelState.Paused) UnPause();
        }
    }

    public void RestartLevel()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
        Time.timeScale = 1f;
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        Time.timeScale = 1f;
    }

    // Public methods to change State
    public void StartCountdown() => ChangeState(LevelState.Countdown);
    public void StartLevel() => ChangeState(LevelState.Running);
    public void PauseLevel() => ChangeState(LevelState.Paused);
    public void FinishLevel() => ChangeState(LevelState.Finished);
}
