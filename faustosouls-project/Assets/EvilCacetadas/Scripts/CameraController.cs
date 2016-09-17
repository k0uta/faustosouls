using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CameraController : MonoBehaviour {

	private SpriteRenderer evilBackgroundRenderer;

	private bool recording;

	private bool ghosting;

	private float score;

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

	public float recordingMinutes = 1;

	public float ghostingMinutes = 1;

	public Transform hudCanvas;

	public Transform hauntedAreasGroup;

	// Use this for initialization
	void Start () {
		Cursor.lockState = CursorLockMode.Locked;

		recording = true;

		ghosting = true;

		score = 0;

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
				Debug.Log ("Game Over");
				Time.timeScale = 0;
			}
		}

		CheckCameraArea ();

		if (ghosting) {
			ghostingTime -= Time.deltaTime;
			if (recordingTime <= 0.0f) {
				StopGhosting ();
			}
		}

		transform.Translate (new Vector3 (Input.GetAxis ("Mouse X") * speed, Input.GetAxis ("Mouse Y") * speed));
		CheckBoundaries ();

		UpdateStatusText ();
	}

	void CheckCameraArea() {
		foreach (var hauntedArea in hauntedAreas) {
			Vector3 viewPosition = Camera.main.WorldToViewportPoint (hauntedArea.transform.position);
//			Vector2 hauntedAreaSize = hauntedArea.GetComponent<BoxCollider2D> ().size;
//			viewPosition += new Vector3 (hauntedAreaSize.x * 0.5f, hauntedAreaSize.y * 0.5f);
			if ((viewPosition.x >= 0 && viewPosition.x <= 1) && (viewPosition.y >= 0 && viewPosition.y <= 1)) {
				if (recording)
					score += Mathf.Floor(Time.deltaTime * hauntedArea.GetCurrentValue ());
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

		foreach (var ghost in ghosts) {
			ghost.SetState(true);
		}
	}

	void StopGhosting() {
		ghosting = false;
		evilBackgroundRenderer.enabled = ghosting;
		foreach (var ghost in ghosts) {
			ghost.SetState(false);
		}
	}

	void StartRecording() {
		recording = true;
		recordingGroup.gameObject.SetActive (true);
//		recordingGroup.SetActive (true);
	}

	void StopRecording() {
		recording = false;
		recordingGroup.gameObject.SetActive (false);
//		recordingGroup.SetActive (false);
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
