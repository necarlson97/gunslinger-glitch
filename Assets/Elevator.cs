using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Elevator : MonoBehaviour {

    // Just some nicer colors
    Color blue = new Color(0.02f, 0.23f, 0.50f);
    Color green = new Color(0.02f, 0.5f, 0.03f);
    Color red = new Color(0.5f, 0.27f, 0.02f);

    Player player;
    void Start() {
        player = GetComponentInParent<Player>();
    }

    void Update() {
        // Show the floor we are going to
        var t = GetComponentInChildren<Text>();
        t.text = "" + player.desiredLevel;

        // If we are there, green
        if (player.OnTheWay()) t.color = blue;
        else if (player.WaitingForButton()) {
            if (player.GoingDown()) t.color = red;
            else t.color = green;
        }
        // Otherwise, we are there
        else t.color = Color.black;
    }
}
