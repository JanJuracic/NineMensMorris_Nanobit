using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class KeyboardInputHandler : MonoBehaviour
{
    [SerializeField] UnityEvent OnEscapePressed;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnEscapePressed.Invoke();
        }
    }
}
