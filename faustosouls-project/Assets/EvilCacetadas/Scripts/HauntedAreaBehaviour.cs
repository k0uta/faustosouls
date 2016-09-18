using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;

public class HauntedAreaBehaviour : MonoBehaviour {

	private float timeWithoutCheck;

	private float currentValue;

	private List<GhostBehaviour> ghosts;

	private AudioSource audioSource;

	private bool bloopering;

	private enum BlooperTrigger
	{
		NONE,
		WEAK,
		STRONG
	};

	private BlooperTrigger blooperTrigger;

	private bool blooped;

	private SkeletonAnimation skeletonAnimation;

	private Spine.AnimationState spineAnimationState;

	public float checkTime = 5.0f;

	public float blooperProbability = 0.5f;

	public float strongBlooperHauntValue = 0.6f;

	public float ghostHauntWeight = 0.1f;

	public float weakBlooperValuePerSecond = 100;

	public float strongBlooperValuePerSecond = 200;

	public float blooperValueDecayPerSecond = 5f;

	public AudioClip[] blooperSounds;

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
		spineAnimationState.Event += EventHandler;
		blooperTrigger = BlooperTrigger.NONE;
		audioSource = GetComponent<AudioSource> ();
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

	void EventHandler(Spine.AnimationState state, int trackIndex, Spine.Event spineEvent) {
		Debug.Log (spineEvent.Data.name);
		switch (spineEvent.Data.name) {
		case "snd_cassetada_1":
			audioSource.PlayOneShot (blooperSounds [0]);
			break;
		case "snd_cassetada_2":
			audioSource.PlayOneShot (blooperSounds [1]);
			break;
		case "snd_cassetada_3":
			audioSource.PlayOneShot (blooperSounds [2]);
			break;
		default:
			break;			
		}
	}

	void CheckBlooperAnimation() {
		if (blooperTrigger == BlooperTrigger.NONE)
			return;

		bloopering = true;
		currentValue = (blooperTrigger == BlooperTrigger.WEAK) ? weakBlooperValuePerSecond : strongBlooperValuePerSecond;

		string blooperAnimation = (blooperTrigger == BlooperTrigger.WEAK) ? "blooper1" : "blooper2";
		blooperTrigger = BlooperTrigger.NONE;

		spineAnimationState.SetAnimation (0, blooperAnimation, false);

		for (int i = 0; i < ghosts.Count; i++) {
			ghosts [i].StayCheck (this, true);
		}
	}

	void CheckForBlooper() {
		float totalHauntValue = 0f;
		for (int i = 0; i < ghosts.Count; i++) {
			totalHauntValue += (ghosts[i].hauntValue * ghostHauntWeight);
		}

		float valueToBlooper = Random.Range(blooperProbability, 1.0f);
		if (totalHauntValue >= valueToBlooper) {
			blooperTrigger = (totalHauntValue < strongBlooperHauntValue) ? BlooperTrigger.WEAK : BlooperTrigger.STRONG;
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

	public bool CanBeHaunted() {
		return (!blooped && !bloopering && (blooperTrigger == BlooperTrigger.NONE));
	}
}
