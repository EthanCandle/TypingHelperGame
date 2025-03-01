using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New WordBank", menuName = "Word Bank Type")]
public class WordBank : ScriptableObject
{
    public List<string> words;
}
