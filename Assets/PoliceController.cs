using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoliceController : MonoBehaviour {
	public float maxFollowDst;
	public float speed;
	public float maxAngle;
	public float angleMultiplier;

	float lastY;
	bool chasing = true;

	Transform player;
	Rigidbody2D rb;

	void Start () {
		player = GameObject.Find ("Player").transform;
		rb = GetComponent<Rigidbody2D> ();

		ResetPolice ();
	}

	public void ResetPolice () {
		transform.position = new Vector3 (-maxFollowDst, 0f, 0f);
		rb.velocity = Vector2.right * speed;
		lastY = transform.position.y;

		chasing = true;
	}
	
	void FixedUpdate () {
		if (chasing) {
			float y = player.transform.position.y;
			float yDiff = y - lastY;

			float x;

			if (transform.position.x < player.position.x - maxFollowDst) {
				x = player.position.x - maxFollowDst;
			} else {
				x = transform.position.x;
			}

			transform.position = new Vector3 (x, y, 0f);

			float newRot = Mathf.Clamp (yDiff * angleMultiplier, -maxAngle, maxAngle); 
			transform.rotation = Quaternion.Euler (new Vector3 (0f, 0f, newRot));

			lastY = y;
		}
	}

	void OnTriggerEnter2D (Collider2D coll) {
		if (coll.tag == "Player") {
			rb.velocity = Vector2.zero;
			chasing = false;

			player.GetComponentInParent<PlayerController> ().Caught ();
		}
	}
}
