using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Wave", menuName = "Wave", order = 2)]
public class WaveScriptableObject : ScriptableObject
{
    public List<int> enemyAmounts = new List<int> { 0, 0, 0 }; // [0],[1],[2] for each lane amount

}
