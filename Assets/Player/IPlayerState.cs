using UnityEngine;

public interface IPlayerState
{
    void Enter();
    void Update();
    void Exit();
}

public interface IPlayerChangeState
{
    void ChangeState(IPlayerState newState);
}
public static class ToggleSneak
{
    private static readonly KeyCode Key = KeyCode.LeftControl;

    // 현재 토글 상태 (ON=눌린 상태처럼 동작)
    public static bool IsActive { get; private set; }

    // 이번 프레임에 "토글 OFF"가 발생했는지(= Up 합성)
    private static int lastUpFrame = -1;
    // 필요 시 Up 대신 Down을 추적하고 싶으면 사용
    private static int lastDownFrame = -1;

    /// <summary>실제 하드웨어 입력의 KeyDown. 누를 때마다 토글 전환</summary>
    public static bool GetKeyDown()
    {
        if (Input.GetKeyDown(Key))
        {
            IsActive = !IsActive;
            if (IsActive) lastDownFrame = Time.frameCount;
            else lastUpFrame = Time.frameCount;
            return true; // IdleState의 분기 그대로 살림
        }
        return false;
    }

    /// <summary>토글 상태를 '홀드 중'처럼 반환</summary>
    public static bool GetKey()
    {
        return IsActive;
    }

    /// <summary>이번 프레임에 토글이 OFF로 전환됐으면 Up으로 합성</summary>
    public static bool GetKeyUp()
    {
        return lastUpFrame == Time.frameCount;
    }

    /// <summary>필요하면 외부에서 강제로 토글 리셋</summary>
    public static void Reset(bool active = false)
    {
        IsActive = active;
        lastUpFrame = -1;
        lastDownFrame = -1;
    }
}