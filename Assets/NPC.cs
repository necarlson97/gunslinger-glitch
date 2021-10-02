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
    public static List<Texture2D> humanImgs;
    public static List<string> androidQuotes;
    public static List<string> humanQuotes;
    

    void Awake() {
        // Load and shuffle images
        androidImgs = NPC.Shuffle<Texture2D>(new List<Texture2D>(Resources.LoadAll<Texture2D>("Androids")));
        humanImgs = NPC.Shuffle<Texture2D>(new List<Texture2D>(Resources.LoadAll<Texture2D>("Humans")));
        
        // Load and shuffle quotes (as each line from text)
        var t = (TextAsset) Resources.Load("android-quotes", typeof(TextAsset));
        androidQuotes = NPC.Shuffle<string>(new List<string>(t.text.Split('\n')));
        t = (TextAsset) Resources.Load("human-quotes", typeof(TextAsset));
        humanQuotes = NPC.Shuffle<string>(new List<string>(t.text.Split('\n')));
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

        var gore = transform.Find("Android Gore");
        if(human) {
            gore = transform.Find("Human Gore");
        }
        gore.GetComponentInChildren<ParticleSystem>().Play();
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
        // Select an image and cycle it to the back
        var textures = androidImgs;
        if (human) textures = humanImgs;
        var img = textures[0];
        textures.RemoveAt(0);
        textures.Add(img);

        // Select a quote
        var quotes = androidQuotes;
        if (human) quotes = humanQuotes;
        var quote = quotes[0];
        quotes.RemoveAt(0);
        quotes.Add(quote);

        // Display quote (TODO tint?)
        transform.Find("Speech/Text").GetComponent<Text>().text = quote;

        // Get a random lightish color
        var tint = Color.HSVToRGB(Random.Range(0f, 1f), 0.2f, 1f);

        // Dispaly the tinted portrait
        var rawImg = transform.Find("Portrait").GetComponentInChildren<RawImage>();
        var imgBorder = transform.Find("Portrait").GetComponentInChildren<SpriteRenderer>();
        rawImg.texture = img;
        rawImg.color = tint;
        imgBorder.color = tint;

        // Tint the npc sprite
        var sprite = transform.Find("Sprite").GetComponent<SpriteRenderer>();
        sprite.color = tint;

        // Place the portrait off to the side
        if (facing < 0) {
            transform.Find("Portrait").localScale = new Vector3(-1, 1, 1);
        }
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
