using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class OutGame: MonoBehaviour {
	public GameObject gameQuitPanel;

	public void GameStart() {
		SceneManager.LoadScene ("InGame");
	}

	public void OkGameQuit() {
		Application.Quit ();
	}

	public void CancelGameQuit() {
		gameQuitPanel.SetActive (false);
	}

	void Update() {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			if (gameQuitPanel.activeSelf) {			
				gameQuitPanel.SetActive (false);
			} else {
				gameQuitPanel.SetActive (true);
			}
		}
	}
}
