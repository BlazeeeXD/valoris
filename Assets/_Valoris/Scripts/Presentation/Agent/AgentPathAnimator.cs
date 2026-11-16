
using System.Collections;
using UnityEngine;
using Valoris.Core.Contracts;

[RequireComponent(typeof(AgentRenderer))]
public class AgentPathAnimator : MonoBehaviour
{
    [SerializeField, Range(0.01f, 1f)]
    private float stepDelay = 0.075f;

    private AgentRenderer _renderer;
    private GridManager _grid;

    public void Initialize(GridManager grid, float delay)
    {
        stepDelay = Mathf.Clamp(delay, 0.01f, 1f);

        _grid = grid;

        _renderer = GetComponent<AgentRenderer>();

        if (_renderer == null)
            _renderer = gameObject.AddComponent<AgentRenderer>();

        _renderer.Initialize(grid);
    }

    public IEnumerator PlayEvaluation(EvaluationResult evaluation)
    {
        if (_renderer == null ||
            evaluation == null ||
            evaluation.Path == null ||
            evaluation.Path.Count == 0)
        {
            yield break;
        }

        AgentVisualState finalState = evaluation.ReachedGoal
            ? AgentVisualState.Success
            : AgentVisualState.Failure;

        // Show the starting position.
        _renderer.ShowAt(
            evaluation.Path[0],
            AgentVisualState.Moving
        );

        // Replay each recorded transition.
        for (int i = 0; i < evaluation.Path.Count - 1; i++)
        {
            if (evaluation.Actions != null &&
                i < evaluation.Actions.Count)
            {
                MoveAction action = evaluation.Actions[i];

                if (IsWallBreakAction(action))
                {
                    ReplayWallBreak(action, evaluation.Path[i]);
                }
            }

            GridPosition nextPosition = evaluation.Path[i + 1];

            bool isFinalPosition =
                i == evaluation.Path.Count - 2;

            _renderer.ShowAt(
                nextPosition,
                isFinalPosition
                    ? finalState
                    : AgentVisualState.Moving
            );

            yield return new WaitForSeconds(stepDelay);
        }

        // Handle evaluations with only a starting position.
        if (evaluation.Path.Count == 1)
        {
            _renderer.ShowAt(
                evaluation.Path[0],
                finalState
            );
        }

        yield return new WaitForSeconds(
            Mathf.Max(stepDelay, 0.35f)
        );

        _renderer.Hide();
    }

    private void ReplayWallBreak(
        MoveAction action,
        GridPosition currentPosition)
    {
        if (_grid == null)
            return;

        Vector2Int direction;

        switch (action)
        {
            case MoveAction.BreakUp:
                direction = Vector2Int.up;
                break;

            case MoveAction.BreakRight:
                direction = Vector2Int.right;
                break;

            case MoveAction.BreakDown:
                direction = Vector2Int.down;
                break;

            case MoveAction.BreakLeft:
                direction = Vector2Int.left;
                break;

            default:
                return;
        }

        Vector2Int wallPosition =
            new Vector2Int(
                currentPosition.X,
                currentPosition.Y
            ) + direction;

        _grid.ClearItem(wallPosition);
    }

    private bool IsWallBreakAction(MoveAction action)
    {
        return action == MoveAction.BreakUp ||
               action == MoveAction.BreakRight ||
               action == MoveAction.BreakDown ||
               action == MoveAction.BreakLeft;
    }
}
