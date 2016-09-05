using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Board : MonoBehaviour {
	private GameObject[,] _blocks = null;
	private GameObject _selectedBlock = null;
	private List<Block> _disappearingBlocks = new List<Block> ();
	private List<Block> _fallingBlocks = new List<Block> ();

	private LimitTimer _limitTimer = null;

	public GameObject blockPrefab = null;
	public Progressbar progressbarObject = null;
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
				Block block = _createBlock (_getBlockPos (col, row), col, row);
				_blocks [col, row] = block.gameObject;

				while (_matchingBlock (block.gameObject).Count > 0) {
					_setRandomBlockColor (block);
				}
			}
		}
		_limitTimer = new LimitTimer ();
		_limitTimer.SetLimitSec (60.0f); //60Seconds
	}
	
	void Update () {
		if (Input.GetMouseButton (0)) {
			Vector2 pos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			RaycastHit2D hit = Physics2D.Raycast (pos, Vector2.zero, 0f);

			if (hit.collider != null) {
                GameObject hitObject = hit.collider.gameObject;
                Block hitBlock = hitObject.GetComponent<Block>();

                if(hitBlock.state == Block.State.NORMAL) {
                    if (_selectedBlock != null && _selectedBlock != hitObject) {
                        if (_checkAdjoinBlock(_selectedBlock, hitObject)) {
                            StartCoroutine(_swapCoroutine(_selectedBlock, hitObject));
                            _selectedBlock = null;
                        }
                        else {
                            _selectedBlock = hitObject;
                        }
                    }
                    else {
                        _selectedBlock = hitObject;
                    }
                }
            }
		}

		_checkDisappearingBlocks ();
		_checkFallingBlocks ();

		_limitTimer.UpdateSec (Time.deltaTime);
		if (progressbarObject != null) {
			progressbarObject.SetProgress (1.0f - _limitTimer.Ratio);
		}
	}

	private void _checkDisappearingBlocks() {
		if (_disappearingBlocks.Count == 0) {
			return;
		}

		foreach (Block block in _disappearingBlocks) {
			if (block.state != Block.State.NORMAL) {
				return;
			}
		}
		foreach (Block block in _disappearingBlocks) {
			_blocks[block.col, block.row] = null;
			Destroy(block.gameObject);
		}
		_fallBlocks ();
		_disappearingBlocks.Clear ();
	}

	private void _checkFallingBlocks() {
		if (_fallingBlocks.Count == 0) {
			return;
		}

		foreach (Block block in _fallingBlocks) {
			if (block.state != Block.State.NORMAL) {
				return;
			}
		}

		List<GameObject> matchedBlocks = new List<GameObject> ();

		foreach (Block block in _fallingBlocks) {
			matchedBlocks.AddRange (_matchingBlock (block.gameObject));
		}

		_disappearBlocks (matchedBlocks);
		_fallingBlocks.Clear ();
	}

	private Block _createBlock(Vector2 pos, int col, int row) {
		GameObject go = Instantiate (blockPrefab);
		go.transform.parent = transform;
		go.transform.localPosition = pos;

		Block block = go.GetComponent<Block>();
		block.SetPos (col, row);
		block.board = this;

		_setRandomBlockColor (block);

		return block;
	}

	private void _setRandomBlockColor(Block block) {
		int kind = Random.Range(0, (int) Block.Kind.MAX);
		block.kind = (Block.Kind) kind;

		SpriteRenderer sr = block.gameObject.GetComponent<SpriteRenderer>();
		sr.color = blockColors[kind];
	}

	private Vector2 _getBlockPos(int col, int row) {
		return new Vector2 (_blockSize.x * col, -_blockSize.y * row);
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
		Block block1 = blockObject1.GetComponent<Block> ();
		Block block2 = blockObject2.GetComponent<Block> ();

		//Swap Ani Start
		Vector2 block1Pos = _getBlockPos(block1.col, block1.row);
		Vector2 block2Pos = _getBlockPos(block2.col, block2.row);

		block1.state = Block.State.SWAP;
		block2.state = Block.State.SWAP;

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

        block1.state = Block.State.NORMAL;
        block2.state = Block.State.NORMAL;

        _swapBlockPosition (blockObject1, blockObject2);

		List<GameObject> matchedBlocks1 = _matchingBlock (blockObject1);
		List<GameObject> matchedBlocks2 = _matchingBlock (blockObject2);

		if (matchedBlocks1.Count == 0 && matchedBlocks2.Count == 0) {
            block1.state = Block.State.SWAP;
            block2.state = Block.State.SWAP;

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

            block1.state = Block.State.NORMAL;
            block2.state = Block.State.NORMAL;

            yield break;
		}

        _disappearBlocks (matchedBlocks1);
        _disappearBlocks (matchedBlocks2);
	}

	private List<GameObject> _matchingBlock(GameObject targetObject){
		Block targetBlock = targetObject.GetComponent<Block> ();

		int targetCol = targetBlock.col;
		int targetRow = targetBlock.row;
		Block.Kind targetKind = targetBlock.kind;

		int startCol = targetCol - 2 < 0 ? 0 : targetCol - 2;
		int endCol = targetCol + 3 > maxCol ? maxCol : targetCol + 3;
		int startRow = targetRow - 2 < 0 ? 0 : targetRow - 2;
		int endRow = targetRow + 3 > maxRow ? maxRow : targetRow + 3;

		List<GameObject> matchingBlocks = new List<GameObject> ();
		List<GameObject> matchedBlocks = new List<GameObject> ();

		//Horizontal
		Block.Kind currKind = Block.Kind.MAX;
		for (int col = startCol; col < endCol; ++col) {
			GameObject blockObject = _blocks [col, targetRow];
			if (blockObject == null) {
				continue;
			}
			Block block = blockObject.GetComponent<Block> ();
            if (block.state != Block.State.NORMAL) {
                continue;
            }
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
            if (block.state != Block.State.NORMAL) {
                continue;
            }
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

    private void _disappearBlocks(List<GameObject> targetBlocks) {
        foreach (GameObject blockObject in targetBlocks) {
            Block block = blockObject.GetComponent<Block>();
			_disappearingBlocks.Add (block);
            block.Disappear();
        }
    }

    private void _fallBlocks() {
		for (int col = 0; col < maxCol; ++col)
		{
			int blankCount = 0;
			for (int row = maxRow - 1; row >= 0; --row) {
				if (_blocks[col, row] == null) {
					++blankCount;
				}
				else {
					if (blankCount > 0) {
						Block aboveBlock = _blocks[col, row].GetComponent<Block>();
						_blocks[col, row + blankCount] = _blocks[col, row];
						aboveBlock.SetPos(col, row + blankCount);
						_blocks[col, row] = null;

						aboveBlock.Fall(_getBlockPos(col, row + blankCount));
						_fallingBlocks.Add (aboveBlock);
					}
				}
			}

			for (int i = 0; i < blankCount; ++i) {
				int fallRow = blankCount - (i + 1);
				Block block = _createBlock(_getBlockPos(col, -(i + 1)), col, fallRow);
				_blocks[col, blankCount - (i + 1)] = block.gameObject;

				block.Fall(_getBlockPos(col, blankCount - (i + 1)));
				_fallingBlocks.Add (block);
			}
		}
	}
}
