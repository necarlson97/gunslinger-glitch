using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shooter : MonoBehaviour {
    // Script for both player and NPC that handles
    // ducking, shooting, etc

    // How much progress we have getting our gun out to shoot
    // At 1, we move our hand to our gun
    // At 50 the gun comes out
    // At 100 we shoot
    // If it is negative we shoot left
    internal float drawProgress = 0f;

    // A multiplier to make us faster or slower
    // at drawing our gun
    internal float drawSpeed = 1f;

    // Info that the NPC / Player script can use to control
    internal bool ducking = false;
    bool lastFrameDucking = false;
    internal int drawing = 0; // -1 for left, 1 for right

    // How long it takes to prepare a shot
    internal int reloadTime = 100;
    internal int reloadTimer = 0;

    // alive, dead, or injured
    internal string state = "alive";

    void Start() {
    }

    void FixedUpdate() {
        if (state == "alive") {
            UpdateGun();
        } else if (state == "dead") {
            // Don't show mid-reload for dead
            reloadTimer = 0;
        }

        UpdateSpriteVisuals();
        UpdateReloadVisuals();
    }

    public void NoInput() {
        // Clear inputs for this shooter
        drawing = 0;
    }

    void UpdateGun() {
        // Make sure bullet is inactive by default
        transform.Find("Gun/Bullet").gameObject.SetActive(false);

        // Play sound if we just started ducking
        // TODO unducking?
        if (lastFrameDucking != ducking) {
            PlaySound(ducking ? "crouch" : "stand");
            lastFrameDucking = ducking;
        }

        // Continue drawing weapon (or put it away)
        drawProgress += drawing * drawSpeed;
        if (drawing == 0) {
            drawProgress = 0;
        }

        // Always reload if we need to
        if (reloadTimer > 0) {
            reloadTimer -= 1;
        } else if (Mathf.Abs(drawProgress) > 100) {
            // If we are reloaded and drawn, shoot!
            reloadTimer = reloadTime;
            Shoot();
        }
    }

    void Shoot() {
        transform.Find("Gun/Bullet").gameObject.SetActive(true);
        var gun = transform.Find("Gun");
        if (drawProgress < 0) {
            gun.localRotation = Quaternion.Euler(0, 180, 0);
            SetPose("left shoot");
        } else {
            gun.localRotation = Quaternion.Euler(0, 0, 0);
            SetPose("right shoot");
        }

        PlayParticles(gun);
        PlaySound("shoot");
        Invoke("PlayShellSound", 1.5f);
    }

    void PlayShellSound() {
        PlaySound("eject-shell");
    }

    public void PlayParticles(Transform t) {
        var particles = t.GetComponentsInChildren<ParticleSystem>();
        foreach (var p in particles) {
            p.Play();
        }
    }

    public void PlaySound(string s, Transform t=null) {
        var a = GetComponentInChildren<AudioSource>();
        if (t != null) {
            a = t.GetComponentInChildren<AudioSource>();
        }
        a.clip = Resources.Load<AudioClip>("Sounds/"+s);
        a.Play();
    }

    void UpdateSpriteVisuals() {
        // Update our sprite to match whats going on
        if (state == "dead" || state == "injured") {
            SetPose(state);
            return;
        }

        var gun = transform.Find("Gun");
        var body = transform.Find("Body");

        // Must match sprite gun;
        var drawUp = 0.42f;
        var drawRight = 1f;
        
        var handPose = "idle";
        if (drawProgress > 50) {
            // Drawn right
            gun.localPosition = new Vector3(drawRight, drawUp, 0);
            handPose = "right draw";
        } else if (drawProgress > 1) {
            // Hand right
            gun.localPosition = new Vector3(0.2f, 0, 0);
            handPose = "right holster";
        } else if (drawProgress < -50) {
            // Drawn left
            gun.localPosition = new Vector3(-drawRight, drawUp, 0);
            handPose = "left draw";
        } else if (drawProgress < -1) {
            // Hand left
            gun.localPosition = new Vector3(-0.2f, 0, 0);
            handPose = "left holster";
        } else {
            // Hands up
            gun.localPosition = new Vector3(0, 0, 0);
        }
        SetPose(handPose);

        if (ducking) {
            body.localScale = new Vector3(1, 1, 1);
            body.localPosition = new Vector3(0, -0.5f, 0.1f);
            gun.localPosition += new Vector3(0, -drawUp, 0);
        } else {
            // Upright
            body.localScale = new Vector3(1, 2, 1);
            body.localPosition = new Vector3(0, 0, 0.1f);
        }
    }

    void UpdateReloadVisuals() {
        // Set the white text over gray to show reload progress
        var r = transform.Find("Reloading");
        if (reloadTimer == 0) {
            r.gameObject.SetActive(false);
            return;
        }
        r.gameObject.SetActive(true);
        var bg = "Reloading";
        int progress = (int) (ReloadProgress() * bg.Length);
        var fg = bg.Substring(0, progress);
        r.Find("FG Text").GetComponent<Text>().text = fg;

        // If we are almost about to shoot, keep gun where it is going
        if (ReloadProgress() > 0.8f || ReloadProgress() < 0.2f) {
            return;
        }
        // Show reloading
        var gun = transform.Find("Gun");
        if (drawProgress < 0) {
            SetPose("left reload");
            gun.localPosition = new Vector3(-0.2f, 0, 0);
        } else {
            SetPose("right reload");
            gun.localPosition = new Vector3(0.2f, 0, 0);
        }
    }

    void SetPose(string handPose="idle") {
        var standPose = ducking ? "down" : "up";
        var image = transform.Find("Sprite").GetComponent<SpriteRenderer>();
        image.sprite = Resources.Load<Sprite>("Glitch Gunslingers/"+standPose+" "+handPose);
    }

    public float ReloadProgress() {
        // 0f to 1f of how much we have reloaded
        return 1f - ((float) reloadTimer / (float) reloadTime);
    }

    void Update() {
        // Just for dev, it will be disabled for real run
        var debug = transform.Find("Debug").gameObject;
        if (FindObjectOfType<Player>().debugging) {
            debug.SetActive(true);
            var msg = "DEBUG";
            if (GetComponent<NPC>()) msg = GetComponent<NPC>().DebugMsg();
            else if (GetComponent<Player>()) msg = GetComponent<Player>().DebugMsg();
            else if (GetComponent<Boss>()) msg = GetComponent<Boss>().DebugMsg();
            debug.GetComponentInChildren<Text>().text = msg;
        } else {
            debug.SetActive(false);
        }
    }
}
