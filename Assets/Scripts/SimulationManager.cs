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
        public int DayIndex;
        public int Qa_Stock;
        public int Qb_Stock;
        public int Qa_Sales;
        public int Qb_Sales;
    }

    //Public Simulation Information Display Variables
    public Text ProductAQuantity;
    public Text ProductBQuantity;
    public Text DayIndex;

    public UILineRenderer ProductALine;
    public UILineRenderer ProductBLine;

    public int UpdateInterval = 1;

    //Public Simulation Control Variables
    private static bool SimulationPaused   = false;
    private static bool SimulationComplete = false;
    private static bool SimulationReset    = false;
    private static bool SimulationSave     = false;

    //Private Simulation Trace Variables
    private List<RuntimeData> SimulationTrace = new List<RuntimeData>();

    //Simulation Input Variables
    public int SimulationDurration        = 60;
    public int MaxAStock                  = 60;
    public int MaxBStock                  = 30;
    public float PercentRestockThresholdA = 0.2f;
    public float PercentRestockThresholdB = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        //Public Simulation Control Variables
        SimulationPaused   = false;
        SimulationReset    = false;
        SimulationSave     = false;
        SimulationComplete = false;

    //Private Simulation Calculation Variables
        SimulationDurration = 60;
        MaxBStock           = 30;
        MaxAStock           = 60;
        SimulationPaused    = true;


        //Clear Points
        ProductALine.points.Clear();
        ProductBLine.points.Clear();

        //Initialize Product to MAx Stock
        ProductALine.points.Add(new Vector2(0, 0));
        ProductALine.points.Add(new Vector2(0, 0));

    }

    // Update is called once per frame
    void Update()
    {   
            if( SimulationSave)
            {
                
                if (SimulationTrace.Count > 0)
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
        
            if (!SimulationComplete)
            {
                CalculateSimulation();
                SimulationComplete = true;
                for(int i = 0; i < SimulationTrace.Count; i++)
                {
                    ProductALine.points.Add(new Vector2(SimulationTrace[i].DayIndex * 0.5f, SimulationTrace[i].Qa_Stock*0.5f));
                    ProductBLine.points.Add(new Vector2(SimulationTrace[i].DayIndex * 0.5f, SimulationTrace[i].Qb_Stock * 0.5f));
                }
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

        string path = @"Assets\OutputFiles\Inventory Simulation for "+ DateTime.Now.ToString("yyyymmddhhmmss") + @".txt";
        try
        {
            // Create the file, or overwrite if the file exists.
            using (FileStream fs = File.Create(path))
            {
                byte[] infoh = new UTF8Encoding(true).GetBytes("Day Index" + "," + "Qa Sales" + "," + "Qb Sales" + "," + "Qa Stock" + "," + "Qb Stock" + "\n");
                
                // Add some information to the file.
                fs.Write(infoh, 0, infoh.Length);

                for (int i = 0; i < SimulationTrace.Count; i++)
                {
                    byte[] info = new UTF8Encoding(true).GetBytes(SimulationTrace[i].DayIndex.ToString() + "," + SimulationTrace[i].Qa_Sales.ToString() 
                    + "," + SimulationTrace[i].Qb_Sales.ToString() + "," + SimulationTrace[i].Qa_Stock.ToString()
                    + "," + SimulationTrace[i].Qb_Stock.ToString() + "\n");
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

    public void CalculateSimulation()
    {
        RuntimeData TraceLineItem = new RuntimeData();
        int Total_ProductA = MaxAStock;
        int Total_ProductB = MaxBStock;
        int NextAPurchaseDate = 0;
        int NextBPurchaseDate = 0;
        int RestockLevelA = (int)Math.Ceiling(PercentRestockThresholdA * MaxAStock);
        int RestockLevelB = (int)Math.Ceiling(PercentRestockThresholdA * MaxAStock);
        int SalesA = 0;
        int SalesB = 0;

        for (int CurrentDay= 0; CurrentDay < SimulationDurration; CurrentDay++)
        {
            //Record Starting Totals
            TraceLineItem.Qa_Stock = Total_ProductA;
            TraceLineItem.Qb_Stock = Total_ProductB;
            TraceLineItem.DayIndex = CurrentDay;
            TraceLineItem.Qa_Sales = SalesA;
            TraceLineItem.Qb_Sales = SalesB;
            SimulationTrace.Add(item: TraceLineItem);

            //When It is Time for a Customer to Order Product A
            if (CurrentDay == NextAPurchaseDate)
            {
                NextAPurchaseDate = CurrentDay + (int)UnityEngine.Random.Range(1, 4);
                int ASalesAmount  = (int)UnityEngine.Random.Range(1,(int)(0.2f * Total_ProductA));
                Total_ProductA   -= ASalesAmount;
                SalesA           += ASalesAmount;
            }

            //When It is Time for a Customer to Order Product B
            if (CurrentDay == NextBPurchaseDate)
            {
                NextBPurchaseDate = CurrentDay + (int)UnityEngine.Random.Range(1, 4);
                int BSalesAmount = (int)UnityEngine.Random.Range(1, (int)(0.2f * Total_ProductB));
                Total_ProductB -= BSalesAmount;
                SalesB += BSalesAmount;
            }

            //When It is Time to Restock Product A
            if (Total_ProductA < RestockLevelA)
            {
                Total_ProductA = MaxAStock;
            }

            //When It is Time to Restock Product B
            if (Total_ProductB < RestockLevelB)
            {
                Total_ProductB = MaxBStock;
            }

            //On the Last Day Record Totals At the End as Well
            if (CurrentDay+1 == SimulationDurration)
            {
                TraceLineItem.Qa_Stock = Total_ProductA;
                TraceLineItem.Qb_Stock = Total_ProductB;
                TraceLineItem.DayIndex = CurrentDay;
                SimulationTrace.Add(TraceLineItem);
            }
        }
    }
}
