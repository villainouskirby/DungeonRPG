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

    [Header("유효 타일 판정 옵션")]
    [SerializeField] Color blockedColor = Color.red;     // 불가 지점에서 라인/마크 색상
    [SerializeField] bool blockOutsideView = true;       // 뷰박스 밖은 무조건 불가 처리
    [SerializeField] bool tileCenterSnap = true;         // 타일 중앙에 스냅해서 표시(권장)

    [Header("미리보기 비주얼")]
    [SerializeField] LineRenderer line;          // 빈 오브젝트 + LineRenderer 달아두고 할당
    [SerializeField] Transform targetMark;       // 타겟 Crosshair 스프라이트 Transform
    [SerializeField] int curveSegments = 24;     // 곡선 샘플 수
    [SerializeField] float arcHeight = 2.0f;     // 곡선 최고점 높이(거리 비례 가산)
    [SerializeField] LayerMask groundMask = ~0;  // 필요시 지면 스냅용

    Camera cam;
    bool isAiming;
    public bool IsAiming => isAiming;
    // 취소 직후 같은 프레임/다음 프레임까지 E 입력 무시
    public int suppressEUntilFrame { get; private set; } = -1;
    public bool IsESuppressedNow => Time.frameCount <= suppressEUntilFrame;
    Material dashedMat;
    Texture2D dashedTex;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        cam = Camera.main;

        if (line == null)
            line = gameObject.AddComponent<LineRenderer>();

        var shader = Shader.Find("Particles/Standard Unlit");
        if (shader == null)
        {
            Debug.LogError("Shader not found: Particles/Standard Unlit");
        }
        else
        {
            line.material = new Material(shader);
            line.material.SetColor("_Color", defaultColor);
        }

        // 기본 렌더러 옵션
        line.widthMultiplier = 0.05f;
        line.alignment = LineAlignment.View;
        line.textureMode = LineTextureMode.Stretch; // 기본 실선
        line.numCornerVertices = 2;
        line.numCapVertices = 2;

        // 색상(그라디언트)도 같이 지정 — 셰이더가 버텍스 컬러를 받으면 더 선명
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(defaultColor, 0f), new GradientColorKey(defaultColor, 1f) },
            new[] { new GradientAlphaKey(defaultColor.a, 0f), new GradientAlphaKey(defaultColor.a, 1f) }
        );
        line.colorGradient = grad;

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

            

            // 곡선(단순 2차 베지어) 샘플링
            Vector3 control = (origin + target) * 0.5f + Vector3.up * (baseArc + 0.25f * dir.magnitude);
            DrawQuadratic(origin, control, target);
            ApplyLineStyleAfterDraw();

            if (targetMark) targetMark.position = target;

            // 타일 중앙 스냅
            if (tileCenterSnap)
            {
                var t = WorldToTile(target);
                target = new Vector3(t.x + 0.5f, t.y + 0.5f, target.z);
            }

            // 유효 타일 여부 판정
            bool allowed = IsTargetableByChunk(target);

            // 라인/마크 그리기 (기존 DrawQuadratic/Apply... 이후에 색상 반영)
            var colorToUse = allowed ? defaultColor : blockedColor;
            SetLineAndMarkColor(colorToUse);

            // 입력 처리(둘 다 지원)
            if (WasLeftReleased())
            {
                if (allowed)
                {
                    res.confirmed = true;
                    res.target = target;
                    break;
                }
                else
                {
                    // 불가 클릭: 확정하지 않고 계속 조준을 유지 (던지기 차단)
                }
            }
            if (WasRightReleased() || WasEscapeReleased() || WasThrowReleased())
            {
                res.confirmed = false;
                res.target = origin;
                suppressEUntilFrame = Time.frameCount + 1;
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

        var shader = Shader.Find("Particles/Standard Unlit");
        dashedMat = new Material(shader);

        // 64x1 반복 텍스처 생성
        int texW = 64, texH = 1;
        dashedTex = new Texture2D(texW, texH, TextureFormat.RGBA32, false, true);
        dashedTex.wrapMode = TextureWrapMode.Repeat;
        dashedTex.filterMode = FilterMode.Bilinear;

        int dashPx = Mathf.Clamp(Mathf.RoundToInt(texW * (dashLength / Mathf.Max(0.001f, dashLength + gapLength))), 1, texW - 1);
        for (int x = 0; x < texW; x++)
        {
            bool isDash = x < dashPx;
            Color c = isDash ? Color.white : new Color(1, 1, 1, 0); // 흰색/투명
            dashedTex.SetPixel(x, 0, c);
        }
        dashedTex.Apply();

        dashedMat.SetTexture("_MainTex", dashedTex);
        dashedMat.SetColor("_Color", defaultColor);
    }
    void ApplyLineStyleAfterDraw()
    {
        if (lineStyle == LineStyle.Solid)
        {
            line.textureMode = LineTextureMode.Stretch;

            // 머티리얼/그라디언트 둘 다 색 업데이트
            if (line.material.HasProperty("_Color"))
                line.material.SetColor("_Color", defaultColor);

            var grad = line.colorGradient;
            var keys = grad.colorKeys;
            if (keys.Length >= 2)
            {
                keys[0].color = defaultColor;
                keys[keys.Length - 1].color = defaultColor;
                grad.SetKeys(keys, grad.alphaKeys);
                line.colorGradient = grad;
            }
            return;
        }

        // 점선
        EnsureDashedMaterial();
        line.textureMode = LineTextureMode.Tile;
        line.material = dashedMat;

        if (line.material.HasProperty("_Color"))
            line.material.SetColor("_Color", defaultColor);

        // 라인 총 길이 계산
        float totalLen = 0f;
        for (int i = 1; i < line.positionCount; i++)
            totalLen += Vector3.Distance(line.GetPosition(i - 1), line.GetPosition(i));

        float oneCycle = Mathf.Max(0.001f, dashLength + gapLength);
        float tileCount = Mathf.Max(1f, totalLen / oneCycle);

        line.material.SetTextureScale("_MainTex", new Vector2(tileCount, 1f));
    }

    // 월드 좌표 → 타일 좌표(타일 사이즈가 1이라면 그대로 Floor)
    static Vector2Int WorldToTile(Vector3 world)
    {
        return new Vector2Int(Mathf.FloorToInt(world.x), Mathf.FloorToInt(world.y));
    }

    // 라인/타겟 색 즉시 갱신(실선·점선 공통 반영)
    void SetLineAndMarkColor(Color c)
    {
        // 라인 머티리얼/그라디언트
        if (line != null)
        {
            if (line.material && line.material.HasProperty("_Color"))
                line.material.SetColor("_Color", c);

            var g = line.colorGradient;
            var cks = g.colorKeys;
            if (cks != null && cks.Length > 0)
            {
                cks[0].color = c;
                cks[cks.Length - 1].color = c;
                g.SetKeys(cks, g.alphaKeys);
                line.colorGradient = g;
            }
        }

        // 타겟 마크 색(있으면)
        if (targetMark != null)
        {
            var sr = targetMark.GetComponent<SpriteRenderer>();
            if (sr) sr.color = c;
        }
    }

    // 이 월드 지점이 포인팅 가능한가?
    bool IsTargetableByChunk(Vector3 world)
    {
        var cm = ChunkManager.Instance;
        if (cm == null) return false;

        var tile = WorldToTile(world);
        var chunkPos = cm.GetChunkPos(tile);

        // 뷰박스 밖이면(LoadedChunkIndex에 키 없음) 차단할지 옵션
        if (blockOutsideView && (cm.LoadedChunkIndex == null || !cm.LoadedChunkIndex.ContainsKey(chunkPos)))
            return false;

        // 예외에 안전하게: 뷰박스 밖 요청이 들어가면 Key 에러 날 수 있으니 try-catch
        try
        {
            int v = cm.GetTile(tile); // HeightManager.Instance.GroundLayer 기준으로 조회
                                      // 요구사항: 0이 아닌 값만 가능
            return (v != 0);
        }
        catch
        {
            return false;
        }
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

    bool WasThrowReleased()
    {
#if ENABLE_INPUT_SYSTEM
    var k = UnityEngine.InputSystem.Keyboard.current;
    return k != null && k.eKey.wasReleasedThisFrame;   // ← 수정
#else
        return Input.GetKeyUp(KeyCode.E);
#endif
    }
}