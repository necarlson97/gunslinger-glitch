using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    int maxLevel = 4;
    float elevatorSpeed = 1.6f;

    // If we want to be somewhere else,
    // but are waiting for player to press up
    internal int desiredLevel = 1;

    // Y offset for where the floor looks like it is
    float floorLevel = -2.2f;

    // TODO default false
    public bool debugging = true;

    int wins = 0;

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

            var left = GameObject.Instantiate(NPCPrefab, new Vector3(-5, floorLevel + level * levelHeight, 0), Quaternion.identity);
            var right = GameObject.Instantiate(NPCPrefab, new Vector3(5, floorLevel + level * levelHeight, 0), Quaternion.identity);
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

        // Boss too
        var b = GameObject.Find("Boss").GetComponent<Boss>();
        if (b.s.state == "dead") b.Refresh();
    }

    bool GoToFloor(int floor) {
        // If we are in motion wait
        if (OnTheWay()) {
            return false;
        }
        toLevel = floor;
        s.PlaySound("elevator", transform.Find("Elevator"));
        return true;
    }

    void ArriveOnFloor() {
        // if we got to a new floor, we are alive or respawning
        s.state = "alive";
        currentLevel = toLevel;

        // Never want to stay below
        if (currentLevel == 0) {
            desiredLevel = 1;
            SpawnOpponents();
            return;
        }
        if (currentLevel < 0) {
            desiredLevel = 0;
            return;
        }

        if (currentLevel == maxLevel+1) {
            GameObject.Find("Boss").GetComponent<Boss>().Activate();
            return;
        }
        if (currentLevel == maxLevel+2) {
            wins += 1;
            desiredLevel = 0;
            GameObject.Find("Floor (6)/Victory/Score").GetComponent<Text>().text = "" + wins;
            return;
        }
        foreach (var o in opponents[currentLevel]) {
            o.GetComponent<NPC>().Activate();
        }
    }

    // Update is called once per frame
    void Update() {
        var toVec = new Vector3(0, floorLevel + toLevel * levelHeight, 0);

        // TODO gross and potentially buggy
        // If we are close, but not there yet, then we have just 'arrrived' probs
        if (transform.position != toVec && Vector3.Distance(transform.position, toVec) < 0.2f) {
            // Snap to the floor once we are close
            transform.position = toVec;
            ArriveOnFloor();
        }
        
        if (OnTheWay()) {
            // Keep your arms and legs in the elivator
            transform.position = Vector3.Lerp(transform.position, toVec, elevatorSpeed * Time.deltaTime);
            s.NoInput();
            return;
        }

        // Press up to next level
        // TODO indicator above their head?
        transform.Find("Elevator Help").gameObject.SetActive(false);
        if (WaitingForButton()) {
            if (Input.GetKey("w") || Input.GetKey("up")) {
                GoToFloor(desiredLevel);
            } else if (s.ReloadProgress() == 1) {
                transform.Find("Elevator Help").gameObject.SetActive(true);
            }
        }

        if (Input.GetKey("a") || Input.GetKey("left")) {
            s.drawing = -1;
        } else if (Input.GetKey("d") || Input.GetKey("right")) {
            s.drawing = 1;
        } else {
            s.drawing = 0;
        }

        // Option floors
        if (Input.GetKeyDown("1")) {
            desiredLevel = -1;
        } else if (Input.GetKey("2")) {
            desiredLevel = -2;
        }  else if (Input.GetKey("9")) {
            desiredLevel = maxLevel + 1;
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
        Debug.Log("Shot! " + other.gameObject.name);
        s.state = "injured";
        s.PlayParticles(transform.Find("Human Gore"));

        CheckShot(gameObject);
    }

    public void CheckShot(GameObject killed) {
        // Get rid of these folks, we don't need them
        if (opponents.ContainsKey(currentLevel)) {
            foreach (var o in opponents[currentLevel]) {
                o.GetComponent<NPC>().Deactivate();
            }
        }
        // if someone already died, then whatever
        if (WaitingForButton()) {
            return;
        }

        // Go to start immediatly if I (and someone else) was killed
        if (killed == gameObject) {
            GoToFloor(0);
            desiredLevel = 1;
            return;
        }

        if (killed.GetComponent<Boss>()) {
            desiredLevel = currentLevel + 1;
            killed.GetComponent<Boss>().Deactivate();
            return;
        }

        var npc = killed.GetComponent<NPC>();
        // Go to start when ready
        if (npc.human) desiredLevel = 0;
        // Go to next level when ready
        else desiredLevel = currentLevel + 1;
    }

    public string DebugMsg() {
        return string.Format("{0}\n{1} - {2}\ndrawing: {3}\nducking: {4}\nreloading: {5}\nlevels {6}-{7}",
                "Player", s.state, currentLevel,
                s.drawProgress, s.ducking, s.ReloadProgress(),
                desiredLevel, toLevel);
    }

    public bool OnTheWay() {
        // If the elevator is moving to a floor
        return toLevel != currentLevel;
    }

    public bool WaitingForButton() {
        // If the elevator has a new destination.
        // and we are just waitinf for the player
        return desiredLevel != toLevel;
    }

    public bool GoingDown() {
        // If we are headed down next
        return desiredLevel < toLevel;
    }
}
