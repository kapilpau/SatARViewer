using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Satellite : MonoBehaviour
{
    float t;
    public int satId;
    Vector3 startPosition;
    Vector3 target;
    float timeToReachTarget;
    private string name = "";
    Text textBox;
    Text TextBox;
     void Start()
     {
             startPosition = target = transform.position;
             t = 0;
             timeToReachTarget = 300;
             TextBox = (Text) Resources.Load("Prefabs/TextBox", typeof(Text));
             textBox = Instantiate(TextBox);
             textBox.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);
             textBox.gameObject.SetActive(false);
        }
     void Update() 
     {

            //  if (transform.position != target){
                //  Debug.Log("akjbfiusdbhfiusbhdfiushfiuwhiuh");
                // Debug.Log(Vector3.Dot(startPosition, target)*Time.deltaTime/timeToReachTarget);
            //  transform.position = Vector3.MoveTowards(transform.position, target, Vector3.Dot(startPosition, target)/timeToReachTarget);
            // }
     }



    public void ToggleName() {
        textBox.transform.position = Camera.main.WorldToScreenPoint(transform.position) - new Vector3(1, 1, 1);
        textBox.text = name;
        textBox.gameObject.SetActive(!textBox.gameObject.activeSelf);
        Debug.Log(name + " was tapped is " + textBox.gameObject.activeSelf);
    }

     public void SetDestination(Vector3 destination, float time)
     {
            this.t = 0;
            this.startPosition = transform.position;
            this.timeToReachTarget = time;
            this.target = destination; 
     }

     public void SetName(string name){
         this.name = name;
        // textBox.text = name;
     }

     public string GetName() {
         return name;
     }

}
