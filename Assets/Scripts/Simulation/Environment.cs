using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment : MonoBehaviour
{
    private VisibilityHandler visbilityHandler;
    private VisibilityPlaneGenerator visibilityPlaneGenerator;

    void Start()
    {
        visbilityHandler = GetComponentInChildren<VisibilityHandler>();
        visibilityPlaneGenerator = GetComponentInChildren<VisibilityPlaneGenerator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
