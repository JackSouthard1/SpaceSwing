using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paralax : MonoBehaviour {
	public Vector3 anchorPos;
	public float paralaxScale;

	float lastCamPosX;

	bool setup = false;

	Transform mc;

	void Start () {
		mc = Camera.main.transform;

		lastCamPosX = mc.position.x;
	}

	public void Init (Vector3 _anchorPos, float _paralaxScale) {
		anchorPos = _anchorPos;
		paralaxScale = _paralaxScale;

		setup = true;
	}

	void Update () {
		if (setup) {
//			float camDiff = anchorPos.x - mc.position.x;
//			float x = anchorPos.x - (camDiff / paralaxScale);

//			float camDiff = mc.position.x - anchorPos.x;
//			float x = camDiff + mc.position.x;
//	
//			transform.position = new Vector3 (x, anchorPos.y, anchorPos.z);

			float parallax = (lastCamPosX - mc.position.x) * paralaxScale;

			//set a target x position that is the current position plus the parallax
			float targetX = transform.position.x - parallax;
			transform.position = new Vector3 (targetX, anchorPos.y, anchorPos.z);

			lastCamPosX = mc.position.x;
		}
	}
}
