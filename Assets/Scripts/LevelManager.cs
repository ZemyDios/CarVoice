using System;
using Unity.VisualScripting;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public event Action LevelFinished;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        LevelFinished += OnLevelFinished;
    }

    public void TriggerLevelFinished()
    {
        LevelFinished?.Invoke();
    }

    private void OnLevelFinished()
    {
        Debug.Log("Level Finished");
    }
}
