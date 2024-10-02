using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicSourceContainer : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
