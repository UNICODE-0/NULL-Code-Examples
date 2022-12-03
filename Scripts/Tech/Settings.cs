using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [SerializeField] private AudioMixer _AudioMixer;
    [SerializeField] private Slider _AudioSlider;
    [SerializeField] private Toggle _SyntaxHighlightToggle;

    public static bool syntaxHighlight { get; private set; } = true;
    public static float audioLevel { get; private set; } = 1f;
    private void Awake() 
    {
        _AudioSlider.value = audioLevel;
        _SyntaxHighlightToggle.isOn = syntaxHighlight;
    }
    public void ChangeVolume(float Level)
    {
        audioLevel = Level;
        _AudioMixer.SetFloat("Master", Mathf.Log10(Level) * 20);
    }
    public void ChangeSyntaxHighlight(bool State)
    {
        syntaxHighlight = State;
    }
}
