using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreen : Layout
{
    public void ShowTitle()
    {
        HideTitle();
        _title = new Layout("Title", 20, 10, 0, 0, 1, container);
        _title.Add(new Label("GAME\nTITLE", Color.black, 10, 5, fontSize: 50));
        _title.SetViewportPosition(new Vector2(0.5f, 0.75f));
        _title.SetPosition(_title.position + (Vector3.left * _title.width * 0.5f) + (Vector3.forward * _title.height * 0.5f));
    }

    public void HideTitle()
    {
        if ( _title != null )
            Remove(_title);

        _title?.Destroy();
        _title = null;
    }

    public void ShowCampaigns()
    {
        HideCampaigns();
        CampaignData campaignData = Assets.Get(Assets.CampaignDataSets.Default);
        int count = 0;

        List<Button> buttons = new List<Button>();

        for ( int i = 0; campaignData.campaignSets.Count > i; i++)
        {
            int iIndex = i;

            for ( int j = 0; campaignData.campaignSets[ i ].campaignDefinitions.Count > j; j++)
            {
                int jIndex = j;

                buttons.Add(new Button("Campaign", 10, 3, container, "CampaignButton",
                    Enter: (Button butt) => butt.SetColor(Color.green),
                    Stay: (Button butt) =>
                    {
                        if (Input.GetMouseButtonDown(0))
                            selectedCampaign = campaignData.campaignSets[ iIndex ].campaignDefinitions[ jIndex ];
                    },
                    Exit: (Button butt) => butt.SetColor(Color.white)));

                count++;
            }
        }

        Add( _campaigns = new Layout("Campaigns", 20, ( count > 1 ? count / 2 : 1 ) * 3 , 0, 0.2f , count > 1 ? count / 2 : 1 , container) );

        _campaigns.Add(buttons, true);
        _campaigns.SetViewportPosition(new Vector2(0.5f, 0.25f));
        _campaigns.SetPosition(_campaigns.position + (Vector3.left * _campaigns.width * 0.5f) + (Vector3.forward * _campaigns.height * 0.5f));
    }

    public void HideCampaigns()
    {
        if (_campaigns != null)
            Remove(_campaigns);

        _campaigns?.Destroy();
        _campaigns = null;
    }

    public CampaignDefinition selectedCampaign { get; private set; }

    private Layout _title;
    private Layout _campaigns;

    public TitleScreen(GameObject parent) : base(typeof(TitleScreen).Name, parent)
    {

    }
}
