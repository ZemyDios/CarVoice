using Lasp;
using UnityEngine;

public class VoiceInput : MonoBehaviour
{
    [Header("Lasp Trackers")]
    [SerializeField] private AudioLevelTracker volumeTracker;
    [SerializeField] private AudioLevelTracker lowsTracker;
    [SerializeField] private AudioLevelTracker highsTracker;

    // Public properties
    public float Volume => volumeTracker ? volumeTracker.normalizedLevel : 0f;
    public float Lows => lowsTracker ? lowsTracker.normalizedLevel : 0f;
    public float Highs => highsTracker ? highsTracker.normalizedLevel : 0f;

    // Volume history (last 1 second)
    private float[] volumeHistory = new float[20]; // 10 values (0.1s each)
    private int historyIndex = 0;
    private float timer = 0f;

    private void Awake()
    {
        AudioServices.RegisterVoiceInput(this);
    }

    private void Start()
    {
        volumeTracker.autoGain = false;
        // Load player preferences
        volumeTracker.gain = PlayerPrefs.GetFloat(MicConfiguration.PREF_GAIN, 10f);
        volumeTracker.dynamicRange = PlayerPrefs.GetFloat(MicConfiguration.PREF_DB, 25f);
    }

    void Update()
    {
        // Sampling timer
        timer += Time.deltaTime;

        if (timer >= 0.1f)
        {
            timer -= 0.1f;

            // Store current volume
            volumeHistory[historyIndex] = Volume;
            //Debug.Log(Volume);
            // Move circular index
            historyIndex++;
            if (historyIndex >= volumeHistory.Length)
                historyIndex = 0;
        }
    }

    /// <summary>
    /// Returns how loud the user was X seconds ago.
    /// Example: GetVolumeAgo(0.5f) -> volume half a second ago.
    /// </summary>
    public float GetVolumeAgo(float secondsAgo)
    {
        // If you want to change the sampling interval, make this a serialized field.
        float sampleInterval = 0.1f;

        int N = volumeHistory.Length;
        if (N == 0) return 0f;

        // Maximum seconds we can look back given the buffer size:
        float maxSeconds = (N - 1) * sampleInterval;
        secondsAgo = Mathf.Clamp(secondsAgo, 0f, maxSeconds);

        // Convert seconds to number of steps in the buffer
        int stepsAgo = Mathf.RoundToInt(secondsAgo / sampleInterval);

        // Index of the sample we want (historyIndex points to the next write position,
        // so last written sample is historyIndex - 1)
        int index = historyIndex - 1 - stepsAgo;

        // Wrap index into [0, N-1] safely
        index = ((index % N) + N) % N;

        // Neighbour indices with wrap-around
        int prev = (index - 1 + N) % N;
        int next = (index + 1) % N;

        // Return the maximum of previous, current and next sample
        return Mathf.Max(volumeHistory[prev], volumeHistory[index], volumeHistory[next]);
    }
}
