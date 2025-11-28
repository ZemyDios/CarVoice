using UnityEngine;

public class SoundReaction : MonoBehaviour
{
    [SerializeField] float scaleMultiplier = 1;
    Vector3 startingScale;

    private void Start()
    {
        startingScale = transform.localScale;
    }

    private void Update()
    {
        float volume = AudioServices.VoiceInput.Volume;
        transform.localScale = startingScale * (volume * scaleMultiplier + 1);
    }
}
