using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _board;
    [SerializeField] private GameObject _blockPrefab;
    private GameObject _block;

    private Transform[,] _blockSpawnPosition;
    //private List<BlockData>;

    struct BlockData
    {
        public GameObject Block;
        public bool checkBlanks;
        public int num;
        public int x;
        public int y;
        //Color 블럭 컬러;
    }

    // 보드가 생성되었을 , 혹은 플레이어 입력이 들어왔을 때
    // 랜덤 위치에 2 또는 낮은 확률로 4 블록 생성
    private void OnEnable()
    {
        _blockSpawnPosition = new Transform[4, 4];
        Initialization();
    }

    private void Initialization()
    {
        int childIndex = -1;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                _blockSpawnPosition[i, j] = _board.transform.GetChild(++childIndex).gameObject.transform;
            }
        }
    }

    public void Spawn(int x, int y)
    {
        GameObject block = Instantiate(_blockPrefab, _board.transform);
        Debug.Log(_blockSpawnPosition[x, y].position);
        block.transform.position = _blockSpawnPosition[x, y].position;
    }
}
