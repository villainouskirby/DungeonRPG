using UnityEngine;

public class IdleState : IPlayerState
{
    private PlayerController player;

    public IdleState(PlayerController player) { this.player = player; }

    public void Enter()
    {
        Debug.Log("Idle 상태 시작");
    }

    public void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        if (moveX != 0 || moveY != 0) player.ChangeState(new MoveState(player));

        else if (Input.GetKeyDown(KeyCode.LeftShift) && player.canSneak) player.ChangeState(new SneakState(player));
        else if (Input.GetKeyDown(KeyCode.Z)) player.ChangeState(new AttackState(player));
        else if (Input.GetKeyDown(KeyCode.F)) player.ChangeState(new ForageState(player));
    }

    public void Exit() => Debug.Log("Idle 상태 종료");
    public override string ToString() => "Idle";
}

public class MoveState : IPlayerState
{
    private PlayerController player;
    public MoveState(PlayerController player) { this.player = player; }

    public void Enter()
    {
        Debug.Log("Move 상태 시작");
    }

    public void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        player.SetMoveInput(moveX);
        player.SetMoveInput(moveY);
        if (moveX == 0 || moveY == 0) player.ChangeState(new IdleState(player));
        else if (Input.GetKeyDown(KeyCode.A)) player.ChangeState(new AttackState(player));
        else if (Input.GetKeyDown(KeyCode.LeftShift) && player.canSneak) player.ChangeState(new SneakMoveState(player));
    }

    public void Exit()
    {
        Debug.Log("Move 상태 종료");
    }

    public override string ToString() => "Move";
}

public class SneakMoveState : IPlayerState
{
    private PlayerController player;
    public SneakMoveState(PlayerController player) { this.player = player; }

    public void Enter()
    {
        Debug.Log("SneakMoveState 상태 시작");
    }

    public void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        player.SetMoveInput(moveX);
        player.SetMoveInput(moveY);
        if (moveX == 0 && moveY == 0)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
                player.ChangeState(new SneakState(player));
            else
                player.ChangeState(new IdleState(player)); ;
        }
        if (Input.GetKeyDown(KeyCode.Z)) player.ChangeState(new AttackState(player));
    }

    public void Exit()
    {
        Debug.Log("SneakMoveState 상태 종료");
    }

    public override string ToString() => "SneakMoveState";
}

public class SneakState : IPlayerState
{
    private PlayerController player;
    public SneakState(PlayerController player) { this.player = player; }

    public void Enter()
    {
        Debug.Log("SneakState 상태 시작");
    }

    public void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        player.SetMoveInput(moveX);
        player.SetMoveInput(moveY);
        if (moveX == 0 || moveY == 0) player.ChangeState(new IdleState(player));
        else if (Input.GetKeyDown(KeyCode.Z)) player.ChangeState(new AttackState(player));
    }

    public void Exit()
    {
        Debug.Log("SneakState 상태 종료");
    }

    public override string ToString() => "SneakState";
}

public class AttackState : IPlayerState
{
    private PlayerController player;

    public AttackState(PlayerController player) { this.player = player; }

    public void Enter()
    {
        Debug.Log("Attack 상태 시작");
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift)) player.ChangeState(new SneakState(player));

        if (Input.GetKeyUp(KeyCode.Z)) player.ChangeState(new IdleState(player));

        //player.PlayerNormalAttack();

    }

    public void Exit() => Debug.Log("Attack 상태 종료");
    public override string ToString() => "Attack";
}

public class ForageState : IPlayerState
{
    private PlayerController player;

    public ForageState(PlayerController player) { this.player = player; }

    public void Enter()
    {
        Debug.Log("Forage 상태 시작");
    }

    public void Update()
    {
        if (Input.GetKeyUp(KeyCode.F)) player.ChangeState(new IdleState(player));
    }

    public void Exit() => Debug.Log("Forage 상태 종료");
    public override string ToString() => "Forage";
}