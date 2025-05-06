using UnityEngine;

public class IdleState : IPlayerState
{
    private IPlayerChangeState player;

    public IdleState(IPlayerChangeState player) { this.player = player; }

    public void Enter()
    {
        //Debug.Log("Idle 상태 시작");
    }

    public void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        if (moveX != 0 || moveY != 0) player.ChangeState(new MoveState(player));

        else if (Input.GetKeyDown(KeyCode.LeftShift)) player.ChangeState(new SneakState(player));
        else if (Input.GetKeyDown(KeyCode.Z)) player.ChangeState(new AttackState(player));
        else if (Input.GetKeyDown(KeyCode.F)) player.ChangeState(new ForageState(player));
    }

    public void Exit() { } //Debug.Log("Idle 상태 종료");
    public override string ToString() => "Idle";
}

public class MoveState : IPlayerState
{
    private IPlayerChangeState player;
    public MoveState(IPlayerChangeState player) { this.player = player; }

    public void Enter()
    {
        Debug.Log("Move 상태 시작");
    }

    public void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
       
        if (moveX == 0 && moveY == 0) player.ChangeState(new IdleState(player));
        if (Input.GetKeyDown(KeyCode.Z)) player.ChangeState(new AttackState(player));
        if (Input.GetKeyDown(KeyCode.LeftShift)) player.ChangeState(new SneakMoveState(player));
        if (Input.GetKey(KeyCode.X)) player.ChangeState(new RunState(player));
    }

    public void Exit()
    {
        //Debug.Log("Move 상태 종료");
    }

    public override string ToString() => "Move";
}
public class RunState : IPlayerState
{
    private IPlayerChangeState player;
    public RunState(IPlayerChangeState player) { this.player = player; }

    public void Enter()
    {
        //Debug.Log("Run 상태 시작");
    }

    public void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
      
        if (moveX == 0 && moveY == 0) player.ChangeState(new IdleState(player));
        if (Input.GetKeyUp(KeyCode.X)) player.ChangeState(new MoveState(player));
        if (Input.GetKeyDown(KeyCode.LeftShift)) player.ChangeState(new SneakMoveState(player));
    }

    public void Exit()
    {
        //Debug.Log("Run 상태 종료");
    }

    public override string ToString() => "Run";
}
public class SneakMoveState : IPlayerState
{
    private IPlayerChangeState player;
    public SneakMoveState(IPlayerChangeState player) { this.player = player; }

    public void Enter()
    {
        //Debug.Log("SneakMoveState 상태 시작");
    }

    public void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        
        if (moveX == 0 && moveY == 0)
        {
            if (Input.GetKey(KeyCode.LeftShift))
                player.ChangeState(new SneakState(player));
            else
                player.ChangeState(new IdleState(player)); ;
        }
        else
        {
            if (Input.GetKeyUp(KeyCode.LeftShift))
                player.ChangeState(new MoveState(player));
        }
        if (Input.GetKeyDown(KeyCode.Z)) player.ChangeState(new AttackState(player));
    }

    public void Exit()
    {
        //Debug.Log("SneakMoveState 상태 종료");
    }

    public override string ToString() => "SneakMoveState";
}

public class SneakState : IPlayerState
{
    private IPlayerChangeState player;
    public SneakState(IPlayerChangeState player) { this.player = player; }

    public void Enter()
    {
        //Debug.Log("SneakState 상태 시작");
    }

    public void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        
        if (moveX == 0 && moveY == 0) {
            if (Input.GetKeyUp(KeyCode.LeftShift)) { player.ChangeState(new IdleState(player)); }
        }
        else { player.ChangeState(new SneakMoveState(player)); }
        if (Input.GetKeyDown(KeyCode.Z)) player.ChangeState(new AttackState(player));
        
    }

    public void Exit()
    {
        //Debug.Log("SneakState 상태 종료");
    }

    public override string ToString() => "SneakState";
}

public class AttackState : IPlayerState
{
    private IPlayerChangeState player;

    public AttackState(IPlayerChangeState player) { this.player = player; }

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

    public void Exit() { } //Debug.Log("Attack 상태 종료");
    public override string ToString() => "Attack";
}

public class ForageState : IPlayerState
{
    private IPlayerChangeState player;

    public ForageState(IPlayerChangeState player) { this.player = player; }

    public void Enter()
    {
        Debug.Log("Forage 상태 시작");
    }

    public void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        if (moveX != 0 || moveY != 0) player.ChangeState(new MoveState(player));
        if (Input.GetKeyUp(KeyCode.F)) player.ChangeState(new IdleState(player));
        if (Input.GetKeyDown(KeyCode.Z)) player.ChangeState(new AttackState(player));
    }

    public void Exit() { }// Debug.Log("Forage 상태 종료");
    public override string ToString() => "Forage";
}