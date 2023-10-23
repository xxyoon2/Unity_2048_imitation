using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [SerializeField] private GameObject _blockSpawner;
    [SerializeField] private GameObject _playerController;

    private Controller _controller;
    private BlockManager _blockManager;

    private int[,] _board = new int[4, 4];

    private const int BOARD_SIZE = 4;
    private const int BLANK = 0;
    private const int NUNBER_BLOCKS_CREATE = 2;

    private MoveState _moveState = MoveState.NONE;
    private bool _gameOver = false;

    private enum MoveState
    {
        NONE,
        UP,
        DOWN,
        RIGHT,
        LEFT
    }

    private void OnEnable()
    {
        GameManager.Instance.OnResetGame.AddListener(ResetBoard);
        GameManager.Instance.OnGameOver.AddListener(GameOver);
    }

    void Start()
    {
        _blockManager = _blockSpawner.transform.GetComponent<BlockManager>();
        _controller = _playerController.transform.GetComponent<Controller>();

        Initialization();
    }

    /// <summary>
    /// 초기화
    /// </summary>
    private void Initialization()
    {
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                _board[i, j] = BLANK;
            }
        }

        int num = 2;
        for (int i = 0; i < NUNBER_BLOCKS_CREATE; i++)
        {
            SetBlock(num);
        }

        _gameOver = false;
    }

    /// <summary>
    /// 블럭을 랜덤 포지션에 세팅 
    /// </summary>
    /// <param name="num">생성할 블럭의 숫자</param>
    private void SetBlock(int num)
    {
        // 여기서 스폰 할 때 애니메이션 타입도 같이 보내면 좋을듯
        int x, y;
        (x, y) = RandomPositionSetting();
        if ((x, y) != (-1, -1))
        {
            _board[x, y] = num;
            _blockManager.Spawn(ConvertCoordinateToSquareID(x, y), num);
        }
    }

    /// <summary>
    /// 랜덤으로 빈 칸을 찾아 블록을 생성할 지점을 지정
    /// </summary>
    /// <returns>X좌표, Y좌표</returns>
    private (int, int) RandomPositionSetting()
    {
        bool checkBlanks = false;
        List<(int, int)> blanks = new List<(int, int)>();
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (_board[i, j] == BLANK)
                {
                    checkBlanks = true;
                    blanks.Add((i, j));
                }
            }
        }

        if (!checkBlanks)
        {
            return (-1, -1);
        }

        int posX, posY;
        int randomIndex = Random.Range(0, blanks.Count);
        (posX, posY) = blanks[randomIndex];

        return (posX, posY);
    }

    /// <summary>
    /// 병합할 블록 체크
    /// </summary>
    /// <returns>병합할 블록이 있다면 true, 아니라면 false를 반환</returns>
    private bool CheckMergeBlock()
    {
        bool needMerge = false;
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                int blockNum = _board[i, j];
                if (i + 1 < BOARD_SIZE)
                {
                    int rightBlockNum = _board[i + 1, j];
                    if (blockNum == rightBlockNum)
                    {
                        needMerge = true;
                    }
                }

                if (j + 1 < BOARD_SIZE)
                {
                    int downBlockNum = _board[i, j + 1];
                    if (blockNum == downBlockNum)
                    {
                        needMerge = true;
                    }
                }
            }

            if (needMerge)
            {
                break;
            }
        }

        return needMerge;
    }

    /// <summary>
    /// 입력에 대한 블록 이동
    /// </summary>
    private void MoveBlock()
    {
        bool createBlock = false;
        switch (_moveState)
        {
            case MoveState.UP:
                for (int i = 0; i != 4; i++)
                {
                    bool merge = false;

                    for (int j = 1; j < 4; j++)
                    {
                        // UP
                        if (_board[j, i] != BLANK)
                        {
                            int blockNum = _board[j, i];
                            int currentSquareID = ConvertCoordinateToSquareID(j, i);
                            if (createBlock == false)
                            {
                                createBlock = true;
                            }

                            // 여기가 진짜 문제
                            for (int k = j; k != 0; k--)
                            {
                                // 마지막 탐색 칸이 빈칸이라면 이동
                                if ((k + -1) == 0 && _board[k + -1, i] == BLANK)
                                {
                                    _board[k + -1, i] = blockNum;
                                    _board[j, i] = 0;
                                    _blockManager.BlockMoveRequest(currentSquareID, ConvertCoordinateToSquareID(k + -1, i));
                                    break;
                                }

                                // 이동시킬 자리 탐색
                                if (_board[k + -1, i] != BLANK) // 만약 블록이 있다면
                                {
                                    if (merge == false && _board[k + -1, i] == blockNum) // 그게 같은 숫자의 블록이라면
                                    {
                                        // 이동 병합
                                        int mergeNum = blockNum + blockNum;
                                        merge = true;
                                        _board[k + -1, i] = mergeNum;
                                        GameManager.Instance.AddScore(_board[k + -1, i]);
                                        _board[j, i] = 0;
                                        _blockManager.BlockMoveRequest(currentSquareID, ConvertCoordinateToSquareID(k + -1, i), merge, mergeNum);
                                    }
                                    else // 아니라면
                                    {
                                        // 이동
                                        if (k != j)
                                        {
                                            _board[k, i] = _board[j, i];
                                            _board[j, i] = 0;
                                            _blockManager.BlockMoveRequest(currentSquareID, ConvertCoordinateToSquareID(k, i));
                                        }
                                        merge = false;
                                    }

                                    break;
                                }
                            }
                        }
                    }
                }
                break;
            case MoveState.DOWN:
                for (int i = 0; i != 4; i++)
                {
                    bool merge = false;

                    for (int j = 2; j > -1; j--)
                    {
                        // UP
                        if (_board[j, i] != BLANK)
                        {
                            int blockNum = _board[j, i];
                            int currentSquareID = ConvertCoordinateToSquareID(j, i);
                            if (createBlock == false)
                            {
                                createBlock = true;
                            }

                            // 여기가 진짜 문제
                            for (int k = j; k != 3; k++)
                            {
                                // 마지막 탐색 칸이 빈칸이라면 이동
                                if ((k + 1) == 3 && _board[k + 1, i] == BLANK)
                                {
                                    _board[k + 1, i] = blockNum;
                                    _board[j, i] = 0;
                                    _blockManager.BlockMoveRequest(currentSquareID, ConvertCoordinateToSquareID(k + 1, i));
                                    break;
                                }

                                // 이동시킬 자리 탐색
                                if (_board[k + 1, i] != BLANK) // 만약 블록이 있다면
                                {
                                    if (merge == false && _board[k + 1, i] == blockNum) // 그게 같은 숫자의 블록이라면
                                    {
                                        // 이동 병합
                                        int mergeNum = blockNum + blockNum;
                                        merge = true;
                                        _board[k + 1, i] = mergeNum;
                                        GameManager.Instance.AddScore(_board[k + 1, i]);
                                        _board[j, i] = 0;
                                        _blockManager.BlockMoveRequest(currentSquareID, ConvertCoordinateToSquareID(k + 1, i), merge, mergeNum);
                                    }
                                    else // 아니라면
                                    {
                                        // 이동
                                        if (k != j)
                                        {
                                            _board[k, i] = _board[j, i];
                                            _board[j, i] = 0;
                                            _blockManager.BlockMoveRequest(currentSquareID, ConvertCoordinateToSquareID(k, i));
                                        }
                                        merge = false;
                                    }

                                    break;
                                }
                            }
                        }
                    }
                }
                break;
            case MoveState.LEFT:
                for (int i = 0; i != 4; i++)
                {
                    bool merge = false;

                    for (int j = 1; j < 4; j++)
                    {
                        // UP
                        if (_board[i, j] != BLANK)
                        {
                            int blockNum = _board[i, j];
                            int currentSquareID = ConvertCoordinateToSquareID(i, j);
                            if (createBlock == false)
                            {
                                createBlock = true;
                            }

                            // 여기가 진짜 문제
                            for (int k = j; k != 0; k--)
                            {
                                // 마지막 탐색 칸이 빈칸이라면 이동
                                if ((k + -1) == 0 && _board[i, k + -1] == BLANK)
                                {
                                    _board[i, k + -1] = blockNum;
                                    _board[i, j] = 0;
                                    _blockManager.BlockMoveRequest(currentSquareID, ConvertCoordinateToSquareID(i, k + -1));
                                    break;
                                }

                                // 이동시킬 자리 탐색
                                if (_board[i, k + -1] != BLANK) // 만약 블록이 있다면
                                {
                                    if (merge == false && _board[i, k + -1] == blockNum) // 그게 같은 숫자의 블록이라면
                                    {
                                        // 이동 병합
                                        int mergeNum = blockNum + blockNum;
                                        merge = true;
                                        _board[i, k + -1] = mergeNum;
                                        GameManager.Instance.AddScore(_board[i, k + -1]);
                                        _board[i, j] = 0;
                                        _blockManager.BlockMoveRequest(currentSquareID, ConvertCoordinateToSquareID(i, k + -1), merge, mergeNum);
                                    }
                                    else // 아니라면
                                    {
                                        // 이동
                                        if (k != j)
                                        {
                                            _board[i, k] = _board[i, j];
                                            _board[i, j] = 0;
                                            _blockManager.BlockMoveRequest(currentSquareID, ConvertCoordinateToSquareID(i, k));
                                        }
                                        merge = false;
                                    }

                                    break;
                                }
                            }
                        }
                    }
                }
                break;
            case MoveState.RIGHT:
                for (int i = 0; i != 4; i++)
                {
                    bool merge = false;

                    for (int j = 2; j > -1; j--)
                    {
                        // UP
                        if (_board[i, j] != BLANK)
                        {
                            int blockNum = _board[i, j];
                            int currentSquareID = ConvertCoordinateToSquareID(i, j);
                            if (createBlock == false)
                            {
                                createBlock = true;
                            }

                            // 여기가 진짜 문제
                            for (int k = j; k != 3; k++)
                            {
                                // 마지막 탐색 칸이 빈칸이라면 이동
                                if ((k + 1) == 3 && _board[i, k + 1] == BLANK)
                                {
                                    _board[i, k + 1] = blockNum;
                                    _board[i, j] = 0;
                                    _blockManager.BlockMoveRequest(currentSquareID, ConvertCoordinateToSquareID(i, k + 1));
                                    break;
                                }

                                // 이동시킬 자리 탐색
                                if (_board[i, k + 1] != BLANK) // 만약 블록이 있다면
                                {
                                    if (merge == false && _board[i, k + 1] == blockNum) // 그게 같은 숫자의 블록이라면
                                    {
                                        // 이동 병합
                                        int mergeNum = blockNum + blockNum;
                                        merge = true;
                                        _board[i, k + 1] = mergeNum;
                                        GameManager.Instance.AddScore(_board[i, k + 1]);
                                        _board[i, j] = 0;
                                        _blockManager.BlockMoveRequest(currentSquareID, ConvertCoordinateToSquareID(i, k + 1), merge, mergeNum);
                                    }
                                    else // 아니라면
                                    {
                                        // 이동
                                        if (k != j)
                                        {
                                            _board[i, k] = _board[i, j];
                                            _board[i, j] = 0;
                                            _blockManager.BlockMoveRequest(currentSquareID, ConvertCoordinateToSquareID(i, k));
                                        }
                                        merge = false;
                                    }

                                    break;
                                }
                            }
                        }
                    }
                }
                break;
        }

        if (createBlock)
        {
            SetBlock(2);
        }
    }

    private bool NoBlank()
    {
        return RandomPositionSetting() == (-1, -1) ? true : false;
    }

    /// <summary>
    /// 보드의 좌표를 칸ID로 바꿔줌
    /// </summary>
    /// <param name="x">보드의 X좌표</param>
    /// <param name="y">보드의 Y좌표</param>
    /// <returns>칸ID</returns>
    private int ConvertCoordinateToSquareID(int x, int y)
    {
        int squareID = 4 * x + y;
        return squareID;
    }

    void Update()
    {
        if (_gameOver)
        {
            return;
        }

        if (_controller.up)
        {
            _moveState = MoveState.UP;
            MoveBlock();
            RandomPositionSetting();
        }

        if (_controller.down)
        {
            _moveState = MoveState.DOWN;
            MoveBlock();
            RandomPositionSetting();
        }

        if (_controller.right)
        {
            _moveState = MoveState.RIGHT;
            MoveBlock();
            RandomPositionSetting();
        }

        if (_controller.left)
        {
            _moveState = MoveState.LEFT;
            MoveBlock();
            RandomPositionSetting();
        }

        if (isGameOver())
        {
            GameOver();
        }
    }

    private bool isGameOver()
    {
        if (NoBlank())
        {
            if (!CheckMergeBlock())
            {
                _gameOver = true;
                return _gameOver;
            }
            else
            {
                return _gameOver;
            }
        }
        else
        {
            return _gameOver;
        }
    }

    private void ResetBoard()
    {
        _blockManager.ResetBlock();
        Initialization();
    }

    private void GameOver()
    {
        GameManager.Instance.OnGameOver.Invoke();
    }

    private void OnDisable()
    {
        GameManager.Instance.OnResetGame.RemoveListener(ResetBoard);
        GameManager.Instance.OnGameOver.RemoveListener(GameOver);
    }
}
