using System;
using System.Collections;
using Valoris.Core.Contracts;

/// <summary>Replays only the final greedy evaluation path; training stays headless.</summary>
public static class TrainingPlayback
{
    public static IEnumerator Play(
        AgentPathAnimator animator,
        EvaluationResult evaluation,
        Action onCompleted)
    {
        if (animator != null && evaluation != null && evaluation.Path != null && evaluation.Path.Count > 0)
            yield return animator.PlayEvaluation(evaluation);
        // Always complete the round, even if a malformed/empty evaluation path cannot be animated.
        onCompleted?.Invoke();
    }
}
