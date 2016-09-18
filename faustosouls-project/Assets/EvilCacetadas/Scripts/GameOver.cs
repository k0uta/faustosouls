using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameOver : MonoBehaviour {

	public Transform hudCanvas;

	// Use this for initialization
	void Start () {
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;

		Text timeText = hudCanvas.FindChild("ScoreGroup").FindChild("TextScore").GetComponent<Text> ();
		Text timeTextShadow = hudCanvas.FindChild("ScoreGroup").FindChild("TextScoreShadow").GetComponent<Text> ();
		timeText.text = timeTextShadow.text = PlayerPrefs.GetInt("Score").ToString("N0");
	}
	
	// Update is called once per frame
	void Update () {
	
	}

}
