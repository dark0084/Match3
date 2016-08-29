using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Board : MonoBehaviour {
	private GameObject[,] _blocks = null;

	public GameObject blockPrefab = null;
	public int maxRow = 8;
	public int maxCol = 8;

	public Color[] blockColors;

	// Use this for initialization
	void Start () {
		SpriteRenderer psr = blockPrefab.GetComponent<SpriteRenderer> ();
		Vector2 spriteSize = psr.sprite.rect.size;
		float pixelToUnit = psr.sprite.pixelsPerUnit;
		Vector2 worldSpriteSize = spriteSize / pixelToUnit;

		float boardWidth = worldSpriteSize.x * maxCol;
		float boardHeight = worldSpriteSize.y * maxRow;

		transform.position = new Vector2((-boardWidth + worldSpriteSize.x) * 0.5f , (boardHeight - worldSpriteSize.y) * 0.5f);

		_blocks = new GameObject[maxRow, maxCol];

		for (int row = 0; row < maxRow; ++row) {
			for (int col = 0; col < maxCol; ++col) {
				GameObject go = Instantiate (blockPrefab);
				go.transform.parent = transform;
				go.transform.localPosition = new Vector2(worldSpriteSize.x * col, -worldSpriteSize.y * row);

				int kind = Random.Range(0, (int) Block.Kind.MAX);
				Block block = go.GetComponent<Block>();
				block.kind = (Block.Kind) kind;

				SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
				sr.color = blockColors[kind];

				_blocks[row, col] = go;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
