using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class Board : MonoBehaviour {
	public enum State {
		Playing,
		Pause,
		TimeOver
	}

	private Block[,] _blocks = null;
	private Block _selectedBlock = null;
	private List<Block> _disappearingBlocks = new List<Block> ();
	private List<Block> _fallingBlocks = new List<Block> ();
	private State _state = State.Playing;

	private LimitTimer _limitTimer = null;

	private int _score = 0;

	public Progressbar progressbarObject = null;
	public Text scoreText = null;
	public Text phraseText = null;
	public Button retryBtn = null;
	public Button returnBtn = null;
	public Combo combo = null;

	public int maxCol = 8;
	public int maxRow = 8;

	public GameObject[] fruitPrefabs;

	private Vector2 _blockSize = new Vector2(0.0f, 0.0f);

	void Start () {
		SpriteRenderer psr = fruitPrefabs[0].GetComponent<SpriteRenderer> ();
		Vector2 spriteSize = psr.sprite.rect.size;
		float pixelToUnit = psr.sprite.pixelsPerUnit;
		_blockSize = spriteSize / pixelToUnit;

		float boardWidth = _blockSize.x * maxCol;
		float boardHeight = _blockSize.y * maxRow;

		transform.position = new Vector2((-boardWidth + _blockSize.x) * 0.5f, (boardHeight - _blockSize.y) * 0.5f);
		_blocks = new Block[maxCol, maxRow];
		_createBlocks ();

		_limitTimer = new LimitTimer ();
		_limitTimer.SetLimitSec (60.0f); //60Seconds

		scoreText.text = "점수 : " + _score;
	}
	
	void Update () {
		if (_state == State.Playing) {
			if (Input.GetMouseButtonDown (0)) {
				Vector2 pos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
				RaycastHit2D hit = Physics2D.Raycast (pos, Vector2.zero, 0f);

				if (hit.collider != null) {
					GameObject hitObject = hit.collider.gameObject;
					Block hitBlock = hitObject.GetComponent<Block> ();
					if (hitBlock.state == Block.State.NORMAL) {
						_selectedBlock = hitBlock;
					}
				}
			}
			if (Input.GetMouseButton (0)) {
				Vector2 pos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
				RaycastHit2D hit = Physics2D.Raycast (pos, Vector2.zero, 0f);

				if (hit.collider != null) {
					GameObject hitObject = hit.collider.gameObject;
					Block hitBlock = hitObject.GetComponent<Block> ();

					if (hitBlock.state == Block.State.NORMAL) {
						if (_selectedBlock != null && _selectedBlock != hitBlock) {
							if (_checkAdjoinBlock (_selectedBlock, hitBlock)) {
								StartCoroutine (_swapCoroutine (_selectedBlock, hitBlock));
								_selectedBlock = null;
							}
						}
					}
				}
			}

			_limitTimer.UpdateSec (Time.deltaTime);
			combo.UpdateSec (Time.deltaTime);

			if (progressbarObject != null) {
				progressbarObject.SetProgress (1.0f - _limitTimer.Ratio);
			}
			if (_limitTimer.IsTimeOver) {
				_state = State.TimeOver;
				phraseText.text = "타임 오버";
				phraseText.gameObject.SetActive (true);
				retryBtn.gameObject.SetActive (true);
				returnBtn.gameObject.SetActive (true);
				combo.hide ();
			}

			if (Input.GetKeyDown (KeyCode.Escape)) {
				phraseText.text = "일시 정지";
				phraseText.gameObject.SetActive (true);
				retryBtn.gameObject.SetActive (true);
				returnBtn.gameObject.SetActive (true);
				_state = State.Pause;
			}
		} else if (_state == State.Pause) {
			if (Input.GetKeyDown (KeyCode.Escape)) {
				phraseText.text = "타임 오버";
				phraseText.gameObject.SetActive (false);
				retryBtn.gameObject.SetActive (false);
				returnBtn.gameObject.SetActive (false);
				_state = State.Playing;
			}
		}

		_checkDisappearingBlocks ();
		_checkFallingBlocks ();
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

		foreach (Block block in _fallingBlocks) {
			List<Block> matchedBlocks = _matchingBlock (block);

			if (matchedBlocks.Count > 0) {
				_calculateScore (matchedBlocks);
				_disappearBlocks (matchedBlocks);
				combo.countCombo ();
			}
		}

		_fallingBlocks.Clear ();
	}

	private Block _createBlock(Block fruitPrefabBlock, Vector2 pos, int col, int row) {
		GameObject go = Instantiate (fruitPrefabBlock.gameObject);
		go.transform.parent = transform;
		go.transform.localPosition = pos;

		Block block = go.GetComponent<Block>();
		block.SetPos (col, row);
		block.board = this;

		return block;
	}

	private Block _getRandomBlock() {
		int index = Random.Range (0, fruitPrefabs.Length);
		GameObject randomPrefab = fruitPrefabs [index];
		return randomPrefab.GetComponent<Block> ();
	}

	private Vector2 _getBlockPos(int col, int row) {
		return new Vector2 (_blockSize.x * col, -_blockSize.y * row);
	}

	private bool _checkAdjoinBlock(Block block1, Block block2) {
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

	private void _swapBlockPosition(Block block1, Block block2) {
		_blocks [block1.col, block1.row] = block2;
		_blocks [block2.col, block2.row] = block1;

		int tempCol = block1.col;
		int tempRow = block1.row;

		block1.SetPos (block2.col, block2.row);
		block2.SetPos (tempCol, tempRow);
	}

	private IEnumerator _swapCoroutine(Block block1, Block block2) {
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
			block1.transform.localPosition = Vector2.Lerp (block1Pos, block2Pos, t);
			block2.transform.localPosition = Vector2.Lerp (block2Pos, block1Pos, t);

			yield return null;
		}

        block1.state = Block.State.NORMAL;
        block2.state = Block.State.NORMAL;

        _swapBlockPosition (block1, block2);

		List<Block> matchedBlocks1 = _matchingBlock (block1);
		List<Block> matchedBlocks2 = _matchingBlock (block2);

		if (matchedBlocks1.Count == 0 && matchedBlocks2.Count == 0) {
            block1.state = Block.State.SWAP;
            block2.state = Block.State.SWAP;

            t = 0.0f;
			while (t < 1.0f) {
				t += 5.0f * Time.deltaTime;
				if (t > 1.0f) {
					t = 1.0f;
				}
				block1.transform.localPosition = Vector2.Lerp (block2Pos, block1Pos, t);
				block2.transform.localPosition = Vector2.Lerp (block1Pos, block2Pos, t);

				yield return null;
			}
			_swapBlockPosition (block1, block2);

            block1.state = Block.State.NORMAL;
            block2.state = Block.State.NORMAL;

            yield break;
		}

		if (matchedBlocks1.Count > 0) {
			_calculateScore (matchedBlocks1);
			_disappearBlocks (matchedBlocks1);
			combo.countCombo ();
		}

		if (matchedBlocks2.Count > 0) {
			_calculateScore (matchedBlocks2);
			_disappearBlocks (matchedBlocks2);
			combo.countCombo ();
		}
	}

	private void _calculateScore(List<Block> matchedBlock) {
		int score = matchedBlock.Count * 10;

		_score += score;
		scoreText.text = "점수 : " + _score;
	}

	private List<Block> _matchingBlock(Block targetBlock){
		int targetCol = targetBlock.col;
		int targetRow = targetBlock.row;
		Block.Kind targetKind = targetBlock.kind;

		int startCol = targetCol - 2 < 0 ? 0 : targetCol - 2;
		int endCol = targetCol + 3 > maxCol ? maxCol : targetCol + 3;
		int startRow = targetRow - 2 < 0 ? 0 : targetRow - 2;
		int endRow = targetRow + 3 > maxRow ? maxRow : targetRow + 3;

		List<Block> matchingBlocks = new List<Block> ();
		List<Block> matchedBlocks = new List<Block> ();

		//Horizontal
		Block.Kind currKind = Block.Kind.MAX;
		for (int col = startCol; col < endCol; ++col) {
			Block block = _blocks [col, targetRow];
			if (block == null) {
				continue;
			}
            if (block.state != Block.State.NORMAL) {
                continue;
            }
            currKind = block.kind;

			if (currKind == targetKind) {
				matchingBlocks.Add (block);
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
			Block block = _blocks [targetCol, row];
			if (block == null) {
				continue;
			}
			if (block.state != Block.State.NORMAL) {
                continue;
            }
            currKind = block.kind;

			if (currKind == targetKind) {
				matchingBlocks.Add (block);
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

		matchedBlocks = new List<Block> (new HashSet<Block> (matchedBlocks));
		return matchedBlocks;
	}

    private void _disappearBlocks(List<Block> targetBlocks) {
        foreach (Block block in targetBlocks) {
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
						Block aboveBlock = _blocks[col, row];
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
				Block block = _createBlock(_getRandomBlock(), _getBlockPos(col, -(i + 1)), col, fallRow);
				_blocks[col, fallRow] = block;

				block.Fall(_getBlockPos(col, fallRow));
				_fallingBlocks.Add (block);
			}
		}
	}

	private void _createBlocks() {
		for (int col = 0; col < maxCol; ++col) {
			for (int row = maxRow - 1; row >= 0; --row) {
				Block block = _getRandomBlock ();
				block.SetPos (col, row);
				_blocks [col, row] = block;

				while (_matchingBlock (block).Count > 0) {
					block = _getRandomBlock ();
					block.SetPos (col, row);
					_blocks [col, row] = block;
				}

				block = _createBlock (block, _getBlockPos (col, -(maxRow - row)), col, row);
				_blocks [col, row] = block;
			}
		}
		for (int col = 0; col < maxCol; ++col) {
			for (int row = maxRow - 1; row >= 0; --row) {
				Block block = _blocks [col, row];
				block.Fall(_getBlockPos(col, row));
				_fallingBlocks.Add (block);
			}
		}
	}

	private void _destoryBlocks() {
		for (int col = 0; col < maxCol; ++col) {
			for (int row = 0; row < maxRow; ++row) {
				Block block = _blocks [col, row];
				_blocks [col, row] = null;
				Destroy (block.gameObject);
			}
		}
	}

	public void returnTitle() {
		SceneManager.LoadScene ("OutGame");
	}

	public void retryGame() {
		_destoryBlocks ();
		_createBlocks ();
		_score = 0;

		scoreText.text = "점수 : " + _score;

		_limitTimer.SetLimitSec (60.0f);

		_state = State.Playing;

		phraseText.gameObject.SetActive (false);
		retryBtn.gameObject.SetActive (false);
		returnBtn.gameObject.SetActive (false);
		combo.hide ();
	}
}
