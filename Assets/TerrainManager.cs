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
	public float starSpawnIntervals;
	public float initialStarCount;

	float farthestStarX;

	float farthestX = 0f;
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

			if (farthestX + objectSpawnBuffer > farthestStarX) {
				CreateNextStar ();
			}

			TestForObjectDespawns ();
		}
	}

	public void ResetTerrain () {
		ClearObjects ();
		farthestX = 0f;
		curChunkX = 0f;
		farthestStarX = player.position.x - 50f;

		if (spawnTerrain) {
			for (int i = 0; i < 3; i++) {
				ExpressNextChunk ();
			}
		}

		for (int i = 0; i < initialStarCount; i++) {
			CreateNextStar ();
		}
	}

	void CreateNextStar () {
		Vector3 spawnPos = new Vector3 (farthestStarX, Random.Range (-50, 30), 10f);
		GameObject star = (GameObject)Instantiate (starPrefab, spawnPos, Quaternion.identity, transform);
		activeObjects.Add (star);

		farthestStarX += starSpawnIntervals;
	}

	void ExpressNextChunk () {
		LevelChunk chunk = allLevelChunks [Random.Range (0, allLevelChunks.Count)];

		foreach (var levelObject in chunk.levelObjects) {
			Vector3 spawnPos = new Vector3 (levelObject.pos.x + curChunkX, levelObject.pos.y, 0f);
			GameObject GO = (GameObject)Instantiate (levelObject.prefab, spawnPos, Quaternion.identity, transform);
			activeObjects.Add (GO);
		}

		curChunkX += chunk.chunkSize;

	}

	void TestForObjectDespawns () {
		float despawnMargin = farthestX - objectDespawnDst;
		List<int> indexesToRemove = new List<int> ();
		for (int i = 0; i < activeObjects.Count; i++) {
			if (activeObjects [i] != null) {
				if (activeObjects [i].transform.position.x < despawnMargin) {
					Destroy (activeObjects [i]);
					indexesToRemove.Add (i);
				}
			}
		}
		foreach (var index in indexesToRemove) {
			activeObjects.RemoveAt (index);
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
