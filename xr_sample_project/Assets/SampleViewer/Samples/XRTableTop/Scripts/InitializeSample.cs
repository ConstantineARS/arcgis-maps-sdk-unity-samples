using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitializeSample : MonoBehaviour
{   
    [SerializeField] private GameObject visionXROrigin;
    [SerializeField] private GameObject xrOrigin;
    void Start()
    {
#if UNITY_VISIONOS
        visionXROrigin.gameObject.SetActive(true);
        xrOrigin.gameObject.SetActive(false);
#else
        visionXROrigin.gameObject.SetActive(false);
        xrOrigin.gameObject.SetActive(true);
#endif
    }
}
