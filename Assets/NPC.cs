using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPC : MonoBehaviour {
    // TOOD all over this class we use 'find'
    // when we don't need to - but it likely doesn't matter
    Shooter s;

    // Are they friendly 'human'
    // or dangerous AI?
    public bool human = true;

    // Should we face left (-1) or right (1)
    internal int facing = -1;

    // How quickly they can draw
    public float difficulty = 0.5f;

    internal bool active = false;

    public static List<Texture2D> androidImgs;

    void Awake() {
        androidImgs = NPC.Shuffle<Texture2D>(new List<Texture2D>(Resources.LoadAll<Texture2D>("Androids")));
    }

    void Start() {
        // If we are to the left, look right
        if (transform.position.x < 0) {
            facing = 1;
        }
        s = GetComponent<Shooter>();
        s.drawSpeed = 2f;
        SetPortrait();
    }

    void ChangeDrawing() {
        // Should we keep drawing our gun / shootin?
        // Or change plans?

        // 20% chance to start shooting 
        // 25% chance to stop shooting 
        // 55% chance to continue
        var r = Random.Range(0f, 1f);
        if (r < 0.2f) {
            s.drawing = facing;
        } else if (r < 0.45f) {
            s.drawing = 0;
        }

        // Humans never shoot while ducking
        if (human && s.ducking) {
            s.drawing = 0;
        }
    }

    void ChangeDucking() {
        // Should we swap up standing / ducking?
        var p = FindObjectOfType<Player>();

        // 20% chance to duck
        // 30% chance to stand
        // 50% chance to continue
        var r = Random.Range(0f, 1f);
        if (r < 0.2f) {
            s.ducking = false;
        } else if (r < 0.3f) {
            s.ducking = true;
        }
    }

    public void Shot(Collider2D other) {
        // I've been shot!
        if (other.transform.IsChildOf(transform)) {
            // Nah it's just my bullet
            return;
        }
        FindObjectOfType<Player>().CheckShot(gameObject);
        s.state = "dead";
    }

    public void Deactivate() {
        // Someone was killed, so stop doing things
        s.NoInput();
        active = false;
        CancelInvoke("ChangeDrawing");
        CancelInvoke("ChangeDucking");
    }

    public void Activate() {
        // Player has arrived on floor, let's do our thing
        if (active) {
            return;
        }
        active = true;
        InvokeRepeating("ChangeDrawing", Random.Range(0f, 1f), 1f);
        InvokeRepeating("ChangeDucking", Random.Range(0f, 1f), 5f);
    }

    public string DebugMsg() {
        return string.Format("{0}\n{1} - {2}\ndrawing: {3}\nducking: {4}\nreloading: {5}\n{6}",
                human ? "Human" : "Robot", s.state, difficulty,
                s.drawProgress, s.ducking, s.ReloadProgress(),
                active ? "" : "DEACTIVATED");
    }

    public void SetPortrait() {
        // Select the portrait and quote for this npc

        // Select this image and cycle it to the back
        var img = androidImgs[0];
        androidImgs.RemoveAt(0);
        androidImgs.Add(img);

        transform.Find("Portrait/RawImage").GetComponent<RawImage>().texture = img;

        var tint = GetTint(img);
        var sprite = transform.Find("Sprite").GetComponent<SpriteRenderer>();
        sprite.color = tint;

        if (facing < 0) {
            transform.Find("Portrait").localPosition += new Vector3(8, 0, 0);
        }
    }

    // TODO
    public Color GetTint(Texture2D tex) {
        // Given an image, return the average color to use as a tint
        Color[] pixels = tex.GetPixels();
        int total = pixels.Length;

        float r = 0;
        float g = 0;
        float b = 0;

        for(int i = 0; i < total; i++){
            r += pixels[i].r;
            g += pixels[i].g;
            b += pixels[i].b;
        }
        return new Color((byte)(r / total) , (byte)(g / total) , (byte)(b / total) , 0);
    }

    public static List<T> Shuffle<T>(List<T> list) {
        // Can't believe we all have to write this
        for (int i = 0; i < list.Count; i++) {
            var temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
        return list;
    }
}
