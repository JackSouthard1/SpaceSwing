using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {
	private GameManager gm;
	private Rigidbody2D rb;
	private Transform sprite;
	private ParticleSystem[] engines;

	public float breakSpeed;

	[Header("Movement")]
	public float maxY;
	public float hookZ;
	public float hookOffset;
	public float radius;
	public float breakBuffer = 0f;
	public float maxTurnAngle;

	float connectedDst;

	[Header("Boost")]
	public float boostStrength;
	public float maxBoostTime;
	public float boostSiphonRate;
	float boostTimeRemaining = 0f;
	bool boosting = false;

	[Header("Indicator")]
	public float maxAngle;
	GameObject boostIndicator;
	RectTransform indicator;

	GameObject hookIndicator;

	bool dead = false;

	float halfScreenWidth;

	GameObject previewObject;
	GameObject hookedObject;
	Transform hook;
	LineRenderer chain;
	SpringJoint2D spring;

	Vector2 hookPos = Vector2.zero;
	enum HookState {
		None,
		Preview,
		Attached
	};
	HookState hookState = HookState.None;
//	bool hookActive = false;

	void Start () {
		halfScreenWidth = Screen.currentResolution.width / 2f;

		hookIndicator = GameObject.Find ("HookIndicator");
		boostIndicator = GameObject.Find ("BoostIndicator");
		indicator = boostIndicator.transform.Find ("Indicator").GetComponent<RectTransform>();
		hookIndicator.SetActive (false);
		boostIndicator.SetActive (false);

		gm = GameObject.Find ("GameManager").GetComponent<GameManager> ();
		gm.ResetStart ();

		sprite = transform.Find ("Sprite");
		rb = GetComponent<Rigidbody2D> ();

		hook = transform.Find ("Chain").Find ("Hook");
		hook.GetComponent<SpriteRenderer> ().enabled = false;

		chain = GetComponentInChildren<LineRenderer> ();
		spring = GetComponent<SpringJoint2D> ();

		if (gm.playedCutscene) {
			rb.gravityScale = 0f;
		}


		engines = GetComponentsInChildren<ParticleSystem> ();
		foreach (var engine in engines) {
			engine.Stop ();
		}
	}

	public void Unpause () {
		if (rb == null) {
			rb = GetComponent<Rigidbody2D> ();
		}
//		boostIndicator.SetActive (true);
		hookIndicator.SetActive (true);
		rb.velocity = Vector2.right * gm.shipSpeed;
		rb.gravityScale = 3f;
	}
	
	void Update () {
		if (dead) {
			return;
		}

		if (gm.inCutscene) {
			rb.velocity = Vector2.right * gm.shipSpeed;
			return;
		}
			
		// touch
		if (gm.touchControls) {
			// get touch info
			bool leftTouch = false;
			bool rightTouch = false;

			Touch[] touches = Input.touches;
			foreach (var curTouch in touches) {
				if (curTouch.position.x < halfScreenWidth) {
					leftTouch = true;
				} else if (curTouch.position.x > halfScreenWidth) {
					rightTouch = true;
				}
			}

			// test inputs
			if (gm.paused) {
				if (rightTouch || leftTouch) {
					gm.Unpause ();
				}
			}

			if (rightTouch) {
				if (!boosting) {
					StartBoost();
				}
			} else {
				if (boosting) {
					EndBoost();
				}
			}

			if (leftTouch) {
				if (hookState != HookState.Attached) {
					GameObject GO = GetHookableObject (); 
					if (GO != null) {
						HookObject (GO);
					}
				}
			} else {
				if (hookState == HookState.Attached) {
					BreakChain();
				}
			}
		}

		// keyboard
		if (!gm.touchControls) {
			if (gm.paused) {
				if (Input.GetKeyDown (KeyCode.Space)) {
					gm.Unpause ();
				}
			}

			if (Input.GetKeyDown (KeyCode.LeftShift)) {
				if (!boosting) {
					StartBoost ();
				}
			} else if (Input.GetKeyUp (KeyCode.LeftShift)) {
				if (boosting) {
					EndBoost();
				}
			}

			if (Input.GetKey (KeyCode.Space)) {
				if (hookState != HookState.Attached) {
					GameObject GO = GetHookableObject (); 
					if (GO != null) {
						HookObject (GO);
					}
				}
			} else if (Input.GetKeyUp (KeyCode.Space)){
				BreakChain ();
			}
		}

		if (hookState != HookState.Attached){
			GameObject closestPreviewObject = GetHookableObject (); 
			if (closestPreviewObject != null) {
				PreviewObject (closestPreviewObject);
			} else {
				BreakPreview ();
			}
		}
	}

	void FixedUpdate () {
		if (boosting) {
			rb.AddForce (rb.velocity.normalized * boostStrength);
			boostTimeRemaining -= Time.deltaTime;
			if (boostTimeRemaining < 0f) {
				EndBoost ();
				boostIndicator.SetActive (false);
			}
		}

		// hooking
		if (hookState == HookState.Attached) {
			// add boost
			if (hookedObject.name.Contains ("Ship")) {
				if (boostTimeRemaining < maxBoostTime) {
					boostTimeRemaining += boostSiphonRate;
				}

				if (!boostIndicator.activeSelf) {
					boostIndicator.SetActive (true);
				}
			}

			Vector3 hookPos3d = hookedObject.transform.position;
			hookPos = new Vector2 (hookPos3d.x, hookPos3d.y);

			Vector3 endWorldPos = new Vector3 (hookPos.x, hookPos.y, 0f);

			Vector2 hookDir = endWorldPos - transform.position; 

			spring.distance = Mathf.Clamp (connectedDst * 0.995f, 10f, radius);
			spring.distance = connectedDst;
			float curDst = (hookedObject.transform.position - transform.position).magnitude;
			if (connectedDst > curDst) {
				spring.distance = curDst;
			}

			if (hookDir.magnitude - breakBuffer > radius) {
				BreakChain ();
			}
		}
	}

	void LateUpdate () {
		if (dead) {
			return;
		}

		// chain and hook visuals
		if (hookState == HookState.Attached) {
			Vector3 hookPos3d = hookedObject.transform.position;
			hookPos = new Vector2 (hookPos3d.x, hookPos3d.y);

			Vector3 endWorldPos = new Vector3 (hookPos.x, hookPos.y, 0f);

			Vector2 hookDir = endWorldPos - transform.position; 
			hook.right = hookDir; 

			Vector3 chainEndPos = endWorldPos - hook.right * hookOffset;
			chain.SetPosition (0, transform.position);
			chain.SetPosition (1, chainEndPos);

			hook.position = new Vector3 (endWorldPos.x, endWorldPos.y, hookZ);
		}

		// rotation
		if (rb.velocity.magnitude > 0.5f) {
			Vector2 v = rb.velocity;
			float targetAngle = Mathf.Atan2 (v.y, v.x) * Mathf.Rad2Deg;

			sprite.rotation = Quaternion.AngleAxis (targetAngle, Vector3.forward);
		}

		// ceiling
		if (transform.position.y > maxY) {
			rb.velocity = new Vector2 (rb.velocity.x, 0f);
		}

		// boosting
		float boost01Percent = Mathf.Clamp01 (boostTimeRemaining / maxBoostTime);

		// update Indicator
		float indicatorAngle = maxAngle - (boost01Percent * maxAngle * 2f);
		indicator.rotation = Quaternion.Euler (new Vector3 (0f, 0f, indicatorAngle));

		if (hookState == HookState.Preview) {
			Vector3 hookPos3d = previewObject.transform.position;
			hookPos = new Vector2 (hookPos3d.x, hookPos3d.y);

			Vector3 endWorldPos = new Vector3 (hookPos.x, hookPos.y, 0f);

			Vector3 chainEndPos = endWorldPos - hook.right * hookOffset;
			chain.SetPosition (0, transform.position);
			chain.SetPosition (1, chainEndPos);
		}
	}

	void PreviewObject (GameObject previewTarget) {
		chain.enabled = true;
		chain.material.color = new Color (1,1,1, 0.25f);
		hookState = HookState.Preview;
		previewObject = previewTarget;
	}

	void BreakPreview () {
		chain.enabled = false;
		hookState = HookState.None;
	}

	public void StartBoost () {
		if (boostTimeRemaining > 0) {
			boosting = true;
			foreach (var engine in engines) {
				engine.Play ();
			}
		}
	}

	void EndBoost () {
		boosting = false;
		foreach (var engine in engines) {
			engine.Stop ();
		}
	}

	void HookObject (GameObject hookTarget) {
		spring.connectedBody = hookTarget.GetComponent<Rigidbody2D> ();

		hook.position = new Vector3 (hookTarget.transform.position.x, hookTarget.transform.position.y, hookZ);
		hook.GetComponent<SpriteRenderer> ().enabled = true;

		chain.enabled = true;
		chain.material.color = Color.white;
		spring.enabled = true;
		hookedObject = hookTarget;

		connectedDst = (hookTarget.transform.position - transform.position).magnitude;

		Ship ship = hookedObject.GetComponent<Ship> ();
		if (ship != null) {
			ship.HookStart ();
		}

		hookState = HookState.Attached;
	}

	public void BreakChain () {
		hook.GetComponent<SpriteRenderer> ().enabled = false;
		chain.enabled = false;
		spring.enabled = false;
		hookState = HookState.None;

		if (hookedObject != null) {
			if (hookedObject.GetComponent<Ship> () != null) {
				hookedObject.GetComponent<Ship> ().HookStop ();
			}
		}

		hookedObject = null;
	}

	GameObject GetHookableObject ()
	{
		Vector2 center = new Vector2 (transform.position.x, transform.position.y);

		Collider2D[] allOverlappingColliders = Physics2D.OverlapCircleAll(center, radius);
		List<Collider2D> allHookableObjectsList = new List<Collider2D> ();
		foreach (var coll in allOverlappingColliders) {
			if (coll.gameObject.tag == "Hookable") {
				allHookableObjectsList.Add (coll);
			}
		}
		Collider2D[] allHookableObjects = new Collider2D[allHookableObjectsList.Count];
		for (int i = 0; i < allHookableObjects.Length; i++) {
			allHookableObjects [i] = allHookableObjectsList [i];
		}

		if (allHookableObjects.Length > 0) {
			float closestDstAbove = Mathf.Infinity;
			float closestDstInfront = Mathf.Infinity;
			float closestDist = Mathf.Infinity;

			int closestIndexAbove = -1;
			int closestIndexInfront = -1;
			int closestIndex = 0;

			for (int i = 0; i < allHookableObjects.Length; i++) {
				float curDist = (allHookableObjects [i].transform.position - transform.position).magnitude;

				if (curDist < closestDist) {
					closestDist = curDist;
					closestIndex = i;
				}

				if (allHookableObjects [i].transform.position.y > transform.position.y) { // -10f because you should be able to grab objects right under you
					if (curDist < closestDstAbove) {
						closestDstAbove = curDist;
						closestIndexAbove = i;
					}
				}

				if (allHookableObjects [i].transform.position.x > transform.position.x) { // -10f because you should be able to grab objects right under you
					if (curDist < closestDstInfront) {
						closestDstInfront = curDist;
						closestIndexInfront = i;
					}
				}
			}

			if (closestIndexInfront != -1) {
				return allHookableObjects [closestIndexInfront].gameObject;
			} else if (closestIndexAbove != -1) {
				return allHookableObjects [closestIndexAbove].gameObject;
			} else {
				return allHookableObjects [closestIndex].gameObject;
			}
		} else {
			return null;
		}
	}

	void Explode () {
		GameObject explosionPrefab = Resources.Load ("Explosion") as GameObject;
		GameObject explosion = (GameObject)Instantiate (explosionPrefab);
		explosion.transform.position = transform.position;

		sprite.GetComponent<SpriteRenderer> ().enabled = false;

		Reset ();
	}

	public void Caught () {
		Reset ();
	}

	public void Reset () {
		if (Time.time < 0.5f) {
			return;
		}
		foreach (var engine in engines) {
			engine.Stop ();
		}
		rb.velocity = Vector2.zero;
		rb.isKinematic = true;
		dead = true;
		BreakChain ();
		BreakPreview ();
		GameObject.Find ("GameManager").GetComponent<GameManager> ().ResetGame ();

//		transform.position = Vector3.zero;
//		rb.velocity = Vector2.zero;
	}

	public void StartCutscene () {
		rb.gravityScale = 0f;

		transform.position = new Vector3 (-gm.playerShipStartOffset, 0f, 0f);

		foreach (var engine in engines) {
			engine.Play ();
		}
	}

	public void SputterEngines () {
		StartCoroutine (ToggleEngines());
		Animation anim = GetComponentInChildren<Animation> ();
		anim.Play ();
		anim.GetComponent<SpriteRenderer> ().enabled = true;
	}

	public void ExitCutscene () {
		StopCoroutine (ToggleEngines ());
		Animation anim = GetComponentInChildren<Animation> ();
		anim.Stop ();
		anim.GetComponent<SpriteRenderer> ().enabled = false;

//		boostIndicator.SetActive (true);
		hookIndicator.SetActive (true);

		rb.gravityScale = 3f;
		foreach (var engine in engines) {
			engine.Stop ();
		}
	}

	void OnCollisionEnter2D (Collision2D coll) {
		if (!dead) {
			if (coll.gameObject.tag == "Deadly") {
				Explode ();
			}

			if (!coll.gameObject.name.Contains ("Ship")) {
				if (coll.relativeVelocity.magnitude > breakSpeed) {
					Explode ();
				}
			}
		}
	}

	void OnTriggerEnter2D (Collider2D coll) {
		if (!dead) {
			if (coll.tag == "DeathZone") {
				Explode ();
			}
		}
	}

	IEnumerator ToggleEngines () {
		for (int i = 0; i < 2; i++) {
			foreach (var engine in engines) {
				engine.Stop ();
			}
			yield return new WaitForSeconds(0.45f);
			foreach (var engine in engines) {
				engine.Play ();
			}
			yield return new WaitForSeconds(0.2f);
		}
		for (int i = 0; i < 3; i++) {
			foreach (var engine in engines) {
				engine.Stop ();
			}
			yield return new WaitForSeconds(0.25f);
			foreach (var engine in engines) {
				engine.Play ();
			}
			yield return new WaitForSeconds(0.1f);
		}
		foreach (var engine in engines) {
			engine.Stop ();
		}
	}
}
