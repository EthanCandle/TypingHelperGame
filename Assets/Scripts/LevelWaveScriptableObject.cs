using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Level", menuName = "Level", order = 1)]
public class LevelWaveScriptableObject : ScriptableObject
{
    public List<WaveScriptableObject> waves; // holds each wave to spawn
}
