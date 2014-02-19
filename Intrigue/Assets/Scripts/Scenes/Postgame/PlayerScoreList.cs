﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerScoreList : MonoBehaviour {

	private PhotonView photonView = null;
	private Player player;
	public GameObject playerPrefab;
	public GameObject guardTable;
	private GameObject team1s;
	private GameObject team2s;
	private GameObject wins;
	private float totteam1 = 0;
	private float totteam2 = 0;
	private List<string> team1= new List<string>();
	private List<string> team2 = new List<string>();

	// Use this for initialization
	void Start(){
		Screen.lockCursor = false;
		this.photonView = PhotonView.Get(this);
		PhotonNetwork.networkingPeer.NewSceneLoaded();
		player = GameObject.Find("Player").GetComponent<Player>();
		if(player.TeamID==1){
			photonView.RPC("addTeam1", PhotonTargets.AllBuffered, player.Handle, player.Score);
		}
		else{
			photonView.RPC("addTeam2", PhotonTargets.AllBuffered, player.Handle, player.Score);
		}

		team1s = GameObject.FindGameObjectWithTag("Team1Score");
		team2s = GameObject.FindGameObjectWithTag("Team2Score");
		wins = GameObject.FindGameObjectWithTag("Winner");

		if(player.TeamID==1){
			team1s.GetComponent<UILabel>().text = "TEAM 1: " + "[FF0000]"+player.TeamScore;
			team2s.GetComponent<UILabel>().text = "TEAM 2: " + "[FF0000]"+player.EnemyScore;
			if(player.TeamScore>player.EnemyScore)
				wins.GetComponent<UILabel>().text = "TEAM 1 WINS";
			else
				wins.GetComponent<UILabel>().text = "TEAM 2 WINS";				
		}
		else{
			team1s.GetComponent<UILabel>().text = "TEAM 1: " + "[FF0000]"+player.EnemyScore;
			team2s.GetComponent<UILabel>().text = "TEAM 2: " + "[FF0000]"+player.TeamScore;
			if(player.TeamScore<player.EnemyScore)
				wins.GetComponent<UILabel>().text = "TEAM 1 WINS";
			else
				wins.GetComponent<UILabel>().text = "TEAM 2 WINS";	
		}



	}
	
	// Update is called once per frame
	void Update(){
			photonView.RPC("editPing", PhotonTargets.AllBuffered, player.Handle, player.TeamID, player.Score, PhotonNetwork.networkingPeer.RoundTripTime);
	}

	[RPC]
	void editPing(string handle, int TeamID, int score, int ping){
		string pingColor = "[000000]";
		if (ping<50)
			pingColor = "[00FF00]";
		else if(ping<100)
			pingColor = "[FF9D00]";
		else
			pingColor = "[FF0000]";

		if(TeamID==1){
					foreach(Transform child in transform){
						if(child.gameObject.GetComponent<UILabel>().user == handle){
							child.gameObject.GetComponent<UILabel>().text = "[0000FF]" + handle + " : [000000]" + score + "     [000000]("+ pingColor+ping+"[-]" + ") ms";
						}
					}
				}
		else{
			foreach(Transform child in guardTable.transform){
				if(child.gameObject.GetComponent<UILabel>().user == handle){
					child.gameObject.GetComponent<UILabel>().text = "[0000FF]" + handle + " : [000000]" + score + "     [000000]("+ pingColor+ping+"[-]"  + ") ms";
				}
			}
		}
	}

	[RPC]
	void addTeam1(string handle, int score){
		team1.Add(handle);
		GameObject playerInfo = NGUITools.AddChild(gameObject, playerPrefab);
		Vector3 temp = new Vector3(0f,(team1.Count-1)*0.1f,0);
		playerInfo.transform.position-=temp;
		UILabel label = playerInfo.GetComponent<UILabel>();
		label.user = handle;
		label.text = handle + " : " + score;
	}

	[RPC]
	void addTeam2(string handle, int score){
		team2.Add(handle);
		GameObject playerInfo = NGUITools.AddChild(guardTable, playerPrefab);
		Vector3 temp = new Vector3(0f,(team2.Count-1)*0.1f,0);
		playerInfo.transform.position-=temp;
		UILabel label = playerInfo.GetComponent<UILabel>();
		label.user = handle;
		label.text = handle + " : " + score;
	}

}