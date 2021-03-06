﻿using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour {

	public enum Kind {
		BLUE,
		GREEN,
		ORANGE,
		PURPLE,
		RED,
		TEAL,
		YELLOW,
		MAX
	}

    public enum State {
        NORMAL,
        SWAP,
        DISAPPEAR,
        FALL
    }

	public Kind kind = Kind.BLUE;
    public State state = State.NORMAL;

	public int col = -1;
	public int row = -1;
	public Board board = null;

	public void SetPos(int col, int row) {
		this.col = col;
		this.row = row;
	}

	public void Fall(Vector2 destPos) {
		StartCoroutine (_fallCoroutine (destPos));
	}

	private IEnumerator _fallCoroutine(Vector2 destPos) {
		Vector2 startPos = transform.localPosition;
		Vector2 endPos = destPos;

        state = State.FALL;

		float t = 0.0f;

		while (t < 1.0f) {
			t += 3.0f * Time.deltaTime;
			if (t >= 1.0f) {
				t = 1.0f;
			}
			transform.localPosition = Vector2.Lerp (startPos, endPos, t);

			yield return null;
		}

        state = State.NORMAL;
	}

    public void Disappear() {
        StartCoroutine(_disappearCoroutine());
    }

    private IEnumerator _disappearCoroutine() {
        Vector3 startScale = transform.localScale;
        Vector3 endScale = new Vector3();

        state = State.DISAPPEAR;

        float t = 0.0f;
        while (t < 1.0f) {
            t += 2.0f * Time.deltaTime;
            if (t >= 1.0f) {
                t = 1.0f;
            }
            transform.localScale = Vector3.Lerp(startScale, endScale, t);

            yield return null;
        }

        state = State.NORMAL;
    }
}
