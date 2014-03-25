using UnityEngine;
using System.Collections;

public class Spy : BasePlayer{
	
	private UIPanel objPanel;
	private UISlider objSlider;
	public float percentComplete = 0;
	public bool doingObjective = false;
	public string objectiveType;

	protected override void Update () {
		base.Update();
		//Locate the necessary NGUI objects
		/*------------------------------------------------------*/
		locateNGUIObjects();
		/*------------------------------------------------------*/



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



		//NGUI code for updating time/round display
		/*------------------------------------------------------*/
		if(timeLabel!=null)
			updateTimeLabel();
		/*------------------------------------------------------*/



		//Code for interacting
		/*------------------------------------------------------*/
		if(Camera.main!=null)
			attemptInteract();
		/*------------------------------------------------------*/



		//Code to add [] display for active objectives
		/*------------------------------------------------------*/
		addObjectiveText();
		/*------------------------------------------------------*/
	}


	void locateNGUIObjects(){
		guiLabels = GetComponentsInChildren<UILabel>();
		guiPanels = GetComponentsInChildren<UIPanel>(true);
		if(objPanel == null){
			foreach(UIPanel uiP in guiPanels){
				if(uiP.gameObject.CompareTag("ObjectivePanel")){
					objPanel = uiP;
				}
			}
		}
		if(timeLabel == null || outLabel == null){
			foreach(UILabel lab in guiLabels){
				if(lab.gameObject.CompareTag("TimeLabel")){
					timeLabel = lab.gameObject;
				}
				else if(lab.gameObject.CompareTag("OutLabel")){
					outLabel = lab.gameObject;
				}
			}
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

	void updateTimeLabel(){
		int minutesLeft = Mathf.RoundToInt(Mathf.Floor(intrigue.GetTimeLeft/60));
		int seconds = Mathf.RoundToInt(intrigue.GetTimeLeft%60);
		int curRound = intrigue.GetRounds - intrigue.GetRoundsLeft +1;
		string secondsS;
		if(seconds<10)
			secondsS = "0"+seconds.ToString();
		else
			secondsS = seconds.ToString();

		timeLabel.GetComponent<UILabel>().text = minutesLeft +":" 
												+ secondsS + "\nRound: " + 
												curRound +"/" + 
												(intrigue.GetRounds+1);
	}

	void attemptInteract(){
		Ray ray = Camera.main.ScreenPointToRay( screenPoint );
		if (Input.GetKey("e")){
			RaycastHit hit;
			if( Physics.Raycast(ray, out hit, 7.0f) ){
				if( hit.transform.tag == "Objective" ){
					Objective hitObjective = hit.transform.GetComponent<Objective>();
					hitObjective.useObjective(gameObject);
					objectiveType = hitObjective.objectiveType;
				}
				else
					doingObjective = false;
			}
			else
					doingObjective = false;
		}
		else
			doingObjective = false;
	}

	[RPC]
	void destroySpy(){
		if( photonView.isMine){
			isOut = true;
			gameObject.GetComponent<NetworkCharacter>().isOut = true;
		}
	}

		[RPC]
	void giveHandle(string handle){
		localHandle = handle;
	}

	[RPC]
	void giveScore(int score){
		remoteScore = score;
	}

	[RPC]
	void givePing(int ping){
		localPing = ping;
	}

	[RPC]
	void addPlayerScore(int scoreToAdd){
		if(photonView.isMine){
			player.Score += scoreToAdd;
			photonView.RPC("giveScore", PhotonTargets.All, player.Score);
		}
	}
}