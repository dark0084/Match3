using UnityEngine;
using System.Collections;

public class LimitTimer {
	private float currSec = 0.0f;
	private float limitSec = 60.0f;

	public bool IsTimeOver {
		get {
			return currSec >= limitSec;
		}
	}

	public float Ratio {
		get {
			return currSec / limitSec;
		}
	}

	public void SetLimitSec(float limitSec) {
		this.limitSec = limitSec;
		this.currSec = 0.0f;
	}

	public void UpdateSec(float addSec){
		if (currSec < limitSec) {
			currSec += addSec;
			if (currSec >= limitSec) {
				currSec = limitSec;
			}
		}
	}
}
