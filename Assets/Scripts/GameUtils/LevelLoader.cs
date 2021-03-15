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

        private bool isLoadingNextLevel;
        private bool isLoadingCurrentLevel;
        private const float AnimationDuration = 0.5f;

        private Animator animator;

        public void Start()
        {
            signalBus.Subscribe<LevelCompletedSignal>(OnLevelComplete);
            signalBus.Subscribe<LevelFailedSignal>(OnLevelFail);
            signalBus.Subscribe<GameLaunchedSignal>(OnGameLaunch);
            signalBus.Subscribe<PlayButtonClickedSignal>(OnPlayButtonClick);
        }

        private void OnLevelComplete()
        {
            StartCoroutine(LoadNextLevel());
        }

        private void OnLevelFail()
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

        private IEnumerator LoadNextLevel()
        {
            if (isLoadingNextLevel) yield break;
            isLoadingNextLevel = true;
        
            if (!isLoadingCurrentLevel) yield return PlayEndAnimation();
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
        
        private IEnumerator LoadLevel(Level level)
        {
            if (transform.childCount != 0) DestroyCurrentLevel();
            yield return null;
        
            container.InstantiatePrefab(level.levelPrefab, transform);
            levelState.Initialize(level);
            isLoadingNextLevel = false;
            isLoadingCurrentLevel = false;
        }
        
        private void DestroyCurrentLevel()
        {
            var currentLevelGameObject = transform.GetChild(0).gameObject;
            Destroy(currentLevelGameObject);
        }
        
        private IEnumerator LoadCurrentLevel()
        {
            if (isLoadingCurrentLevel) yield break;
            isLoadingCurrentLevel = true;
        
            yield return PlayEndAnimation();
            if (isLoadingNextLevel) yield break;
        
            var currentLevel = levelProvider.ProvideCurrentLevel();
            StartCoroutine(LoadLevel(currentLevel));
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
    }
}
