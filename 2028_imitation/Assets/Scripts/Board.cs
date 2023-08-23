using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 블럭 이동이 없으면 블럭 생성도 하지 않음

public class Board : MonoBehaviour
{
    [SerializeField] private GameObject _blockSpawner;

    private Controller _controller;
    private BlockSpawner _spawner;

    private int[,] _board;
    // 실질적인 보드 연산하는 곳
    // 만약 이동 방향에 같은 블록이 있다면, 생성할 블록 연산 , 합쳐진 블록 삭제

    // 보드 생성
    void Start()
    {
        //_controller = GetComponent<Controller>();
        _spawner = _blockSpawner.transform.GetComponent<BlockSpawner>();

        _board = new int[4, 4];
        Initialization();
    }

    private void Initialization()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                _board[i, j] = 0;
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
            Debug.Log((x, y));
            _spawner.Spawn(x, y);
            // 블록 생성
        }
    }

    private (int, int) RandomPositionSetting()
    {
        bool checkBlanks = false;
        int minInclusive = 10;
        int maxExclusive = -10;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (_board[i, j] == 0)
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
        int posY = 0;
        int countBlanks = 0;
        List<int> blanks = new List<int>();
        if (minInclusive + 1 != maxExclusive)
        {
            posX = Random.Range(minInclusive, maxExclusive);
        }

        for (int i = 0; i < 4; i++)
        {
            if (_board[posX, i] == 0)
            {
                ++countBlanks;
                blanks.Add(i);
            }
        }

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


    void Update()
    {
        
    }
}
