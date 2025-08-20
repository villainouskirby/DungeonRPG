using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;


public sealed class ThrowAimUI : MonoBehaviour
{
    public static ThrowAimUI Instance { get; private set; }
    public enum LineStyle { Solid, Dashed }

    [Header("라인 스타일/색상")]
    [SerializeField] Color defaultColor = Color.green;
    [SerializeField] LineStyle lineStyle = LineStyle.Solid;

    // 점선 옵션
    [Tooltip("대쉬(선분) 하나의 길이(월드 단위)")]
    [SerializeField] float dashLength = 0.5f;
    [Tooltip("대쉬 사이 공백 길이(월드 단위)")]
    [SerializeField] float gapLength = 0.3f;

    [Header("미리보기 비주얼")]
    [SerializeField] LineRenderer line;          // 빈 오브젝트 + LineRenderer 달아두고 할당
    [SerializeField] Transform targetMark;       // 타겟 Crosshair 스프라이트 Transform
    [SerializeField] int curveSegments = 24;     // 곡선 샘플 수
    [SerializeField] float arcHeight = 2.0f;     // 곡선 최고점 높이(거리 비례 가산)
    [SerializeField] LayerMask groundMask = ~0;  // 필요시 지면 스냅용

    Camera cam;
    bool isAiming;
    Material dashedMat;
    Texture2D dashedTex;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        cam = Camera.main;

        // LineRenderer 없으면 생성
        if (line == null)
            line = gameObject.AddComponent<LineRenderer>();

        // 머티리얼을 강제로 새로 만들지 말고, 기본 Unlit/Color로 지정
        // (URP라면 "Universal Render Pipeline/Unlit" 사용 권장)
#if USING_URP
    var shader = Shader.Find("Universal Render Pipeline/Unlit");
#else
        var shader = Shader.Find("Unlit/Color");
#endif
        if (line.sharedMaterial == null || line.sharedMaterial.shader != shader)
            line.sharedMaterial = new Material(shader);

        // 색/두께/기본 옵션
        line.startColor = defaultColor;   // 아래 2)에서 선언할 변수
        line.endColor = defaultColor;
        line.widthMultiplier = 0.05f;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.alignment = LineAlignment.View;        // 2D UI 성격이면 View가 보통 보기 좋아
        line.textureMode = LineTextureMode.Stretch; // 실선 기본: Stretch

        // 비주얼 초기 상태
        line.positionCount = 0;
        line.enabled = false;
        if (targetMark) targetMark.gameObject.SetActive(false);
    }

    public struct Result
    {
        public bool confirmed;
        public Vector3 target;
    }

    /// <summary>
    /// 조준 UI 실행. 좌클릭 확정, 우클릭/ESC 취소.
    /// </summary>
    public async UniTask<Result> AimAndSelect(Vector3 origin, float maxDistance, float? overrideArcHeight = null)
    {
        if (isAiming) return new Result { confirmed = false, target = origin };
        isAiming = true;

        line.enabled = true;
        if (targetMark) targetMark.gameObject.SetActive(true);

        float baseArc = overrideArcHeight ?? arcHeight;
        Result res = new Result { confirmed = false, target = origin };

        // 한 프레임 지연(카메라 등 초기화 여유)
        await UniTask.Yield();

        while (true)
        {
            Vector3 mouseWorld = GetMouseWorldPoint(origin.z);
            Vector3 dir = mouseWorld - origin;
            dir.z = 0f;

            // 최대 사거리 클램프
            if (dir.magnitude > maxDistance)
                dir = dir.normalized * maxDistance;

            Vector3 target = origin + dir;

            // (선택) 지면에 스냅하고 싶다면 Raycast로 target 보정
            // var hit = Physics2D.Raycast(target, Vector2.down, 100f, groundMask); ...

            // 곡선(단순 2차 베지어) 샘플링
            Vector3 control = (origin + target) * 0.5f + Vector3.up * (baseArc + 0.25f * dir.magnitude);
            DrawQuadratic(origin, control, target);
            ApplyLineStyleAfterDraw();

            if (targetMark) targetMark.position = target;

            // 입력 처리(둘 다 지원)
            if (WasLeftReleased())
            {
                res.confirmed = true;
                res.target = target;
                break;
            }
            if (WasRightReleased() || WasEscapeReleased())
            {
                res.confirmed = false;
                res.target = origin;
                break;
            }

            await UniTask.Yield();
        }

        // 끄기
        line.enabled = false;
        line.positionCount = 0;
        if (targetMark) targetMark.gameObject.SetActive(false);
        isAiming = false;
        return res;
    }

    void DrawQuadratic(Vector3 a, Vector3 b, Vector3 c)
    {
        if (!line) return;
        if (curveSegments < 2) curveSegments = 2;

        EnsureLineCount(curveSegments + 1);
        for (int i = 0; i <= curveSegments; i++)
        {
            float t = i / (float)curveSegments;
            // Q(t) = (1-t)^2 * a + 2(1-t)t * b + t^2 * c
            Vector3 p = (1 - t) * (1 - t) * a + 2 * (1 - t) * t * b + t * t * c;
            line.SetPosition(i, p);
        }
    }

    void EnsureLineCount(int n)
    {
        if (line.positionCount != n) line.positionCount = n;
    }
    void EnsureDashedMaterial()
    {
        if (dashedMat != null) return;

#if USING_URP
    var shader = Shader.Find("Universal Render Pipeline/Unlit");
#else
        var shader = Shader.Find("Unlit/Texture");
#endif
        dashedMat = new Material(shader);

        // 가로 64px, 세로 1px 텍스처 생성: [대쉬][공백] 패턴 반복
        int texW = 64, texH = 1;
        dashedTex = new Texture2D(texW, texH, TextureFormat.RGBA32, false, true);
        dashedTex.wrapMode = TextureWrapMode.Repeat;
        dashedTex.filterMode = FilterMode.Bilinear;

        // 패턴 채우기: (dash: 불투명, gap: 투명)
        int dashPx = Mathf.RoundToInt(texW * (dashLength / (dashLength + gapLength)));
        for (int x = 0; x < texW; x++)
        {
            bool isDash = x < dashPx;
            Color c = isDash ? Color.white : new Color(1, 1, 1, 0); // 틴트를 위해 흰색/투명
            dashedTex.SetPixel(x, 0, c);
        }
        dashedTex.Apply();

        dashedMat.mainTexture = dashedTex;
        dashedMat.SetFloat("_Surface", 1); // (URP Unlit일 때 투명 표면)
        dashedMat.EnableKeyword("_ALPHATEST_ON");
        dashedMat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
    }
    void ApplyLineStyleAfterDraw()
    {
        if (lineStyle == LineStyle.Solid)
        {
            // 실선: 색만 적용, Stretch 모드
            line.textureMode = LineTextureMode.Stretch;
            line.sharedMaterial.SetColor("_Color", defaultColor); // Unlit/Color
            line.startColor = defaultColor;
            line.endColor = defaultColor;
            return;
        }

        // 점선
        EnsureDashedMaterial();
        line.textureMode = LineTextureMode.Tile;   // 타일 반복
        line.sharedMaterial = dashedMat;

        // 색상 틴트 (흰색 패턴 * 틴트 = 최종 색)
        if (dashedMat.HasProperty("_Color")) dashedMat.SetColor("_Color", defaultColor);
        line.startColor = defaultColor;
        line.endColor = defaultColor;

        // 총 길이로 타일링 조정
        float totalLen = 0f;
        for (int i = 1; i < line.positionCount; i++)
            totalLen += Vector3.Distance(line.GetPosition(i - 1), line.GetPosition(i));

        // 텍스처 한 주기의 월드 길이 = dashLength + gapLength
        float oneCycle = Mathf.Max(0.001f, dashLength + gapLength);
        float tileCount = Mathf.Max(1f, totalLen / oneCycle);

        // 메인 텍스처의 U 방향 타일 수 조정
        if (dashedMat.mainTexture != null)
            dashedMat.mainTextureScale = new Vector2(tileCount, 1f);
    }
    Vector3 GetMouseWorldPoint(float zPlane)
    {
#if ENABLE_INPUT_SYSTEM
        var pos = UnityEngine.InputSystem.Mouse.current?.position.ReadValue() ?? Vector2.zero;
        Vector3 sp = new Vector3(pos.x, pos.y, Mathf.Abs(cam.transform.position.z - zPlane));
        return cam.ScreenToWorldPoint(sp);
#else
        Vector3 sp = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(cam.transform.position.z - zPlane));
        return cam.ScreenToWorldPoint(sp);
#endif
    }

    bool WasLeftReleased()
    {
#if ENABLE_INPUT_SYSTEM
        var m = UnityEngine.InputSystem.Mouse.current;
        return m != null && m.leftButton.wasReleasedThisFrame;
#else
        return Input.GetMouseButtonUp(0);
#endif
    }
    bool WasRightReleased()
    {
#if ENABLE_INPUT_SYSTEM
        var m = UnityEngine.InputSystem.Mouse.current;
        return m != null && m.rightButton.wasReleasedThisFrame;
#else
        return Input.GetMouseButtonUp(1);
#endif
    }
    bool WasEscapeReleased()
    {
#if ENABLE_INPUT_SYSTEM
        var k = UnityEngine.InputSystem.Keyboard.current;
        return k != null && k.escapeKey.wasReleasedThisFrame;
#else
        return Input.GetKeyUp(KeyCode.Escape);
#endif
    }
}