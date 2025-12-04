using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Lasp;
using System.Linq;
using System.Collections.Generic;

public class MicConfiguration : MonoBehaviour
{
    private const string PREF_MIC = "SelectedMic";
    private const string PREF_DB = "MicDynamicRange";
    private const string PREF_GAIN = "MicGain";

    [Header("UI")]
    [SerializeField] private TMP_Dropdown micDropdown;
    [SerializeField] private Slider dynamicRangeSlider;
    [SerializeField] private Slider gainSlider;
    [SerializeField] private Slider micLevelBar;
    [SerializeField] private RectTransform micSelectedLevelBar;

    private List<DeviceDescriptor> micDevices;
    private AudioLevelTracker tracker;

    private float dynamicRange;
    private float gain;

    private void Awake()
    {
        tracker = GetComponent<AudioLevelTracker>();
    }

    private void Start()
    {
        LoadMicrophones();

        dynamicRange = PlayerPrefs.GetFloat(PREF_DB, dynamicRangeSlider.value);
        gain = PlayerPrefs.GetFloat(PREF_GAIN, gainSlider.value);

        dynamicRangeSlider.value = dynamicRange;
        gainSlider.value = gain;

        tracker.autoGain = false;
        tracker.dynamicRange = dynamicRange;
        tracker.gain = gain;

        dynamicRangeSlider.onValueChanged.AddListener(OnDynamicRangeSliderChanged);
        gainSlider.onValueChanged.AddListener(OnGainSliderChanged);

        UpdateMicVolumeBar();
    }

    private void Update()
    {
        micLevelBar.value = tracker.inputLevel;
    }

    private void LoadMicrophones()
    {
        micDropdown.ClearOptions();

        micDevices = AudioSystem.InputDevices.ToList();

        foreach (DeviceDescriptor device in micDevices)
        {
            micDropdown.options.Add(new TMP_Dropdown.OptionData(device.Name));
        }

        micDropdown.RefreshShownValue();

        if (micDevices.Count == 0)
            micDropdown.interactable = false;
    }

    public void OnMicChanged()
    {
        PlayerPrefs.SetInt(PREF_MIC, micDropdown.value);
        ApplySelectedMic();
    }

    private void ApplySelectedMic()
    {
        if (micDevices.Count == 0) return;

        DeviceDescriptor slectedDeviceName = micDevices[micDropdown.value];

    }

    private void OnDynamicRangeSliderChanged(float value)
    {
        dynamicRange = value;
        tracker.dynamicRange = value;

        PlayerPrefs.SetFloat(PREF_DB, value);
        PlayerPrefs.Save();

        UpdateMicVolumeBar();
    }

    private void OnGainSliderChanged(float value)
    {
        gain = value;
        tracker.gain = value;

        PlayerPrefs.SetFloat(PREF_GAIN, value);
        PlayerPrefs.Save();

        UpdateMicVolumeBar();
    }

    private void UpdateMicVolumeBar()
    {
        Vector2 size = micSelectedLevelBar.sizeDelta;
        Vector2 position = micSelectedLevelBar.anchoredPosition;

        float range = dynamicRange + gain > 60 ? 60 - gain : dynamicRange;
        size.x = range * (1000 / 60);
        position.x = -size.x / 2 - gain * (1000 / 60);
        
        micSelectedLevelBar.sizeDelta = size;
        micSelectedLevelBar.anchoredPosition = position;
    }
}
