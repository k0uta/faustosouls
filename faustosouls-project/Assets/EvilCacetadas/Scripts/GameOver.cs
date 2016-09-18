using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameOver : MonoBehaviour {

	public Transform hudCanvas;

	private AudioSource audioSource;

	public AudioClip applause;

	public List<AudioClip> oneLinersAudios = new List<AudioClip>();

	private string[] oneLiners = new string[] {
		"ORRRA MEU!",
		"ESSA FIGURA!",
		"ÔLOCO MEU!"
	};

	// Use this for initialization
	void Start () {
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;

		audioSource = GetComponent<AudioSource> ();

		Text timeText = hudCanvas.FindChild("ScoreGroup").FindChild("TextScore").GetComponent<Text> ();
		Text timeTextShadow = hudCanvas.FindChild("ScoreGroup").FindChild("TextScoreShadow").GetComponent<Text> ();
		timeText.text = timeTextShadow.text = PlayerPrefs.GetInt("Score").ToString("N0");

		int oneLinerIndex = Random.Range(0, oneLiners.Length);

		Text faustoText = hudCanvas.FindChild("Fausto").FindChild("TextBubble").GetComponent<Text>();
		faustoText.text = oneLiners[oneLinerIndex];

		audioSource.PlayOneShot(oneLinersAudios[oneLinerIndex]);
		StartCoroutine (Applause (oneLinersAudios[oneLinerIndex].length));
	}

	IEnumerator Applause(float length) {
		yield return new WaitForSeconds(length);
		audioSource.PlayOneShot(applause);
		yield return new WaitForSeconds(0);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

}
