using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Combo : MonoBehaviour {
	public Progressbar comboProgressBar = null;
	public Text comboText = null;
	public Text comboCountText = null;

	private int _comboCount = 0;
	private LimitTimer _comboLimitTime = new LimitTimer ();

	// Use this for initialization
	void Start () {
		_comboLimitTime.SetLimitSec (3.0f);
		comboProgressBar.gameObject.SetActive (false);
		comboText.gameObject.SetActive (false);
		comboCountText.gameObject.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
		_comboLimitTime.UpdateSec (Time.deltaTime);

		if (_comboCount > 1) {
			comboProgressBar.SetProgress (1.0f - _comboLimitTime.Ratio);
		}
		if (_comboLimitTime.IsTimeOver) {
			_comboCount = 0;
			comboProgressBar.gameObject.SetActive (false);
			comboText.gameObject.SetActive (false);
			comboCountText.gameObject.SetActive (false);
		}
	}

	public void countCombo () {
		_comboCount += 1;
		_comboLimitTime.SetLimitSec (3.0f);

		if (_comboCount > 1) {
			comboProgressBar.gameObject.SetActive (true);
			comboText.gameObject.SetActive (true);
			comboCountText.gameObject.SetActive (true);

			comboProgressBar.SetProgress (1.0f);
			comboCountText.text = (_comboCount - 1).ToString();
		}
	}
}
