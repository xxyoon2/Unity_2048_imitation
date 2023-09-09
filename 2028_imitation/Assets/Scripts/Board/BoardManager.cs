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
    private int[,] _mergeBlockMatch = new int[4, 4];

    private const int BOARD_SIZE = 4;
    private const int BLANK = 0;
    private const int NUNBER_BLOCKS_CREATE = 2;

    private MoveState _moveState = MoveState.NONE;

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
        _board[x, y] = num;
        _blockManager.Spawn(ConvertCoordinateToSquareID(x, y), num);
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
                int blockNum1 = _board[i, j];

                // UP   => i - k
                for (int k = 1; k < BOARD_SIZE; k++)
                {
                    if (i - k < 0)
                    {
                        break;
                    }

                    int blockNum2 = _board[i - k, j];
                    if (blockNum2 != BLANK)
                    {
                        if (blockNum2 == blockNum1)
                        {
                            needMerge = true;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        continue;
                    }
                }

                // DOWN => i + k
                for (int k = 1; k < BOARD_SIZE; k++)
                {
                    if (i + k >= BOARD_SIZE)
                    {
                        break;
                    }

                    int blockNum2 = _board[i + k, j];
                    if (blockNum2 != BLANK)
                    {
                        if (blockNum2 == blockNum1)
                        {
                            needMerge = true;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        continue;
                    }
                }

                // LEFT => j - k
                for (int k = 1; k < BOARD_SIZE; k++)
                {
                    if (j - k < 0)
                    {
                        break;
                    }

                    int blockNum2 = _board[i, j - k];
                    if (blockNum2 != BLANK)
                    {
                        if (blockNum2 == blockNum1)
                        {
                            needMerge = true;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        continue;
                    }
                }

                // RIGHT=> j + k
                for (int k = 1; k < BOARD_SIZE; k++)
                {
                    if (j + k >= BOARD_SIZE)
                    {
                        break;
                    }

                    int blockNum2 = _board[i, j + k];
                    if (blockNum2 != BLANK)
                    {
                        if (blockNum2 == blockNum1)
                        {
                            needMerge = true;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        continue;
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
    /// 병합할 블록 체크
    /// </summary>
    /// <returns>병합할 블록이 있다면 true, 아니라면 false를 반환</returns>
    //private bool CheckMergeBlock()
    //{
    //    bool needMerge = false;
    //    int mergeCount = 0;
    //    switch (_moveState)
    //    {
    //        case MoveState.NONE:
    //            for (int i = 0; i < BOARD_SIZE; i++)
    //            {
    //                for (int j = 0; j < BOARD_SIZE; j++)
    //                {
    //                    int num = _board[i, j];
    //                    Debug.Assert(num != BLANK, "보드매니저 블록 데이터 버그");
    //                    // 네 방향으로 체크
    //                    // 만약 하나라도 병합할 수 있는 블럭이 있다면
    //                    // true르 반환
    //                    if ((i - 1 >= 0 && _board[i - 1, j] == num) || (i + 1 < BOARD_SIZE && _board[i + 1, j] == num))
    //                    {
    //                        return needMerge = true;
    //                    }
    //                    else if ((j - 1 >= 0 && _board[i, j - 1] == num) || (j + 1 < BOARD_SIZE && _board[i, j + 1] == num))
    //                    {
    //                        return needMerge = true;
    //                    }
    //                }
    //            }
    //            return needMerge;
    //        case MoveState.UP:
    //            for (int i = 0; i != BOARD_SIZE; i++)
    //            {
    //                for (int j = 1; j < BOARD_SIZE; j++)
    //                {
    //                    if (_board[j, i] != BLANK)
    //                    {
    //                        int num = _board[j, i];
    //                        for (int k = j; k != 0; k--)
    //                        {
    //                            if (_board[k + -1, i] == num && _mergeBlockMatch[k + -1, i] == BLANK)
    //                            {
    //                                // 이동 병합
    //                                _mergeBlockMatch[j, i] = _mergeBlockMatch[k + -1, i] = ++mergeCount;
    //                            }
    //                            else
    //                            {
    //                                _mergeBlockMatch[j, i] = BLANK;
    //                            }
    //                        }
    //                    }
    //                    else
    //                    {
    //                        _mergeBlockMatch[j, i] = BLANK;
    //                    }
    //                }
    //            }
    //            return needMerge = mergeCount == BLANK ? false : true;
    //        case MoveState.DOWN:
    //            for (int i = 0; i != BOARD_SIZE; i++)
    //            {
    //                for (int j = 2; j > -1; j--)
    //                {
    //                    if (_board[j, i] != BLANK)
    //                    {
    //                        int num = _board[j, i];
    //                        for (int k = j; k != 3; k++)
    //                        {
    //                            if (_board[k + 1, i] == num && _mergeBlockMatch[k + 1, i] == BLANK)
    //                            {
    //                                _mergeBlockMatch[j, i] = _mergeBlockMatch[k + 1, i] = ++mergeCount;
    //                            }
    //                            else
    //                            {
    //                                _mergeBlockMatch[j, i] = BLANK;
    //                            }
    //                        }
    //                    }
    //                    else
    //                    {
    //                        _mergeBlockMatch[j, i] = BLANK;
    //                    }
    //                }
    //            }
    //            return needMerge = mergeCount == BLANK ? false : true;
    //        case MoveState.LEFT:
    //            for (int i = 0; i != BOARD_SIZE; i++)
    //            {
    //                for (int j = 1; j < BOARD_SIZE; j++)
    //                {
    //                    if (_board[i, j] != BLANK)
    //                    {
    //                        int num = _board[i, j];
    //                        for (int k = j; k != 0; k--)
    //                        {
    //                            if (_board[i, k + -1] == num && _mergeBlockMatch[i, k + -1] == BLANK)
    //                            {
    //                                _mergeBlockMatch[i, j] = _mergeBlockMatch[i, k + -1] = ++mergeCount;
    //                            }
    //                            else
    //                            {
    //                                _mergeBlockMatch[i, j] = BLANK;
    //                            }
    //                        }
    //                    }
    //                    else
    //                    {
    //                        _mergeBlockMatch[i, j] = BLANK;
    //                    }
    //                }
    //            }
    //            return needMerge = mergeCount == BLANK ? false : true;
    //        case MoveState.RIGHT:
    //            for (int i = 0; i != BOARD_SIZE; i++)
    //            {
    //                for (int j = 2; j > -1; j--)
    //                {
    //                    if (_board[i, j] != BLANK)
    //                    {
    //                        int num = _board[i, j];
    //                        for (int k = j; k != 3; k++)
    //                        {
    //                            if (_board[i, k + 1] == num && _mergeBlockMatch[i, k + 1] == BLANK)
    //                            {
    //                                _mergeBlockMatch[i, j] = _mergeBlockMatch[i, k + 1] = ++mergeCount;
    //                            }
    //                            else
    //                            {
    //                                _mergeBlockMatch[i, j] = BLANK;
    //                            }
    //                        }
    //                    }
    //                    else
    //                    {
    //                        _mergeBlockMatch[i, j] = BLANK;
    //                    }
    //                }
    //            }
    //            return needMerge = mergeCount == BLANK ? false : true;
    //    }
    //
    //    return needMerge;
    //}

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


    /// <summary>
    /// 입력에 대한 블록 이동
    /// </summary>
    //private void MoveBlock()
    //{
    //    // 병합할 블록 체크
    //    bool createBlock = CheckMergeBlock();
    //    for (int i = 0; i < BOARD_SIZE; i++)
    //    {
    //        for (int j = 0; j < BOARD_SIZE; j++)
    //        {
    //            if (_mergeBlockMatch[i, j] != BLANK)
    //            {
    //                Debug.Log("얘는 병합해야합니다.");
    //                _blockManager.UpdateIsNeedMerge(ConvertCoordinateToSquareID(i, j), true);
    //            }
    //        }
    //    }

    //    List<int> margeSquareIDList = new List<int>();
    //    List<int> mergeResultList = new List<int>();

    //    switch (_moveState)
    //    {
    //        case MoveState.UP:
    //            for (int i = 0; i != BOARD_SIZE; i++)
    //            {
    //                for (int j = 1; j < BOARD_SIZE; j++)
    //                {
    //                    if (_board[j, i] != BLANK)
    //                    {
    //                        int blockNum = _board[j, i];
    //                        int previousID = ConvertCoordinateToSquareID(j, i);
    //                        if (createBlock == false)
    //                        {
    //                            createBlock = true;
    //                        }

    //                        for (int k = j; k != 0; k--)
    //                        {
    //                            int currentID;
    //                            if ((k + -1) == 0 && _board[k + -1, i] == BLANK)
    //                            {
    //                                _board[k + -1, i] = blockNum;
    //                                _board[j, i] = BLANK;

    //                                currentID = ConvertCoordinateToSquareID(k + -1, i);
    //                                _blockManager.BlockMoveRequest(previousID, currentID);

    //                                break;
    //                            }

    //                            if (_board[k + -1, i] != BLANK)
    //                            {
    //                                if (_mergeBlockMatch[k + -1, i] == _mergeBlockMatch[j, i])
    //                                {
    //                                    int mergeNum = blockNum * 2;
    //                                    _board[k + -1, i] = mergeNum;
    //                                    _board[j, i] = BLANK;

    //                                    GameManager.Instance.AddScore(mergeNum);
    //                                    currentID = ConvertCoordinateToSquareID(k + -1, i);
    //                                    _blockManager.BlockMoveRequest(previousID, currentID);

    //                                    margeSquareIDList.Add(currentID);
    //                                    mergeResultList.Add(mergeNum);
    //                                }
    //                                else
    //                                {
    //                                    if (k != j)
    //                                    {
    //                                        _board[k, i] = _board[j, i];
    //                                        _board[j, i] = BLANK;
    //                                        currentID = ConvertCoordinateToSquareID(k, i);
    //                                        _blockManager.BlockMoveRequest(previousID, currentID);
    //                                    }
    //                                }

    //                                break;
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //            break;
    //        case MoveState.DOWN:
    //            for (int i = 0; i != BOARD_SIZE; i++)
    //            {
    //                for (int j = 2; j > -1; j--)
    //                {
    //                    if (_board[j, i] != BLANK)
    //                    {
    //                        int blockNum = _board[j, i];
    //                        int previousID = ConvertCoordinateToSquareID(j, i);
    //                        if (createBlock == false)
    //                        {
    //                            createBlock = true;
    //                        }

    //                        for (int k = j; k != 3; k++)
    //                        {
    //                            int currentID;
    //                            if ((k + 1) == 3 && _board[k + 1, i] == BLANK)
    //                            {
    //                                _board[k + 1, i] = blockNum;
    //                                _board[j, i] = BLANK;

    //                                currentID = ConvertCoordinateToSquareID(k + 1, i);
    //                                _blockManager.BlockMoveRequest(previousID, currentID);

    //                                break;
    //                            }

    //                            if (_board[k + 1, i] != BLANK)
    //                            {
    //                                if (_mergeBlockMatch[k + 1, i] == _mergeBlockMatch[j, i])
    //                                {
    //                                    int mergeNum = blockNum * 2;
    //                                    _board[k + 1, i] = mergeNum;
    //                                    _board[j, i] = BLANK;

    //                                    GameManager.Instance.AddScore(mergeNum);
    //                                    currentID = ConvertCoordinateToSquareID(k + 1, i);
    //                                    _blockManager.BlockMoveRequest(previousID, currentID);

    //                                    margeSquareIDList.Add(currentID);
    //                                    mergeResultList.Add(mergeNum);
    //                                }
    //                                else
    //                                {
    //                                    if (k != j)
    //                                    {
    //                                        _board[k, i] = _board[j, i];
    //                                        _board[j, i] = BLANK;
    //                                        currentID = ConvertCoordinateToSquareID(k, i);
    //                                        _blockManager.BlockMoveRequest(previousID, currentID);
    //                                    }
    //                                }

    //                                break;
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //            break;
    //        case MoveState.LEFT:
    //            for (int i = 0; i != BOARD_SIZE; i++)
    //            {
    //                for (int j = 1; j < BOARD_SIZE; j++)
    //                {
    //                    if (_board[i, j] != BLANK)
    //                    {
    //                        int blockNum = _board[i, j];
    //                        int previousID = ConvertCoordinateToSquareID(i, j);
    //                        if (createBlock == false)
    //                        {
    //                            createBlock = true;
    //                        }

    //                        for (int k = j; k != 0; k--)
    //                        {
    //                            int currentID;
    //                            if ((k + -1) == 0 && _board[i, k + -1] == BLANK)
    //                            {
    //                                _board[i, k + -1] = blockNum;
    //                                _board[i, j] = BLANK;

    //                                currentID = ConvertCoordinateToSquareID(i, k + -1);
    //                                _blockManager.BlockMoveRequest(previousID, currentID);

    //                                break;
    //                            }

    //                            if (_board[i, k + -1] != BLANK)
    //                            {
    //                                if (_mergeBlockMatch[i, k + -1] == _mergeBlockMatch[i, j])
    //                                {
    //                                    int mergeNum = blockNum * 2;
    //                                    _board[i, k + -1] = mergeNum;
    //                                    _board[i, j] = BLANK;

    //                                    GameManager.Instance.AddScore(mergeNum);
    //                                    currentID = ConvertCoordinateToSquareID(i, k + -1);
    //                                    _blockManager.BlockMoveRequest(previousID, currentID);

    //                                    margeSquareIDList.Add(currentID);
    //                                    mergeResultList.Add(mergeNum);
    //                                }
    //                                else
    //                                {
    //                                    if (k != j)
    //                                    {
    //                                        _board[i, k] = _board[i, j];
    //                                        _board[i, j] = BLANK;
    //                                        currentID = ConvertCoordinateToSquareID(i, k);
    //                                        _blockManager.BlockMoveRequest(previousID, currentID);
    //                                    }
    //                                }

    //                                break;
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //            break;
    //        case MoveState.RIGHT:
    //            for (int i = 0; i != 4; i++)
    //            {
    //                for (int j = 2; j > -1; j--)
    //                {
    //                    if (_board[i, j] != BLANK)
    //                    {
    //                        int blockNum = _board[i, j];
    //                        int previousID = ConvertCoordinateToSquareID(i, j);
    //                        if (createBlock == false)
    //                        {
    //                            createBlock = true;
    //                        }

    //                        for (int k = j; k != 3; k++)
    //                        {
    //                            int currentID;
    //                            if ((k + 1) == 3 && _board[i, k + 1] == BLANK)
    //                            {
    //                                _board[i, k + 1] = blockNum;
    //                                _board[i, j] = BLANK;

    //                                currentID = ConvertCoordinateToSquareID(i, k + 1);
    //                                _blockManager.BlockMoveRequest(previousID, currentID);

    //                                break;
    //                            }

    //                            if (_board[i, k + 1] != BLANK)
    //                            {
    //                                if (_mergeBlockMatch[i, k + 1] == _mergeBlockMatch[i, j])
    //                                {
    //                                    int mergeNum = blockNum * 2;
    //                                    _board[i, k + 1] = mergeNum;
    //                                    _board[i, j] = BLANK;

    //                                    GameManager.Instance.AddScore(mergeNum);
    //                                    currentID = ConvertCoordinateToSquareID(i, k + 1);
    //                                    _blockManager.BlockMoveRequest(previousID, currentID);

    //                                    margeSquareIDList.Add(currentID);
    //                                    mergeResultList.Add(mergeNum);
    //                                }
    //                                else
    //                                {
    //                                    if (k != j)
    //                                    {
    //                                        _board[i, k] = _board[i, j];
    //                                        _board[i, j] = BLANK;
    //                                        currentID = ConvertCoordinateToSquareID(i, k);
    //                                        _blockManager.BlockMoveRequest(previousID, currentID);
    //                                    }
    //                                }

    //                                break;
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //            break;
    //    }

    //    if (createBlock)
    //    {
    //        SetBlock(2);
    //    }
    //}

    //IEnumerator RequestMerge(List<int> mergeSquareIDList, List<int> mergeResultList)
    //{
    //    yield return new WaitForSeconds(0.01f);
    //    // 머지 요청
    //    _blockManager.MergeBlock(mergeSquareIDList);

    //    int count = mergeResultList.Count;
    //    for (int i = 0; i < count; i++)
    //    {
    //        _blockManager.Spawn(mergeSquareIDList[i], mergeResultList[i]);
    //    }

    //    // 입력 받기 가능 체크
    //    ProcessingStatusCheck(false);

    //    yield break;
    //}

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
        //if (_moveState != MoveState.NONE)
        //{
        //    return;
        //}

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
        if (NoBlank() && CheckMergeBlock())
        {
            return true;
        }
        else
        {
            return false;
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
        // 플레이어 입력 막기 
    }

    private void OnDisable()
    {
        GameManager.Instance.OnResetGame.RemoveListener(ResetBoard);
        GameManager.Instance.OnGameOver.RemoveListener(GameOver);
    }
}
