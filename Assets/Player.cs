using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    
    Shooter s;
    public GameObject NPCPrefab;

    // The two opponents we are currently on a level with
    Dictionary<int, GameObject[]> opponents = new Dictionary<int, GameObject[]>();

    // How high in the structure we are, and what difficulty enemies
    // we are fighting
    int currentLevel = 0;
    int toLevel = 0;
    int levelHeight = 10;
    // TODO probs 10 idk
    int maxLevel = 5;

    // TODO default false
    public bool debugging = true;

    void Start() {
        s = GetComponent<Shooter>();
        s.drawSpeed = 2f;

        SpawnOpponents();
    }

    void SpawnOpponents() {
        // Respawn the entire building, placing enemies on floors 1-maxLevel
        // Create the opponents

        // TODO delete existing opponents
        for (int level=1; level<maxLevel+1; level++) {

            // Remove any existing opponents
            if (opponents.ContainsKey(level)) {
                foreach(var o in opponents[level]) {
                    GameObject.Destroy(o);
                }
            }

            var left = GameObject.Instantiate(NPCPrefab, new Vector3(-5, -3 + level * levelHeight, 0), Quaternion.identity);
            var right = GameObject.Instantiate(NPCPrefab, new Vector3(5, -3 + level * levelHeight, 0), Quaternion.identity);
            var os = new GameObject[]{left, right};

            // Decide who will be enemy
            var leftScript = left.GetComponent<NPC>();
            var rightScript = right.GetComponent<NPC>();

            leftScript.human = Random.Range(0f, 1f) > 0.5f;
            rightScript.human = !leftScript.human;

            // Set their difficulty dependent on level
            leftScript.difficulty += (float) level / 10f;
            rightScript.difficulty += (float) level / 10f;
            opponents[level] = os;
        }
    }

    bool GoToFloor(int floor) {
        // If we are in motion wait
        if (toLevel != currentLevel) {
            return false;
        }
        toLevel = floor;
        return true;
    }

    void ArriveOnFloor() {
        // if we got to a new floor, we are alive or respawning
        s.state = "alive";
        currentLevel = toLevel;
        if (currentLevel == 0) {
            SpawnOpponents();
            return;
        }
        if (currentLevel == maxLevel+1) {
            Debug.Log("You win!");
            return;
        }
        foreach (var o in opponents[currentLevel]) {
            o.GetComponent<NPC>().Activate();
        }
    }

    // Update is called once per frame
    void Update() {
        var toVec = new Vector3(0, -3 + toLevel * levelHeight, 0);

        // TODO gross and potentially buggy
        // If we are close, but not there yet, then we have just 'arrrived' probs
        if (transform.position != toVec && Vector3.Distance(transform.position, toVec) < 0.2f) {
            // Snap to the floor once we are close
            transform.position = toVec;
            ArriveOnFloor();
        }
        
        if (toLevel != currentLevel) {
            // Keep your arms and legs in the elivator
            transform.position = Vector3.Lerp(transform.position, toVec, 0.002f);
            s.NoInput();
            return;
        }

        // Press up to start
        if (currentLevel == 0 && (Input.GetKey("w") || Input.GetKey("up"))) {
            GoToFloor(1);
        }

        if (Input.GetKey("a") || Input.GetKey("left")) {
            s.drawing = -1;
        } else if (Input.GetKey("d") || Input.GetKey("right")) {
            s.drawing = 1;
        } else {
            s.drawing = 0;
        }

        if (Input.GetKeyDown("0")) {
            Debug.Log("Toggel debugging");
            debugging = !debugging;
        }

        s.ducking = (Input.GetKey("s") || Input.GetKey("down"));
    }

    public void Shot(Collider2D other) {
        // I've been shot! Back to start
        if (other.transform.IsChildOf(transform)) {
            // Nah it's just my bullet
            return;
        }
        CheckShot(gameObject);
        s.state = "injured";
    }

    public void CheckShot(GameObject killed) {
        // Get rid of these folks, we don't need them
        foreach (var o in opponents[currentLevel]) {
            o.GetComponent<NPC>().Deactivate();
        }

        // Go to start
        if (killed == gameObject) {
            Debug.Log("I am dead!");
            GoToFloor(0);
            return;
        }

        var npc = killed.GetComponent<NPC>();
        if (npc.human) {
            // Go to start! or go down a level or two? Hm..
            Debug.Log("Human Dead!");
            GoToFloor(0);
        } else {
            // Go to next level!
            Debug.Log("Robot Dead!");
            GoToFloor(currentLevel + 1);
        }
    }

    public string DebugMsg() {
        return string.Format("{0}\n{1} - {2}\ndrawing: {3}\nducking: {4}\nreloading: {5}",
                "Player", s.state, currentLevel,
                s.drawProgress, s.ducking, s.ReloadProgress());
    }
}
