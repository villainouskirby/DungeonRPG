using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AgentChunkMove : MonoBehaviour
{
    private NavMeshAgent _agent;

    IEnumerator Start()
    {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        agent.autoTraverseOffMeshLink = false;
        while (true)
        {
            if (agent.isOnOffMeshLink)
            {
                yield return StartCoroutine(MoveChunk(agent));
                agent.CompleteOffMeshLink();
            }
            yield return null;
        }
    }

    IEnumerator MoveChunk(NavMeshAgent agent)
    {
        OffMeshLinkData data = agent.currentOffMeshLinkData;
        Vector3 endPos = data.endPos;
        while (Vector3.Distance(agent.transform.position, endPos) > 0.01f)
        {
            agent.transform.position = Vector3.MoveTowards(agent.transform.position, endPos, agent.speed * Time.deltaTime);
            yield return null;
        }
    }
}
