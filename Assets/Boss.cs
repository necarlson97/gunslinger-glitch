using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Boss : MonoBehaviour {
    // TOOD all over this class we use 'find'
    // when we don't need to - but it likely doesn't matter
    internal Shooter s;

    internal bool active = false;

    public static List<Texture2D> androidImgs;
    public static List<Texture2D> humanImgs;
    public static List<string> androidQuotes;
    public static List<string> humanQuotes;
    
    void Start() {
        s = GetComponent<Shooter>();
        s.drawSpeed = 2f;
        s.reloadTime = 50;

        SetPortait();
    }

    public void SetPortait() {
        var rawImg = transform.Find("Portrait").GetComponentInChildren<RawImage>();
        rawImg.texture = Resources.Load<Texture2D>("Boss/scary-"+Random.Range(1,5));
    }

    public void Refresh() {
        s.state = "alive";
        ChangeQuote("Ah, welcome back");
        SetPortait();
    }

    void Update() {
        // Need to handle the boss visuals here
        if (s.state == "dead") {
            SetPose("dead");
            return;
        }

        if (s.drawProgress < 20) SetPose("up");
        else if (s.drawProgress < 50) SetPose("idle-2");
        else SetPose("idle-1");
    }

    void SetPose(string pose="idle-1") {
        var image = transform.Find("Boss Sprite").GetComponent<SpriteRenderer>();
        image.sprite = Resources.Load<Sprite>("Boss/"+pose);
    }

    void ChangeDrawing() {
        // Should we keep drawing our gun / shootin?
        // Or change plans?

        // 20% chance to start shooting 
        // 20% chance to stop shooting 
        // 55% chance to continue
        var r = Random.Range(0f, 1f);
        if (r < 0.2f) {
            s.drawing = 1;
        } else if (r < 0.4f) {
            s.drawing = 0;
        }
    }

    public void Shot(Collider2D other) {
        // I've been shot!
        if (other.transform.IsChildOf(transform)) {
            // Nah it's just my bullet
            return;
        }
        s.state = "dead";

        // All the gore
        s.PlayParticles(transform.Find("Android Gore"));
        s.PlayParticles(transform.Find("Human Gore"));

        s.PlaySound("android-die");
        FindObjectOfType<Player>().CheckShot(gameObject);
    }

    public void Deactivate() {
        // Someone was killed, so stop doing things
        s.NoInput();
        active = false;
        CancelInvoke("ChangeDrawing");

        if (s.state == "alive") {
            ChangeQuote("Figures.");
        } else {
            ChangeQuote("");
        }
    }

    public void Activate() {
        // Player has arrived on floor, let's do our thing
        if (active) {
            return;
        }
        active = true;
        s.drawing = 1; // Start guns out!
        InvokeRepeating("ChangeDrawing", Random.Range(0f, 1f), 0.2f);
    }

    public string DebugMsg() {
        return string.Format("{0}\n{1} - {2}\ndrawing: {3}\nducking: {4}\nreloading: {5}\n{6}",
                "boss", s.state, "tough",
                s.drawProgress, s.ducking, s.ReloadProgress(),
                active ? "" : "DEACTIVATED");
    }

    public void ChangeQuote(string q) {
        transform.Find("Speech/Text").GetComponent<Text>().text = q;
    }
}
