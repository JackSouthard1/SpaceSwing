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

	[Header("Teirs")]
	public float dstPerTeir;
	public int chunksPerTeir;
	int teir = 0;
	int maxTeir;

	[Header("Stars")]
	public GameObject starPrefab;
	public float starParalaxScale;
	public float starSpawnIntervals;
	public float initialStarCount;
	public float initialStarX;
	Transform lastStar = null;

	[Header("Background Objects")]
	public Sprite[] backgroundSprites;
	public GameObject backgroundPrefab;
	public float backgroundParalaxScale;
	public float backgroundSpawnIntervals;
	public float initialBackgroundCount;
	public float initialBackgroundX;
	Transform lastBackground = null;

	[Header("Planet Chunks")]
	public GameObject[] planetChunkPrefabs;
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
	int lastChunkID = -1;

	void Start () {
		player = GameObject.Find ("Player").transform;

		SetSeed (GameObject.Find ("GameManager").GetComponent<GameManager> ().sessionSeed);

		maxTeir = Mathf.FloorToInt (allLevelChunks.Count / chunksPerTeir);

		// spawn initial chunks
		ResetTerrain();
	}
		
	void Update () {
		if (player.transform.position.x > farthestX) {
			farthestX = player.transform.position.x;

			if (teir < maxTeir) {
				if (farthestX > (teir + 1) * dstPerTeir) {
					teir++;
//					print ("Increasing difficutly to teir " + teir);
				}
			}

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

			if (lastBackground == null) {
				CreateNextBackground ();
			} else if (farthestX + objectSpawnBuffer > lastBackground.position.x) {
				CreateNextBackground ();
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
		initialBackgroundX += player.position.x;
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

		for (int i = 0; i < initialBackgroundCount; i++) {
			CreateNextBackground ();
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

	void CreateNextBackground () {
		Vector3 spawnPos;
		if (lastBackground == null) {
			spawnPos = new Vector3 (initialBackgroundX, Random.Range (-35, 25), 5f);
		} else {
			spawnPos = new Vector3 (lastBackground.position.x + backgroundSpawnIntervals, Random.Range (-35, 25), 5f);
		}
		GameObject background = (GameObject)Instantiate (backgroundPrefab, spawnPos, Quaternion.identity, transform);
		background.GetComponent<Paralax> ().Init (spawnPos, backgroundParalaxScale);
		lastBackground = background.transform;

		Sprite sprite = backgroundSprites [Random.Range (0, backgroundSprites.Length)];
		background.GetComponent<SpriteRenderer> ().sprite = sprite;

		activeObjects.Add (background);
	} 

	void CreateNextPlanetChunk () {
		Vector3 spawnPos;
		if (lastPlanetChunk == null) {
			spawnPos = new Vector3 (initialPlanetChunkX, planetChunkY, 10f);
		} else {
			spawnPos = new Vector3 (lastPlanetChunk.position.x + planetChunkSpawnIntervals, planetChunkY, 10f);
		}
		GameObject prefab = planetChunkPrefabs [Random.Range (0, planetChunkPrefabs.Length)];
		GameObject atmo = (GameObject)Instantiate (prefab, spawnPos, Quaternion.identity, transform);
		atmo.GetComponent<Paralax> ().Init (spawnPos, planetChunkParalaxScale);
		lastPlanetChunk = atmo.transform;

		bool flipped = (Random.value > 0.5f);
		if (flipped) {
			atmo.transform.localScale = new Vector3 (-atmo.transform.localScale.x, atmo.transform.localScale.y, 1f);
		}

		activeObjects.Add (atmo);
	}

	void ExpressNextChunk () {
		int chunkTeir = Random.Range (0, teir + 1);
		LevelChunk chunk = GetChunkOfTeir (chunkTeir);

		foreach (var levelObject in chunk.levelObjects) {
			Vector3 spawnPos = new Vector3 (levelObject.pos.x + curChunkX, levelObject.pos.y, 1f);
			GameObject GO = (GameObject)Instantiate (levelObject.prefab, spawnPos, Quaternion.identity, transform);
			activeObjects.Add (GO);
		}

		curChunkX += chunk.chunkSize;

	}

	LevelChunk GetChunkOfTeir (int teir) {
		List<LevelChunk> levelChunksOfTeir = new List<LevelChunk> ();
		int startingIndex = teir * chunksPerTeir;
		for (int i = startingIndex; i < startingIndex + chunksPerTeir; i++) {
			if (i >= allLevelChunks.Count) {
				i = allLevelChunks.Count - 1;
			}
//			print ("Got: " + i);

			levelChunksOfTeir.Add (allLevelChunks [i]);
		}
//		print ("Getting Chunks of index " + startingIndex + " to " + (startingIndex + chunksPerTeir - 1));	

		int random = Random.Range (0, levelChunksOfTeir.Count);
//		while (random == lastChunkID) {
//			random = Random.Range (0, levelChunksOfTeir.Count);
//		}
		lastChunkID = random;

		return levelChunksOfTeir [random];
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

	void SetSeed (int _seed) {
		Random.InitState (_seed);
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
