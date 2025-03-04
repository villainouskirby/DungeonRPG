using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Interval
{
    public float Start { get; set; }
    public float End { get; set; }

    public Interval(float start, float end)
    {
        Start = start;
        End = end;
    }

    public void Reset()
    {
        Start = 0;
        End = 0;
    }
}

public class IntervalNode
{
    // Left Right은 단방향임으로 다른 방향은 세팅할 필요가 없다.
    // 시작점에서만 중요하다.
    public Interval Interval { get; set; }
    public IntervalNode Left { get; set; }
    public IntervalNode Right { get; set; }

    public IntervalNode(Interval interval)
    {
        Interval = interval;
    }

    public void Reset()
    {
        Interval.Reset();
        Left = null;
        Right = null;
    }
}

public class IntervalTree
{
    private IntervalNode _root;
    private IntervalNodePool _pool;

    public IntervalTree(IntervalNodePool pool)
    {
        _pool = pool;
    }

    public void Insert(Interval interval)
    {
        // 이미 해당 범위가 포함되어있다면 스킵
        if (Overlaps(interval))
            return;
        _root = Insert(_root, interval);
    }

    private IntervalNode Insert(IntervalNode node, Interval interval)
    {
        // 새로운 범위가 들어온다는 가정. 이미 중복되는 범위는 걸러졌다.

        if (node == null)
            return _pool.Get(interval);

        // 중앙 기준 왼쪽이라면 왼쪽 노드에 넘긴다.
        if (interval.End < node.Interval.Start)
        {
            node.Left = Insert(node.Left, interval);
        }
        // 중앙 기준 오른쪽이라면 오른쪽 노드에 넘긴다.
        else if (interval.Start > node.Interval.End)
        {
            node.Right = Insert(node.Right, interval);
        }
        // 남은건 곂치는 경우 뿐이다.
        // 이제 어디까지 곂치는지를 판별한다.
        // 해당 범위보다 더 큰 경우 - 좌우로 확장
        else if (interval.End >= node.Interval.End && interval.Start <= node.Interval.Start)
        {
            LeftExpansion(node, node.Left, interval);
            RightExpansion(node, node.Right, interval);
        }
        // 왼쪽으로 속하면서 곂치는 경우 - 좌로 확장
        else if (interval.End >= node.Interval.Start && interval.Start < node.Interval.Start)
        {
            LeftExpansion(node, node.Left, interval);
        }
        // 오른쪽으로 속하면서 곂치는 경우 - 우로 확장
        else if (interval.Start <= node.Interval.End && interval.End >= node.Interval.End)
        {
            RightExpansion(node, node.Right, interval);
        }

        return node;
    }

    private enum OverLap
    {
        None = 0,
        OverLap = 1,
        Over = 2,
    }

    private void LeftExpansion(IntervalNode startNode, IntervalNode node, Interval interval)
    {
        switch (LeftCheck(node, interval))
        {
            case OverLap.None:
                startNode.Left = node;
                startNode.Interval = new Interval(interval.Start, startNode.Interval.End);
                return;
            case OverLap.OverLap:
                startNode.Left = node.Left;
                startNode.Interval = new Interval(node.Interval.Start, startNode.Interval.End);
                _pool.Release(node);
                return;
            case OverLap.Over:
                LeftExpansion(startNode, node.Left, interval);
                _pool.Release(node);
                return;
        }
    }

    private void RightExpansion(IntervalNode startNode, IntervalNode node, Interval interval)
    {
        switch (RightCheck(node, interval))
        {
            case OverLap.None:
                startNode.Right = node;
                startNode.Interval = new Interval(startNode.Interval.Start, interval.End);
                return;
            case OverLap.OverLap:
                startNode.Right = node.Right;
                startNode.Interval = new Interval(startNode.Interval.Start, node.Interval.End);
                _pool.Release(node);
                return;
            case OverLap.Over:
                RightExpansion(startNode, node.Right, interval);
                _pool.Release(node);
                return;
        }
    }

    public bool Overlaps(Interval interval)
    {
        return Overlaps(_root, interval);
    }

    private bool Overlaps(IntervalNode node, Interval interval)
    {
        if (node == null)
            return false;

        // 안에 있는 경우
        if (node.Interval.End >= interval.End && node.Interval.Start <= interval.Start)
            return true;

        // 더 큰 범위인 경우
        // node 추가시 자동 병합이기에 범위가 크다 = 속하지않는다와 동일
        else if (node.Interval.End < interval.End && node.Interval.Start > interval.Start)
            return false;

        // 왼쪽에 위치하는 경우
        // 왼쪽 node에 넘겨서 탐색 시작
        else if (node.Interval.Start > interval.End)
            return Overlaps(node.Left, interval);

        // 오른쪽에 위치하는 경우
        // 오른쪽 node에 넘겨서 탐색 시작
        else if (node.Interval.End < interval.Start)
            return Overlaps(node.Right, interval);

        // 그 외는 전부 속하지 않는걸로 판별
        return false;
    }

    // 0 -> 없음
    // 1 -> 곂침
    // 2 -> 넘어감
    private OverLap LeftCheck(IntervalNode node, Interval interval)
    {
        if (node == null)
            return OverLap.None;

        // 넘어가는 경우
        if (interval.Start <= node.Interval.Start)
            return OverLap.Over;

        // 도달도 못하는 경우
        else if (interval.Start > node.Interval.End)
            return OverLap.None;

        // 곂치는 경우
        else if (interval.Start <= node.Interval.End)
            return OverLap.OverLap;

        return OverLap.None;
    }

    private OverLap RightCheck(IntervalNode node, Interval interval)
    {
        if (node == null)
            return OverLap.None;

        // 넘어가는 경우
        if (interval.End >= node.Interval.End)
            return OverLap.Over;

        // 도달도 못하는 경우
        else if (interval.End < node.Interval.Start)
            return OverLap.None;

        // 곂치는 경우
        else if (interval.End >= node.Interval.Start)
            return OverLap.OverLap;

        return OverLap.None;
    }

    public void PrintInOrder()
    {
        PrintInOrder(_root);
    }

    private void PrintInOrder(IntervalNode node)
    {
        if (node != null)
        {
            PrintInOrder(node.Left);
            Debug.Log($"Interval: [{node.Interval.Start}, {node.Interval.End}]");
            PrintInOrder(node.Right);
        }
    }

    // 모든 노드를 풀로 반환하며 트리를 초기화하는 Clear 메서드 추가
    public void Clear()
    {
        ClearNodes(_root);
        _root = null;
    }

    private void ClearNodes(IntervalNode node)
    {
        if (node == null)
            return;

        ClearNodes(node.Left);
        ClearNodes(node.Right);
        _pool.Release(node);
    }
}
