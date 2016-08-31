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

						//_destoryBlocks (_matchingBlocks (selectedBlock));
						//_destoryBlocks (_matchingBlocks (hit.collider.gameObject));

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

		_blocks [block1.row, block1.col] = blockObject2;
		_blocks [block2.row, block2.col] = blockObject1;

		int tempRow = block1.row;
		int tempCol = block1.col;

		block1.SetPos (block2.row, block2.col);
		block2.SetPos (tempRow, tempCol);

		Vector2 block1Pos = blockObject1.transform.localPosition;
		Vector2 block2Pos = blockObject2.transform.localPosition;
		block1.Move (block2Pos);
		block2.Move (block1Pos);
		//Vector3 tempPos = blockObject1.transform.localPosition;
		//blockObject1.transform.localPosition = blockObject2.transform.localPosition;
		//blockObject2.transform.localPosition = tempPos;
	}

	private List<GameObject> _matchingBlocks(GameObject targetObject){
		Block targetBlock = targetObject.GetComponent<Block> ();

		int targetRow = targetBlock.row;
		int targetCol = targetBlock.col;
		Block.Kind targetKind = targetBlock.kind;

		int startRow = targetRow - 2 < 0 ? 0 : targetRow - 2;
		int endRow = targetRow + 3 > maxRow ? maxRow : targetRow + 3;
		int startCol = targetCol - 2 < 0 ? 0 : targetCol - 2;
		int endCol = targetCol + 3 > maxCol ? maxCol : targetCol + 3;

		List<GameObject> matchingBlocks = new List<GameObject>();
		List<GameObject> matchedBlocks = new List<GameObject> ();

		//Vertical
		Block.Kind currKind = Block.Kind.MAX;
		for (int row = startRow; row < endRow; ++row) {
			GameObject blockObject = _blocks [row, targetCol];
			Block block = blockObject.GetComponent<Block> ();
			currKind = block.kind;

			if (currKind == targetKind) {
				matchingBlocks.Add (blockObject);
			} else {
				// case : OOOXX, OOOOX, XOOOX
				if (matchingBlocks.Count >= 3) {
					break;
				}
				matchingBlocks.Clear ();
			}
		}
		// no case : XXXOO, XXXXO
		if (matchingBlocks.Count >= 3) {
			matchedBlocks.AddRange (matchingBlocks);
		}
		matchingBlocks.Clear ();

		//Horizontal
		currKind = Block.Kind.MAX;
		for (int col = startCol; col < endCol; ++col) {
			GameObject blockObject = _blocks [targetRow, col];
			Block block = blockObject.GetComponent<Block> ();
			currKind = block.kind;

			if (currKind == targetKind) {
				matchingBlocks.Add (blockObject);
			} else {
				if (matchingBlocks.Count >= 3) {
					break;
				}
				matchingBlocks.Clear ();
			}
		}

		if (matchingBlocks.Count >= 3) {
			matchedBlocks.AddRange (matchingBlocks);
		}
		matchingBlocks.Clear ();

		return matchedBlocks;
	}

	private void _destoryBlocks(List<GameObject> targetBlocks) {
		foreach (GameObject blockObject in targetBlocks) {
			Block block = blockObject.GetComponent<Block> ();
			_blocks [block.row, block.col] = null;
			Destroy (blockObject);
		}
	}
}
