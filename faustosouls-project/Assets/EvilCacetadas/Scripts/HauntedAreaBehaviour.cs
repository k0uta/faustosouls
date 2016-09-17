using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;

public class HauntedAreaBehaviour : MonoBehaviour {

	private float timeWithoutCheck;

	private float currentValue;

	private List<GhostBehaviour> ghosts;

	private bool bloopering;

	private bool triggerBlooper;

	private bool blooped;

	private SkeletonAnimation skeletonAnimation;

	private Spine.AnimationState spineAnimationState;

	public float checkTime = 5.0f;

	public float blooperProbability = 0.3f;

	public float ghostHauntWeight = 0.1f;

	public float blooperValuePerSecond = 100;

	public float blooperValueDecayPerSecond = 5f;

	// Use this for initialization
	void Start () {
		ghosts = new List<GhostBehaviour> ();
		timeWithoutCheck = 0f;
		bloopering = false;
		blooped = false;
		currentValue = 0f;
		skeletonAnimation = GetComponent<SkeletonAnimation> ();
		spineAnimationState = skeletonAnimation.state;
		spineAnimationState.Complete += OnSpineComplete;
		triggerBlooper = false;
	}

	void OnSpineComplete (Spine.AnimationState state, int trackIndex, int loopCount) {
		string currentAnimationName = state.GetCurrent(trackIndex).animation.name;
		switch (currentAnimationName) {
		case "normal_idle":
			CheckBlooperAnimation ();
			break;
		case "blooper1":
			FinishBlooper ();
			spineAnimationState.AddAnimation (0, "blooper1_idle", true, 0);
			break;
		case "blooper2":
			FinishBlooper ();
			spineAnimationState.AddAnimation (0, "blooper2_idle", true, 0);
			break;
		default:
			break;
		}
	}

	void FinishBlooper() {
		currentValue = 0f;
		bloopering = false;
		blooped = true;
	}

	void CheckBlooperAnimation() {
		if (!triggerBlooper)
			return;

		bloopering = true;
		triggerBlooper = false;
		currentValue = blooperValuePerSecond;
		spineAnimationState.SetAnimation (0, "blooper2", false);
		foreach (var ghost in ghosts) {
			ghost.StayCheck (this);
		}
	}

	void CheckForBlooper() {
		float totalHauntValue = 0f;
		foreach (var ghost in ghosts) {
			totalHauntValue += (ghost.hauntValue * ghostHauntWeight);
		}

		float valueToBlooper = Random.Range(blooperProbability, 1.0f);
		if (totalHauntValue >= valueToBlooper) {
			triggerBlooper = true;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (!bloopering && !blooped) {
			timeWithoutCheck += Time.deltaTime;
			if (timeWithoutCheck > checkTime) {
				CheckForBlooper ();
				timeWithoutCheck = 0f;
			}
		} else {
			if(currentValue > 0)
				currentValue -= blooperValueDecayPerSecond * Time.deltaTime;
		}
	}

	public void AddGhost(GhostBehaviour ghost) {
		ghosts.Add (ghost);
	}

	public void RemoveGhost(GhostBehaviour ghost) {
		ghosts.Remove (ghost);
	}

	public float GetCurrentValue() {
		return currentValue;
	}

	public void CheckForBlooperRecovery() {
		if (!blooped)
			return;

		blooped = false;
		spineAnimationState.SetAnimation (0, "normal_idle", true);
	}
}
