using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

public class LevelDataManager : MonoBehaviour
{
    [SerializeField] private levelsInfo[] _LevelsInfo;
    [SerializeField] private static levelsInfo[] _StaticLevelsInfo;

    public int LevelsCount
    {
        get { return _LevelsInfo.Length; }
    }
    
    void Awake()
    {
        if(File.Exists(Application.persistentDataPath + "/LevelsData.json"))
        {
            List<LevelData> LevelsData = ReadListFromJSON<LevelData>("LevelsData.json");
            if(LevelsData.Count != LevelsCount)
            {
                List<LevelData> NewLevelsData = new List<LevelData>();
                for (int i = 0; i < LevelsCount; i++)
                {
                    if(i < LevelsData.Count) NewLevelsData.Add(LevelsData[i]);
                    else NewLevelsData.Add(new LevelData(_LevelsInfo[i].levelId,0,String.Empty));
                }

                InitializeLevelsDataJson(NewLevelsData);
            }
        } else
        {
            InitializeLevelsDataJson();
        }

        _StaticLevelsInfo = new levelsInfo[_LevelsInfo.Length];
        for (int i = 0; i < _LevelsInfo.Length; i++)
        {
            _StaticLevelsInfo[i] = new levelsInfo(_LevelsInfo[i]);
        }
    }
    private void InitializeLevelsDataJson(List<LevelData> DataToSet = null) 
    {
        List<LevelData> LevelsData;

        if(DataToSet is null)
        {
            LevelsData = new List<LevelData>();       
            for (int i = 0; i < LevelsCount; i++)
            {
                LevelsData.Add(new LevelData(_LevelsInfo[i].levelId,0,String.Empty));
            }
        } 
        else LevelsData = new List<LevelData>(DataToSet);
        
        WriteFile(GetPath("LevelsData.json"), JsonHelper.ToJson(LevelsData.ToArray()), FileMode.Create);
    }
    public static int GetLevelMemoryLimit(int LevelId)
    {
        if(LevelId < 1)
        {
            Debug.LogError($"Can't get level memeory limit by levelId: {LevelId}");
            return 0;
        } 
        if(LevelId > _StaticLevelsInfo.Length)
        {
            Debug.LogError($"Can't get level memeory limit by levelId: {LevelId}");
            return 0;
        } 

        return _StaticLevelsInfo[LevelId - 1].memoryLimit;
    }
    private static void WriteFile(string Path, string Data, FileMode Mode = FileMode.Truncate) 
    {
        FileStream fileStream = new FileStream(Path, Mode);

        using(StreamWriter writer = new StreamWriter(fileStream)) 
        {
            writer.Write(Data);
        }
    }
    public static void SaveToJSON<T>(List<T> Data, string Filename) 
    {
        string content = JsonHelper.ToJson<T>(Data.ToArray());
        WriteFile(GetPath(Filename), content);
    }
    public static void SaveToJSON<T>(T Data, string Filename) 
    {
        string content = JsonUtility.ToJson(Data);
        WriteFile(GetPath(Filename), content);
    }
    public static List<T> ReadListFromJSON<T>(string filename) 
    {
        string JsonString = ReadFile(GetPath(filename));

        if(string.IsNullOrEmpty(JsonString) || JsonString == "{}") 
        {
            return new List<T> ();
        }

        List<T> Data = JsonHelper.FromJson<T>(JsonString).ToList();

        return Data;
    }
    public static T ReadFromJSON<T>(string filename) 
    {
        string JsonString = ReadFile(GetPath(filename));

        if(string.IsNullOrEmpty(JsonString) || JsonString == "{}") 
        {
            return default(T);
        }

        T Data = JsonUtility.FromJson<T>(JsonString);

        return Data;
    }
    public static void SetLevelInfoToJson(int LevelId, int MemoryUsed, string CodeEditorString, bool IsPassed = true)
    {
        List<LevelData> Data = ReadListFromJSON<LevelData>("LevelsData.json");

        foreach (var LevelData in Data)
        {
            if(LevelData.LevelId == LevelId)
            {
                if(IsPassed)
                {
                    LevelData.IsPassed = true;
                    LevelData.MemoryUsed = MemoryUsed;
                }
                LevelData.CodeEditorString = CodeEditorString;
                break;
            }
        }

        SaveToJSON(Data,"LevelsData.json");
    }
    public static List<LevelData> GetLevelsData()
    {
        return ReadListFromJSON<LevelData>("LevelsData.json");
    }
    public static LevelData GetLevelDataById(int LevelId)
    {
        List<LevelData> Data = ReadListFromJSON<LevelData>("LevelsData.json");
        foreach (var LevelData in Data)
        {
            if(LevelData.LevelId == LevelId)
            {
                return LevelData;
            }
        }
        return null;
    }
    private static string ReadFile(string Path) 
    {
        if(File.Exists(Path)) 
        {
            using (StreamReader reader = new StreamReader(Path)) 
            {
                return reader.ReadToEnd ();
            }
        } else return String.Empty;
    }
    private static string GetPath(string filename) 
    {
        Debug.Log(Application.persistentDataPath );
        return Application.persistentDataPath + "/" + filename;
    }

    [Serializable]
    private class levelsInfo
    {
        [SerializeField] private int _LevelId;
        public int levelId
        {
            get { return _LevelId; }
        }
        [SerializeField] private int _MemoryLimit;
        public int memoryLimit
        {
            get { return _MemoryLimit; }
        }

        public levelsInfo(levelsInfo ToCopy)
        {
            this._LevelId = ToCopy.levelId;
            this._MemoryLimit = ToCopy.memoryLimit;
        }
    }
}

[Serializable]
public class LevelData
{
    public int LevelId;
    public bool IsPassed;
    public int MemoryUsed;
    public string CodeEditorString;
    public LevelData(int LevelId, int MemoryUsed, string CodeEditorString, bool IsPassed = false)
    {
        this.LevelId = LevelId;
        this.MemoryUsed = MemoryUsed;
        this.CodeEditorString = CodeEditorString;
        this.IsPassed = IsPassed;
    }
}

public static class JsonHelper 
{
    public static T[] FromJson<T>(string json) 
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    public static string ToJson<T>(T[] array) 
    {
        Wrapper<T> wrapper = new Wrapper<T> ();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper, true);
    }

    [Serializable]
    private class Wrapper<T> {

        public T[] Items;
    }
}
