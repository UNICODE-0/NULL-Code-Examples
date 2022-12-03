using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
public class MenuAudioController : MonoBehaviour
{
    [SerializeField] private AudioSource _Ambient;
    [SerializeField] private AudioSource _ButtonSound;
    // private void Start()
    // {
    //     _Ambient.Play();
    // }
    public void PlayButtonSound()
    {
        _ButtonSound.Play();
    }
}
