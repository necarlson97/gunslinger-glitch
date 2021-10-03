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
    public float difficulty = 0.1f;

    internal bool active = false;

    public static List<Texture2D> androidImgs;
    public static List<Texture2D> humanImgs;
    // The clue below their image
    public static List<string> androidQuotes;
    public static List<string> humanQuotes;
    // Just something to say if they survived
    public static List<string> androidQuips;
    public static List<string> humanQuips;

    // For stats on how the player has done
    public static int androidImgsSeen = 0;
    public static int humanImgsSeen = 0;
    public static int androidQuotesSeen = 0;
    public static int humanQuotesSeen = 0;

    void Awake() {
        // Load and shuffle images
        androidImgs = NPC.Shuffle<Texture2D>(new List<Texture2D>(Resources.LoadAll<Texture2D>("Androids")));
        humanImgs = NPC.Shuffle<Texture2D>(new List<Texture2D>(Resources.LoadAll<Texture2D>("Humans")));
        
        // Load and shuffle quotes (as each line from text)
        var t = (TextAsset) Resources.Load("android-quotes", typeof(TextAsset));
        androidQuotes = NPC.Shuffle<string>(new List<string>(t.text.Split('\n')));
        t = (TextAsset) Resources.Load("human-quotes", typeof(TextAsset));
        humanQuotes = NPC.Shuffle<string>(new List<string>(t.text.Split('\n')));

        // Load and shuffle taunts/congradulations
        t = (TextAsset) Resources.Load("android-quips", typeof(TextAsset));
        androidQuips = NPC.Shuffle<string>(new List<string>(t.text.Split('\n')));
        t = (TextAsset) Resources.Load("human-quips", typeof(TextAsset));
        humanQuips = NPC.Shuffle<string>(new List<string>(t.text.Split('\n')));
    }

    void Start() {
        // If we are to the left, look right
        if (transform.position.x < 0) {
            facing = 1;
        }
        s = GetComponent<Shooter>();
        s.drawSpeed = difficulty;
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
        s.state = "dead";

        var gore = transform.Find("Android Gore");
        if (human) gore = transform.Find("Human Gore");
        s.PlayParticles(gore);

        if (human) s.PlaySound("human-die");
        else s.PlaySound("android-die");

        FindObjectOfType<Player>().CheckShot(gameObject);
    }

    public void Deactivate() {
        // Someone was killed, so stop doing things
        s.NoInput();
        active = false;
        CancelInvoke("ChangeDrawing");
        CancelInvoke("ChangeDucking");

        var quip = "";
        if (s.state == "alive") {
            // TODO list of quotes
            if (human) quip = Cycle(humanQuips);
            else quip = Cycle(androidQuips);;
        } 
        ChangeQuote(quip);
    }

    public void Activate() {
        // Player has arrived on floor, let's do our thing
        if (active) {
            return;
        }
        active = true;
        InvokeRepeating("ChangeDrawing", Random.Range(0f, 1f), 1f);
        InvokeRepeating("ChangeDucking", Random.Range(0f, 1f), 0.5f);

        // TODO not exactly sure where is best to put *shrug*
        // Boost stats!
        if (human) {
            humanImgsSeen++;
            humanQuotesSeen++;
        } else {
            androidImgsSeen++;
            androidQuotesSeen++;
        }
    }

    public string DebugMsg() {
        return string.Format("{0}\n{1} - {2}\ndrawing: {3}\nducking: {4}\nreloading: {5}\n{6}",
                human ? "Human" : "Robot", s.state, difficulty,
                s.drawProgress, s.ducking, s.ReloadProgress(),
                active ? "" : "DEACTIVATED");
    }

    public T Cycle<T>(List<T> l) {
        // Dequee the front of the list,
        // pushing it to the back for later
        var ret = l[0];
        l.RemoveAt(0);
        l.Add(ret);
        return ret;
    }

    public void SetPortrait() {
        // Select the portrait and quote for this npc
        // Select an image and cycle it to the back
        var textures = androidImgs;
        if (human) textures = humanImgs;
        var img = Cycle(textures);

        // Select a quote
        var quotes = androidQuotes;
        if (human) quotes = humanQuotes;

        // Display quote
        ChangeQuote(Cycle(quotes));

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

    public void ChangeQuote(string q) {
        transform.Find("Speech/Text").GetComponent<Text>().text = q;
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
