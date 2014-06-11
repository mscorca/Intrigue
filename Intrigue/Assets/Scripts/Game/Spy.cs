/**************************************************************************
 **                                                                       *
 **                COPYRIGHT AND CONFIDENTIALITY NOTICE                   *
 **                                                                       *
 **    Copyright (c) 2014 Hacksaw Games. All rights reserved.             *
 **                                                                       *
 **    This software contains information confidential and proprietary    *
 **    to Hacksaw Games. It shall not be reproduced in whole or in        *
 **    part, or transferred to other documents, or disclosed to third     *
 **    parties, or used for any purpose other than that for which it was  *
 **    obtained, without the prior written consent of Hacksaw Games.      *
 **                                                                       *
 **************************************************************************/

using UnityEngine;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Spy : BasePlayer{
	
	[HideInInspector] public float percentComplete = 0;
	[HideInInspector] public bool doingObjective = false;
	[HideInInspector] public string objectiveType;
	
	public UILabel stunsUI;
	public UIPanel objPanel;
	public UISlider objSlider;
	public AudioSource stunAudio;

	private int stuns = 3;
	private GameObject [] serverInUse; 

	void Awake(){
		stunsUI.text = "Stun Charges:\n[00FF00]3[-]";
		serverInUse = GameObject.FindGameObjectsWithTag("ObjectiveMain");
	}

	protected override void Start(){
		layerMask = (1 << GUEST) | (1 << GUARD) |
					(1 << Objective.OBJECTIVE) |
					(1 << ObjectiveMain.OBJECTIVEMAIN) |
					(1 << WALL);
		PhotonNetwork.player.SetCustomProperties(new Hashtable(){{"Team", "Spy"},{"Ping", PhotonNetwork.GetPing()}, {"isOut", false}});	
		base.Start();
	}

	protected override void Update () {
		base.Update();
		if(photonView.isMine){
			//NGUI code for doing Objectives
			/*------------------------------------------------------*/
			NGUITools.SetActive(objPanel.gameObject, doingObjective);
			if(doingObjective){
				objSlider = objPanel.GetComponentInChildren<UISlider>();
				objSlider.value = percentComplete;
			}
			/*------------------------------------------------------*/

			//NGUI code for getting out
			/*------------------------------------------------------*/
			if(isOut){
				NGUITools.SetActive(outLabel, true);
				if(hairHat!=null)
					hairHat.GetComponent<Renderer>().enabled = true;
			}
			else{
				NGUITools.SetActive(outLabel, false);
			}
			/*------------------------------------------------------*/


			//Code for interacting
			/*------------------------------------------------------*/
			if( Input.GetKey(Settings.Interact)){
				if(Camera.main!=null && !isChatting)
					attemptInteract();
			}
			else{
				doingObjective = false;
			}

			if( Input.GetKeyUp(Settings.Interact) ){
				foreach(GameObject serv in serverInUse){
					serv.GetComponent<PhotonView>().RPC("setInUse",PhotonTargets.All, false);
				}
			}

			/*------------------------------------------------------*/

			//Code for stunning
			/*------------------------------------------------------*/
			if ( Input.GetKeyUp(Settings.Stun) ){
				if(Camera.main!=null)
					attemptStun();
			}
			/*------------------------------------------------------*/

			//Code to add [] display for active objectives
			/*------------------------------------------------------*/
			addObjectiveText();
			/*------------------------------------------------------*/

			// Objective Animations
			objAnimations();
		}
	}

	void addObjectiveText(){
		//Create Objective Texts
		GameObject[] objecs = GameObject.FindGameObjectsWithTag("Objective");
		foreach(GameObject objer in objecs){
				if(!objer.GetComponent<Objective>().textAdded && objer.GetComponent<Objective>().isActive){
					objer.GetComponent<Objective>().textAdded = true;
					GameObject textInstance = Instantiate(allytext, objer.transform.position, objer.transform.rotation) as GameObject;
					Vector3 temp = new Vector3(1.5f,-0.75f,0f);
					Vector3 temp2 = new Vector3(270,0,0);
					textInstance.transform.Rotate(temp2);
					textInstance.GetComponent<AllyText>().offset = temp;
					textInstance.transform.localScale += new Vector3(0.5f,0.5f,0.5f);
					textInstance.GetComponent<AllyText>().target = objer.transform;
					textInstance.transform.parent = objer.transform;
					textInstance.GetComponent<TextMesh>().text = "[ ]";
				}
				else if (!objer.GetComponent<Objective>().isActive && objer.GetComponent<Objective>().textAdded){
					objer.GetComponentInChildren<TextMesh>().text = "";
				}
		}
	}

	void attemptInteract(){
		Ray ray = Camera.main.ScreenPointToRay( screenPoint );
		RaycastHit hit;
		if( Physics.Raycast(ray, out hit, 75f, layerMask) ){
			if( hit.transform.gameObject.layer == ObjectiveMain.OBJECTIVEMAIN ){
				ObjectiveMain hitObjective = hit.transform.GetComponent<ObjectiveMain>();
				hitObjective.useObjective(gameObject, player.TeamID);
				objectiveType = hitObjective.objectiveType;
			}
			else if((Vector3.Distance(hit.transform.position, transform.position)<10 && hit.transform.gameObject.layer == Objective.OBJECTIVE)){
				Objective hitObjective = hit.transform.GetComponent<Objective>();
				hitObjective.useObjective(gameObject, player.TeamID);
				objectiveType = hitObjective.objectiveType;				
			}
			else
				doingObjective = false;
		}
		else
				doingObjective = false;
	}

	void attemptStun(){
		Ray ray = Camera.main.ScreenPointToRay( screenPoint );
		RaycastHit hit;
		if( Physics.Raycast(ray, out hit, 15f, layerMask) ){
			if(stuns>=1){
				if(hit.transform.gameObject.layer == BasePlayer.GUARD){
					if(!hit.transform.gameObject.GetComponent<Guard>().stunned && !hit.transform.gameObject.GetComponent<Guard>().recentlyStunned){
					
						//Audio for stun
						if(!stunAudio.isPlaying){
							stunAudio.Play();
							photonView.RPC("playStunAudio", PhotonTargets.Others);
						}

						pointPop.GetComponent<TextMesh>().text = "+50";
						Instantiate(pointPop, hit.transform.position + (Vector3.up * hit.transform.gameObject.GetComponent<Collider>().bounds.size.y), hit.transform.rotation);
						photonView.RPC("addPlayerScore", PhotonTargets.All, 50, player.TeamID);
						PhotonView pv = hit.transform.GetComponent<PhotonView>();
						pv.RPC("stunGuard", pv.owner);
						hit.transform.gameObject.GetComponent<Guard>().stunned = true;
						stuns--;
						base.newEvent("[00CCFF]"+player.Handle+"[-] [FFCC00]has stunned [-][FF2B2B]" + hit.transform.gameObject.GetComponent<BasePlayer>().localHandle + "[-][FFCC00]![-]");
				
					}
				}
				else if(hit.transform.gameObject.layer == BasePlayer.GUEST && !hit.transform.gameObject.GetComponent<BaseAI>().stunned && !hit.transform.gameObject.GetComponent<BaseAI>().recentlyStunned){
					//Audio for stun
					if(!stunAudio.isPlaying){
						stunAudio.Play();
						photonView.RPC("playStunAudio", PhotonTargets.Others);
					}
					
					
					pointPop.GetComponent<TextMesh>().text = "-50";
					Instantiate(pointPop, hit.transform.position + (Vector3.up * hit.transform.gameObject.GetComponent<Collider>().bounds.size.y), hit.transform.rotation);
					photonView.RPC("addPlayerScore", PhotonTargets.All, -50, player.TeamID);
					PhotonView pv = hit.transform.GetComponent<PhotonView>();
					pv.RPC("stunAI", PhotonTargets.MasterClient);
					stuns--;
					base.newEvent("[00CCFF]"+player.Handle+"[-] [FFCC00]has stunned a guest!");
				}
				stunsUI.text = "Stun Charges:\n[FF00FF]"+stuns+"[-]";
			}

		}
	}

	void objAnimations(){
		if(doingObjective){
			if(objectiveType=="Safe"){
				animator.SetBool("InteractSafe",true);
			}
			else if(objectiveType=="Computer"){
				animator.SetBool("InteractComp",true);
			}
			else if(objectiveType=="Server"){
				animator.SetBool("InteractServer",true);
			}
		}
		else{
			animator.SetBool("InteractSafe",false);
			animator.SetBool("InteractComp",false);
			animator.SetBool("InteractServer",false);
		}
	}

	[RPC]
	void playStunAudio(){
		if(!stunAudio.isPlaying)
			stunAudio.Play();
	}

	[RPC]
	void destroySpy(){
		isOut = true;
		PhotonNetwork.player.SetCustomProperties(new Hashtable(){{"isOut", true}});
		gameObject.GetComponent<NetworkCharacter>().isOut = true;
	}

	[RPC]
	void addPlayerScore(int scoreToAdd, int teamID){
		if(photonView.isMine){
			player.Score += scoreToAdd;
			PhotonNetwork.player.SetCustomProperties(new Hashtable(){{"Score", player.Score}});
		}
		//Adding to team scores
		if(teamID == 1){
			player.Team1Score +=scoreToAdd;
			PhotonNetwork.player.SetCustomProperties(new Hashtable(){{"Team1Score", player.Team1Score}});
		}
		else{
			player.Team2Score += scoreToAdd;
			PhotonNetwork.player.SetCustomProperties(new Hashtable(){{"Team2Score", player.Team2Score}});
		}
	}

	[RPC]
	void createTeleport(){
		GameObject teleportInstance = Instantiate(teleportPrefab, transform.position, Quaternion.identity) as GameObject;
		teleportInstance.transform.parent = gameObject.transform;
	}

	[RPC]
	void setLocalHandle(string handle){
		this.localHandle = handle;
	}
}