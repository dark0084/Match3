using UnityEngine;
using UnityEngine.UI;

using System.Collections;

public class Progressbar : MonoBehaviour {
	public RectTransform progressFill = null;

	[Range(0.0f, 1.0f)]
	public float progressValue = 1.0f;
	public float maxFillWidth = 300.0f;

	public void SetProgress(float value) {
		float fillWidth = maxFillWidth * value;
		Vector2 fillSize = progressFill.sizeDelta;
		progressFill.sizeDelta = new Vector2 (fillWidth, fillSize.y);
	}
}
