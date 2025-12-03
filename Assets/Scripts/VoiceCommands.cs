using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.Speech;

public enum VoiceCommandType
{
    Accelerate,
    Brake,
    Left,
    Right,
    Straight,
    Stop,
    Pause
}

/// <summary>
/// Central voice command dispatcher.
/// Detects commands through Windows Speech and broadcasts:
/// - The recognized command (as enum)
/// - The intensity taken from VoiceInput history
/// 
/// Implemented as a safe Singleton.
/// </summary>
public class VoiceCommands : MonoBehaviour
{
    /// <summary>
    /// Singleton reference. Always points to the active VoiceCommands instance.
    /// </summary>
    public static VoiceCommands Instance { get; private set; }

    /// <summary>
    /// Fired when a voice command is detected.
    /// VoiceCommandType = which command
    /// float = intensity (0-1 normalized)
    /// </summary>
    public event Action<VoiceCommandType, float> OnCommandDetected;

    private KeywordRecognizer recognizer;
    private Dictionary<string, VoiceCommandType> commandMap;

    private void Awake()
    {
        // Ensure only one instance exists
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("A second VoiceCommands instance was created and destroyed.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Map spoken text => Command enum
        commandMap = new Dictionary<string, VoiceCommandType>
        {
            { "acelera", VoiceCommandType.Accelerate },
            { "frena", VoiceCommandType.Brake },
            { "izquierda", VoiceCommandType.Left },
            { "derecha", VoiceCommandType.Right },
            { "recto", VoiceCommandType.Straight },
            { "para", VoiceCommandType.Stop },
            { "pausa", VoiceCommandType.Pause },
        };

        recognizer = new KeywordRecognizer(commandMap.Keys.ToArray());
        recognizer.OnPhraseRecognized += OnPhraseRecognized;
        recognizer.Start();

        Debug.Log("Reconocimiento de voz ACTIVADO");
    }

    /// <summary>
    /// Call this method when a command is detected.
    /// It raises the event and notifies all listeners.
    /// </summary>
    /// <param name="command">The recognized speech command.</param>
    /// <param name="intensity">The intensity of the voice when saying the command.</param>
    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        // Convert recognized text => enum
        VoiceCommandType command = commandMap[args.text];

        // Compute intensity from VoiceInput history
        float secondsAgo = 0.5f + args.text.Length * 0.05f;
        float intensity = AudioServices.VoiceInput.GetVolumeAgo(secondsAgo);

        Debug.Log($"Comando \"{args.text}\" = {command} con intensidad {intensity}");

        // Fire event
        OnCommandDetected?.Invoke(command, intensity);
    }

    private void OnDestroy()
    {
        if (recognizer != null && recognizer.IsRunning) recognizer.Stop();
    }
}
