using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
	static GameManager instance;

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

	bool awaitingReset = false;
	float timeToResetScene = 2f;
	float resetSceneStartTime;

	Animation cutsceneExit;
	PlayerController pc;
	PoliceController police;

	void Awake () {
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad (this);
		
		} else if (this != instance) {
			Destroy (gameObject);
		}
	}

	void Start () {
		playerShipStartOffset = shipSpeed * cutSceneLength;

		pc = GameObject.Find ("Player").GetComponent<PlayerController> ();
		police = GameObject.Find ("Police").GetComponent<PoliceController> ();
		cutsceneExit = GameObject.Find ("Canvas").GetComponentInChildren<Animation> ();

		if (!playedCutscene) {
			StartCutscene ();
		}
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

		if (awaitingReset) {
			if (Time.time - resetSceneStartTime > timeToResetScene) {
				awaitingReset = false;
				SceneManager.LoadScene (0);
			}
		}
	}

	public void ResetGame () {
		awaitingReset = true;
		resetSceneStartTime = Time.time;
	}
}
