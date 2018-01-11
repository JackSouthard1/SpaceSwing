using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
	static GameManager instance;

	[HideInInspector]
	public int score = 0;
	int lastScore;

	[Header("Cutscene")]
	public bool playedCutscene = false;
	public float shipSpeed;
	public float cutSceneLength;
	public float fuelGuageAppearDelay;
	public float policeStartOffset;

	[HideInInspector]
	public float playerShipStartOffset;

	[HideInInspector]
	public bool inCutscene = false;

	[HideInInspector]
	public bool paused = false;

	bool awaitingReset = false;
	float timeToResetScene = 2f;
	float resetSceneStartTime;

	Animation cutsceneExit;
	PlayerController pc;
	PoliceController police;

	GameObject gameSummary;
	Text scoreText;
	TerrainManager tm;

	void Awake () {
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad (this);
		
		} else if (this != instance) {
			Destroy (gameObject);
		}

		if (playedCutscene) {
			paused = true;	
		}
	}

	void Start () {
		playerShipStartOffset = shipSpeed * cutSceneLength;

		gameSummary = GameObject.Find ("Canvas").transform.Find ("GameSummary").gameObject;
		gameSummary.SetActive (false);

		scoreText = GameObject.Find ("Canvas").transform.Find ("Score").GetComponent<Text> ();
		tm = GameObject.Find ("TerrainManager").GetComponent<TerrainManager> ();

		pc = GameObject.Find ("Player").GetComponent<PlayerController> ();
		police = GameObject.Find ("Police").GetComponent<PoliceController> ();
		cutsceneExit = GameObject.Find ("Canvas").GetComponentInChildren<Animation> ();

		if (!playedCutscene) {
			StartCutscene ();
		} else {
			scoreText.enabled = true;
		}
	}

	public void ResetStart () {
		if (gameSummary == null) {
			gameSummary = GameObject.Find ("Canvas").transform.Find ("GameSummary").gameObject;
		}
		gameSummary.transform.Find ("LastScore").GetComponent<Text> ().text = "Last Score: " + lastScore;
	}

	public void Unpause () {
		if (pc == null) {
			pc = GameObject.Find ("Player").GetComponent<PlayerController> ();
		}
		if (police == null) {
			police = GameObject.Find ("Police").GetComponent<PoliceController> ();
		}
		if (gameSummary == null) {
			gameSummary = GameObject.Find ("Canvas").transform.Find ("GameSummary").gameObject;
		}
		gameSummary.SetActive (false);

		scoreText.enabled = true;
		paused = false;
		pc.Unpause ();
		police.Unpause ();
	}

	void StartCutscene () {
		inCutscene = true;
		cutsceneExit.transform.GetChild(0).gameObject.SetActive (true);
		cutsceneExit.transform.GetChild(1).gameObject.SetActive (true);

		pc.StartCutscene ();
		police.EnterCutScene();
		Camera.main.GetComponent<CameraController> ().ResetCamera ();
	}

	void FinishCutscene () {
		cutsceneExit.Play ();
		pc.ExitCutscene ();
		police.ExitCutScene();

		scoreText.enabled = true;
		inCutscene = false;
		playedCutscene = true;
	}
	
	void Update () {
		if (inCutscene) {
			if (Time.time > cutSceneLength) {
				FinishCutscene ();
			} else if (Time.time > fuelGuageAppearDelay) {
				pc.SputterEngines ();
			}
		}

		// update score
		if (tm == null) {
			tm = GameObject.Find ("TerrainManager").GetComponent<TerrainManager> ();
		}
		if (scoreText == null) {
			scoreText = GameObject.Find ("Canvas").transform.Find ("Score").GetComponent<Text> ();
		}
		score = Mathf.RoundToInt (tm.farthestX / 30f);
		if (score < 0) {
			score = 0;
		}
		scoreText.text = score.ToString ();
			
		if (awaitingReset) {
			if (Time.time - resetSceneStartTime > timeToResetScene) {
				awaitingReset = false;
				paused = true;
				lastScore = score;
				score = 0;
				SceneManager.LoadScene (0);
			}
		}
	}

	public void ResetGame () {
		awaitingReset = true;
		resetSceneStartTime = Time.time;
	}
}
