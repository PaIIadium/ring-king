using System.IO;
using Levels;
using UnityEngine;

namespace GameUtils
{
    public class LevelProvider
    {
        private Levels.Levels levelsData;
        private GameObject startMiniature;
        private int currentLevel;

        public LevelProvider()
        {
            levelsData = Resources.Load<Levels.Levels>(Path.Combine("Levels", "LevelsData"));
            startMiniature = Resources.Load<GameObject>("Miniature");
        }
        public Level ProvideNextLevel()
        {
            if (currentLevel == levelsData.levels.Count - 1) currentLevel = 0;
            else ++currentLevel;
            return levelsData.levels[currentLevel];
        }

        public Level ProvideCurrentLevel()
        {
            return levelsData.levels[currentLevel]; 
        }

        public GameObject ProvideStartMiniature()
        {
            return startMiniature;
        }
    }
}
    
    
    