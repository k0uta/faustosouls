using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;

public class BtPressed : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
	public Sprite btUp;
	public Sprite btDown;
	private Image image;

	// Use this for initialization
	void Start () {
		image = GetComponent<Image> ();
		image.sprite = btUp;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnPointerDown(PointerEventData eventData){
		image.sprite = btDown;
    }

	public void OnPointerUp(PointerEventData eventData){
		image.sprite = btUp;
    }

	public void onClick() {
		SceneManager.LoadScene("MainScene");
	}
}
