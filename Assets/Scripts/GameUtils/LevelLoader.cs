using System.Collections;
using Levels;
using Signals;
using UnityEngine;
using Zenject;

namespace GameUtils
{
    public class LevelLoader : MonoBehaviour
    {
        [Inject]
        private SignalBus signalBus;

        [Inject]
        private LevelProvider levelProvider;

        [Inject]
        private DiContainer container;

        [Inject]
        private LevelState levelState;

        private bool isNextLevelLoading;
        private bool isCurrentLevelLoading;
        private const float AnimationDuration = 0.5f;

        private Animator animator;

        public void Start()
        {
            signalBus.Subscribe<GameLaunchedSignal>(OnGameLaunch);
            signalBus.Subscribe<LevelFailedSignal>(OnTimeIsUp);
            signalBus.Subscribe<PlayButtonClickedSignal>(OnPlayButtonClick);
            signalBus.Subscribe<LevelCompletedSignal>(OnLevelComplete);
        }

        private void OnLevelComplete()
        {
            StartCoroutine(LoadNextLevel());
        }

        private void OnTimeIsUp()
        {
            StartCoroutine(LoadCurrentLevel());
        }

        private void OnPlayButtonClick()
        {
            StartCoroutine(LoadCurrentLevel());
        }
    
        private void OnGameLaunch()
        {
            var startMiniature = levelProvider.ProvideStartMiniature();
            StartCoroutine(LoadMiniature(startMiniature));
        }

        private IEnumerator LoadLevel(Level level)
        {
            if (transform.childCount != 0) DestroyCurrentLevel();
            yield return null;
        
            container.InstantiatePrefab(level.levelPrefab, transform);
            levelState.Initialize(level);
            isNextLevelLoading = false;
            isCurrentLevelLoading = false;
        }

        private IEnumerator LoadMiniature(GameObject miniature)
        {
            levelState.Reset();
            if (transform.childCount != 0)
            {
                yield return PlayEndAnimation();
                DestroyCurrentLevel();
                yield return null;
            }
            container.InstantiatePrefab(miniature, transform);
        }
        private void DestroyCurrentLevel()
        {
            var currentLevelGameObject = transform.GetChild(0).gameObject;
            Destroy(currentLevelGameObject);
        }

        private IEnumerator LoadCurrentLevel()
        {
            if (isCurrentLevelLoading) yield break;
            isCurrentLevelLoading = true;
        
            yield return PlayEndAnimation();
            if (isNextLevelLoading) yield break;
        
            var currentLevel = levelProvider.ProvideCurrentLevel();
            StartCoroutine(LoadLevel(currentLevel));
        }

        private IEnumerator LoadNextLevel()
        {
            if (isNextLevelLoading) yield break;
            isNextLevelLoading = true;
        
            if (!isCurrentLevelLoading) yield return PlayEndAnimation();
            else yield return new WaitForSeconds(AnimationDuration);
        
            var nextLevel = levelProvider.ProvideNextLevel();
            StartCoroutine(LoadLevel(nextLevel));
        }
    
        private IEnumerator PlayEndAnimation()
        {
            animator = FindObjectOfType<Animator>();
            animator.Play("OnLevelEndAnimation");
            yield return new WaitForSeconds(AnimationDuration);
        }
    }
}
