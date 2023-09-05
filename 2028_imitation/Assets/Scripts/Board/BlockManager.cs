using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockManager : MonoBehaviour
{
    [SerializeField] private GameObject _board;
    [SerializeField] private GameObject _blockPrefab;

    private List<Transform> _blockSpawnPosition;
    private List<BlockData> _blockList;    // 사용하지 않는 블록 리스트

    private const int BLANK = -1;

    private const int BLOCK_COUNT = 20;
    private const int BLOCKT_COUNT_INDEX = BLOCK_COUNT - 1;

    /// <summary>
    /// 블록 관련 데이터
    /// </summary>
    struct BlockData
    {
        private int _id;
        //public int ID
        //{
        //    get
        //    {
        //        return _id;
        //    }
        //    set
        //    {
        //        _id = value;
        //    }
        //}

        private GameObject _block;

        public BlockData(GameObject ob = null, int id = BLANK)
        {
            _block = ob;
            _id = id;
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
    }

    private void OnEnable()
    {
        _blockSpawnPosition = new List<Transform>();
        _blockList = new List<BlockData>();

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
            BlockData data = _blockList[i];
            data.SetID(BLANK);
            _blockList[i] = data;
        }

        Debug.Log("블록 초기화 완료!!");
    }

    public void SortBlockList()
    {
        _blockList.Sort((x, y) =>
        {
            return x.GetID().CompareTo(y.GetID());
        });
    }

    public void BlockMoveRequest(int previousSquareID, int currentSquareID)
    {
        // 블록 아이디 계산해서 어디서부터 어디까지 이동해야하는지 BlockBehavior에 요청
        Transform targetPoint = _blockSpawnPosition[currentSquareID];

        // previousSquareID와 같은 blockData 찾기
        GameObject block = null;
        for (int i = 0; i < BLOCK_COUNT; i++)
        {
            if (_blockList[i].GetID() == previousSquareID)
            {
                // ID 갱신
                BlockData data = _blockList[i];
                data.SetID(currentSquareID);
                _blockList[i] = data;
                block = _blockList[i].GetBlock();
                SortBlockList();

                break;
            }
        }
        Debug.Assert(block != null, "블록 오브젝트를 찾지 못 함");
        // targetPoint까지 이동하라 지시
         block.transform.GetComponent<BlockBehavior>().Move(targetPoint);
    }

    /// <summary>
    /// 블록 생성
    /// </summary>
    /// <param name="squareID">칸ID</param>
    public void Spawn(int squareID, int num)
    {
        BlockData data = _blockList[0];
        data.SetID(squareID);
        _blockList[0] = data;

        data.GetBlock().transform.position = _blockSpawnPosition[squareID].position;
        data.GetBlock().SetActive(true);
        data.GetBlock().GetComponent<BlockBehavior>().Creation(num);

        SortBlockList();
    }

    public void MergeBlock(List<int> mergeSquareIDList)
    {
        // 블록 애니메이션 재생함수
        int idListCount = mergeSquareIDList.Count;
        int blockListCount = _blockList.Count;

        List<GameObject> blockList = new List<GameObject>();
        for (int i = 0; i < idListCount; i++)
        {
            for (int j = 0; j < blockListCount; j++)
            {
                if (_blockList[j].GetID() == mergeSquareIDList[i])
                {
                    _blockList[j].GetBlock().GetComponent<BlockBehavior>().Extinction();
                    blockList.Add(_blockList[j].GetBlock());

                    BlockData data = _blockList[j];
                    data.SetID(BLANK);
                    _blockList[j] = data;

                    SortBlockList();
                }
            }
        }

        // 코루틴으로 병합된 블럭 SetActive(false);
        StartCoroutine(BlockCleanup(blockList));
    }

    IEnumerator BlockCleanup(List<GameObject> blockList)
    {
        yield return new WaitForSeconds(0.5f);

        foreach(GameObject block in blockList)
        {
            block.SetActive(false);
        }

        yield break;
    }

    public void ResetBlock()
    {
        // 리스트 한 번 싹 갈아치워버려야ㅏㄹ지도 
        Initialization();
    }
}