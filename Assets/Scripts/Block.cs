using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour {

	public enum Kind {
		RED,
		BLUE,
		GREEN,
		MAGENTA,
		YELLOW,
		BROWN,
		AQUA,
		MAX
	}

	public Kind kind = Kind.RED;

	public int row = -1;
	public int col = -1;

	public void SetPos(int row, int col) {
		this.row = row;
		this.col = col;
	}

	public void Move(Vector2 destPos) {
		StartCoroutine (_moveCoroutine (destPos));
	}

	private IEnumerator _moveCoroutine(Vector2 destPos) {
		Vector2 startPos = transform.localPosition;
		Vector2 endPos = destPos;

		float t = 0.0f;

		while (t < 1.0f) {
			t += Time.deltaTime;
			if (t >= 1.0f) {
				t = 1.0f;
			}
			transform.localPosition = Vector2.Lerp (startPos, endPos, t);

			yield return null;
		}
	}
}
