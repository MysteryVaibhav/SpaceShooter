using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveHorizontal : MonoBehaviour {

	private Rigidbody rb;
	public float xSpeed;
    public float zSpeed;

	void Start () {
		rb = GetComponent<Rigidbody>();
		rb.velocity = new Vector3 (xSpeed, 0.0f, zSpeed);
	}
    
    public void setSpeed(float x, float z) {
        xSpeed = x;
        zSpeed = z;
    }
}
