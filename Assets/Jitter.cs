using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jitter : MonoBehaviour {
    float size = 5f;
    Vector3 start;
    void Start() {
        start = transform.localPosition;
    }
    void Update() {
        transform.localPosition = start + new Vector3(
            Random.Range(-size, size), Random.Range(-size, size), Random.Range(-size, size));
    }
}
