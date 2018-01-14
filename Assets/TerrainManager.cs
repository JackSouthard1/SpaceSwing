using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour {
	public bool spawnTerrain;

	[Header("Chunks")]
	public float objectSpawnBuffer;
	public float objectDespawnDst;
	public List<LevelChunk> allLevelChunks;
	public List<GameObject> activeObjects = new List<GameObject>();

	[Header("Stars")]
	public GameObject starPrefab;
	public float starParalaxScale;
	public float starSpawnIntervals;
	public float initialStarCount;
	public float initialStarX;
	Transform lastStar = null;

	[Header("Planet Chunks")]
	public GameObject planetChunkPrefab;
	public float planetChunkY;
	public float planetChunkParalaxScale;
	public float planetChunkSpawnIntervals;
	public float initialPlanetChunkCount;
	public float initialPlanetChunkX;
	Transform lastPlanetChunk = null;

	[HideInInspector]
	public float farthestX = 0f;
	Transform player;
	float curChunkX = 0f;

	void Start () {
		player = GameObject.Find ("Player").transform;

		// spawn initial chunks
		ResetTerrain();
	}
	
	void Update () {
		if (player.transform.position.x > farthestX) {
			farthestX = player.transform.position.x;

			if (spawnTerrain) {
				if (farthestX + objectSpawnBuffer > curChunkX) {
					ExpressNextChunk ();
				}
			}

			if (lastStar == null) {
				CreateNextStar ();
			} else if (farthestX + objectSpawnBuffer > lastStar.position.x) {
				CreateNextStar ();
			}

			if (lastPlanetChunk == null) {
				CreateNextPlanetChunk ();
			} else if (farthestX + objectSpawnBuffer > lastPlanetChunk.position.x) {
				CreateNextPlanetChunk ();
			}

			TestForObjectDespawns ();
		}
	}

	public void ResetTerrain () {
		ClearObjects ();

		initialStarX += player.position.x;
		initialPlanetChunkX += player.position.x;

		farthestX = 0f;
		curChunkX = 0f;

		if (spawnTerrain) {
			for (int i = 0; i < 3; i++) {
				ExpressNextChunk ();
			}
		}

		for (int i = 0; i < initialStarCount; i++) {
			CreateNextStar ();
		}

		for (int i = 0; i < initialPlanetChunkCount; i++) {
			CreateNextPlanetChunk ();
		}
	}

	void CreateNextStar () {
		Vector3 spawnPos;
		if (lastStar == null) {
			spawnPos = new Vector3 (initialStarX, Random.Range (-50, 30), 50f);
		} else {
			spawnPos = new Vector3 (lastStar.position.x + starSpawnIntervals, Random.Range (-50, 30), 50f);
		}
		GameObject star = (GameObject)Instantiate (starPrefab, spawnPos, Quaternion.identity, transform);
		star.GetComponent<Paralax> ().Init (spawnPos, starParalaxScale);
		lastStar = star.transform;

		activeObjects.Add (star);
	}

	void CreateNextPlanetChunk () {
		Vector3 spawnPos;
		if (lastPlanetChunk == null) {
			spawnPos = new Vector3 (initialPlanetChunkX, planetChunkY, 10f);
		} else {
			spawnPos = new Vector3 (lastPlanetChunk.position.x + planetChunkSpawnIntervals, planetChunkY, 10f);
		}
		GameObject atmo = (GameObject)Instantiate (planetChunkPrefab, spawnPos, Quaternion.identity, transform);
		atmo.GetComponent<Paralax> ().Init (spawnPos, planetChunkParalaxScale);
		lastPlanetChunk = atmo.transform;

		bool flipped = (Random.value > 0.5f);
		if (flipped) {
			atmo.transform.localScale = new Vector3 (-atmo.transform.localScale.x, atmo.transform.localScale.y, 1f);
		}

		activeObjects.Add (atmo);
	}

	void ExpressNextChunk () {
		LevelChunk chunk = allLevelChunks [Random.Range (0, allLevelChunks.Count)];

		foreach (var levelObject in chunk.levelObjects) {
			Vector3 spawnPos = new Vector3 (levelObject.pos.x + curChunkX, levelObject.pos.y, 1f);
			GameObject GO = (GameObject)Instantiate (levelObject.prefab, spawnPos, Quaternion.identity, transform);
			activeObjects.Add (GO);
		}

		curChunkX += chunk.chunkSize;

	}

	void TestForObjectDespawns () {
		float despawnMargin = farthestX - objectDespawnDst;
		List<GameObject> objectsToDelete = new List<GameObject> ();
		for (int i = 0; i < activeObjects.Count; i++) {
			if (activeObjects [i] != null) {
				if (activeObjects [i].transform.position.x < despawnMargin) {
					objectsToDelete.Add (activeObjects [i].gameObject);
				}
			}
		}
		foreach (var GO in objectsToDelete) {
			activeObjects.Remove (GO);
			Destroy (GO);

		}
			
	}

	void ClearObjects () {
		for (int i = 0; i < activeObjects.Count; i++) {
			Destroy (activeObjects[i]);
		}
		activeObjects.Clear ();
	}

	[System.Serializable]
	public class LevelChunk {
		public float chunkSize;
		public List<LevelObject> levelObjects;
	}

	[System.Serializable]
	public class LevelObject {
		public Vector2 pos;
		public GameObject prefab;
	}
}
