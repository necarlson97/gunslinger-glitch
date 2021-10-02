using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Body : MonoBehaviour {
    private void OnTriggerEnter2D(Collider2D other) {
        // I've been shot!
        if (GetComponentInParent<NPC>()) {
            GetComponentInParent<NPC>().Shot(other);
        } else if (GetComponentInParent<Player>()) {
            GetComponentInParent<Player>().Shot(other);
        } else {
            Debug.Log("What did we shot? " + gameObject.name);
        }
    }
}
