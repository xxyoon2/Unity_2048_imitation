using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 블럭 이동이 없으면 블럭 생성도 하지 않음

public class BoardManager : MonoBehaviour
{
    [SerializeField] private GameObject _blockSpawner;
    [SerializeField] private GameObject _playerController;

    private Controller _controller;
    private BlockManager _blockManager;

    private int[,] _board;

    private enum BlockState
    {
        NONE,
        UP,
        DOWN,
        RIGHT,
        LEFT
    }

    // 보드 생성
    void Start()
    {
        _blockManager = _blockSpawner.transform.GetComponent<BlockManager>();
        _controller = _playerController.transform.GetComponent<Controller>();

        _board = new int[4, 4];
        Initialization();
    }

    private BlockState _blockState = BlockState.NONE;

    private int SwitchPlusMinus(int plusMinus)
    {
        return plusMinus == 1 ? -1 : 1;
    }

    //Vertical
    private void MoveBlock()
    {
        int FirstSearchPoint = BLANK;
        int SecondSearchPoint = BLANK;
        int plusMinus = BLANK;
        switch (_blockState)
        {
            case BlockState.UP: case BlockState.LEFT:
                FirstSearchPoint = 1;
                SecondSearchPoint = 4;
                plusMinus = 1;
                break;
            case BlockState.DOWN: case BlockState.RIGHT:
                FirstSearchPoint = 2;
                SecondSearchPoint = -1;
                plusMinus = -1;
                break;
            default:
                break;
        }

        bool createBlock = false;
        for (int i = FirstSearchPoint; i != SecondSearchPoint; i += plusMinus)
        {
            for (int j = 0; j < 4; j++)
            {
                
                switch(_blockState)
                {
                    case BlockState.UP: case BlockState.DOWN:
                        // Vertical Search
                        if (_board[i, j] != BLANK)
                        {
                            int blockNum = _board[i, j];
                            if (createBlock == false)
                            {
                                createBlock = true;
                            }

                            plusMinus = SwitchPlusMinus(plusMinus);

                            // 여기가 진짜 문제
                            for (int k = i; k != FirstSearchPoint + plusMinus; k += plusMinus)
                            {
                                // 이동시킬 자리 탐색
                                if (_board[k + plusMinus, j] != BLANK)
                                {
                                    Debug.Log("왜");
                                    if (_board[k + plusMinus, j] == blockNum)
                                    {
                                        // 이동 병합
                                        _board[k + plusMinus, j] += blockNum;
                                        _board[i, j] = 0;
                                        _blockManager.BlockMoveRequest(ConvertCoordinateToSquareID(i, j), ConvertCoordinateToSquareID(k + plusMinus, j));
                                    }
                                    else
                                    {
                                        // 이동
                                        if (k != i)
                                        {
                                            _board[k, j] = _board[i, j];
                                            _blockManager.BlockMoveRequest(ConvertCoordinateToSquareID(i, j), ConvertCoordinateToSquareID(k, j));
                                        }
                                    }
                                }
                                else if ((k + plusMinus) == FirstSearchPoint + plusMinus && _board[k + plusMinus, j] == BLANK)
                                {
                                    _board[k + plusMinus, j] = blockNum;
                                    _board[i, j] = 0;
                                    _blockManager.BlockMoveRequest(ConvertCoordinateToSquareID(i, j), ConvertCoordinateToSquareID(k + plusMinus, j));
                                }
                            }

                            plusMinus = SwitchPlusMinus(plusMinus);
                        }
                        break;

                    case BlockState.LEFT: case BlockState.RIGHT:
                        // Horizontal Search
                        if (_board[j, i] != BLANK)
                        {
                            int blockNum = _board[j, i];
                            if (createBlock == false)
                            {
                                createBlock = true;
                            }

                            plusMinus = SwitchPlusMinus(plusMinus);

                            // 여기가 진짜 문제
                            for (int k = i; k != FirstSearchPoint + plusMinus; k += plusMinus)
                            {
                                // 이동시킬 자리 탐색
                                if (_board[j, k + plusMinus] != BLANK)
                                {
                                    Debug.Log("왜");
                                    if (_board[j, k + plusMinus] == blockNum)
                                    {
                                        // 이동 병합
                                        _board[j, k + plusMinus] += blockNum;
                                        _board[j, i] = 0;
                                        _blockManager.BlockMoveRequest(ConvertCoordinateToSquareID(j, i), ConvertCoordinateToSquareID(j, k + plusMinus));
                                    }
                                    else
                                    {
                                        // 이동
                                        if (k != i)
                                        {
                                            _board[k, j] = _board[i, j];
                                            _blockManager.BlockMoveRequest(ConvertCoordinateToSquareID(j, i), ConvertCoordinateToSquareID(j, k));
                                        }
                                    }
                                }
                                else if ((k + plusMinus) == FirstSearchPoint + plusMinus && _board[j, k + plusMinus] == BLANK)
                                {
                                    _board[j, k + plusMinus] = blockNum;
                                    _board[j, i] = 0;
                                    _blockManager.BlockMoveRequest(ConvertCoordinateToSquareID(j, i), ConvertCoordinateToSquareID(j, k + plusMinus));
                                }
                            }

                            plusMinus = SwitchPlusMinus(plusMinus);
                        }
                        break;
                }
            }
        }
    }

    private const int BLANK = 0;
    private const int NUNBER_BLOCKS_CREATE = 2;
    /// <summary>
    /// 초기화
    /// </summary>
    private void Initialization()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                _board[i, j] = BLANK;
            }
        }

        for (int i = 0; i < NUNBER_BLOCKS_CREATE; i++)
        {
            int x, y;
            (x, y) = RandomPositionSetting();

            if (x == -1)
            {
                // 게임 오버
                break;
            }

            _board[x, y] = 2;
            Debug.Log(ConvertCoordinateToSquareID(x, y));
            _blockManager.Spawn(ConvertCoordinateToSquareID(x, y));
        }
    }

    /// <summary>
    /// 랜덤으로 빈 칸을 찾아 블록을 생성할 지점을 지정
    /// </summary>
    /// <returns>X좌표, Y좌표</returns>
    private (int, int) RandomPositionSetting()
    {
        bool checkBlanks = false;
        int minInclusive = 10;
        int maxExclusive = -10;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (_board[i, j] == BLANK)
                {
                    checkBlanks = true;
                    if (minInclusive > i)
                    {
                        minInclusive = i;
                    }
                    if (maxExclusive < i + 1)
                    {
                        maxExclusive = i + 1;
                    }
                }
            }
        }

        if (!checkBlanks)
        {
            return (-1, -1);
        }

        int posX = minInclusive;
        int countBlanks = 0;
        List<int> blanks = new List<int>();
        if (minInclusive + 1 != maxExclusive)
        {
            posX = Random.Range(minInclusive, maxExclusive);
        }

        for (int i = 0; i < 4; i++)
        {
            if (_board[posX, i] == BLANK)
            {
                ++countBlanks;
                blanks.Add(i);
            }
        }

        int posY = 0;
        if (countBlanks == 1)
        {
            posY = blanks[0];
        }
        else
        {
            posY = blanks[Random.Range(0, countBlanks)];
        }

        return (posX, posY);
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

    private bool _processing = false;
    /// <summary>
    /// 입력을 받기 위해 블록이 이동중인지 체크해야 함
    /// </summary>
    /// <param name="processing">블록이 아직 이동중이라면 true, 아니라면 false</param>
    private void ProcessingStatusCheck(bool processing)
    {
        _processing = processing;
    }

    void Update()
    {
        //if (_processing)
        //{
        //    return;
        //}

        if (_controller.up)
        {
            Debug.Log("위");
            _blockState = BlockState.UP;
            MoveBlock();
            RandomPositionSetting();
        }

        if (_controller.down)
        {
            Debug.Log("아래");
            _blockState = BlockState.DOWN;
            MoveBlock();
            RandomPositionSetting();
        }

        if (_controller.right)
        {
            Debug.Log("오른쪽");
            _blockState = BlockState.RIGHT;
            MoveBlock();
            RandomPositionSetting();
        }

        if (_controller.left)
        {
            Debug.Log("왼쪽");
            _blockState = BlockState.LEFT;
            MoveBlock();
            RandomPositionSetting();
        }
    }
}
