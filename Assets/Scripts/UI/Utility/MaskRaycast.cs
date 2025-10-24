using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MaskRaycast : MaskableGraphic, ICanvasRaycastFilter
{
    [SerializeField] private Rect _raycastArea;

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, sp, eventCamera, out Vector2 localPoint);
        Vector2 delta = localPoint - _raycastArea.position;

        bool isOutside = Mathf.Abs(delta.x) > _raycastArea.width / 2 || Mathf.Abs(delta.y) > _raycastArea.height / 2;
        if (isOutside)
        {
            Debug.Log(isOutside);
        }
        Debug.Log("IsOverObject : " + EventSystem.current.IsPointerOverGameObject().ToString());

        return isOutside;
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect rect = GetPixelAdjustedRect();
        Rect hole = _raycastArea != null ? _raycastArea : Rect.zero;
        Vector2 holePos = _raycastArea != null ? _raycastArea.position : Vector2.zero;

        Color32 col = color;

        float left = holePos.x - hole.width / 2;
        float right = holePos.x + hole.width / 2;
        float top = holePos.y + hole.height / 2;
        float bottom = holePos.y - hole.height / 2;

        AddQuad(vh, new Vector2(rect.xMin, top), new Vector2(rect.xMax, rect.yMax), col);    // 위쪽
        AddQuad(vh, new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMax, bottom), col); // 아래쪽
        AddQuad(vh, new Vector2(rect.xMin, bottom), new Vector2(left, top), col);            // 왼쪽
        AddQuad(vh, new Vector2(right, bottom), new Vector2(rect.xMax, top), col);           // 오른쪽
    }

    private void AddQuad(VertexHelper vh, Vector2 bottomLeft, Vector2 topRight, Color32 col)
    {
        int startIndex = vh.currentVertCount;

        vh.AddVert(new Vector3(bottomLeft.x, bottomLeft.y), col, Vector2.zero);
        vh.AddVert(new Vector3(bottomLeft.x, topRight.y), col, Vector2.up);
        vh.AddVert(new Vector3(topRight.x, topRight.y), col, Vector2.one);
        vh.AddVert(new Vector3(topRight.x, bottomLeft.y), col, Vector2.right);

        vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
        vh.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
    }
}
