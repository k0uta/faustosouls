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

	private Transform recordingGroup;

	private Image recordingImage;

	private float recordingTime;

	private float ghostingTime;

	private List<HauntedAreaBehaviour> hauntedAreas;

	public Transform background;

	public Transform ghostsGroup;

	public float speed = 1.0f;

	public float batteryChargeAnimationSpeed = 40;

	public float recordingMinutes = 1;

	public float ghostingMinutes = 1;

	public Transform hudCanvas;

	private RectTransform batteryChargeMask;

	private Vector2 batteryChargeMaskSize;

	private RectTransform batteryCharge;

	private RectTransform batteryCharge2;

	private RectTransform batteryChargeSize;

	private Vector2 batteryChargeAnimationTarget;

	public Transform hauntedAreasGroup;

	// Use this for initialization
	void Start () {
		Cursor.lockState = CursorLockMode.Locked;

		recording = true;

		ghosting = true;

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
		batteryChargeMask = hudCanvas.FindChild ("BatteryChargeMask").GetComponent (typeof (RectTransform)) as RectTransform;
		batteryChargeMaskSize = batteryChargeMask.sizeDelta; 
		batteryCharge = hudCanvas.FindChild ("BatteryChargeMask").FindChild ("BatteryCharge").GetComponent (typeof (RectTransform)) as RectTransform;
		batteryCharge2 = hudCanvas.FindChild ("BatteryChargeMask").FindChild ("BatteryCharge2").GetComponent (typeof (RectTransform)) as RectTransform;
		batteryChargeAnimationTarget = batteryCharge.anchoredPosition - new Vector2(batteryCharge.sizeDelta.x, 0);

		scoreText = hudCanvas.FindChild("ScoreText").GetComponent<Text> ();
		scoreTextShadow = hudCanvas.FindChild("ScoreTextShadow").GetComponent<Text> ();

		StartCoroutine (RecordingHudImageLoop ());
	}

	IEnumerator RecordingHudImageLoop() {
		while (true) {
			recordingImage.enabled = !recordingImage.enabled;
			yield return new WaitForSeconds (0.4f);
		}
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape))
			Cursor.visible = true;

		if (Input.GetMouseButton (1)) {
			if (!ghosting && ghostingTime > 0f) {
				StartGhosting ();
			}
		} else {
			if (ghosting)
				StopGhosting ();
		}

		if (Input.GetMouseButton (0)) {
			if (!recording) {
				StartRecording ();
			}
		} else {
			if (recording)
				StopRecording ();
		}

		if (recording) {
			recordingTime -= Time.deltaTime;
			if (recordingTime <= 0.0f) {
				PlayerPrefs.SetInt("Score", score);
				SceneManager.LoadScene("GameOver");
			}
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

		transform.Translate (new Vector3 (Input.GetAxis ("Mouse X") * speed, Input.GetAxis ("Mouse Y") * speed));
		CheckBoundaries ();

		UpdateStatusText ();
	}

	void CheckCameraArea() {
		for (int i = 0; i < hauntedAreas.Count; i++) {
			HauntedAreaBehaviour hauntedArea = hauntedAreas [i];
			Vector2 hauntedAreaSize = hauntedArea.GetComponent<BoxCollider2D> ().size;
			Vector3 viewPositionLeft = Camera.main.WorldToViewportPoint (hauntedArea.transform.position - new Vector3(hauntedAreaSize.x * 0.5f, 0));
			Vector3 viewPositionRight = Camera.main.WorldToViewportPoint (hauntedArea.transform.position + new Vector3 (hauntedAreaSize.x * 0.5f, 0));
			if ((viewPositionRight.x >= 0 && viewPositionLeft.x <= 1) && (viewPositionLeft.y >= 0 && viewPositionLeft.y <= 1)) {
				if (recording)
					score += (int) Mathf.Floor(Time.deltaTime * hauntedArea.GetCurrentValue ());
			} else {
				hauntedArea.CheckForBlooperRecovery ();
			}
		}
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

		for (int i = 0; i < ghosts.Count; i++) {
			ghosts [i].SetState (true);
		}
	}

	void StopGhosting() {
		ghosting = false;
		evilBackgroundRenderer.enabled = ghosting;
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
