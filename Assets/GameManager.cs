using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
	static GameManager instance;

	public bool touchControls;

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

	bool sputteredEngines = false;

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

		// fps
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 60;
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

		// initialize high score
		if (!PlayerPrefs.HasKey ("HighScore")) {
			PlayerPrefs.SetInt ("HighScore", 0);
		}
	}

	public void ResetStart () {
		if (gameSummary == null) {
			gameSummary = GameObject.Find ("Canvas").transform.Find ("GameSummary").gameObject;
		}
		gameSummary.transform.Find ("LastScore").GetComponent<Text> ().text = "Last Score: " + lastScore;

		int highScore = PlayerPrefs.GetInt ("HighScore");
		gameSummary.transform.Find ("HighScore").GetComponent<Text> ().text = "High Score: " + highScore;
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

		if (scoreText == null) {
			scoreText = GameObject.Find ("Canvas").transform.Find ("Score").GetComponent<Text> ();
		}
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
			} else if (Time.time > fuelGuageAppearDelay && !sputteredEngines) {
				pc.SputterEngines ();
				sputteredEngines = true;
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
				int curHighScore = PlayerPrefs.GetInt("HighScore");
				if (score > curHighScore) {
//					print ("Updating high score to: " + score);
					PlayerPrefs.SetInt ("HighScore", score);
				}
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
