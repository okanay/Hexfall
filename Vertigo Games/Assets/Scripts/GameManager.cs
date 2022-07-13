using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Awake()
    { 
        Application.targetFrameRate = Screen.currentResolution.refreshRate;
    
#if UNITY_EDITOR
        Application.targetFrameRate = Int32.MaxValue;
#endif
        
    }
}
