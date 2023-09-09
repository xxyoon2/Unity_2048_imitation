using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockManager : MonoBehaviour
{
    [SerializeField] private GameObject _board;
    [SerializeField] private GameObject _blockPrefab;

    private List<Transform> _blockSpawnPosition;
    private List<BlockData> _blockList;
    //private List<BlockData> _currentBlockList;

    private const int BLANK = -1;
    private const int BLOCK_COUNT = 20;

    /// <summary>
    /// 블록 관련 데이터
    /// </summary>
    struct BlockData
    {
        private int _id;
        private GameObject _block;
        //private bool _isNeedMerge;

        public BlockData(GameObject ob = null, int id = BLANK)
        {
            _block = ob;
            _id = id;
           //_isNeedMerge = isNeedMerge;
        }

        public void SetID(int id)
        {
            _id = id;
        }

        public int GetID()
        {
            return _id;
        }

        public GameObject GetBlock()
        {
            return _block;
        }

        //public bool GetIsNeedMerge()
        //{
        //    return _isNeedMerge; 
        //}

        //public void SetNeedMerge(bool isNeedMerge)
        //{
        //    _isNeedMerge = isNeedMerge;
        //}
    }

    private void OnEnable()
    {
        _blockSpawnPosition = new List<Transform>();
        _blockList = new List<BlockData>();
        //_currentBlockList = new List<BlockData>();

        int childCount = _board.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            _blockSpawnPosition.Add(_board.transform.GetChild(i).gameObject.transform);
        }

        for (int i = 0; i < BLOCK_COUNT; i++)
        {
            GameObject block = Instantiate(_blockPrefab, _board.transform);
            _blockList.Add(new BlockData(block));
        }

        Initialization();
    }

    /// <summary>
    /// 초기화
    /// </summary>
    private void Initialization()
    {
        for (int i = 0; i < BLOCK_COUNT; i++)
        {
            _blockList[i].GetBlock().SetActive(false);
            UpdateSquareID(i, BLANK);
        }
        //_blockList = _currentBlockList;
    }

    /// <summary>
    /// 블록 생성
    /// </summary>
    /// <param name="squareID">칸ID</param>
    public void Spawn(int squareID, int num)
    {
        //_currentBlockList = _blockList;
        int index = _blockList.FindIndex(x => x.GetID().Equals(BLANK));
        BlockData data = _blockList[index];
        UpdateSquareID(index, squareID);

        GameObject block = data.GetBlock();
        block.transform.position = _blockSpawnPosition[squareID].position;
        block.SetActive(true);
        block.GetComponent<BlockBehavior>().Creation(num);
    }

    /// <summary>
    /// 칸ID 갱신
    /// </summary>
    /// <param name="index">블록데이터리스트 인덱스</param>
    /// <param name="id">칸ID</param>
    private void UpdateSquareID(int index, int id)
    {
        BlockData data = _blockList[index];
        data.SetID(id);
        _blockList[index] = data;
    }

    private void SortBlockList()
    {
        _blockList.Sort((x, y) =>
        {
            return x.GetID().CompareTo(y.GetID());
        });
        //_blockList = _currentBlockList;
    }

    //public void UpdateIsNeedMerge(int id, bool isNeed)
    //{
    //    int index = 0;
    //    for (int i = 0; i < BLOCK_COUNT; i++)
    //    {
    //        if (_currentBlockList[i].GetID() == id)
    //        {
    //            index = i;
    //            BlockData data = _currentBlockList[index];
    //            data.SetNeedMerge(isNeed);
    //            _currentBlockList[i] = data;

    //            break;
    //        }
    //    }
    //}

    public void BlockMoveRequest(int previousSquareID, int currentSquareID, bool isNeedMerge = false, int mergeBlockNumber = 2)
    {
        //_currentBlockList = _previousBlockList;

        // 블록 아이디 계산해서 어디서부터 어디까지 이동해야하는지 BlockBehavior에 요청
        Transform targetPoint = _blockSpawnPosition[currentSquareID];

        // previousSquareID와 같은 blockData 찾기
        int index = _blockList.FindIndex(x => x.GetID().Equals(previousSquareID));
        //for (int i = 0; i < BLOCK_COUNT; i++)
        //{
        //    if (_currentBlockList[i].GetID() == previousSquareID)
        //    {
        //        index = i;
        //        break;
        //    }
        //}

        UpdateSquareID(index, currentSquareID);
        GameObject block = _blockList[index].GetBlock();
        block.transform.GetComponent<BlockBehavior>().Move(targetPoint);

        if (isNeedMerge)
        {
            StartCoroutine(Merge(block, currentSquareID, mergeBlockNumber));
        }
    }

    IEnumerator Merge(GameObject block, int mergeBlockSquareID, int mergeBlockNumber)
    {
        yield return new WaitWhile(() => block.transform.GetComponent<BlockBehavior>().IsBlockMove());

        // currentSquareID와 같은 아이디인 블록 두 개 찾아서 애니메이션 재생 후 Active off
        int count = _blockList.Count;
        int index1 = 0, index2 = 0;
        GameObject mergeBlock1 = null;
        GameObject mergeBlock2 = null;
        for (int i = 0; i < count; i++)
        {
            if (_blockList[i].GetID() == mergeBlockSquareID)
            {
                if (mergeBlock1 == null)
                {
                    index1 = i;
                    mergeBlock1 = _blockList[i].GetBlock();
                }
                else
                {
                    index2 = i;
                    mergeBlock2 = _blockList[i].GetBlock();
                    break;
                }
            }
        }

        mergeBlock1.transform.GetComponent<BlockBehavior>().Extinction();
        mergeBlock2.transform.GetComponent<BlockBehavior>().Extinction();

        // 블럭 하나 새로 생성
        Spawn(mergeBlockSquareID, mergeBlockNumber);

        yield return new WaitForSeconds(0.1f);

        // 머지 한 블록들 Set.Active(false);
        mergeBlock1.SetActive(false);
        mergeBlock2.SetActive(false);
        UpdateSquareID(index1, BLANK);
        UpdateSquareID(index2, BLANK);

        yield break;
    }

    //public void MergeBlock(List<int> mergeSquareIDList)
    //{
    //    // 블록 애니메이션 재생함수
    //    int idListCount = mergeSquareIDList.Count;
    //    int blockListCount = _blockList.Count;

    //    List<GameObject> blockList = new List<GameObject>();
    //    for (int i = 0; i < idListCount; i++)
    //    {
    //        for (int j = 0; j < blockListCount; j++)
    //        {
    //            if (_blockList[j].GetID() == mergeSquareIDList[i])
    //            {
    //                _blockList[j].GetBlock().GetComponent<BlockBehavior>().Extinction();
    //                blockList.Add(_blockList[j].GetBlock());

    //                // 아이디 먼저 바꿔주고 조금 시간 지난 후에 Setoff시켜주는 바람에 이미
    //                // 다른 아이디로 새롭게 소환된 상태에서 블록이 off되는 버그가 발생
    //                UpdateSquareID(j, BLANK);
    //            }
    //        }
    //    }

    //    // 코루틴으로 병합된 블럭 SetActive(false);
    //    StartCoroutine(BlockCleanup(blockList));
    //}

    //IEnumerator BlockCleanup(List<GameObject> blockList)
    //{
    //    yield return new WaitForSeconds(0.5f);

    //    foreach(GameObject block in blockList)
    //    {
    //        block.SetActive(false);
    //    }

    //    yield break;
    //}

    /// <summary>
    /// 블록 초기화
    /// </summary>
    public void ResetBlock()
    {
        Initialization();
    }
}
