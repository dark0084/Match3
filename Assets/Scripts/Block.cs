using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour {

	public enum Kind {
		K1,
		K2,
		K3,
		K4,
		K5,
		K6,
		K7,
		MAX
	}

	public Kind kind = Kind.K1;
}
