using UnityEngine;
using System.Collections;

public class GhostBehaviour : MonoBehaviour {

	private Vector3 nextTarget;

	private float timeWithoutCheck;

	private bool isWandering;

	public float moveSpeed = 5.0f;

	public float checkTimeThreshold = 2.0f;

	public float stayProbability = 0.3f;

	// Use this for initialization
	void Start () {
		isWandering = true;
		nextTarget = transform.position;
		timeWithoutCheck = 0;
	}

	void AssignNewTarget() {
		float xSign = (Random.value > 0.5f) ? -1.0f : 1.0f;
		float ySign = (Random.value > 0.5f) ? -1.0f : 1.0f;
		nextTarget = transform.position + new Vector3 (Random.Range (1, 2) * xSign, Random.Range (1, 2) * ySign);
	}

	void Wander() {
		if(transform.position.Equals(nextTarget)) {
			AssignNewTarget ();
		} else {
			transform.position = Vector3.MoveTowards (transform.position, nextTarget, moveSpeed * Time.deltaTime);
		}
	}
	
	// Update is called once per frame
	void Update () {
		timeWithoutCheck += Time.deltaTime;

		if(isWandering)
			Wander ();
	}

	bool StayCheck() {
		if (timeWithoutCheck > checkTimeThreshold) {
			timeWithoutCheck = 0.0f;
			if (Random.value <= stayProbability) {
				return true;
			}
		}

		return false;
	}

	void StayAt(HauntedAreaBehaviour hauntedArea) {
		isWandering = false;
		Debug.Log (hauntedArea.transform.position);
	}

	void OnTriggerStay2D(Collider2D other) {
		if (other.tag.Equals("Cacetarea")) {
			if (StayCheck ()) {
				StayAt (other.GetComponent<HauntedAreaBehaviour>());
			}
		}
	}
}
