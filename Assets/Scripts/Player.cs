using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using SimpleJSON;
using UnityEngine.Networking;

public class Player : MonoBehaviour
{
    public Satellite satellite;
    private int updateAt = 0;
    private Boolean updating = false;
    private List<Satellite> satellites = new List<Satellite>();
    Ray ray;
    RaycastHit hit;
    public Text nameText;
    private Satellite selectedSatellite = null;
    IEnumerator Start()
    {
        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser){
            string url = "https://satellite-explorer-api.eu-gb.mybluemix.net/satPositions/51.02673/-1.399106/10";
            StartCoroutine(GetRequest(url));
        } else{
        

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
            // Debug.Log("Timed out");
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed){
            // Debug.Log("Unable to determine device location");
            yield break;
        } else {
            // Access granted and location value could be retrieved
            string msg = "Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp;
            // Debug.Log(msg);  
            string url = String.Format("https://satellite-explorer-api.eu-gb.mybluemix.net/satPositions/{0}/{1}/{2}", Input.location.lastData.latitude, Input.location.lastData.longitude, Input.location.lastData.altitude);
            StartCoroutine(GetRequest(url));
        }
        }
    }

    public IEnumerator GetRequest(string uri)
    {
        updating = true;
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Debug.Log(uri);
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
                // Debug.Log(msg);
                Boolean exists = false;
                APIResp resp = APIResp.CreateFromJSON(msg);
                foreach (var sat in resp.satellites) {
                    foreach (var satObj in satellites) {
                        Debug.Log(sat.satId);
                        Debug.Log(satObj.satId);
                        if (sat.satId == satObj.satId) {
                            Debug.Log("ayyyyyyyyyyyy");
                            satObj.SetDestination(new Vector3((sat.positions[1].satlongitude - Input.location.lastData.longitude)/10, (sat.positions[1].sataltitude - Input.location.lastData.altitude)/50, (sat.positions[1].satlatitude - Input.location.lastData.latitude)/10), 300);
                            exists = true;
                            break;
                        }
                    }
                    if (!exists)
                    {
                        Satellite satelliteObj = Instantiate(satellite, new Vector3((sat.positions[0].satlongitude - Input.location.lastData.longitude)/10, (sat.positions[0].sataltitude - Input.location.lastData.altitude)/50, (sat.positions[0].satlatitude - Input.location.lastData.latitude)/10), Quaternion.identity);
                        satelliteObj.SetDestination(new Vector3((sat.positions[1].satlongitude - Input.location.lastData.longitude)/10, (sat.positions[1].sataltitude - Input.location.lastData.altitude)/50, (sat.positions[1].satlatitude - Input.location.lastData.latitude)/10), 300);
                        satelliteObj.SetName(sat.satName);
                        satelliteObj.satId = sat.satId;
                        satellites.Add(satelliteObj);
                    }
                    exists = false;
                }
                updateAt = ((int) (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds) + 300;
                updating = false;
            }
        }
    }

    public void Update() {
        if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            for(int i = 0; i<Input.touchCount; i++)
            {
                ray = Camera.main.ScreenPointToRay(Input.GetTouch(i).position);
                if (Physics.Raycast(ray, out hit, Mathf.Infinity)){
                    if (selectedSatellite == null){
                        selectedSatellite = hit.transform.gameObject.GetComponent<Satellite>();
                    } else if (selectedSatellite == hit.transform.gameObject.GetComponent<Satellite>()) {
                        selectedSatellite = null;
                    } else {
                        selectedSatellite = hit.transform.gameObject.GetComponent<Satellite>();
                    }
                }
            }
        }

        if (selectedSatellite != null) {
            nameText.text = "You have selected: " + selectedSatellite.GetName();
        } else {
            nameText.text = "";
        }

        if ((int) (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds > updateAt && !updating){
            string url = String.Format("https://satellite-explorer-api.eu-gb.mybluemix.net/satPositions/{0}/{1}/{2}", Input.location.lastData.latitude, Input.location.lastData.longitude, Input.location.lastData.altitude);
            StartCoroutine(GetRequest(url));            
        }
    }

    private void checkTouch(Vector3 touchPos){
         Vector3 wp  = Camera.main.ScreenToWorldPoint(touchPos);
         
         Collider[] hitList = Physics.OverlapBox(wp, transform.localScale / 2, Quaternion.identity);
        foreach (Collider coll in hitList){
             // You can check if this is an object that you want to move with coll.tag == "MoveObject"
             coll.GetComponent<Satellite>().ToggleName();
         }
     }

    [Serializable]
    private class APIResp {
        public SatelliteData[] satellites;

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