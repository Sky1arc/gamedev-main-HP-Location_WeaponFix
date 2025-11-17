using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    void Awake()
    {
        // This tells Unity not to destroy this GameObject when a new scene loads.
        DontDestroyOnLoad(this.gameObject);
    }
}