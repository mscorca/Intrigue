﻿using UnityEngine;
using System.Collections;

public class LeaveLobby : MonoBehaviour {

	void OnClick(){
		PhotonNetwork.LeaveRoom();
		PhotonNetwork.LoadLevel( "MainMenu" );
	}
}