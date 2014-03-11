using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RBS;
using BehaviorTree;

public class BaseAI : Photon.MonoBehaviour {

	public NavMeshAgent agent;

	private Vector3 correctPlayerPos;
	private Quaternion correctPlayerRot;
	private Rule currentRule;
	private float updateWants = 5f;
	// private int indent;

	protected List<Rule> rules;

	// private static List<GameObject> ais = new List<GameObject>();

	// AI info
	[HideInInspector] public Animator anim;
	[HideInInspector] public Vector3 destination;
	[HideInInspector] public AI_RoomState room;
	[HideInInspector] public Task tree = null;
	[HideInInspector] public bool isYourTurn = false;
	[HideInInspector] public Status status = Status.False;
	[HideInInspector] public float distFromDest = 5f;

	// Wants, needs, and feelings 0-100 scale
	[HideInInspector] public float thirst = 0f;
	[HideInInspector] public float bored = 0f;
	[HideInInspector] public float hunger = 0f;
	[HideInInspector] public float lonely = 0f;
	[HideInInspector] public float tired = 0f;
	[HideInInspector] public float anxiety = 0f;
	[HideInInspector] public float bladder = 0f;

	void Start(){
		// indent = ais.Count;
		// ais.Add(gameObject);
		anim = GetComponent<Animator>();
		anim.speed = 1f;
		initAI();
		// thirst = Random.Range(0, 100);
		bored = 51;//Random.Range(0, 100);
		// hunger = Random.Range(0, 100);
		lonely = 51;//Random.Range(0, 100);
		// tired = Random.Range(0, 100);
		// anxiety = Random.Range(0, 100);
		// bladder = Random.Range(0, 100);
	}

	public void Update(){
		if(!PhotonNetwork.isMasterClient){
			transform.position = Vector3.Lerp(transform.position, this.correctPlayerPos, Time.deltaTime * 5);
			transform.rotation = Quaternion.Lerp(transform.rotation, this.correctPlayerRot, Time.deltaTime * 5);
		} else {
			switch(status){
				case Status.False:
					//Sort the list in terms of weight
					rules.Sort();

					for (int i = 0; i < rules.Count; i++){
						if (rules[i].isFired()){
							currentRule = rules[i];
							rules[i].weight -= 15;
							status = rules[i].consequence(gameObject);
							break;
						}
					}
					break;

				case Status.True:
					status = Status.Waiting;
					Invoke("backToRule", 2.5f);
					break;

				case Status.Tree:
					if( tree.run(gameObject) == Status.True){
						Invoke("backToRule", 5f);
						tree = null;
						status = Status.Waiting;
					}
					break;

				case Status.Waiting:
					if(agent.hasPath && agent.remainingDistance < distFromDest){
						anim.SetFloat("Speed", 0f);
						agent.ResetPath();
						if(tree == null)
							status = Status.False;
						else
							status = Status.Tree;
					}
					break;
			}
		}
	}

	void FixedUpdate(){
		if(updateWants < 0){
			if( thirst < 100) thirst += 1f;
			if( bored < 100) bored += 1f;
			if( hunger < 100) hunger += 1f;
			if( lonely < 100) lonely += 1f;
			if( tired < 100) tired += 1f;
			if( anxiety < 100) anxiety += 1f;
			if( bladder < 100) bladder += 1f;
			updateWants = 5f;
		} else {
			updateWants -= Time.deltaTime;
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
		if (stream.isWriting){
			// We own this player: send the others our data
			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
			stream.SendNext((float) anim.GetFloat("Speed"));

		}else{
			// Network player, receive data
			this.correctPlayerPos = (Vector3) stream.ReceiveNext();
			this.correctPlayerRot = (Quaternion) stream.ReceiveNext();
			anim.SetFloat("Speed", (float) stream.ReceiveNext());
		}
	}

	// void OnGUI(){
	// 	GUI.color = Color.black;
	// 	GUILayout.BeginArea(new Rect(100 * indent, 0, 100, 200));
	// 		GUILayout.Label( "thirst " + thirst);
	// 		GUILayout.Label( "bored " + bored);
	// 		GUILayout.Label( "hunger " + hunger);
	// 		GUILayout.Label( "lonely " + lonely);
	// 		GUILayout.Label( "tired " + tired);
	// 		GUILayout.Label( "anxiety " + anxiety);
	// 		GUILayout.Label( "bladder " + bladder);
	// 	GUILayout.EndArea();
	// }

	void initAI(){
		rules = new List<Rule>();

		Rule rule0 = new WantToGetDrink(gameObject);
		rule0.weight = 7;
		rules.Add(rule0);

		Rule rule4 = new WantToConverse(gameObject);
		rule4.weight = 4;
		rules.Add(rule4);

		Rule rule5 = new NeedToUseRestroom(gameObject);
		rule5.weight = 10;
		rules.Add(rule5);
	}

	void backToRule(){
		Debug.Log("Back to rule");
		if(currentRule.antiConsequence != null)
			currentRule.antiConsequence();
		if(!agent.hasPath) status = Status.False;
	}

	public void addDrink(){
		Debug.Log("adding Drink");
		this.thirst -= 10;
		this.bladder += 5;
	}
}
