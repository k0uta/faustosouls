using UnityEngine;
using System.Collections;
using Spine.Unity;

public class GhostBehaviour : MonoBehaviour {

	private Vector3 nextTarget;

	private float timeWithoutCheck;

	private bool isWandering;

	private Vector2 boundaries;

	private Vector2 size;

	private HauntedAreaBehaviour currentHauntedArea;

	private SkeletonAnimation skeletonAnimation;

	private Spine.AnimationState animationState;

	public float hauntValue;

	public float hauntSpeed = 0.1f;

	public float moveSpeed = 5.0f;

	public float enterCheckTime = 2.0f;

	public float exitCheckTime = 4.0f;

	public float enterProbability = 0.3f;

	public float exitProbability = 0.5f;

	void Start () {
		isWandering = true;
		nextTarget = transform.position;
		timeWithoutCheck = enterCheckTime;
		hauntValue = 0f;

		Transform background = GameObject.Find ("Background").transform;

		BoxCollider2D ghostBoxCollider = transform.GetComponent<BoxCollider2D> ();
		SpriteRenderer backgroundSprite = background.FindChild("DefaultBackground").GetComponent<SpriteRenderer> ();
		boundaries = new Vector2 ((float)backgroundSprite.bounds.size.x * 0.5f - (float)ghostBoxCollider.size.x * 0.5f, (float)backgroundSprite.bounds.size.y * 0.5f - (float)ghostBoxCollider.size.y * 0.5f);

		skeletonAnimation = transform.FindChild ("Spine").GetComponent<SkeletonAnimation> ();
		animationState = skeletonAnimation.state;
	}

	float RandomSign() {
		return (Random.value > 0.5f) ? -1.0f : 1.0f;
	}

	void AssignNewHauntTarget() {
		nextTarget = currentHauntedArea.transform.position + new Vector3 (Random.value * 0.5f * RandomSign (), Random.value * 0.5f * RandomSign ());
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

	void Haunt() {
		hauntValue += Time.deltaTime * hauntSpeed;
		hauntValue = Mathf.Min (hauntValue, 1.0f);

		if (currentHauntedArea) {
			if(transform.position.Equals(nextTarget)) {
				AssignNewHauntTarget ();
			} else {
				transform.position = Vector3.MoveTowards (transform.position, nextTarget, moveSpeed * Time.deltaTime);
			}
//			transform.position = Vector3.MoveTowards (transform.position, ), moveSpeed * Time.deltaTime);
		}
		StayCheck (currentHauntedArea);
	}

	void Update () {
		timeWithoutCheck += Time.deltaTime;

		if (isWandering) {
			Wander ();
		} else {
			Haunt ();
		}
	}

	public void StayCheck(HauntedAreaBehaviour hauntedArea) {
		if (isWandering) {
			if (timeWithoutCheck > enterCheckTime) {
				timeWithoutCheck = 0.0f;
				if (Random.value <= enterProbability) {
					StayAt (hauntedArea);
				}
			}
		} else {
			if (timeWithoutCheck > exitCheckTime) {
				timeWithoutCheck = 0.0f;
				if (Random.value <= exitProbability) {
					ExitFrom (hauntedArea);
				}
			}
		}
	}

	void StayAt(HauntedAreaBehaviour hauntedArea) {
		isWandering = false;
		hauntValue = 0f;
		hauntedArea.AddGhost (this);
		currentHauntedArea = hauntedArea;
		animationState.SetAnimation (0, "blooper", true);
	}

	void ExitFrom(HauntedAreaBehaviour hauntedArea) {
		isWandering = true;
		hauntValue = 0f;
		hauntedArea.RemoveGhost (this);
		currentHauntedArea = null;
		animationState.SetAnimation (0, "normal_idle", true);
	}

	void OnTriggerStay2D(Collider2D other) {
		if (other.tag.Equals("HauntedArea")) {
			StayCheck (other.GetComponent<HauntedAreaBehaviour> ());
		}
	}

	public void SetState(bool state) {
		animationState.SetAnimation (1, (state ? "show" : "hide"), true);
	}
}
