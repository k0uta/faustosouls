using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class CameraController : MonoBehaviour {

	private SpriteRenderer evilBackgroundRenderer;

	private bool recording;

	private bool ghosting;

	private int score;

	private List<GhostBehaviour> ghosts;

	private Vector2 boundaries;

	private Text timeText;

	private Text timeTextShadow;

	private Text scoreText;

	private Text scoreTextShadow;

	private Transform ghostRecordingGroup;

	private Image ghostRecordingImage;

	private Transform recordingGroup;

	private Image recordingImage;

	private float recordingTime;

	private float ghostingTime;

	private List<HauntedAreaBehaviour> hauntedAreas;

	private AudioSource laughSource;

	public Transform background;

	public Transform ghostsGroup;

	public float speed = 1.0f;

	public float batteryChargeAnimationSpeed = 40;

	public float recordingMinutes = 1;

	public float focusBonus = 1.0f;

	public float ghostingMinutes = 1;

	public Transform hudCanvas;

	private RectTransform batteryChargeMask;

	private Vector2 batteryChargeMaskSize;

	private RectTransform batteryCharge;

	private RectTransform batteryCharge2;

	private RectTransform batteryChargeSize;

	private Vector2 batteryChargeAnimationTarget;

	public Transform hauntedAreasGroup;

	public Transform tutorialCanvas;

	private bool isShowingTutorial = false;

	// Use this for initialization
	void Start () {
		Cursor.lockState = CursorLockMode.Locked;

		#if UNITY_ANDROID || UNITY_IOS
		if(SystemInfo.supportsGyroscope)
			Input.gyro.enabled = true;
		#endif

		score = 0;
		PlayerPrefs.SetInt("Score", score);

		evilBackgroundRenderer = background.FindChild ("EvilBackground").GetComponent<SpriteRenderer> ();

		SpriteRenderer backgroundSprite = background.FindChild("DefaultBackground").GetComponent<SpriteRenderer> ();

		Vector3 screenSize = Camera.main.ScreenToWorldPoint (new Vector3 (Screen.width, Screen.height));
		boundaries = new Vector2 (((float)backgroundSprite.bounds.size.x * 0.5f) - (screenSize.x), ((float)backgroundSprite.bounds.size.y * 0.5f) - (screenSize.y));

		ghosts = new List<GhostBehaviour> ();
		for (int i = 0; i < ghostsGroup.childCount; i++) {
			ghosts.Add(ghostsGroup.GetChild (i).GetComponent<GhostBehaviour> ());
		}

		hauntedAreas = new List<HauntedAreaBehaviour> ();
		for (int i = 0; i < hauntedAreasGroup.childCount; i++) {
			hauntedAreas.Add(hauntedAreasGroup.GetChild(i).GetComponent<HauntedAreaBehaviour>());
		}

		recordingTime = recordingMinutes * 60.0f;

		ghostingTime = ghostingMinutes * 60.0f;

		timeText = hudCanvas.FindChild("TimeText").GetComponent<Text> ();
		timeTextShadow = hudCanvas.FindChild("TimeTextShadow").GetComponent<Text> ();
		recordingGroup = hudCanvas.FindChild ("RecordingGroup");
		recordingImage = recordingGroup.FindChild("RecordingImage").GetComponent<Image> ();
		ghostRecordingGroup = hudCanvas.FindChild("GhostRecordingGroup");
		ghostRecordingImage = ghostRecordingGroup.FindChild("GhostRecording").GetComponent<Image> ();
		batteryChargeMask = hudCanvas.FindChild ("BatteryChargeMask").GetComponent (typeof (RectTransform)) as RectTransform;
		batteryChargeMaskSize = batteryChargeMask.sizeDelta; 
		batteryCharge = hudCanvas.FindChild ("BatteryChargeMask").FindChild ("BatteryCharge").GetComponent (typeof (RectTransform)) as RectTransform;
		batteryCharge2 = hudCanvas.FindChild ("BatteryChargeMask").FindChild ("BatteryCharge2").GetComponent (typeof (RectTransform)) as RectTransform;
		batteryChargeAnimationTarget = batteryCharge.anchoredPosition - new Vector2(batteryCharge.sizeDelta.x, 0);

		scoreText = hudCanvas.FindChild("ScoreText").GetComponent<Text> ();
		scoreTextShadow = hudCanvas.FindChild("ScoreTextShadow").GetComponent<Text> ();
		
		#if UNITY_ANDROID || UNITY_IOS
			tutorialCanvas.FindChild("TutorialMobile").GetComponent<Image>().enabled = true;
			tutorialCanvas.FindChild("Tutorial").GetComponent<Image>().enabled = false;
		#else
			tutorialCanvas.FindChild("TutorialMobile").GetComponent<Image>().enabled = false;
			tutorialCanvas.FindChild("Tutorial").GetComponent<Image>().enabled = true;
		#endif
		isShowingTutorial = true;

		laughSource = transform.FindChild ("Laughs").GetComponent<AudioSource> ();

		StopGhosting();
		StopRecording();
		UpdateStatusText ();

		StartCoroutine (RecordingHudImageLoop ());
		StartCoroutine (GhostRecordingHudImageLoop ());
	}

	IEnumerator RecordingHudImageLoop() {
		while (true) {
			recordingImage.enabled = !recordingImage.enabled;
			yield return new WaitForSeconds (0.4f);
		}
	}

	IEnumerator GhostRecordingHudImageLoop() {
		while (true) {
			ghostRecordingImage.enabled = !ghostRecordingImage.enabled;
			yield return new WaitForSeconds (0.4f);
		}
	}

	// Update is called once per frame
	void Update () {
		if(isShowingTutorial) {
			if(Input.GetMouseButton(0)) {
				isShowingTutorial = false;
				tutorialCanvas.gameObject.SetActive(false);
			}
			
			return;
		}

		if (Input.GetKeyDown (KeyCode.Escape))
			Cursor.visible = true;

		bool ghostInput = Input.GetMouseButton(1);
		bool recordInput = !ghostInput && Input.GetMouseButton(0);
		if (ghostInput) {
			if (!ghosting && ghostingTime > 0f) {
				StartGhosting ();
			}
		} else {
			if (ghosting)
				StopGhosting ();
		}

		if (recordInput) {
			if (!recording) {
				StartRecording ();
			}
		} else {
			if (recording)
				StopRecording ();
		}

		if (recording) {
			recordingTime -= Time.deltaTime;
		}

		if (recordingTime <= 0.0f || ghostingTime <= 0.0f) {
			PlayerPrefs.SetInt("Score", score);
			PlayerPrefs.Save ();
			SceneManager.LoadScene("GameOver");
		}

		CheckCameraArea ();

		if (ghosting) {
			ghostingTime -= Time.deltaTime;

			batteryCharge.anchoredPosition = Vector2.MoveTowards (batteryCharge.anchoredPosition, batteryChargeAnimationTarget, batteryChargeAnimationSpeed * Time.deltaTime);
			batteryCharge2.anchoredPosition = Vector2.MoveTowards (batteryCharge2.anchoredPosition, batteryChargeAnimationTarget, batteryChargeAnimationSpeed * Time.deltaTime);
			if(batteryCharge.anchoredPosition.Equals(batteryChargeAnimationTarget)) {
				batteryCharge.anchoredPosition = new Vector2(batteryCharge2.anchoredPosition.x + batteryCharge.sizeDelta.x, batteryCharge.anchoredPosition.y);
			} else if(batteryCharge2.anchoredPosition.Equals(batteryChargeAnimationTarget)) {
				batteryCharge2.anchoredPosition = new Vector2(batteryCharge.anchoredPosition.x + batteryCharge.sizeDelta.x, batteryCharge2.anchoredPosition.y);
			}

			batteryChargeMask.sizeDelta = new Vector2 (batteryChargeMaskSize.x * (ghostingTime / (ghostingMinutes * 60.0f)), batteryChargeMaskSize.y);

			if (ghostingTime <= 0.0f) {
				StopGhosting ();
			}
		}

		#if UNITY_ANDROID || UNITY_IOS
		if(Input.gyro.enabled) {
			transform.Translate(new Vector3(-Input.gyro.rotationRateUnbiased.y * speed * 2.0f, Input.gyro.rotationRateUnbiased.x * speed * 2.0f));
		} else {
			transform.Translate(new Vector3(Input.acceleration.x * speed * 3.0f, 0f));
		}
		#else
		transform.Translate (new Vector3(Input.GetAxis ("Mouse X") * speed, Input.GetAxis ("Mouse Y") * speed));
		#endif

		CheckBoundaries ();

		UpdateStatusText ();
	}

	void CheckCameraArea() {
		int newScore = score;
		for (int i = 0; i < hauntedAreas.Count; i++) {
			HauntedAreaBehaviour hauntedArea = hauntedAreas [i];
			Vector2 hauntedAreaSize = hauntedArea.GetComponent<BoxCollider2D> ().size;
			Vector3 viewPositionCenter = Camera.main.WorldToViewportPoint (hauntedArea.transform.position);
			Vector3 viewPositionLeft = Camera.main.WorldToViewportPoint (hauntedArea.transform.position - new Vector3(hauntedAreaSize.x * 0.5f, 0));
			Vector3 viewPositionRight = Camera.main.WorldToViewportPoint (hauntedArea.transform.position + new Vector3 (hauntedAreaSize.x * 0.5f, 0));
			if ((viewPositionRight.x >= 0 && viewPositionLeft.x <= 1) && (viewPositionCenter.y >= 0 && viewPositionCenter.y <= 1)) {
				if (recording) {
					float positionBonus = 0.5f - ((viewPositionCenter.x > 0.5f) ? (viewPositionCenter.x - 0.5f) : (0.5f - viewPositionCenter.x));
					positionBonus += 0.5f - ((viewPositionCenter.y > 0.5f) ? (viewPositionCenter.y - 0.5f) : (0.5f - viewPositionCenter.y));
					positionBonus *= focusBonus;
					newScore += (int)Mathf.Floor(Time.deltaTime * hauntedArea.GetCurrentValue () * positionBonus);
				}
			} else {
				hauntedArea.CheckForBlooperRecovery ();
			}
		}

		if (recording && newScore != score) {
			laughSource.volume = 0.2f;
		} else {
			laughSource.volume = 0f;
		}

		score = newScore;
	}

	void UpdateStatusText() {
		string timeString = (Mathf.Floor(recordingTime / 60).ToString ("00") + "’" + Mathf.Floor(recordingTime % 60).ToString ("00") + "”");
		string scoreString = "Score:\n" + score.ToString ("N0");

		timeText.text = timeString;
		timeTextShadow.text = timeString;

		scoreText.text = scoreString;
		scoreTextShadow.text = scoreString;
	}

	void StartGhosting() {
		ghosting = true;
		evilBackgroundRenderer.enabled = ghosting;
		ghostRecordingGroup.gameObject.SetActive (ghosting);
		for (int i = 0; i < ghosts.Count; i++) {
			ghosts [i].SetState (true);
		}
	}

	void StopGhosting() {
		ghosting = false;
		evilBackgroundRenderer.enabled = ghosting;
		ghostRecordingGroup.gameObject.SetActive (ghosting);
		for (int i = 0; i < ghosts.Count; i++) {
			ghosts [i].SetState (false);
		}
	}

	void StartRecording() {
		recording = true;
		recordingGroup.gameObject.SetActive (true);
	}

	void StopRecording() {
		recording = false;
		recordingGroup.gameObject.SetActive (false);
		laughSource.volume = 0;
	}

	void CheckBoundaries() {
		Vector3 currentPosition = transform.position;
		if(Mathf.Abs(currentPosition.x) > boundaries.x) {
			float positionSign = Mathf.Abs (currentPosition.x) / currentPosition.x;
			currentPosition = new Vector3 (boundaries.x * positionSign, currentPosition.y, currentPosition.z);
		}
		if(Mathf.Abs(currentPosition.y) > boundaries.y) {
			float positionSign = Mathf.Abs (currentPosition.y) / currentPosition.y;
			currentPosition = new Vector3 (currentPosition.x, boundaries.y * positionSign, currentPosition.z);
		}
		if (!currentPosition.Equals (transform.position)) {
			transform.position = currentPosition;
		}
	}
}
