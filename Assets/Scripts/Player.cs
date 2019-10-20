using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using SimpleJSON;
using UnityEngine.Networking;

public class Player : MonoBehaviour
{
    public Satellite satellite;
    IEnumerator Start()
    {
        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser){
            string url = "http://9.240.41.246:3000/satPositions/51.02673/-1.399106";
            StartCoroutine(GetRequest(url));
        } else{
            Debug.Log("IS ENABLED BITCH");
        

        // Start service before querying location
        Input.location.Start();

        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
        if (maxWait < 1)
        {
            Debug.Log("Timed out");
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed){
            Debug.Log("Unable to determine device location");
            yield break;
        } else {
            // Access granted and location value could be retrieved
            string msg = "Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp;
            Debug.Log(msg);  
            string url = String.Format("http://9.240.41.246:3000/satPositions/{0}/{1}", Input.location.lastData.latitude, Input.location.lastData.longitude);
            StartCoroutine(GetRequest(url));
        }
        }
    }

    IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError)
            {
                string msg = pages[page] + ": Error: " + webRequest.error;
                Debug.Log(msg);
            }
            else
            {
                string msg = webRequest.downloadHandler.text;
                APIResp resp = APIResp.CreateFromJSON(msg);
                foreach (var sat in resp.response) {
                    Instantiate(satellite, new Vector3((sat.positions[0].satlongitude - Input.location.lastData.longitude)/10, (sat.positions[0].sataltitude - Input.location.lastData.altitude)/50, (sat.positions[0].satlatitude - Input.location.lastData.latitude)/10), Quaternion.identity);
                }
            }
        }
    }

    [Serializable]
    private class APIResp {
        public SatelliteData[] response;

        public static APIResp CreateFromJSON(string jsonString) {
            return JsonUtility.FromJson<APIResp>(jsonString);
        }
    }

    [Serializable]
    private class SatelliteData {
        public string satName;
        public int satId;
        public Position[] positions;

        public static SatelliteData CreateFromJSON(string jsonString) {
            return JsonUtility.FromJson<SatelliteData>(jsonString);
        }
    }

    [Serializable]
    private class Position {
        public float satlatitude;
        public float satlongitude;
        public int sataltitude;
        public float azimuth;
        public float elevation;
        public float ra;
        public float dec;
        public int timestamp;
    }

}