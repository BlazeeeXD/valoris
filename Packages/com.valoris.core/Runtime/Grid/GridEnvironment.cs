
using System;
using Valoris.Core.Contracts;

namespace Valoris.Core.Grid
{
    /// <summary>
    /// Deterministic, headless environment.
    /// Supports movement and one Wall Breaker use per episode.
    /// </summary>
    public sealed class GridEnvironment
    {
        private LevelDefinition _level;
        private RewardModel _rewards;

        private int[] _cells;
        private bool[] _playerPlacedWalls;

        private GridPosition _position;
        private bool _isTerminal;
        private bool _wallBreakerAvailable;

        private TerminationReason _terminationReason;
        private int _steps;

        public GridPosition Position => _position;
        public int StepsTaken => _steps;
        public bool IsTerminal => _isTerminal;
        public bool WallBreakerAvailable => _wallBreakerAvailable;

        public TerminationReason TerminationReason => _terminationReason;
        public LevelDefinition Level => _level;

        public GridEnvironment(RewardModel rewards = null)
        {
            _rewards = rewards ?? new RewardModel();
        }

        public void Reset(LevelDefinition level)
        {
            if (level == null)
                throw new ArgumentNullException(nameof(level));

            LevelValidationResult validation =
                new LevelValidator().Validate(level);

            if (!validation.IsValid)
            {
                throw new ArgumentException(
                    "Invalid level: " +
                    string.Join("; ", validation.Messages),
                    nameof(level));
            }

            _level = level;

            ResetEpisode();
        }

        /// <summary>
        /// Restores the original board and Wall Breaker charge
        /// for a fresh training episode.
        /// </summary>
        public void ResetEpisode()
        {
            EnsureInitialized();

            _cells = _level.CopyCells();
            _playerPlacedWalls = _level.CopyPlayerPlacedWalls();

            _position = _level.Start;

            _isTerminal = false;
            _wallBreakerAvailable = true;

            _terminationReason = TerminationReason.None;
            _steps = 0;
        }

        public GridObservation Observe()
        {
            EnsureInitialized();

            return new GridObservation(
                _position,
                _level.Goal,

                GetNeighbourCell(MoveAction.Up),
                GetNeighbourCell(MoveAction.Right),
                GetNeighbourCell(MoveAction.Down),
                GetNeighbourCell(MoveAction.Left),

                IsPlayerWallInDirection(MoveAction.Up),
                IsPlayerWallInDirection(MoveAction.Right),
                IsPlayerWallInDirection(MoveAction.Down),
                IsPlayerWallInDirection(MoveAction.Left),

                _wallBreakerAvailable
            );
        }

        public StepResult Step(MoveAction action)
        {
            EnsureInitialized();

            if (_isTerminal)
            {
                throw new InvalidOperationException(
                    "Episode has terminated. Call ResetEpisode before stepping again.");
            }

            if ((byte)action >= 8)
                throw new ArgumentOutOfRangeException(nameof(action));

            _steps++;

            double reward = _rewards.StepPenalty;

            // Wall Breaker actions do not move the agent.
            if (IsBreakAction(action))
            {
                return BreakWall(action, reward);
            }

            GridPosition target = GridRules.Apply(_position, action);

            if (!GridRules.IsInside(
                target.X,
                target.Y,
                _level.Width,
                _level.Height))
            {
                reward += _rewards.WallPenalty;

                return CreateStepResult(
                    reward,
                    false,
                    TerminationReason.None);
            }

            CellType destination = GetCell(target.X, target.Y);

            if (destination == CellType.Wall)
            {
                reward += _rewards.WallPenalty;

                return CreateStepResult(
                    reward,
                    false,
                    TerminationReason.None);
            }

            _position = target;

            if (destination == CellType.Trap)
            {
                reward += _rewards.TrapPenalty;

                _isTerminal = true;
                _terminationReason = TerminationReason.EnteredTrap;

                return CreateStepResult(
                    reward,
                    true,
                    _terminationReason);
            }

            if (_position == _level.Goal)
            {
                reward += _rewards.GoalReward;

                _isTerminal = true;
                _terminationReason = TerminationReason.ReachedGoal;

                return CreateStepResult(
                    reward,
                    true,
                    _terminationReason);
            }

            if (destination == CellType.Mud)
                reward += _rewards.MudPenalty;

            return CreateStepResult(
                reward,
                false,
                TerminationReason.None);
        }

        private StepResult BreakWall(
            MoveAction action,
            double reward)
        {
            if (!_wallBreakerAvailable)
            {
                throw new InvalidOperationException(
                    "The Wall Breaker has already been used.");
            }

            GridPosition target = GetTargetPosition(action);

            if (!GridRules.IsInside(
                target.X,
                target.Y,
                _level.Width,
                _level.Height))
            {
                throw new InvalidOperationException(
                    "Cannot break a wall outside the level.");
            }

            int index = target.Y * _level.Width + target.X;

            if (!_playerPlacedWalls[index] ||
                _cells[index] != (int)CellType.Wall)
            {
                throw new InvalidOperationException(
                    "The Wall Breaker can only destroy player placed walls.");
            }

            // Remove the wall from the episode's mutable board.
            _cells[index] = (int)CellType.Empty;
            _playerPlacedWalls[index] = false;

            // All directional breaking actions share one charge.
            _wallBreakerAvailable = false;

            return CreateStepResult(
                reward,
                false,
                TerminationReason.None);
        }

        private GridPosition GetTargetPosition(MoveAction action)
        {
            switch (action)
            {
                case MoveAction.BreakUp:
                    return new GridPosition(_position.X, _position.Y - 1);

                case MoveAction.BreakRight:
                    return new GridPosition(_position.X + 1, _position.Y);

                case MoveAction.BreakDown:
                    return new GridPosition(_position.X, _position.Y + 1);

                case MoveAction.BreakLeft:
                    return new GridPosition(_position.X - 1, _position.Y);

                default:
                    throw new ArgumentOutOfRangeException(nameof(action));
            }
        }

        private bool IsBreakAction(MoveAction action)
        {
            return action == MoveAction.BreakUp ||
                   action == MoveAction.BreakRight ||
                   action == MoveAction.BreakDown ||
                   action == MoveAction.BreakLeft;
        }

        private CellType GetNeighbourCell(MoveAction direction)
        {
            GridPosition next = GridRules.Apply(_position, direction);

            if (!GridRules.IsInside(
                next.X,
                next.Y,
                _level.Width,
                _level.Height))
            {
                return CellType.Wall;
            }

            return GetCell(next.X, next.Y);
        }

        private bool IsPlayerWallInDirection(MoveAction direction)
        {
            GridPosition next = GridRules.Apply(_position, direction);

            if (!GridRules.IsInside(
                next.X,
                next.Y,
                _level.Width,
                _level.Height))
            {
                return false;
            }

            int index = next.Y * _level.Width + next.X;

            return _playerPlacedWalls[index] &&
                   _cells[index] == (int)CellType.Wall;
        }

        private CellType GetCell(int x, int y)
        {
            int index = y * _level.Width + x;
            return (CellType)_cells[index];
        }

        private StepResult CreateStepResult(
            double reward,
            bool terminal,
            TerminationReason reason)
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
            if (_level == null)
            {
                throw new InvalidOperationException(
                    "Environment has not been reset with a level.");
            }
        }
    }
}
