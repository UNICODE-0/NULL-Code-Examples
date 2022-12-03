using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelPanel : MonoBehaviour
{
    [SerializeField] private int _LevelId;
    [Space]
    [SerializeField] private TMP_Text _LevelUsedMemoryText;
    [SerializeField] private TMP_Text _LevelButtonText;
    [SerializeField] private Button _LevelButton;

    private const float RED_COLOR_OVERFLOW = 1.5f; // Multiplier

    private bool _IsMemoryOverflow;
    public bool isMemoryOverflow
    {
        get { return _IsMemoryOverflow; }
    }
    
    public void EnableLevelAccess(int MemoryUsed = 0)
    {
        _LevelButton.interactable = true;
        Color ButtonTextColor = _LevelButtonText.color;
        _LevelButtonText.color = new Color(ButtonTextColor.r,ButtonTextColor.g,ButtonTextColor.b,1f);

        int MemoryAvalible = LevelDataManager.GetLevelMemoryLimit(_LevelId);
        string MemoryUsedString = "";
        if(MemoryUsed > MemoryAvalible * RED_COLOR_OVERFLOW)
        {
            MemoryUsedString = $"Memory used: <color=#CC2222>{MemoryUsed}/{MemoryAvalible}</color>";
            _IsMemoryOverflow = true;
        } else if (MemoryUsed > MemoryAvalible)
        {
            MemoryUsedString = $"Memory used: <color=#DE8300>{MemoryUsed}/{MemoryAvalible}</color>";
            _IsMemoryOverflow = true;
        }else
        {
            MemoryUsedString = $"Memory used: {MemoryUsed}/{MemoryAvalible}";
            _IsMemoryOverflow = false;
        }
        _LevelUsedMemoryText.text = MemoryUsedString;
    }
    public void DisableLevelAccess()
    {
        _LevelButton.interactable = false;
        Color ButtonTextColor = _LevelButtonText.color;
        _LevelButtonText.color = new Color(ButtonTextColor.r,ButtonTextColor.g,ButtonTextColor.b,0.55f);
        _LevelUsedMemoryText.text = $"Memory used: 0/{LevelDataManager.GetLevelMemoryLimit(_LevelId)}";
    }
    public void LoadLevel(string Name)
    {
        LevelManager.levelId = _LevelId;
        SceneManager.LoadScene(Name);
    }
}
