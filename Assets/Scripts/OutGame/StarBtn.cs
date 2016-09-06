using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class StarBtn : MonoBehaviour {
	public void GameStart() {
		SceneManager.LoadScene ("InGame");
	}
}
