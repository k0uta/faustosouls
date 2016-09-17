using UnityEngine;
using System.Collections;

public class GhostSpawner : MonoBehaviour {

	public int totalGhosts = 48;

	public Transform ghostPrefab;

	// Use this for initialization
	void Awake () {
		Transform background = GameObject.Find ("Background").transform;
		SpriteRenderer backgroundSprite = background.FindChild("DefaultBackground").GetComponent<SpriteRenderer> ();

		Vector3 screenSize = Camera.main.ScreenToWorldPoint (new Vector3 (Screen.width, Screen.height));
		Vector2 boundaries = new Vector2 (((float)backgroundSprite.bounds.size.x * 0.5f) - (screenSize.x), ((float)backgroundSprite.bounds.size.y * 0.5f) - (screenSize.y));

		for (int i = 0; i < totalGhosts; i++) {
			Transform ghost = Instantiate (ghostPrefab, new Vector3 (Random.Range (-boundaries.x, boundaries.x), Random.Range (-boundaries.y, boundaries.y)), Quaternion.identity) as Transform;
			ghost.parent = transform;
			ghost.name = "Ghost" + i;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
