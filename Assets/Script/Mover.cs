using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : MonoBehaviour {

	private Rigidbody rb;
	public float speed;

	void Start () {
		rb = GetComponent<Rigidbody>();
		rb.velocity = transform.forward * speed;
	}
    
    public void increaseSpeed(float val) {
        speed += val;
    }
    
    public void setSpeed(float val) {
        speed = val;
    }
}
