using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 public class Parallax : MonoBehaviour {

     // % Distance between each layer
     float gapSize = 0.2f;
     
     void Update () {
         // For each successive background...
        var p = GameObject.Find("Player").transform;
        var distance = 1f;
        foreach (Transform t in transform) {
            distance -= gapSize;
            t.position = new Vector3(t.position.x, p.position.y * distance, t.position.z);
        }
     }
 }