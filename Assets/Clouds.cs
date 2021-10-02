using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clouds : MonoBehaviour {
    // Simple class for having the texture
    // drift back and florth slowly
    float speed = 0.2f;

    void Update() {
        transform.position = new Vector3(Mathf.Sin(Time.time * speed) * 10,
            transform.position.y, transform.position.z);
    }
}
