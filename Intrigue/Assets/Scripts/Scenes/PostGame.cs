﻿/**************************************************************************
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
using System.Collections.Generic;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PostGame : MonoBehaviour {

	public GameObject playerPrefab;
	public GameObject guardTable;
	public GameObject spyTable;
	public UITextList textList;
	public UIInput mInput;

	public GameObject team1s;
	public GameObject team2s;
	public GameObject wins;
	private List<string> team1= new List<string>();
	private List<string> team2 = new List<string>();
	private int curSpy = 0;
	private int curGuard = 0;
	private Player player;
	private PhotonView photonView = null;

	// Use this for initialization 
	void Start(){
		Screen.lockCursor = false;
		//Sets Chat Max Line Count
		mInput.label.maxLineCount = 1;
		player = GameObject.Find("Player").GetComponent<Player>();
		this.photonView = PhotonView.Get(this);

		foreach(PhotonPlayer play in PhotonNetwork.playerList){
			if((int)play.customProperties["TeamID"] == 1){
				addTeam1((string)play.customProperties["Handle"], (int)play.customProperties["Score"], play.ID);
			}
			else{
				addTeam2((string)play.customProperties["Handle"], (int)play.customProperties["Score"], play.ID);
			}
		}



		PhotonPlayer playRef = PhotonNetwork.player;
		team1s.GetComponent<UILabel>().text = "TEAM 1: " + "[8169FF]"+  (int)playRef.customProperties["Team1Score"];
		team2s.GetComponent<UILabel>().text = "TEAM 2: " + "[FF2b2b]"+ (int)playRef.customProperties["Team2Score"];
		if((int)playRef.customProperties["Team1Score"] > (int)playRef.customProperties["Team2Score"])
			wins.GetComponent<UILabel>().text = "[FFCC00]TEAM 1 WINS";
		else
			wins.GetComponent<UILabel>().text = "[FFCC00]TEAM 2 WINS";				
	}
	
	// Update is called once per frame
	void Update(){

	}

	void leaveLobby(){
		PhotonNetwork.LeaveRoom();
		PhotonNetwork.LoadLevel( "MainMenu" );
	}

	void addTeam1(string handle, int score, int playerID){
		team1.Add(handle);

		//Name
		GameObject playerInfo = NGUITools.AddChild(spyTable, playerPrefab);
		Vector3 temp = new Vector3(0,-(curSpy)*30f,0);
		playerInfo.transform.localPosition = temp;
		UILabel label = playerInfo.GetComponent<UILabel>();
		label.user = playerID;
		label.text = "[00CCFF]" + handle;

		//Score
		GameObject playerInfo2 = NGUITools.AddChild(spyTable, playerPrefab);
		Vector3 temp2 = new Vector3(330,-(curSpy)*30f,0);
		playerInfo2.transform.localPosition = temp2;
		UILabel label2 = playerInfo2.GetComponent<UILabel>();
		label2.user = playerID;
		label2.text = "[FFFFFF]" + score;

		++curSpy;
	}

	void addTeam2(string handle, int score, int playerID){
		team2.Add(handle);

		//Name
		GameObject playerInfo = NGUITools.AddChild(guardTable, playerPrefab);
		Vector3 temp = new Vector3(0,-(curGuard)*30f,0);
		playerInfo.transform.localPosition = temp;
		UILabel label = playerInfo.GetComponent<UILabel>();
		label.user = playerID;
		label.text = "[FF2B2B]" + handle;

		//Score
		GameObject playerInfo2 = NGUITools.AddChild(guardTable, playerPrefab);
		Vector3 temp2 = new Vector3(330,-(curGuard)*30f,0);
		playerInfo2.transform.localPosition = temp2;
		UILabel label2 = playerInfo2.GetComponent<UILabel>();
		label2.user = playerID;
		label2.text = "[FFFFFF]" + score;
		
		++curGuard;
	}

	public void OnSubmit(){
		if (textList != null)
		{
			// It's a good idea to strip out all symbols as we don't want user input to alter colors, add new lines, etc
			string text = NGUIText.StripSymbols(mInput.value);
			text = StringCleaner.CleanString(text);
			if (!string.IsNullOrEmpty(text) && text.Length>=2){
				if(player.TeamID == 1){
					textList.Add("[8169FF]"+player.Handle+": [-]"+text);
					photonView.RPC("receiveMessage", PhotonTargets.Others, "[8169FF]"+player.Handle+": [-]"+text);
				} else if(player.TeamID == 2) {
					textList.Add("[FF2B2B]"+player.Handle+": [-]"+text);
					photonView.RPC("receiveMessage", PhotonTargets.Others, "[FF2B2B]"+player.Handle+": [-]"+text);
				} else {
					textList.Add(player.Handle+": [-]"+text);
					photonView.RPC("receiveMessage", PhotonTargets.Others, player.Handle+": [-]"+text);
				}
				mInput.value = "";
			}
		}
	}

	[RPC]
	public void receiveMessage(string s){
		textList.Add(s);
	}
}