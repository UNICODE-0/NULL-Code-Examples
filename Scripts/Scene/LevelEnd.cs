using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEnd : MonoBehaviour
{
    [SerializeField] private GameObject _Selection;
    private bool OnChangingLevel = false;
    private void OnTriggerEnter(Collider other)
    {
        if(OnChangingLevel) return;

        if(other.tag == "Robot")
        {
            OnChangingLevel = true;
            _Selection.SetActive(false);
            
            LevelManager.instance.ChangeLevel();
            UIManager.instance.SetLevelEndUIState();
        } 
    }
}
