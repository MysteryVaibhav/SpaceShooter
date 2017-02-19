using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomRotator : MonoBehaviour {

	public float tumble;
	private Rigidbody rb;

	void Start () {
		rb = GetComponent<Rigidbody>();
		rb.angularVelocity = Random.insideUnitSphere * tumble; 
	}
    
    public void increaseTumble(float val) {
        tumble += val;
    }
    
    public void setTumble(float val) {
        tumble = val;
    }
}
