using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Board : MonoBehaviour {
	private GameObject[,] _blocks = null;
	private GameObject selectedBlock = null;

	public GameObject blockPrefab = null;
	public int maxRow = 8;
	public int maxCol = 8;

	public Color[] blockColors;

	void Start () {
		SpriteRenderer psr = blockPrefab.GetComponent<SpriteRenderer> ();
		Vector2 spriteSize = psr.sprite.rect.size;
		float pixelToUnit = psr.sprite.pixelsPerUnit;
		Vector2 worldSpriteSize = spriteSize / pixelToUnit;

		float boardWidth = worldSpriteSize.x * maxCol;
		float boardHeight = worldSpriteSize.y * maxRow;

		transform.position = new Vector2((-boardWidth + worldSpriteSize.x) * 0.5f, (boardHeight - worldSpriteSize.y) * 0.5f);

		_blocks = new GameObject[maxRow, maxCol];

		for (int row = 0; row < maxRow; ++row) {
			for (int col = 0; col < maxCol; ++col) {
				GameObject go = Instantiate (blockPrefab);
				go.transform.parent = transform;
				go.transform.localPosition = new Vector2(worldSpriteSize.x * col, -worldSpriteSize.y * row);

				int kind = Random.Range(0, (int) Block.Kind.MAX);
				Block block = go.GetComponent<Block>();
				block.kind = (Block.Kind) kind;
				block.SetPos (row, col);

				SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
				sr.color = blockColors[kind];

				_blocks[row, col] = go;
			}
		}
	}
	
	void Update () {
		if (Input.GetMouseButtonDown (0)) {
			Vector2 pos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			RaycastHit2D hit = Physics2D.Raycast (pos, Vector2.zero, 0f);

			if (hit.collider != null) {
				if (selectedBlock != null) {
					if (_checkAdjoinBlock (selectedBlock, hit.collider.gameObject)) {
						_swapPositionBlock (selectedBlock, hit.collider.gameObject);
						selectedBlock = null;
					} else {
						selectedBlock = hit.collider.gameObject;
					}
				} else {
					selectedBlock = hit.collider.gameObject;
				}
			}
		}
	}

	private bool _checkAdjoinBlock(GameObject blockObject1, GameObject blockObject2) {
		Block block1 = blockObject1.GetComponent<Block> ();
		Block block2 = blockObject2.GetComponent<Block> ();

		//Up
		if (block1.row - 1 == block2.row && block1.col == block2.col) {
			return true;
		}
		//Down
		else if (block1.row + 1 == block2.row && block1.col == block2.col) {
			return true;
		}
		//Left
		else if (block1.row == block2.row && block1.col - 1 == block2.col) {
			return true;
		}
		//Right
		else if (block1.row == block2.row && block1.col + 1 == block2.col) {
			return true;
		}

		return false;
	}

	private void _swapPositionBlock(GameObject blockObject1, GameObject blockObject2) {
		Block block1 = blockObject1.GetComponent<Block> ();
		Block block2 = blockObject2.GetComponent<Block> ();

		int tempRow = block1.row;
		int tempCol = block1.col;

		block1.SetPos (block2.row, block2.col);
		block2.SetPos (tempRow, tempCol);

		_blocks [block1.row, block1.col] = blockObject2;
		_blocks [block2.row, block2.col] = blockObject1;

		Vector3 tempPos = blockObject1.transform.localPosition;
		blockObject1.transform.localPosition = blockObject2.transform.localPosition;
		blockObject2.transform.localPosition = tempPos;
	}

	private void _matchingBlock(){
		List<GameObject> matchingBlocks = new List<GameObject> ();
		List<GameObject> removeBlocks = new List<GameObject> ();

		Block.Kind prevKind = Block.Kind.MAX;
		Block.Kind currKind = Block.Kind.MAX;

		for (int row = 0; row < maxRow; ++row) {
			prevKind = Block.Kind.MAX;
			currKind = Block.Kind.MAX;

			if (matchingBlocks.Count >= 3) {
				removeBlocks.AddRange (matchingBlocks);
			}
			matchingBlocks.Clear ();

			for (int col = 0; col < maxCol; ++col) {
				GameObject blockObject = _blocks [row, col];
				Block block = GetComponent<Block> ();
				currKind = block.kind;

				if (prevKind != currKind) {
					if (matchingBlocks.Count >= 3) {
						removeBlocks.AddRange (matchingBlocks);
					}
					matchingBlocks.Clear ();
				}
				matchingBlocks.Add (blockObject);
				prevKind = currKind;
			}
		}

		if (matchingBlocks.Count >= 3) {
			removeBlocks.AddRange (matchingBlocks);
		}
		matchingBlocks.Clear ();

		for (int col = 0; col < maxCol; ++col) {
			prevKind = Block.Kind.MAX;
			currKind = Block.Kind.MAX;

			if (matchingBlocks.Count >= 3) {
				foreach (GameObject matchingBlock in matchingBlocks) {
					if (!removeBlocks.Contains (matchingBlock)) {
						removeBlocks.Add (matchingBlock);
					}
				}
			}
			matchingBlocks.Clear ();

			for (int row = 0; row < maxRow; ++row) {
				GameObject blockObject = _blocks [row, col];
				Block block = GetComponent<Block> ();
				currKind = block.kind;

				if (prevKind != currKind) {
					if (matchingBlocks.Count >= 3) {
						foreach (GameObject matchingBlock in matchingBlocks) {
							if (!removeBlocks.Contains (matchingBlock)) {
								removeBlocks.Add (matchingBlock);
							}
						}
					}
					matchingBlocks.Clear ();
				}
				matchingBlocks.Add (blockObject);
				prevKind = currKind;
			}
		}
	}
}
