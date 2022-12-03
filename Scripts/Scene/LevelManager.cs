using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static int levelId { get; set; }
    public static int avalibleMemory { get; private set;}

    public static List<IRestartable> restartableObjects { get; set; } = new List<IRestartable>();
    public static LevelManager instance { get; set; }
    public static Elevator elevator { get; set; }
    public static FreeCam freeCam { get; set; }
    public static GameAudioController audioController { get; set; }

    private const int LAST_LEVEL_ID = 15;

    private bool _IsLevelChanging = false;
    private void Awake() 
    {
        instance = this;

        avalibleMemory = LevelDataManager.GetLevelMemoryLimit(levelId);
    }
    private void Start() 
    {
        SetSavedData();
        StartCoroutine(ScreenFade());
    }
    private void OnDisable() 
    {
        if(!_IsLevelChanging) WriteLevelData(IsLevelPassed: false);
        restartableObjects.Clear();
    }
    public void SetSavedData()
    {
        LevelData CurrentLevelData = LevelDataManager.GetLevelDataById(levelId);
        if(CurrentLevelData is not null) IDEManager.codeEditor.inputField.text = CurrentLevelData.CodeEditorString;
    }
    public void RestartLevel()
    {
        if(restartableObjects is null) return;

        foreach (var RestartableObject in restartableObjects)
        {
            RestartableObject.SetInitialState();
        }

        IDEManager.interpreter.StopExexute();
    }
    public void ChangeLevel()
    {
        StartCoroutine(LevelChangeSequence());
    }
    private IEnumerator ScreenFade()
    {
        UIManager Manager = UIManager.instance;
        float AttenuationRate = 0.55f;
        UIManager.instance.blackScreenTransparency = 1;
        while (Manager.blackScreenTransparency > 0)
        {
            yield return new WaitForFixedUpdate();
            Manager.blackScreenTransparency = Manager.blackScreenTransparency - (AttenuationRate * Time.deltaTime);
        }
    }
    private IEnumerator LevelChangeSequence()
    {
        _IsLevelChanging = true;
        
        elevator.Close();
        IDEManager.interpreter.StopExexute();
        WriteLevelData(IsLevelPassed: true);

        UIManager Manager = UIManager.instance;
        float AttenuationRate = 0.55f;
        UIManager.instance.blackScreenTransparency = 0;
        while (Manager.blackScreenTransparency < 1)
        {
            yield return new WaitForFixedUpdate();
            Manager.blackScreenTransparency = Manager.blackScreenTransparency + (AttenuationRate * Time.deltaTime);
        }
        yield return new WaitForSeconds(0.1f);
        
        LoadNextLevel();
    }
    private void LoadNextLevel()
    {
        string NextSceneName = $"Level_{levelId + 1}";
        if(!DoesSceneExist(NextSceneName) || levelId + 1 == LAST_LEVEL_ID)
        {
            NextSceneName = "MainMenu";
        } else levelId++;

        LoadLevel(NextSceneName);
    }
    private void WriteLevelData(bool IsLevelPassed)
    {
        LevelDataManager.SetLevelInfoToJson(levelId, IDEManager.interpreter.memoryUsed, IDEManager.codeEditor.inputText, IsLevelPassed);
    }
    public void LoadLevel(int LevelId)
    {
        SceneManager.LoadScene(LevelId);
    }
    public void LoadLevel(string LevelName)
    {
        SceneManager.LoadScene(LevelName);
    }
    public bool DoesSceneExist(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;

        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            var scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            var lastSlash = scenePath.LastIndexOf("/");
            var sceneName = scenePath.Substring(lastSlash + 1, scenePath.LastIndexOf(".") - lastSlash - 1);

            if (string.Compare(name, sceneName, true) == 0)
                return true;
        }

        return false;
    }
}

public interface IRestartable
{
    void SetInitialState();
}
