using System.Collections;
using UnityEngine;
using Valoris.Core.Contracts;

[RequireComponent(typeof(AgentRenderer))]
public class AgentPathAnimator : MonoBehaviour
{
    [SerializeField, Range(0.01f, 1f)] private float stepDelay = 0.075f;
    private AgentRenderer _renderer;

    public void Initialize(GridManager grid, float delay)
    {
        stepDelay = Mathf.Clamp(delay, 0.01f, 1f);
        _renderer = GetComponent<AgentRenderer>();
        if (_renderer == null) _renderer = gameObject.AddComponent<AgentRenderer>();
        _renderer.Initialize(grid);
    }

    public IEnumerator PlayEvaluation(EvaluationResult evaluation)
    {
        if (_renderer == null || evaluation == null || evaluation.Path == null || evaluation.Path.Count == 0)
            yield break;

        AgentVisualState finalState = evaluation.ReachedGoal
            ? AgentVisualState.Success
            : AgentVisualState.Failure;

        for (int i = 0; i < evaluation.Path.Count; i++)
        {
            AgentVisualState state = i == evaluation.Path.Count - 1
                ? finalState
                : AgentVisualState.Moving;
            _renderer.ShowAt(evaluation.Path[i], state);
            if (i + 1 < evaluation.Path.Count) yield return new WaitForSeconds(stepDelay);
        }
        yield return new WaitForSeconds(Mathf.Max(stepDelay, 0.35f));
        _renderer.Hide();
    }
}
