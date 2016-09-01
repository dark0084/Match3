using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Board : MonoBehaviour {
	private GameObject[,] _blocks = null;
	private GameObject _selectedBlock = null;

	public GameObject blockPrefab = null;
	public int maxCol = 8;
	public int maxRow = 8;

	public Color[] blockColors;

	private Vector2 _blockSize = new Vector2(0.0f, 0.0f);

	void Start () {
		SpriteRenderer psr = blockPrefab.GetComponent<SpriteRenderer> ();
		Vector2 spriteSize = psr.sprite.rect.size;
		float pixelToUnit = psr.sprite.pixelsPerUnit;
		_blockSize = spriteSize / pixelToUnit;

		float boardWidth = _blockSize.x * maxCol;
		float boardHeight = _blockSize.y * maxRow;

		transform.position = new Vector2((-boardWidth + _blockSize.x) * 0.5f, (boardHeight - _blockSize.y) * 0.5f);

		_blocks = new GameObject[maxCol, maxRow];

		for (int col = 0; col < maxCol; ++col) {
			for (int row = 0; row < maxRow; ++row) {
				GameObject go = Instantiate (blockPrefab);
				go.transform.parent = transform;
				go.transform.localPosition = new Vector2(_blockSize.x * col, -_blockSize.y * row);

				int kind = Random.Range(0, (int) Block.Kind.MAX);
				Block block = go.GetComponent<Block>();
				block.kind = (Block.Kind) kind;
				block.SetPos (col, row);

				SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
				sr.color = blockColors[kind];

				_blocks[col, row] = go;
			}
		}
	}
	
	void Update () {
		if (Input.GetMouseButtonDown (0)) {
			Vector2 pos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			RaycastHit2D hit = Physics2D.Raycast (pos, Vector2.zero, 0f);

			if (hit.collider != null) {
				if (_selectedBlock != null) {
					if (_checkAdjoinBlock (_selectedBlock, hit.collider.gameObject)) {
						StartCoroutine(_swapCoroutine(_selectedBlock, hit.collider.gameObject));
						_selectedBlock = null;
					} else {
						_selectedBlock = hit.collider.gameObject;
					}
				} else {
					_selectedBlock = hit.collider.gameObject;
				}
			}
		}

	}

	private bool _checkAdjoinBlock(GameObject blockObject1, GameObject blockObject2) {
		Block block1 = blockObject1.GetComponent<Block> ();
		Block block2 = blockObject2.GetComponent<Block> ();

		//Up
		if (block1.col == block2.col && block1.row - 1 == block2.row) {
			return true;
		}
		//Down
		else if (block1.col == block2.col && block1.row + 1 == block2.row) {
			return true;
		}
		//Left
		else if (block1.col - 1 == block2.col && block1.row == block2.row) {
			return true;
		}
		//Right
		else if (block1.col + 1 == block2.col && block1.row == block2.row) {
			return true;
		}

		return false;
	}

	private void _swapBlockPosition(GameObject blockObject1, GameObject blockObject2) {
		Block block1 = blockObject1.GetComponent<Block> ();
		Block block2 = blockObject2.GetComponent<Block> ();

		_blocks [block1.col, block1.row] = blockObject2;
		_blocks [block2.col, block2.row] = blockObject1;

		int tempCol = block1.col;
		int tempRow = block1.row;

		block1.SetPos (block2.col, block2.row);
		block2.SetPos (tempCol, tempRow);
	}

	private IEnumerator _swapCoroutine(GameObject blockObject1, GameObject blockObject2) {
		//Swap Ani Start
		Vector2 block1Pos = blockObject1.transform.localPosition;
		Vector2 block2Pos = blockObject2.transform.localPosition;

		float t = 0.0f;
		while (t < 1.0f) {
			t += 5.0f * Time.deltaTime;
			if (t > 1.0f) {
				t = 1.0f;
			}
			blockObject1.transform.localPosition = Vector2.Lerp (block1Pos, block2Pos, t);
			blockObject2.transform.localPosition = Vector2.Lerp (block2Pos, block1Pos, t);

			yield return null;
		}

		_swapBlockPosition (blockObject1, blockObject2);

		List<GameObject> matchedBlocks1 = _matchingBlocks (blockObject1);
		List<GameObject> matchedBlocks2 = _matchingBlocks (blockObject2);

		if (matchedBlocks1.Count == 0 && matchedBlocks2.Count == 0) {
			t = 0.0f;
			while (t < 1.0f) {
				t += 5.0f * Time.deltaTime;
				if (t > 1.0f) {
					t = 1.0f;
				}
				blockObject1.transform.localPosition = Vector2.Lerp (block2Pos, block1Pos, t);
				blockObject2.transform.localPosition = Vector2.Lerp (block1Pos, block2Pos, t);

				yield return null;
			}
			_swapBlockPosition (blockObject1, blockObject2);
			yield break;
		}

		_destoryBlocks (matchedBlocks1);
		_destoryBlocks (matchedBlocks2);
		_fallBlocks ();
	}

	private List<GameObject> _matchingBlocks(GameObject targetObject){
		Block targetBlock = targetObject.GetComponent<Block> ();

		int targetCol = targetBlock.col;
		int targetRow = targetBlock.row;
		Block.Kind targetKind = targetBlock.kind;

		int startCol = targetCol - 2 < 0 ? 0 : targetCol - 2;
		int endCol = targetCol + 3 > maxCol ? maxCol : targetCol + 3;
		int startRow = targetRow - 2 < 0 ? 0 : targetRow - 2;
		int endRow = targetRow + 3 > maxRow ? maxRow : targetRow + 3;

		List<GameObject> matchingBlocks = new List<GameObject>();
		List<GameObject> matchedBlocks = new List<GameObject> ();

		//Horizontal
		Block.Kind currKind = Block.Kind.MAX;
		for (int col = startCol; col < endCol; ++col) {
			GameObject blockObject = _blocks [col, targetRow];
			if (blockObject == null) {
				continue;
			}
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

		//Vertical
		currKind = Block.Kind.MAX;
		for (int row = startRow; row < endRow; ++row) {
			GameObject blockObject = _blocks [targetCol, row];
			if (blockObject == null) {
				continue;
			}
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


		return matchedBlocks;
	}

	private void _destoryBlocks(List<GameObject> targetBlocks) {
		foreach (GameObject blockObject in targetBlocks) {
			Block block = blockObject.GetComponent<Block> ();
			_blocks [block.col, block.row] = null;
			Destroy (blockObject);
		}
	}

	private Vector2 _getBlockPos(int col, int row) {
		return new Vector2 (_blockSize.x * col, -_blockSize.y * row);
	}

	private void _fallBlocks()
	{
		for (int col = 0; col < maxCol; ++col) {
			int blankCount = 0;
			for (int row = maxRow - 1; row >= 0; --row) {
				if (_blocks [col, row] == null) {
					++blankCount;
				} else {
					if (blankCount > 0) {
						Block aboveBlock = _blocks [col, row].GetComponent<Block> ();
						aboveBlock.Move (_getBlockPos (col, row + blankCount));
						_blocks [col, row + blankCount] = _blocks [col, row];
						aboveBlock.SetPos (col, row + blankCount);
						_blocks [col, row] = null;
					}
				}
			}

			for (int i = 0; i < blankCount; ++i) {
				GameObject go = Instantiate (blockPrefab);
				go.transform.parent = transform;
				go.transform.localPosition = _getBlockPos(col, -(i + 1));

				int kind = Random.Range(0, (int) Block.Kind.MAX);
				Block block = go.GetComponent<Block>();
				block.kind = (Block.Kind) kind;
				block.SetPos (col, blankCount - (i + 1));

				SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
				sr.color = blockColors[kind];

				_blocks[col, blankCount - (i + 1)] = go;
				block.Move (_getBlockPos(col, blankCount - (i + 1)));
			}
		}
	}
}
