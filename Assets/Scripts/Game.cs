using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    public static Game instance;

    [Header("Prefabs")]
    public List<TextAsset> AvailableLevelFiles;
    public List<GameObject> AvailableBackgrounds;

    [Header("UI")]
    public LevelUIHandler LevelUI;
    
    [Header("Block System")]
    public BlockSystem blockSystem;
    public bool IsGameOver {get; private set;} = false;

    [Header("Events")]
    public UnityEvent<LevelStructure> LevelStartEvent;
    public UnityEvent LevelGameOverEvent;
    public UnityEvent EndlessStartEvent;
    public UnityEvent EndlessGameOverEvent;

    void Awake()
    {
        if (instance == null)
            instance = this;


    }

    // Start is called before the first frame update
    void Start()
    {

        // Get the name of the currently loaded scene
        string sceneName = SceneManager.GetActiveScene().name;

        // Check the scene name and apply different behavior
        if (sceneName == "level1")
        {
            GameState.CurrentLevelID = 0;
        }
        else if (sceneName == "level2")
        {
            GameState.CurrentLevelID = 1;
        }
        else if (sceneName == "level3")
        {
            GameState.CurrentLevelID = 2;
        }

/*        if (GameState.CurrentLevelID == 1 || GameState.CurrentLevelID == 2)
        {
            Debug.Log("test");
            GameState.NoFail = true;
        }
        else
        {
            GameState.NoFail = false;
        }*/

        if (GameState.IsEndless) StartEndless();
        else StartLevel();
    }

    public void OnPieceSpawnedFirstPlay(int spawnedBlockCounter)
    {

    }

    private void StartEndless()
    {
        // Set default background
        AvailableBackgrounds.ForEach(x => x.SetActive(false));
        AvailableBackgrounds[1].SetActive(true);

        // Let other systems know endless mode has been started
        EndlessStartEvent.Invoke();
    }

    private void StartLevel()
    {
        // Get the level structure from the level file
        LevelStructure structure = LevelStructure.GetLevelStructureFromAsset(AvailableLevelFiles[GameState.CurrentLevelID]);
            
        // Turn all backgrounds off, only enable correct one
        AvailableBackgrounds.ForEach(x => x.SetActive(false));
        AvailableBackgrounds[structure.BackgroundID].SetActive(true);

        // Let other systems know a level has been started
        LevelStartEvent.Invoke(structure);
    } 

    public void LevelGameOver()
    {
        IsGameOver = true;
        LevelGameOverEvent.Invoke();
    }

    private void EndlessGameOver()
    {
        IsGameOver = true;        
        EndlessGameOverEvent.Invoke();
    }

    public void BlockHitFloor()
    {
        if (!blockSystem.HasMultipleLandedBlocks)
        {
            return;
        }

        if(!IsGameOver && blockSystem.HasFirstBlockLanded)
        {
            if(GameState.IsEndless) EndlessGameOver();
            else LevelGameOver();
        }
    }

    public void BlockHitFakeFloor()
    {
        if (!blockSystem.HasMultipleLandedBlocks)
        {
            return;
        }
    }
}
