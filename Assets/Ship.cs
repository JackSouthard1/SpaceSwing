using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour {
	private Rigidbody2D rb;
	private PlayerController pc;
	private TerrainManager tm;
	private ParticleSystem[] engines;
	private AudioSource engineAudio;

	bool hooked = false;

	public float breakSpeed;

	[Header("Health")]
	public float lifeTime;
	float percentLife;
	float hookedTime = 0;

	[Space(10)]
	[Header("Locomotion")]
	public float acceleration;
	public float maxSpeed;
	public float rotation;
	float curSpeed = 0f;

	// Use this for initialization
	void Start () {
		tm = GameObject.Find ("TerrainManager").GetComponent<TerrainManager> ();
		pc = GameObject.Find ("Player").GetComponent<PlayerController> ();
		rb = GetComponent<Rigidbody2D> ();

		engineAudio = GetComponent<AudioSource> ();
		if (transform.Find ("Engines").childCount > 0) {
			engines = transform.Find ("Engines").GetComponentsInChildren<ParticleSystem> ();
		}

		foreach (var engine in engines) {
			engine.Stop ();
		}
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (hooked) {
			hookedTime += Time.deltaTime;
			percentLife = hookedTime / lifeTime;

			if (percentLife > 1f) {
				Destroyed ();
			}

			if (rb.velocity.magnitude < maxSpeed) {
				curSpeed += acceleration;
			}

			Vector2 localForce2d = Vector2.right * curSpeed;
			Vector3 localForce3d = new Vector3 (localForce2d.x, localForce2d.y, transform.position.z);
			Vector3 worldForce3d = transform.TransformVector (localForce3d);
			rb.velocity = new Vector2 (worldForce3d.x, worldForce3d.y);

			transform.rotation = Quaternion.Euler (new Vector3 (0f, 0f, transform.rotation.eulerAngles.z + rotation));
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

	void OnCollisionEnter2D (Collision2D coll) {
		if (coll.gameObject.tag == "Deadly") {
			Destroyed ();
		}

		if (coll.relativeVelocity.magnitude > breakSpeed) {
			Destroyed ();
		}
//		if (coll.gameObject.GetComponent<Rigidbody2D> () != null) {
//			Vector2 diff = rb.velocity - coll.gameObject.GetComponent<Rigidbody2D> ().velocity;
//			if (rb.velocity.magnitude > breakSpeed) {
//				Destroyed ();
//			}
//		} else {
//			if (rb.velocity.magnitude > breakSpeed) {
//				Destroyed ();
//			}
//		}
	}

	public void HookStart () {
		hooked = true;
		engineAudio.Play ();
		foreach (var engine in engines) {
			engine.Play ();
		}
	}

	public void HookStop () {
		hooked = false;
		engineAudio.Stop ();
		foreach (var engine in engines) {
			engine.Stop ();
		}
	}
}
