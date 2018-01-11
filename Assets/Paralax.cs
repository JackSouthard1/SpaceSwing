using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paralax : MonoBehaviour {
	public Vector3 anchorPos;
	public float paralaxScale;

	bool setup = false;

	Transform mc;

	void Start () {
		mc = Camera.main.transform;
	}

	public void Init (Vector3 _anchorPos, float _paralaxScale) {
		anchorPos = _anchorPos;
		paralaxScale = _paralaxScale;

		setup = true;
	}

	void Update () {
		if (setup) {
			float camDiff = anchorPos.x - mc.position.x;
			float x = anchorPos.x - (camDiff / paralaxScale);
			transform.position = new Vector3 (x, anchorPos.y, anchorPos.z);
		}
	}
}
