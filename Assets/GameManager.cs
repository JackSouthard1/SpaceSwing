using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
	static GameManager instance;

	[Header("Cutscene")]
	public float shipSpeed;
	public float cutSceneLength;
	public float fuelGuageAppearDelay;
	public float policeStartOffset;

	[HideInInspector]
	public float playerShipStartOffset;

	[HideInInspector]
	public bool inCutscene = false;

	[HideInInspector]
	public bool playedCutscene = false;

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
	}

	public void ResetGame () {
		SceneManager.LoadScene (0);
	}
}
