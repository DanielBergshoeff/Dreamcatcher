using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioClip ConnectPillar;
    public AudioClip CompleteArea;
    public AudioClip CompleteForm;

    private void Awake() {
        Instance = this;
    }
}
