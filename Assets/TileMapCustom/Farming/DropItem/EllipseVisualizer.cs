using UnityEngine;

[ExecuteAlways]
public class EllipseVisualizer : MonoBehaviour
{
    [Header("Ellipse Settings")]
    public Vector2 CenterOffset = Vector2.zero;
    public float RadiusX = 2f;
    public float RadiusY = 1f;
    [Range(4, 128)] public int segments = 60;
    public float DropAngle = 90;

    void OnDrawGizmos()
    {
        Vector3 worldCenter = (Vector2)transform.position + CenterOffset;

        // 1) 타원 둘레 그리기
        if (segments >= 4)
        {
            Gizmos.color = Color.green;
            Vector3 prev = worldCenter + new Vector3(RadiusX, 0f, 0f);
            for (int i = 1; i <= segments; i++)
            {
                float theta = 2f * Mathf.PI * i / segments;
                Vector3 next = worldCenter + new Vector3(
                    Mathf.Cos(theta) * RadiusX,
                    Mathf.Sin(theta) * RadiusY,
                    0f
                );

                if (theta >= Mathf.PI * 1.5f - Mathf.Deg2Rad * DropAngle && theta <= Mathf.PI * 1.5f + Mathf.Deg2Rad * DropAngle)
                    Gizmos.DrawLine(prev, next);

                prev = next;
            }
        }
    }

    public static Vector3 GetRandomPos(EllipseVisualizer ev)
    {
        float startAngle = Mathf.PI * 1.5f;
        float angle = Random.Range(Mathf.Deg2Rad * -ev.DropAngle, Mathf.Deg2Rad * ev.DropAngle) + startAngle;
        Vector3 pt = (Vector3)ev.CenterOffset + ev.transform.position + new Vector3(
            Mathf.Cos(angle) * ev.RadiusX,
            Mathf.Sin(angle) * ev.RadiusY,
            0f
        );
        return pt;
    }
}
