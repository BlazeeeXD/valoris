using System;
using Valoris.Core.Contracts;

namespace Valoris.Core.Grid
{
    /// <summary>Deterministic, headless environment. One Step call represents one attempted grid move.</summary>
    public sealed class GridEnvironment
    {
        private LevelDefinition _level;
        private RewardModel _rewards;
        private GridPosition _position;
        private bool _isTerminal;
        private TerminationReason _terminationReason;
        private int _steps;

        public GridPosition Position => _position;
        public int StepsTaken => _steps;
        public bool IsTerminal => _isTerminal;
        public TerminationReason TerminationReason => _terminationReason;
        public LevelDefinition Level => _level;

        public GridEnvironment(RewardModel rewards = null)
        {
            _rewards = rewards ?? new RewardModel();
        }

        public void Reset(LevelDefinition level)
        {
            if (level == null) throw new ArgumentNullException(nameof(level));
            LevelValidationResult validation = new LevelValidator().Validate(level);
            if (!validation.IsValid)
                throw new ArgumentException("Invalid level: " + string.Join("; ", validation.Messages), nameof(level));

            // LevelDefinition is immutable and has already copied its source array.
            _level = level;
            ResetEpisode();
        }

        /// <summary>Resets an episode on the already validated level without repeating BFS validation.</summary>
        public void ResetEpisode()
        {
            EnsureInitialized();
            _position = _level.Start;
            _isTerminal = false;
            _terminationReason = TerminationReason.None;
            _steps = 0;
        }

        public GridObservation Observe()
        {
            EnsureInitialized();
            return new GridObservation(
                _position,
                _level.Goal,
                GridRules.GetNeighbourCell(_level, _position, MoveAction.Up),
                GridRules.GetNeighbourCell(_level, _position, MoveAction.Right),
                GridRules.GetNeighbourCell(_level, _position, MoveAction.Down),
                GridRules.GetNeighbourCell(_level, _position, MoveAction.Left));
        }

        public StepResult Step(MoveAction action)
        {
            EnsureInitialized();
            if (_isTerminal) throw new InvalidOperationException("Episode has terminated. Call Reset before stepping again.");
            if ((byte)action > (byte)MoveAction.Left) throw new ArgumentOutOfRangeException(nameof(action));

            _steps++;
            GridPosition target = GridRules.Apply(_position, action);
            double reward = _rewards.StepPenalty;

            if (!GridRules.IsInside(target.X, target.Y, _level.Width, _level.Height))
            {
                reward += _rewards.WallPenalty;
                return CreateStepResult(reward, false, TerminationReason.None);
            }

            CellType destination = _level.GetCell(target.X, target.Y);
            if (destination == CellType.Wall)
            {
                reward += _rewards.WallPenalty;
                return CreateStepResult(reward, false, TerminationReason.None);
            }

            _position = target;
            if (destination == CellType.Trap)
            {
                reward += _rewards.TrapPenalty;
                _isTerminal = true;
                _terminationReason = TerminationReason.EnteredTrap;
                return CreateStepResult(reward, true, _terminationReason);
            }

            if (_position == _level.Goal)
            {
                reward += _rewards.GoalReward;
                _isTerminal = true;
                _terminationReason = TerminationReason.ReachedGoal;
                return CreateStepResult(reward, true, _terminationReason);
            }

            if (destination == CellType.Mud)
                reward += _rewards.MudPenalty;

            return CreateStepResult(reward, false, TerminationReason.None);
        }

        private StepResult CreateStepResult(double reward, bool terminal, TerminationReason reason)
        {
            return new StepResult
            {
                Position = _position,
                Observation = Observe(),
                Reward = reward,
                IsTerminal = terminal,
                TerminationReason = reason
            };
        }

        private void EnsureInitialized()
        {
            if (_level == null) throw new InvalidOperationException("Environment has not been reset with a level.");
        }
    }
}
