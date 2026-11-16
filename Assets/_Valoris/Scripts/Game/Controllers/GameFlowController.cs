using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;
using Valoris.Core.Contracts;
using Valoris.Core.Grid;

/// <summary>Coordinates map lock, validation, headless training, replay and campaign scoring.</summary>
[DefaultExecutionOrder(100)]
public class GameFlowController : MonoBehaviour
{
    [Header("Training")]
    [SerializeField, Min(1)] private int episodesPerLevel = 3000;
    [SerializeField, Min(1)] private int maxStepsPerEpisode = 300;
    [SerializeField, Min(1)] private int evaluationMaxSteps = 300;
    [SerializeField] private int campaignSeed = 12345;
    [SerializeField, Range(0.01f, 1f)] private float replayStepDelay = 0.075f;

    private ProceduralLevelGenerator _generator;
    private InventoryManager _inventory;
    private GridManager _grid;
    private PlacementController _placement;
    private CampaignManager _campaign;
    private CampaignIntegration _integration;
    private GameSession _session = new GameSession();
    private CampaignHUDController _hud;
    private TrainingUIController _trainingUi;
    private ResultsUIController _resultsUi;
    private InventoryUIController _inventoryUi;
    private AgentPathAnimator _pathAnimator;
    private bool _initialized;

    public bool IsTraining => _session.IsTraining;
    public GameSession Session => _session;

    private void Start()
    {
        Initialize();
        if (_generator != null && _generator.CurrentLevel != null)
            _session.BeginLevel(_generator.CurrentLevel);
        RefreshHud();
    }

    private void Update()
    {
        if (!_initialized || Keyboard.current == null || _session.IsTraining) return;
        if (Keyboard.current.nKey.wasPressedThisFrame) StartNextLevel();
        if (Keyboard.current.rKey.wasPressedThisFrame && _campaign != null && _campaign.State.IsComplete)
            StartNewCampaign();
    }

    private void Initialize()
    {
        if (_initialized) return;
        _placement = FindObjectOfType<PlacementController>();
        _generator = _placement != null ? _placement.levelGenerator : FindObjectOfType<ProceduralLevelGenerator>();
        _inventory = _placement != null ? _placement.inventoryManager : FindObjectOfType<InventoryManager>();
        _grid = _placement != null ? _placement.gridManager : FindObjectOfType<GridManager>();

        if (_generator == null || _inventory == null || _grid == null || _placement == null)
        {
            Debug.LogError("GameFlowController: scene is missing ProceduralLevelGenerator, InventoryManager, GridManager or PlacementController.");
            return;
        }

        _campaign = GetComponent<CampaignManager>();
        if (_campaign == null) _campaign = gameObject.AddComponent<CampaignManager>();
        _hud = GetComponent<CampaignHUDController>();
        if (_hud == null) _hud = gameObject.AddComponent<CampaignHUDController>();
        _trainingUi = GetComponent<TrainingUIController>();
        if (_trainingUi == null) _trainingUi = gameObject.AddComponent<TrainingUIController>();
        _resultsUi = GetComponent<ResultsUIController>();
        if (_resultsUi == null) _resultsUi = gameObject.AddComponent<ResultsUIController>();
        _inventoryUi = GetComponent<InventoryUIController>();
        if (_inventoryUi == null) _inventoryUi = gameObject.AddComponent<InventoryUIController>();
        _pathAnimator = GetComponent<AgentPathAnimator>();
        if (_pathAnimator == null) _pathAnimator = gameObject.AddComponent<AgentPathAnimator>();

        var bridge = new ValorisAgentBridge();
        _integration = new CampaignIntegration(bridge);
        _campaign.BeginCampaign(campaignSeed);
        _integration.BeginCampaign(campaignSeed);
        _pathAnimator.Initialize(_grid, replayStepDelay);
        EnsureUiInputActions();
        _initialized = true;

        int firstLevelSeed = _campaign.GetSeedForLevel(1);
        if (_generator.CurrentLevel == null || _generator.CurrentLevel.levelNumber != 1 ||
            _generator.CurrentLevel.seed != firstLevelSeed)
            _generator.GenerateLevel(1, firstLevelSeed);
        if (_generator.CurrentLevel != null)
        {
            _session.BeginLevel(_generator.CurrentLevel);
            _inventory.ResetInventory(_generator.CurrentLevel.seed);
        }

        _hud.Initialize(_campaign, _inventory);
        _trainingUi.Initialize(_hud);
        _resultsUi.Initialize(_hud);
        _inventoryUi.Initialize(_hud, _inventory);
    }

    public bool TryLockAndRun()
    {
        Initialize();
        if (!_initialized || _session.IsTraining) return false;
        if (_campaign.State.IsComplete)
        {
            _hud.SetStatus("Campaign complete. Press R to start another campaign.");
            return false;
        }
        if (_generator.CurrentLevel == null)
        {
            _hud.SetStatus("No generated level is available. Check the Unity Console.");
            return false;
        }

        LevelDefinition snapshot;
        try { snapshot = LevelSnapshotFactory.Create(_generator.CurrentLevel); }
        catch (Exception ex)
        {
            _hud.SetStatus("Cannot lock board: " + ex.Message);
            return false;
        }

        LevelValidationResult validation = new LevelValidator().Validate(snapshot);
        if (!validation.IsValid || !validation.HasSafePath)
        {
            _hud.SetStatus("Invalid board. Remove sabotage until a safe route remains.\n" + string.Join("\n", validation.Messages));
            return false;
        }

        if (!_placement.TrySetLocked(true)) return false;
        if (!_campaign.State.LockBoard())
        {
            _placement.TrySetLocked(false);
            _hud.SetStatus("Campaign state refused the lock transition. Board remains editable.");
            return false;
        }
        _session.Lock(snapshot);
        TrainAndReplayAsync(snapshot);
        return true;
    }

    private async void TrainAndReplayAsync(LevelDefinition snapshot)
    {
        _session.IsTraining = true;
        _trainingUi.ShowTraining(_campaign.State.CurrentLevel, episodesPerLevel);
        _hud.SetStatus("Board locked. AI training is running off the Unity main thread.\nPlease wait for the final evaluation replay.");

        var options = new TrainingOptions
        {
            EpisodesPerLevel = episodesPerLevel,
            MaxStepsPerEpisode = maxStepsPerEpisode,
            EvaluationMaxSteps = evaluationMaxSteps,
            RandomSeed = unchecked(campaignSeed + _campaign.State.CurrentLevel * 31)
        };

        try
        {
            // Core receives only an immutable data snapshot and performs no Unity API calls.
            TrainingResult result = await Task.Run(() => _integration.TrainLockedLevel(snapshot, options));
            if (this == null) return;
            _session.Complete(result);
            _trainingUi.ShowCompleted(result);
            StartCoroutine(TrainingPlayback.Play(_pathAnimator, result.FinalEvaluation,
                () => CompleteLevel(result)));
        }
        catch (Exception ex)
        {
            _session.IsTraining = false;
            _placement.TrySetLocked(false);
            _campaign.State.ResetBoard();
            _hud.SetStatus("Training failed. Board unlocked.\n" + ex.Message);
            Debug.LogException(ex);
        }
    }

    private void CompleteLevel(TrainingResult result)
    {
        bool aiReachedGoal = result != null && result.FinalEvaluation != null && result.FinalEvaluation.ReachedGoal;
        _campaign.RecordLevelResult(aiReachedGoal);
        _session.IsTraining = false;
        _resultsUi.ShowResult(aiReachedGoal, result, _campaign.State);
        _hud.Refresh(_campaign.State, _inventory);
        if (_campaign.State.IsComplete)
            _hud.SetStatus(_campaign.State.AiScore > _campaign.State.PlayerScore
                ? "CAMPAIGN OVER — AI WINS. Press R to start a new campaign."
                : "CAMPAIGN OVER — PLAYER WINS. Press R to start a new campaign.");
        else
            _hud.SetStatus((aiReachedGoal ? "AI reached the exit." : "AI failed; player gets the point.") +
                "\nPress N to generate the next level.\n" + _hud.GetInventoryHelp());
    }

    public void StartNextLevel()
    {
        Initialize();
        if (!_initialized || _session.IsTraining || !_campaign.CanStartNextLevel) return;
        int level = _campaign.State.CurrentLevel;
        int seed = _campaign.GetSeedForLevel(level);
        _placement.ResetForNextLevel();
        _generator.GenerateLevel(level, seed);
        if (_generator.CurrentLevel == null)
        {
            _hud.SetStatus("Next level generation failed. See the Unity Console.");
            return;
        }
        _session.BeginLevel(_generator.CurrentLevel);
        _inventory.ResetInventory(seed);
        _hud.SetStatus("Level " + level + " generated. Place your sabotage and lock the board.\n" + _hud.GetInventoryHelp());
        _hud.Refresh(_campaign.State, _inventory);
        _inventoryUi.Refresh();
    }

    public void StartNewCampaign()
    {
        Initialize();
        if (!_initialized || _session.IsTraining) return;
        campaignSeed = unchecked(campaignSeed + 1);
        _campaign.BeginCampaign(campaignSeed);
        _integration.BeginCampaign(campaignSeed);
        _placement.ResetForNextLevel();
        int firstLevelSeed = _campaign.GetSeedForLevel(1);
        _generator.GenerateLevel(1, firstLevelSeed);
        if (_generator.CurrentLevel != null)
        {
            _session.BeginLevel(_generator.CurrentLevel);
            _inventory.ResetInventory(firstLevelSeed);
        }
        _hud.Refresh(_campaign.State, _inventory);
        _hud.SetStatus("New campaign started. Place sabotage and lock the board.\n" + _hud.GetInventoryHelp());
    }

    private static void EnsureUiInputActions()
    {
        // The uploaded scene referenced an InputActionAsset GUID that was not in the archive.
        // AssignDefaultActions is the supported Input System fallback for this UI module.
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null) return;
        InputSystemUIInputModule module = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (module != null && module.actionsAsset == null)
            module.AssignDefaultActions();
    }

    public void NotifyInventoryChanged() => _inventoryUi?.Refresh();
    private void RefreshHud() { if (_hud != null) _hud.Refresh(_campaign.State, _inventory); }
}
