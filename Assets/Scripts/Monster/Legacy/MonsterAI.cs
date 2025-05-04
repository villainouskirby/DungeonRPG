using UnityEngine;
using UnityEngine.AI; // Nav Mesh 를 사용하기 위해 필요한 using 문

public class MonsterAI : MonoBehaviour
{
    [SerializeField] Transform target; // 따라갈 타겟

    NavMeshAgent agent; // 탐색 메시 에이전트에 대한 참조가 필요

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false; // Agent 가 Target 을 향해 이동할 때 방향을 회전할지
        agent.updateUpAxis = false; // 캐릭터의 이동을 평면으로 제한하기 위해
    }

    void Update()
    {
        agent.SetDestination(target.position); // Agent에게 target의 현재 위치로 이동하도록 지시
    }
}
