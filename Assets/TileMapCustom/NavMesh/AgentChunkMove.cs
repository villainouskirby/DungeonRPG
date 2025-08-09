using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AgentChunkMove : MonoBehaviour
{
    private NavMeshAgent _agent;
    private bool _isMoving = false;

    IEnumerator Start()
    {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        agent.autoTraverseOffMeshLink = false;
        while (true)
        {
            if (agent.isOnOffMeshLink && !_isMoving)
            {
                _isMoving = true;
                OffMeshLinkData data = agent.currentOffMeshLinkData;
                yield return StartCoroutine(MoveChunk(agent, data));
                agent.CompleteOffMeshLink();
            }
            yield return null;
        }
    }

    IEnumerator MoveChunk(NavMeshAgent agent, OffMeshLinkData data)
    {
        Vector3 endPos = data.endPos;
        while (Vector3.Distance(agent.transform.position, endPos) > 0.01f)
        {
            agent.transform.position = Vector3.MoveTowards(agent.transform.position, endPos, agent.speed * Time.deltaTime);
            yield return null;
        }
        _isMoving = false;
    }
}
