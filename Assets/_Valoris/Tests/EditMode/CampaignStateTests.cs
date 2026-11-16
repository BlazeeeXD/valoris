using NUnit.Framework;

public class CampaignStateTests
{
    [Test]
    public void ResultReleasesBoardLockForNextLevel()
    {
        var state = new CampaignState();
        state.BeginCampaign(123, 15);
        Assert.That(state.LockBoard(), Is.True);
        state.RecordLevelResult(aiReachedGoal: false);

        Assert.That(state.IsBoardLocked, Is.False);
        Assert.That(state.CurrentLevel, Is.EqualTo(2));
        Assert.That(state.PlayerScore, Is.EqualTo(1));
        Assert.That(state.LockBoard(), Is.True);
    }

    [Test]
    public void CampaignEndsAtEightWins()
    {
        var state = new CampaignState();
        state.BeginCampaign(123, 15);
        for (int i = 0; i < CampaignRules.WinsRequired; i++)
        {
            Assert.That(state.LockBoard(), Is.True);
            state.RecordLevelResult(aiReachedGoal: true);
        }
        Assert.That(state.IsComplete, Is.True);
        Assert.That(state.AiScore, Is.EqualTo(8));
        Assert.That(state.CurrentLevel, Is.EqualTo(8));
        Assert.That(state.LockBoard(), Is.False);
    }

    [Test]
    public void CampaignAlsoEndsAtLevelLimit()
    {
        var state = new CampaignState();
        state.BeginCampaign(123, 2);
        for (int i = 0; i < 2; i++)
        {
            Assert.That(state.LockBoard(), Is.True);
            state.RecordLevelResult(aiReachedGoal: false);
        }
        Assert.That(state.IsComplete, Is.True);
        Assert.That(state.CompletedLevels, Is.EqualTo(2));
    }
}
