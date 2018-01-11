using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	public float smoothTime;

	Vector3 velocity = Vector3.zero;
	Transform player;
	Vector3 offset;
	float farthestX;

	void Start () {
		player = GameObject.Find ("Player").transform;
		offset = transform.position;

		ResetCamera ();
	}

	public void ResetCamera () {
		farthestX = player.transform.position.x;
		transform.position = new Vector3 (farthestX, offset.y, offset.z);
	}
	
	void FixedUpdate () {
		if (player.transform.position.x > farthestX) {
			farthestX = player.transform.position.x;
		}

		Vector3 targetPos = new Vector3 (farthestX, offset.y, offset.z);
		transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
	}
}
