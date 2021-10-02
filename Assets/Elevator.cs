using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Elevator : MonoBehaviour {
    Player player;
    void Start() {
        player = GetComponentInParent<Player>();
    }

    void Update() {
        // Show the floor we are going to
        var t = GetComponentInChildren<Text>();
        t.text = "" + player.desiredLevel;

        // TODO nicer colors
        // If we are there, green
        if (player.OnTheWay()) t.color = Color.blue;
        else if (player.WaitingForButton()) {
            if (player.GoingDown()) t.color = Color.red;
            else t.color = Color.green;
        }
        // Otherwise, we are there
        else t.color = Color.black;
    }
}
