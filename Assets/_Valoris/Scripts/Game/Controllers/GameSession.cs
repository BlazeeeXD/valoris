
using System;

[Serializable]
public class GameSession
{
    public int currentLevel = 1;

    public bool isCampaignActive = false;
    public bool isCampaignComplete = false;

    public void StartSession()
    {
        currentLevel = 1;

        isCampaignActive = true;
        isCampaignComplete = false;
    }

    public void RecordLevelResult(bool campaignOver)
    {
        if (!isCampaignActive || isCampaignComplete)
            return;

        if (campaignOver)
        {
            isCampaignComplete = true;
            isCampaignActive = false;
        }
        else
        {
            currentLevel++;
        }
    }

    public void EndSession()
    {
        isCampaignActive = false;
        isCampaignComplete = true;
    }
}
