using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
	private GameManager gm;
	private Rigidbody2D rb;
	private Transform sprite;
	private ParticleSystem[] engines;

	[Header("Hook")]
	public float hookOffset;
	public float radius;
	public float breakBuffer = 0f; 
	GameObject previewObject;
	GameObject hookedObject;
	Transform hook;
	LineRenderer chain;
	SpringJoint2D spring;

	public Vector2 hookPos = Vector2.zero;
	enum HookState {
		None,
		Preview,
		Attached
	};
	HookState hookState = HookState.None;
//	bool hookActive = false;

	void Start () {
		gm = GameObject.Find ("GameManager").GetComponent<GameManager> ();
		sprite = transform.Find ("Sprite");
		rb = GetComponent<Rigidbody2D> ();
		hook = transform.Find ("Chain").Find ("Hook");
		chain = GetComponentInChildren<LineRenderer> ();
		spring = GetComponent<SpringJoint2D> ();

		rb.velocity = Vector2.right * gm.shipSpeed;

		engines = transform.Find ("Engines").GetComponentsInChildren<ParticleSystem> ();
		foreach (var engine in engines) {
			engine.Stop ();
		}
	}
	
	void Update () {
		if (gm.inCutscene) {
			rb.velocity = Vector2.right * gm.shipSpeed;
			return;
		}

		if (Input.GetKeyDown (KeyCode.Space)) {
			if (hookState != HookState.Attached) {
				GameObject GO = GetHookableObject (); 
				if (GO != null) {
					HookObject (GO);
				}
			}
		} else if (Input.GetKeyUp (KeyCode.Space)){
			BreakChain ();
		} else if (hookState != HookState.Attached){
			GameObject closestPreviewObject = GetHookableObject (); 
			if (closestPreviewObject != null) {
				PreviewObject (closestPreviewObject);
			} else {
				BreakPreview ();
			}
		}
	}

	void LateUpdate () {
		if (rb.velocity.magnitude > 0.5f) {
			Vector2 v = rb.velocity;
			float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
//			print (angle);
			sprite.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
		}

		if (hookState == HookState.Attached) {
			Vector3 hookPos3d = hookedObject.transform.position;
			hookPos = new Vector2 (hookPos3d.x, hookPos3d.y);

			Vector3 endWorldPos = new Vector3 (hookPos.x, hookPos.y, 0f);

			Vector2 hookDir = endWorldPos - transform.position; 
			hook.right = hookDir; 

			Vector3 chainEndPos = endWorldPos - hook.right * hookOffset;
			chain.SetPosition (0, transform.position);
			chain.SetPosition (1, chainEndPos);
				
			hook.position = endWorldPos;

			spring.distance = Mathf.Clamp (spring.distance * 0.995f, 10f, radius);

			if (hookDir.magnitude - breakBuffer > radius) {
				BreakChain ();
			}
		} else if (hookState == HookState.Preview) {
			Vector3 hookPos3d = previewObject.transform.position;
			hookPos = new Vector2 (hookPos3d.x, hookPos3d.y);

			Vector3 endWorldPos = new Vector3 (hookPos.x, hookPos.y, 0f);

			Vector3 chainEndPos = endWorldPos - hook.right * hookOffset;
			chain.SetPosition (0, transform.position);
			chain.SetPosition (1, chainEndPos);

//			if (hookDir.magnitude - breakBuffer > radius) {
//				BreakChain ();
//			}
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

	void HookObject (GameObject hookTarget) {
		spring.connectedBody = hookTarget.GetComponent<Rigidbody2D> ();
		hook.GetComponent<SpriteRenderer> ().enabled = true;
		chain.enabled = true;
		chain.material.color = Color.white;
		spring.enabled = true;
		hookedObject = hookTarget;

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
			float closestDist = Mathf.Infinity;
			int closestIndexAbove = -1;
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
			}

			if (closestIndexAbove != -1) {
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

		Reset ();
	}

	public void Caught () {
		Reset ();
	}

	public void Reset () {
		if (Time.time < 0.5f) {
			return;
		}
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
		Animation anim = GetComponentInChildren<Animation> ();
		anim.Play ();
		anim.GetComponent<SpriteRenderer> ().enabled = true;
	}

	public void ExitCutscene () {
		Animation anim = GetComponentInChildren<Animation> ();
		anim.Stop ();
		anim.GetComponent<SpriteRenderer> ().enabled = false;

		rb.gravityScale = 3f;
		foreach (var engine in engines) {
			engine.Stop ();
		}
	}

	void OnTriggerEnter2D (Collider2D coll) {
		if (coll.tag == "DeathZone") {
			Explode ();
		}
	}
}
