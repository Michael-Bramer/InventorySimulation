using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
using System.IO;
using System.Text;
using System.Globalization;
using UnityEngine.SceneManagement;

public class SimulationManager : MonoBehaviour
{
    //Internal Variables
    public int QaMax = 60;
    public int QbMax = 48;

    private int PaMax = 0;
    private int PbMax = 0;

    //Simulation Status Configuration
    private Boolean pauseSimulation   = false;
    private Boolean restartSimulation = false;

    // Start is called before the first frame update
    void Start()
    {
        PaMax = Mathf.CeilToInt(0.3f * QaMax);
        PbMax = Mathf.CeilToInt(0.3f * QbMax);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
