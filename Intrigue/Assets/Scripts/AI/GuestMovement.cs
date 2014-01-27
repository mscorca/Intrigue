﻿using UnityEngine;
using System.Collections;

public class GuestMovement : Photon.MonoBehaviour {

	int actID = -1;
	public NavMeshAgent agent;
	public GameObject room;
	Vector3 finalPosition;
	bool init = true;
	float counter = 0;
	
	private Animator anim;                          // a reference to the animator on the character


	void Start(){
		anim = GetComponent<Animator>();
		room = GameObject.FindWithTag("RoomCollider");
	}

	void Update(){

		anim.SetFloat("Direction", agent.velocity.x);
		anim.SetFloat("Speed", agent.velocity.z);

		Debug.Log("Inside Guest Update");
		if(counter > 0.5f){
			moveGuest();
			init = false;
			Debug.Log("Inside Guest if");
			counter = 0;
		}
		else{
			counter += Time.deltaTime;
		}
	}
	
	void moveGuest(){
		
		Vector3 max;
		Vector3 min;

		Collider roomCollider = room.GetComponent<BoxCollider>();
		max = roomCollider.bounds.max;
		min = roomCollider.bounds.min;

		finalPosition = new Vector3(Random.Range(min.x, max.x), 
									room.transform.position.y,
									Random.Range(min.z, max.z));

		agent.SetDestination(finalPosition);
		Debug.Log("At end of moveGuest()");
	}

	[RPC]
	void sendAnimBool(string name, bool value){
		anim.SetBool(name,value);
	}

	[RPC]
	void sendAnimFloat(string name, float value){
		anim.SetFloat(name,value);
	}
}