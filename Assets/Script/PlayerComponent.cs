using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class Boundary {
	public float xMin, xMax, zMin, zMax;
}

public class PlayerComponent : NetworkBehaviour {

	private Rigidbody rb;
	public float speed;
	public float tilt;
	public Boundary boundary;
	public GameObject shot;
	public Transform shotSpawn;
	public float fireRate;
	private float nextFire;
	private AudioSource audio;

	public SimpleTouchPad touchPad;
	public SimpleTouchAreaButton areaButton;

	void Start () {
		rb = GetComponent<Rigidbody>();
		audio = GetComponent<AudioSource>();
	}

	void Update() {
    
        //if (!isLocalPlayer) {
        //    return;
        //}
        
		if (areaButton.CanFire () && Time.time > nextFire) {
			nextFire = Time.time + fireRate;
			Instantiate (shot, shotSpawn.position, shotSpawn.rotation);
			audio.Play();
		}
	}

	void FixedUpdate () {
    
        //if (!isLocalPlayer) {
        //    return;
        //}
		//float moveHorizontal = Input.GetAxis ("Horizontal");
		//float moveVertical = Input.GetAxis ("Vertical");
		Vector2 direction = touchPad.GetDirection ();
		Vector3 movement = new Vector3 (direction.x, 0.0f, direction.y);
		rb.velocity = movement * speed;

		rb.position = new Vector3 (
			Mathf.Clamp(rb.position.x, boundary.xMin, boundary. xMax),
			0.0f,
			Mathf.Clamp(rb.position.z, boundary.zMin, boundary.zMax)
		);

		rb.rotation = Quaternion.Euler (0.0f, 0.0f, rb.velocity.x * -tilt);
	}
    
    public void setSpeed(float s) {
        speed = s;
    }
    
    public void increaseFireRate(float val) {
        fireRate -= val;
    }
    
    public void setFireRate(float val) {
        fireRate = val;
    }
}
