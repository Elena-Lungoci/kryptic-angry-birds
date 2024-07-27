using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uduino;
using System.Linq;

public class GetSensorsInput : MonoBehaviour
{
    // Start is called before the first frame update
    private List<float> prevForces = new();
    // public float force;
    public float force
    {
        get { return prevForces.Count > 0 ? prevForces.Average() : 0f; }
    }
    public float dForce
    {
        get { return prevForces.Count > 1 ? prevForces.Take(prevForces.Count - 1).Select((v, i) => prevForces[i + 1] - v).Average() : 0f; }
    }

    // public float y;
    private List<float> prevYs = new();
    public float y {
        get { return prevYs.Count > 0 ? prevYs.Average() : 0f; }
    }
    void Start()
    {
        UduinoManager.Instance.OnValueReceived += ReadIMU;
    }

    // Update is called once per frame
    void Update()
    {
    
    }
   public void ReadIMU (string data, UduinoDevice device) {
        //Debug.Log(data);
        string[] values = data.Split('/');
        if(values[0] == "r"){
            if (values.Length == 5) // Rotation of the first one 
            {
                // float w = float.Parse(values[1]);
                // y = float.Parse(values[2]); //should be x
                // y = float.Parse(values[3]); 
                // float z = float.Parse(values[4]);
                // Debug.Log("Gyro:" + x + " " + y + " " + z);
                prevYs.Add( float.Parse(values[3]));
                 if (prevYs.Count > 5) prevYs.RemoveAt(0);
            } else if (values.Length != 5)
            {
                Debug.LogWarning(data);
            }
        }
        if(values[0] == "a"){
            if (values.Length == 2) 
            {
                // force = float.Parse(values[1]);
                prevForces.Add(float.Parse(values[1]));
                if (prevForces.Count > 10) prevForces.RemoveAt(0);
                // Debug.Log("Force:" + force);
                
            } else if (values.Length != 2)
            {
                Debug.LogWarning(data);
            }
        }
        
    }
}
