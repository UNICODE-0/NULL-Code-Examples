using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuController : MonoBehaviour
{
    [SerializeField] private LevelPanel[] _LevelPanels;
    [SerializeField] private GameObject _LevelSelectMenu;
    [SerializeField] private GameObject _SettingsMenu;
    [SerializeField] private GameObject _GuidePanel;
    private List<LevelData> LevelsData;
    private void Start()
    {
        LevelsData = LevelDataManager.GetLevelsData();
        if(LevelsData is null)
        {
            Debug.LogError("Failed to get levels data.");
            return;
        } else if(LevelsData.Count != _LevelPanels.Length)
        {
            Debug.LogError("The number of level panels and the number of levels in json do not match.");
            return;
        }

        _LevelPanels[0].EnableLevelAccess(LevelsData[0].MemoryUsed);
        for (int i = 0; i < LevelsData.Count - 1; i++)
        {
            if(LevelsData[i].IsPassed)
            {
                if(i + 1 == LevelsData.Count - 1)
                {
                    for (int j = 0; j < _LevelPanels.Length - 1; j++)
                    {
                        if(_LevelPanels[j].isMemoryOverflow)
                        {
                            _LevelPanels[i + 1].DisableLevelAccess();
                            return;
                        }
                    }
                } 

                _LevelPanels[i + 1].EnableLevelAccess(LevelsData[i + 1].MemoryUsed);
            } else
            {
                _LevelPanels[i + 1].DisableLevelAccess();
            }
        }
    }
    public void OpenLevelSelectMenu()
    {
        _SettingsMenu.SetActive(false);
        _GuidePanel.SetActive(false);

        if(_LevelSelectMenu.activeSelf) _LevelSelectMenu.SetActive(false);
        else _LevelSelectMenu.SetActive(true);
    }
    public void OpenSettingsMenu()
    {
        _LevelSelectMenu.SetActive(false);
        _GuidePanel.SetActive(false);

        if(_SettingsMenu.activeSelf) _SettingsMenu.SetActive(false);
        else _SettingsMenu.SetActive(true);
    }
    public void OpenGudePanel()
    {
        _LevelSelectMenu.SetActive(false);
        _SettingsMenu.SetActive(false);

        if(_GuidePanel.activeSelf) _GuidePanel.SetActive(false);
        else _GuidePanel.SetActive(true);
    }
    public void ExitGame()
    {
        Application.Quit();
    }
}
