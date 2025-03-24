using UnityEngine;

public class TreeLayoutManager : MonoBehaviour
{
    [SerializeField] private Vector2 _startPosition = new Vector2(80, -160);
    [SerializeField] private Vector2 _cellSize = new Vector2(100, 100);

    [Header("Prefabs")]
    [SerializeField] private GameObject _rightArrow;
    [SerializeField] private GameObject _rightAndDownArrow;
    [SerializeField] private GameObject _downArrow;
    [SerializeField] private GameObject _downToRightArrow;

    private Vector2 _maxPosition = Vector2.zero;

    private Vector2 GetPositonToGrid(string position)
    {
        return new Vector2(position[0] - 'A', position[1] - '1');
    }

    private Vector2 GetContentPos(string position)
    {
        return GetContentPos(GetPositonToGrid(position));
    }

    private Vector2 GetContentPos(Vector2 grid)
    {
        return new Vector2(_startPosition.x - (2 * grid.x * _cellSize.x), _startPosition.y - (2 * grid.y * _cellSize.y));
    }

    /// <summary> 생성된 슬롯 위치 설정 </summary>
    public void SetPosition(RectTransform targetRect, string position)
    {
        Vector2 contentPosition = GetContentPos(position);
        _maxPosition.x = Mathf.Max(contentPosition.x, _maxPosition.x);
        _maxPosition.y = Mathf.Max(-contentPosition.y, _maxPosition.y);
        targetRect.anchoredPosition = contentPosition;
    }

    /// <summary> 화살표 그리기 </summary>
    public void PutArrow(Transform content, string _startCell, string _endCells)
    {
        Vector2 startPos = GetContentPos(_startCell);
        string[] _endCellArr = _endCells.Split(",");
        RectTransform arrowRect;

        if (_endCellArr.Length == 1)
        {
            arrowRect = Instantiate(_rightArrow, content).GetComponent<RectTransform>();
            arrowRect.anchoredPosition = startPos + new Vector2(_cellSize.x, 0);
        }
        else
        {
            arrowRect = Instantiate(_rightAndDownArrow, content).GetComponent<RectTransform>();
            arrowRect.anchoredPosition = startPos + new Vector2(_cellSize.x, 0);

            arrowRect = Instantiate(_downArrow, content).GetComponent<RectTransform>();
            arrowRect.anchoredPosition = startPos + new Vector2(_cellSize.x, -_cellSize.y);

            arrowRect = Instantiate(_downToRightArrow, content).GetComponent<RectTransform>();
            arrowRect.anchoredPosition = startPos + new Vector2(_cellSize.x, - 2 * _cellSize.y);
        }
    }

    public Vector2 GetMaxPosition()
    {
        return _maxPosition + _cellSize * 2;
    }
}
