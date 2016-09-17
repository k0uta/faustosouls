using UnityEngine;
using System.Collections;

public class GhostBehaviour : MonoBehaviour {

	private Vector3 nextTarget;

	private float timeWithoutCheck;

	private bool isWandering;

	private Vector2 boundaries;

	private Vector2 size;

	public Transform background;

	public float moveSpeed = 5.0f;

	public float checkTimeThreshold = 2.0f;

	public float stayProbability = 0.3f;

	// Use this for initialization
	void Start () {
		isWandering = true;
		nextTarget = transform.position;
		timeWithoutCheck = 0;

		SpriteRenderer ghostSprite = transform.FindChild ("Sprite").GetComponent<SpriteRenderer> ();
		SpriteRenderer backgroundSprite = background.FindChild("DefaultBackground").GetComponent<SpriteRenderer> ();
		boundaries = new Vector2 ((float)backgroundSprite.bounds.size.x * 0.5f - (float)ghostSprite.bounds.size.x * 0.5f, (float)backgroundSprite.bounds.size.y * 0.5f - (float)ghostSprite.bounds.size.y * 0.5f);
	}

	float RandomSign() {
		return (Random.value > 0.5f) ? -1.0f : 1.0f;
	}

	void AssignNewTarget() {
		nextTarget = transform.position + new Vector3 (Random.Range (1, 2) * RandomSign(), Random.Range (1, 2) * RandomSign());
	}

	void CheckBoundaries() {
		Vector3 currentPosition = transform.position;
		if(Mathf.Abs(currentPosition.x) > boundaries.x) {
			float positionSign = Mathf.Abs (currentPosition.x) / currentPosition.x;
			currentPosition = new Vector2 (boundaries.x * positionSign, currentPosition.y);
		}
		if(Mathf.Abs(currentPosition.y) > boundaries.y) {
			float positionSign = Mathf.Abs (currentPosition.y) / currentPosition.y;
			currentPosition = new Vector2 (currentPosition.x, boundaries.y * positionSign);
		}
		if (!currentPosition.Equals (transform.position)) {
			transform.position = currentPosition;
			AssignNewTarget ();
		}
	}

	void Wander() {
		if(transform.position.Equals(nextTarget)) {
			AssignNewTarget ();
		} else {
			transform.position = Vector3.MoveTowards (transform.position, nextTarget, moveSpeed * Time.deltaTime);
			CheckBoundaries ();
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
