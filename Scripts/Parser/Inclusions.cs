using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class Inclusions : MonoBehaviour
{
    Dictionary<string,string> _InclusionsData = new Dictionary<string, string>();
    public Dictionary<string,string> inclusionsData
    {
        get { return _InclusionsData; }
    }
    private void Awake() 
    {
        IDEManager.inclusionsInfo = this;
    }
    
    private void Start() 
    {
        Initialize();
    }
    private void Initialize() 
    {
        string InclusionsPath = Application.streamingAssetsPath + "/Include/";
        string ConfigPath = Application.streamingAssetsPath + "/Configs/Include.cfg";

        try
        {
            string[] InclusionNames = File.ReadAllLines(ConfigPath);
            foreach (var IncluionName in InclusionNames)
            {
                string IncluionPath = InclusionsPath + IncluionName + ".inc";

                if(!_InclusionsData.ContainsKey(IncluionName) && IncluionName.Length > 0) 
                _InclusionsData.Add(IncluionName, File.ReadAllText(IncluionPath).Replace("\r",""));
            }
        } catch(Exception ex)
        {
            Debug.LogWarning(ex.Message);
            IDEManager.console.EPrint(ex.Message);
        }
    }
}
