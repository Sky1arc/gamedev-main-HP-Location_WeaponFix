using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentUI : MonoBehaviour
{
    void Awake()
    {
        // Check if an instance already exists
        if (FindObjectOfType<PersistentUI>() != null && FindObjectOfType<PersistentUI>() != this)
        {
            // If another instance exists, destroy this one
            Destroy(gameObject);
        }
        else
        {
            // Otherwise, make this instance persistent
            DontDestroyOnLoad(gameObject);
        }
    }
}