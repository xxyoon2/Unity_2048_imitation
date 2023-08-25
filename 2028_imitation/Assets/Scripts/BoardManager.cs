using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 블럭 이동이 없으면 블럭 생성도 하지 않음

public class BoardManager : MonoBehaviour
{
    [SerializeField] private GameObject _blockSpawner;

    private Controller _controller;
    private BlockManager _blockManager;

    private int[,] _board;

    // 보드 생성
    void Start()
    {
        _blockManager = _blockSpawner.transform.GetComponent<BlockManager>();

        _board = new int[4, 4];
        Initialization();
    }

    private void MergeableBlockCheck()
    {

    }

    private void MoveBlock()
    {
        bool createBlock = false;
        const int END_DIRECTION = 0; // 방향의 가장 끝
        for (int i = END_DIRECTION + 1; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                // 이동시킬 블록이 있는지 탐색
                if (_board[i, j] != BLANK)
                {
                    if (createBlock == false)
                    {
                        createBlock = true;
                    }

                    for (int k = i; k > 0; k--)
                    {
                        // 이동시킬 자리 탐색
                        if (_board[k - 1, j] != BLANK)
                        {
                            if (_board[k - 1, j] == _board[i, j])
                            {
                                // 이동 병합
                                _board[k - 1, j] += _board[i, j];
                            }
                            else
                            {
                                // 이동
                                _board[k, j] = _board[i, j];
                            }

                            _board[i, j] = 0;
                        }
                    }
                }
            }
        }

        if (createBlock)
        {
            // 블록 생
        }
    }

    private const int BLANK = 0;
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

        for (int i = 0; i < 2; i++)
        {
            int x, y;
            (x, y) = RandomPositionSetting();

            if (x == -1)
            {
                // 게임 오버
                break;
            }

            _board[x, y] = 2;
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
            ProcessingStatusCheck(true);
            MoveBlock();
            RandomPositionSetting();
        }

        //if (_controller.down)
        //{
        //    ProcessingStatusCheck(true);
        //}

        //if (_controller.right)
        //{
        //    ProcessingStatusCheck(true);
        //}

        //if (_controller.left)
        //{
        //    ProcessingStatusCheck(true);
        //}
    }
}
