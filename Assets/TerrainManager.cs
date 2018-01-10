using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour {
	public float chunkSpawnBuffer;
	public float objectDespawnDst;
	public List<LevelChunk> allLevelChunks;
	public List<GameObject> activeObjects = new List<GameObject>();
	float farthestX = 0f;
	Transform player;
	float curChunkX = 0f;

	void Start () {
		player = GameObject.Find ("Player").transform;

		// spawn initial chunks
		for (int i = 0; i < 3; i++) {
			ExpressNextChunk ();
		}
	}
	
	void Update () {
		if (player.transform.position.x > farthestX) {
			farthestX = player.transform.position.x;
			if (farthestX + chunkSpawnBuffer > curChunkX) {
				ExpressNextChunk ();
			}

			TestForObjectDespawns ();
		}
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
			if (activeObjects[i].transform.position.x < despawnMargin) {
				Destroy (activeObjects[i]);
				indexesToRemove.Add (i);
			}
		}
		foreach (var index in indexesToRemove) {
			activeObjects.RemoveAt (index);
		}
			
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
