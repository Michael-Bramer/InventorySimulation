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
    //Structure Declaration to Store Runtime Data
    [System.Serializable] public struct RuntimeData
    {
        public float   TotalCount;
        public float   InteriorCount;
        public double  GeneratedX;
        public double  GeneratedY;
        public double  EstimatedPi;
    }

    //Public Simulation Information Display Variables
    public Text TotalNumberOfPoints;
    public Text NumberOfInteriorPoints;
    public Text EstimationOfPi;
    public  int UpdateInterval = 1;
    public UILineRenderer Boundaries;


    //Public Simulation Control Variables
    public static bool SimulationPaused = false;
    public static bool SimulationReset  = false;
    public static bool SimulationSave   = false;

    //Public Simulation Input Parameter Variables
    public float Radius = 5.0f;

    //Private Simulation Trace Variables
    private List<RuntimeData> SimulationTrace = new List<RuntimeData>();

    //Private Simulation Calculation Variables
    private float    Total_N    = 0;
    private float    Interior_N = 0;
    private float    Estimate   = 0;

    // Start is called before the first frame update
    void Start()
    {
        //Public Simulation Control Variables
        SimulationPaused = false;
        SimulationReset  = false;
        SimulationSave   = false;

        //Public Simulation Input Parameter Variables
        Radius = 5.0f;

        //Private Simulation Calculation Variables
        Total_N = 0;
        Interior_N = 0;
        Estimate = 0;
        SimulationPaused = true;
        Boundaries.points.Clear();
        for(int i = 0; i < 101; i++)
        {

            Boundaries.points.Add(new Vector2((i*Radius/100),(Mathf.Sqrt((Radius*Radius)-((i * Radius / 100)* (i * Radius / 100))))));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if( SimulationPaused )
        {
            if( SimulationSave)
            {
                if(SimulationTrace.Count > 0)
                {
                    GenerateFile();
                }       
                SimulationSave = false;
            }
            if( SimulationReset)
            {
                Scene scene = SceneManager.GetActiveScene(); 
                SceneManager.LoadScene(scene.name);
            }     
        }
        else{

            //Generate the Uniform Random Number
            for (int i = 0; i < UpdateInterval; i++)
            {
                double RandomX = UnityEngine.Random.Range(0.0f, Radius);
                double RandomY = UnityEngine.Random.Range(0.0f, Radius);
                Color PointColor = Color.white;

                //Calculate the Result
                if (Math.Sqrt((RandomX * RandomX) + (RandomY * RandomY)) <= Radius)
                {
                    Interior_N += 1;
                    PointColor = Color.blue;   
                }
                else
                {
                    PointColor = Color.green;
                }
                PointColor.a = 0.4f;
                Total_N += 1;
           
            
                Estimate = 4*((float)Interior_N / Total_N);

                //Generate the Interval Record for the Runtime Trace
                RuntimeData TraceLineItem;
                TraceLineItem.GeneratedX    = RandomX;
                TraceLineItem.GeneratedY    = RandomY;
                TraceLineItem.EstimatedPi   = Estimate;
                TraceLineItem.InteriorCount = Interior_N;
                TraceLineItem.TotalCount    = Total_N;
                SimulationTrace.Add(TraceLineItem);

                //Generate Visual
                UILineRenderer Point = UILineRenderer.Instantiate(Boundaries);
                Point.rectTransform.SetParent(Boundaries.rectTransform.parent);
                Point.rectTransform.anchoredPosition = Boundaries.rectTransform.anchoredPosition;
                Point.rectTransform.localScale = new Vector3(1f, 1f, 1f);
                Point.rectTransform.localPosition = new Vector3(Point.rectTransform.localPosition.x, Point.rectTransform.localPosition.y, 0f);
                Point.points.Clear();
                Point.color = PointColor;
                Point.points.Add(new Vector2((float)RandomX - 0.05f,(float)RandomY - 0.05f));
                Point.points.Add(new Vector2((float)RandomX + 0.05f, (float)RandomY + 0.05f));
            }

            //Set the Description Labels
            TotalNumberOfPoints.text    = Total_N.ToString();
            NumberOfInteriorPoints.text = Interior_N.ToString();
            EstimationOfPi.text = Estimate.ToString();
        }
    }
    public void PlaySimulation()
    {
        SimulationPaused = false;
    }
    public void PauseSimulation()
    {
        SimulationPaused = true;
    }
    public void ResetSimulation()
    {
        SimulationReset = true;
    }
    public void SaveSimulation()
    {
        SimulationSave = true;
    }
    public void GenerateFile()
    {

        string path = @"Assets\OutputFiles\Monte Carlo Simulation for "+ DateTime.Now.ToString("yyyymmddhhmmss") + @".txt";
        try
        {
            // Create the file, or overwrite if the file exists.
            using (FileStream fs = File.Create(path))
            {
                for(int i = 0; i < SimulationTrace.Count; i++)
                {
                    byte[] info = new UTF8Encoding(true).GetBytes(SimulationTrace[i].TotalCount.ToString() + "," + SimulationTrace[i].EstimatedPi.ToString() 
                    + "," + SimulationTrace[i].GeneratedX.ToString() + "," + SimulationTrace[i].GeneratedY.ToString() + "\n");
                    // Add some information to the file.
                    fs.Write(info, 0, info.Length);
                }
                SimulationTrace.Clear();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}
