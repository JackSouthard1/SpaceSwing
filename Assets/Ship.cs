using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour {
	private Rigidbody2D rb;
	private PlayerController pc;
	private TerrainManager tm;
	private ParticleSystem[] engines;

	bool hooked = false;

	[Header("Health")]
	public float lifeTime;
	float percentLife;
	float hookedTime = 0;

	[Space(10)]
	[Header("Locomotion")]
	public float acceleration;
	public float maxSpeed;

	// Use this for initialization
	void Start () {
		tm = GameObject.Find ("TerrainManager").GetComponent<TerrainManager> ();
		pc = GameObject.Find ("Player").GetComponent<PlayerController> ();
		rb = GetComponent<Rigidbody2D> ();

		if (transform.Find ("Engines").childCount > 0) {
			engines = transform.Find ("Engines").GetComponentsInChildren<ParticleSystem> ();
		}

		foreach (var engine in engines) {
			engine.Stop ();
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (hooked) {
			hookedTime += Time.deltaTime;
			percentLife = hookedTime / lifeTime;

			if (percentLife > 1f) {
				Destroyed ();
			}

			if (rb.velocity.magnitude < maxSpeed) {
				rb.AddRelativeForce (Vector2.right * acceleration);
			}
		}
	}

	void Destroyed () {
		pc.BreakChain ();
		tm.activeObjects.Remove (gameObject);

		GameObject explosionPrefab = Resources.Load ("Explosion") as GameObject;
		GameObject explosion = (GameObject)Instantiate (explosionPrefab);
		explosion.transform.position = transform.position;

		Destroy (gameObject);
	}

	public void HookStart () {
		hooked = true;
		foreach (var engine in engines) {
			engine.Play ();
		}
	}

	public void HookStop () {
		hooked = false;
		foreach (var engine in engines) {
			engine.Stop ();
		}
	}
}
