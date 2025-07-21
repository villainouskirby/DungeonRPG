using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISave
{
    public void Load(SaveData saveData);
    public void Save(SaveData saveData);
}
