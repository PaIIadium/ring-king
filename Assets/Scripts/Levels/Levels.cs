using System;
using System.Collections.Generic;
using UnityEngine;

namespace Levels
{
    [Serializable]
    public struct Level
    {
        public GameObject levelPrefab;
        public float ballSpeed;
        public float time;
    }

    [CreateAssetMenu(fileName = "LevelsData", menuName = "Levels")]
    public class Levels : ScriptableObject
    {
        public List<Level> levels;
    }
}