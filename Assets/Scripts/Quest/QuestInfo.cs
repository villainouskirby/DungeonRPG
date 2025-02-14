using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestInfo : MonoBehaviour
{
    [SerializeField] private bool _isMainQuest;
    [SerializeField] private Mission[] _missions = new Mission[3];

    [Multiline]
    [SerializeField] private string _questDescription;

    [SerializeField] private Item[] _rewards = new Item[3];

    public bool IsQuestCleared => _missions[0].IsMissionCleared && _missions[1].IsMissionCleared && _missions[2].IsMissionCleared;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
