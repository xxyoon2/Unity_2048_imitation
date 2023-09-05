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

    private const int BOARD_SIZE = 4;
    private const int BLANK = 0;
    private const int NUNBER_BLOCKS_CREATE = 2;

    private BlockState _blockState = BlockState.NONE;
    private enum BlockState
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

        _board = new int[4, 4];
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

        Debug.Log("초기화 완료!");
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
        _blockManager.Spawn(ConvertCoordinateToSquareID(x, y), 2);
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

    private int SwitchPlusMinus(int plusMinus)
    {
        return plusMinus == 1 ? -1 : 1;
    }

    /// <summary>
    /// 입력에 대한 블록 이동
    /// </summary>
    private void MoveBlock()
    {
        ProcessingStatusCheck(true);

        List<int> margeSquareIDList = new List<int>();
        List<int> mergeResultList = new List<int>();
        bool createBlock = false;
        switch(_blockState)
        {
            case BlockState.UP:
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
                                        _board[k + -1, i] += blockNum;
                                        GameManager.Instance.AddScore(_board[k + -1, i]);
                                        _board[j, i] = 0;
                                        _blockManager.BlockMoveRequest(currentSquareID, ConvertCoordinateToSquareID(k + -1, i));
                                        margeSquareIDList.Add(ConvertCoordinateToSquareID(k + -1, i));
                                        mergeResultList.Add(blockNum + blockNum);
                                        merge = true;
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
            case BlockState.DOWN:
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
                                        _board[k + 1, i] += blockNum;
                                        GameManager.Instance.AddScore(_board[k + 1, i]);
                                        _board[j, i] = 0;
                                        _blockManager.BlockMoveRequest(currentSquareID, ConvertCoordinateToSquareID(k + 1, i));
                                        margeSquareIDList.Add(ConvertCoordinateToSquareID(k + 1, i));
                                        mergeResultList.Add(blockNum + blockNum);
                                        merge = true;
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
            case BlockState.LEFT:
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
                                        _board[i, k + -1] += blockNum;
                                        GameManager.Instance.AddScore(_board[i, k + -1]);
                                        _board[i, j] = 0;
                                        _blockManager.BlockMoveRequest(currentSquareID, ConvertCoordinateToSquareID(i, k + -1));
                                        margeSquareIDList.Add(ConvertCoordinateToSquareID(i, k + -1));
                                        mergeResultList.Add(blockNum + blockNum);
                                        merge = true;
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
            case BlockState.RIGHT:
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
                                        _board[i, k + 1] += blockNum;
                                        GameManager.Instance.AddScore(_board[i, k + 1]);
                                        _board[i, j] = 0;
                                        _blockManager.BlockMoveRequest(currentSquareID, ConvertCoordinateToSquareID(i, k + 1));
                                        margeSquareIDList.Add(ConvertCoordinateToSquareID(i, k + 1));
                                        mergeResultList.Add(blockNum + blockNum);
                                        merge = true;
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
        StartCoroutine(RequestMerge(margeSquareIDList, mergeResultList));

        if (createBlock)
        {
            SetBlock(2);
        }
    }

    
    //private void MoveBlock()
    //{
    //    ProcessingStatusCheck(true);

    //    int FirstSearchPoint = BLANK;
    //    int SecondSearchPoint = BLANK;
    //    int plusMinus = BLANK;
    //    switch (_blockState)
    //    {
    //        case BlockState.UP: case BlockState.LEFT:
    //            FirstSearchPoint = 1;
    //            SecondSearchPoint = 4;
    //            plusMinus = 1;
    //            break;
    //        case BlockState.DOWN: case BlockState.RIGHT:
    //            FirstSearchPoint = 2;
    //            SecondSearchPoint = -1;
    //            plusMinus = -1;
    //            break;
    //        default:
    //            break;
    //    }

    //    List<int> margeSquareIDList = new List<int>();
    //    List<int> mergeResultList = new List<int>();
    //    bool createBlock = false;
    //    for (int i = FirstSearchPoint; i != SecondSearchPoint; i += plusMinus)
    //    {
    //        for (int j = 0; j < 4; j++)
    //        {
    //            switch(_blockState)
    //            {
    //                case BlockState.UP: case BlockState.DOWN:
    //                    // Vertical Search
    //                    if (_board[i, j] != BLANK)
    //                    {
    //                        int blockNum = _board[i, j];
    //                        if (createBlock == false)
    //                        {
    //                            createBlock = true;
    //                        }

    //                        plusMinus = SwitchPlusMinus(plusMinus);

    //                        // 여기가 진짜 문제
    //                        for (int k = i; k != FirstSearchPoint + plusMinus; k += plusMinus)
    //                        {
    //                            // 이동시킬 자리 탐색
    //                            if (_board[k + plusMinus, j] != BLANK)
    //                            {
    //                                if (_board[k + plusMinus, j] == blockNum)
    //                                {
    //                                    // 이동 병합
    //                                    _board[k + plusMinus, j] += blockNum;
    //                                    _board[i, j] = 0;
    //                                    _blockManager.BlockMoveRequest(ConvertCoordinateToSquareID(i, j), ConvertCoordinateToSquareID(k + plusMinus, j));
    //                                    margeSquareIDList.Add(ConvertCoordinateToSquareID(k + plusMinus, j));
    //                                    mergeResultList.Add(blockNum + blockNum);
    //                                }
    //                                else
    //                                {
    //                                    // 이동
    //                                    if (k != i)
    //                                    {
    //                                        _board[k, j] = _board[i, j];
    //                                        _board[i, j] = 0;
    //                                        _blockManager.BlockMoveRequest(ConvertCoordinateToSquareID(i, j), ConvertCoordinateToSquareID(k, j));
    //                                    }
    //                                }
    //                            }
    //                            else if ((k + plusMinus) == FirstSearchPoint + plusMinus && _board[k + plusMinus, j] == BLANK)
    //                            {
    //                                _board[k + plusMinus, j] = blockNum;
    //                                _board[i, j] = 0;
    //                                _blockManager.BlockMoveRequest(ConvertCoordinateToSquareID(i, j), ConvertCoordinateToSquareID(k + plusMinus, j));
    //                            }
    //                        }

    //                        plusMinus = SwitchPlusMinus(plusMinus);
    //                    }
    //                    break;

    //                case BlockState.LEFT: case BlockState.RIGHT:
    //                    // Horizontal Search
    //                    if (_board[j, i] != BLANK)
    //                    {
    //                        int blockNum = _board[j, i];
    //                        if (createBlock == false)
    //                        {
    //                            createBlock = true;
    //                        }

    //                        plusMinus = SwitchPlusMinus(plusMinus);

    //                        // 여기가 진짜 문제
    //                        for (int k = i; k != FirstSearchPoint + plusMinus; k += plusMinus)
    //                        {
    //                            // 이동시킬 자리 탐색
    //                            if (_board[j, k + plusMinus] != BLANK)
    //                            {
    //                                if (_board[j, k + plusMinus] == blockNum)
    //                                {
    //                                    // 이동 병합
    //                                    _board[j, k + plusMinus] += blockNum;
    //                                    _board[j, i] = 0;
    //                                    _blockManager.BlockMoveRequest(ConvertCoordinateToSquareID(j, i), ConvertCoordinateToSquareID(j, k + plusMinus));
    //                                    margeSquareIDList.Add(ConvertCoordinateToSquareID(j, k + plusMinus));
    //                                    mergeResultList.Add(blockNum + blockNum);
    //                                }
    //                                else
    //                                {
    //                                    // 이동
    //                                    if (k != i)
    //                                    {
    //                                        _board[k, j] = _board[j, i];
    //                                        _board[j, i] = 0;
    //                                        _blockManager.BlockMoveRequest(ConvertCoordinateToSquareID(j, i), ConvertCoordinateToSquareID(j, k));
    //                                    }
    //                                }
    //                                break;
    //                            }
    //                            else if ((k + plusMinus) == FirstSearchPoint + plusMinus && _board[j, k + plusMinus] == BLANK)
    //                            {
    //                                _board[j, k + plusMinus] = blockNum;
    //                                _board[j, i] = 0;
    //                                _blockManager.BlockMoveRequest(ConvertCoordinateToSquareID(j, i), ConvertCoordinateToSquareID(j, k + plusMinus));
    //                            }
    //                        }

    //                        plusMinus = SwitchPlusMinus(plusMinus);
    //                    }
    //                    break;
    //            }
    //        }
    //    }
    //    StartCoroutine(RequestMerge(margeSquareIDList, mergeResultList));

    //    if (createBlock)
    //    {
    //        SetBlock(2);   
    //    }
    //}

    IEnumerator RequestMerge(List<int> mergeSquareIDList, List<int> mergeResultList)
    {
        yield return new WaitForSeconds(0.01f);
        // 머지 요청
        _blockManager.MergeBlock(mergeSquareIDList);

        int count = mergeResultList.Count;
        for (int i = 0; i < count; i++)
        {
            _blockManager.Spawn(mergeSquareIDList[i], mergeResultList[i]);
        }

        // 입력 받기 가능 체크
        ProcessingStatusCheck(false);

        yield break;
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
        if (NoBlank())
        {
            // 게임 종료
            GameOver();
        }

        if (_processing)
        {
            return;
        }

        if (_controller.up)
        {
            _blockState = BlockState.UP;
            MoveBlock();
            RandomPositionSetting();
        }

        if (_controller.down)
        {
            _blockState = BlockState.DOWN;
            MoveBlock();
            RandomPositionSetting();
        }

        if (_controller.right)
        {
            _blockState = BlockState.RIGHT;
            MoveBlock();
            RandomPositionSetting();
        }

        if (_controller.left)
        {
            _blockState = BlockState.LEFT;
            MoveBlock();
            RandomPositionSetting();
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
