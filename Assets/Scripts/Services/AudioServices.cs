using UnityEngine;

public static class AudioServices
{
    public static VoiceInput VoiceInput { get; private set; }

    public static void RegisterVoiceInput(VoiceInput input)
    {
        VoiceInput = input;
    }
}

