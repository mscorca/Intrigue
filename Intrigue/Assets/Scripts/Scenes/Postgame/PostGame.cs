﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PostGame : MonoBehaviour {


	public GameObject playerPrefab;
	public GameObject guardTable;
	public GameObject spyTable;

	private PhotonView photonView = null;
	private Player player;
	private GameObject team1s;
	private GameObject team2s;
	private GameObject wins;
	private List<string> team1= new List<string>();
	private List<string> team2 = new List<string>();

	// Use this for initialization 
	void Start(){
		Screen.lockCursor = false;
		this.photonView = PhotonView.Get(this);
		player = GameObject.Find("Player").GetComponent<Player>();

		InvokeRepeating("syncPingAndScore", 0, 2F);

		if(player.TeamID==1){
			photonView.RPC("addTeam1", PhotonTargets.AllBuffered, player.Handle, player.Score, PhotonNetwork.player.ID);
		}
		else{
			photonView.RPC("addTeam2", PhotonTargets.AllBuffered, player.Handle, player.Score, PhotonNetwork.player.ID);
		}

		team1s = GameObject.FindGameObjectWithTag("Team1Score");
		team2s = GameObject.FindGameObjectWithTag("Team2Score");
		wins = GameObject.FindGameObjectWithTag("Winner");

		if(player.TeamID==1){
			team1s.GetComponent<UILabel>().text = "TEAM 1: " + "[FFFFFF]"+player.TeamScore;
			team2s.GetComponent<UILabel>().text = "TEAM 2: " + "[FFFFFF]"+player.EnemyScore;
			if(player.TeamScore>player.EnemyScore)
				wins.GetComponent<UILabel>().text = "TEAM 1 WINS";
			else
				wins.GetComponent<UILabel>().text = "TEAM 2 WINS";				
		}
		else{
			team1s.GetComponent<UILabel>().text = "TEAM 1: " + "[FFFFFF]"+player.EnemyScore;
			team2s.GetComponent<UILabel>().text = "TEAM 2: " + "[FFFFFF]"+player.TeamScore;
			if(player.TeamScore<player.EnemyScore)
				wins.GetComponent<UILabel>().text = "TEAM 1 WINS";
			else
				wins.GetComponent<UILabel>().text = "TEAM 2 WINS";	
		}

	}
	
	// Update is called once per frame
	void Update(){
	}

	void syncPingAndScore(){
		photonView.RPC("editPing", PhotonTargets.All, player.Handle, player.TeamID, PhotonNetwork.player.ID, player.Score, PhotonNetwork.GetPing());
	}

	void leaveLobby(){
		PhotonNetwork.LeaveRoom();
		PhotonNetwork.LoadLevel( "MainMenu" );
	}

	[RPC]
	void editPing(string handle, int TeamID, int playerID, int score, int ping){
		string pingColor = "[000000]";
		if (ping<50)
			pingColor = "[00FF00]";
		else if(ping<100)
			pingColor = "[FF9D00]";
		else
			pingColor = "[FF0000]";

		if(TeamID==1){
			foreach(Transform child in spyTable.transform){
				if(child.gameObject.GetComponent<UILabel>().user == playerID){
					child.gameObject.GetComponent<UILabel>().text = "[FFFFFF]" + handle + " : [FFFFFF]" + score + "     [FFFFFF]("+ pingColor+ping+"[-]" + ") ms";
				}
			}
		}
		else{
			foreach(Transform child in guardTable.transform){
				if(child.gameObject.GetComponent<UILabel>().user == playerID){
					child.gameObject.GetComponent<UILabel>().text = "[FFFFFF]" + handle + " : [FFFFFF]" + score + "     [FFFFFF]("+ pingColor+ping+"[-]"  + ") ms";
				}
			}
		}
	}

	[RPC]
	void addTeam1(string handle, int score, int playerID){
		team1.Add(handle);
		GameObject playerInfo = NGUITools.AddChild(spyTable, playerPrefab);
		Vector3 temp = new Vector3(0f,(team1.Count-1)*0.1f,0);
		playerInfo.transform.localPosition-=temp;
		UILabel label = playerInfo.GetComponent<UILabel>();
		label.user = playerID;
		label.text = handle + " : " + score;
		syncPingAndScore();
	}

	[RPC]
	void addTeam2(string handle, int score, int playerID){
		team2.Add(handle);
		GameObject playerInfo = NGUITools.AddChild(guardTable, playerPrefab);
		Vector3 temp = new Vector3(0f,(team2.Count-1)*0.1f,0);
		playerInfo.transform.position-=temp;
		UILabel label = playerInfo.GetComponent<UILabel>();
		label.user = playerID;
		label.text = handle + " : " + score;
		syncPingAndScore();
	}
}
