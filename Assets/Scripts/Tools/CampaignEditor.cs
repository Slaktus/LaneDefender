#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using System;

public class CampaignEditor
{
    public void Update()
    {
        _campaigns?.Update();
        campaignSets?.Update();
        _campaignEditor?.Update();
    }

    public void ShowCampaignSets()
    {
        HideCampaignSets();
        int count = _editor.campaignData.campaignSets.Count;
        campaignSets = new Layout("CampaignSets", 4, count + 1, 0.25f, 0.1f, count + 1, container);

        campaignSets.Add(new List<Button>(
            Button.GetButtons(count,
            (int index) =>
            {
                return new Button("CampaignSet", "Campaign Set", 4, 1, container,
                Enter: (Button button) => button.SetColor(_campaigns != null && selectedCampaignSet == _editor.campaignData.campaignSets[ index ] ? button.color : Color.green),
                Stay: (Button button) =>
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        selectedCampaignSet = _editor.campaignData.campaignSets[ index ];
                        ShowCampaigns(index, button.position + new Vector3(button.width * 0.5f, 0, button.height * 0.5f));
                        button.SetColor(Color.yellow);
                        button.Select();
                    }
                },
                Exit: (Button button) => button.SetColor(_campaigns != null && selectedCampaignSet == _editor.campaignData.campaignSets[ index ] ? button.color : Color.white),
                Close: (Button button) =>
                {
                    if (button.selected && Input.GetMouseButtonDown(0) && (_campaigns == null || !_campaigns.containsMouse))
                    {
                        HideCampaigns();
                        button.Deselect();
                        selectedCampaignSet = null;
                        button.SetColor(Color.white);
                    }
                });
            }))
        {
            new Button("NewCampaignSet", "New Set", 4, 1, container,
                Enter: (Button button) => button.SetColor(Color.green),
                Stay: (Button button) =>
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        HideCampaigns();
                        ScriptableObjects.Add(ScriptableObject.CreateInstance<CampaignSet>(), _editor.campaignData);
                        ShowCampaignSets();

                        if (selectedCampaign != null)
                            ShowCampaignEditor();
                    }
                },
                Exit: (Button button) => button.SetColor(Color.white))
        }, true);

        campaignSets.SetViewportPosition( new Vector2( 0 , 1 ) );
        campaignSets.SetPosition( campaignSets.position + Vector3.up );
    }

    public void HideCampaignSets()
    {
        campaignSets?.Destroy();
        campaignSets = null;
    }

    public void ShowCampaigns( int index , Vector3 position )
    {
        HideCampaigns();
        int count = selectedCampaignSet.campaignDefinitions.Count + 1;
        _campaigns = new Layout( "CampaignLayout" , 4 , count , 0.25f , 0.1f , count , container );
        _campaigns.SetPosition( position + ( Vector3.right * _campaigns.width * 0.5f ) + ( Vector3.back * _campaigns.height * 0.5f ) );

        List<Button> buttons = new List<Button>( count )
        {
            new Button( "NewCampaign" , "New Campaign" , 4 , 1 , container ,
                Enter: ( Button button ) => button.SetColor( Color.green ) ,
                Stay: ( Button button ) =>
                {
                    if ( Input.GetMouseButtonDown( 0 ) )
                    {
                        ScriptableObjects.Add( CampaignDefinition.Default() , selectedCampaignSet );
                        ShowCampaigns( index , position );
                    }
                } ,
                Exit: ( Button button ) => button.SetColor( Color.white ) )
        };

        for ( int i = 0 ; buttons.Capacity - 1 > i ; i++ )
        {
            int capturedIndex = i;
            buttons.Add( new Button( "Campaign" , "Campaign" , 4 , 1 , container ,
                Enter: ( Button button ) => button.SetColor( Color.green ) ,
                Stay: ( Button button ) =>
                {
                    if ( Input.GetMouseButtonDown( 0 ) )
                    {
                        selectedCampaign = selectedCampaignSet.campaignDefinitions[ capturedIndex ];

                        HideCampaigns();
                        HideCampaignSets();
                        ShowCampaignSets();
                        ShowCampaignEditor();
                        _editor.ShowCampaignMap();
                    }
                } ,
                Exit: ( Button button ) => button.SetColor( Color.white ) ) );
        }

        _campaigns.Add( buttons , true );
    }

    public void ShowCampaignEditor()
    {
        _campaignEditor?.Destroy();

        if ( selectedCampaign != null )
        {
            List<Element> campaignEditorButtons = new List<Element>()
            {
                new Label( "Width:" , Color.black , 1.25f , 0.5f , container , fontSize: 20 , anchor: TextAnchor.MiddleCenter ) ,
                new Field( "Width" , selectedCampaign.width.ToString() , 2 , 0.5f , 20 , container , Field.ContentMode.Numbers  , EndInput: ( Field field ) =>
                {
                    float.TryParse( field.label.text , out selectedCampaign.width );
                    _editor.Refresh();
                    Refresh();
                } ) ,

                new Label( "Height:" , Color.black , 1.25f , 0.5f , container , fontSize: 20 , anchor: TextAnchor.MiddleCenter ) ,
                new Field( "Height" , selectedCampaign.height.ToString() , 2 , 0.5f , 20 , container , Field.ContentMode.Numbers  , EndInput: ( Field field ) =>
                {
                    float.TryParse( field.label.text , out selectedCampaign.height );
                    _editor.Refresh();
                    Refresh();
                } ) ,

                new Label( "Rows:" , Color.black , 1.25f , 0.5f , container , fontSize: 20 , anchor: TextAnchor.MiddleCenter ) ,
                new Field( "Rows" , selectedCampaign.rows.ToString() , 2 , 0.5f , 20 , container , Field.ContentMode.Numbers  , EndInput: ( Field field ) =>
                {
                    int.TryParse( field.label.text , out selectedCampaign.rows );
                    _editor.Refresh();
                    Refresh();
                } ) ,

                new Label( "Columns:" , Color.black , 1.25f , 0.5f , container , fontSize: 20 , anchor: TextAnchor.MiddleCenter ) ,
                new Field( "Columns" , selectedCampaign.columns.ToString() , 2 , 0.5f , 20 , container , Field.ContentMode.Numbers  , EndInput: ( Field field ) =>
                {
                    int.TryParse( field.label.text , out selectedCampaign.columns );
                    _editor.Refresh();
                    Refresh();
                } )
            };

            _campaignEditor = new Layout( "CampaignEditor" , 4 , 4 , 0.25f , 0.1f , campaignEditorButtons.Count / 2 , container );
            _campaignEditor.Add( campaignEditorButtons , true );
            _campaignEditor.SetParent( container );
            _campaignEditor.SetPosition( campaignSets.position + ( Vector3.back * ( ( campaignSets.height + _campaignEditor.height ) * 0.5f ) ) );
        }
    }

    public void HideCampaignEditor()
    {
        _campaignEditor?.Destroy();
        _campaignEditor = null;
    }

    public void HideCampaigns()
    {
        _campaigns?.Destroy();
        _campaigns = null;
    }

    public void Hide()
    {
        HideCampaigns();
        HideCampaignSets();
        HideCampaignEditor();
    }

    public void Refresh()
    {
        _campaigns?.Refresh();
        campaignSets?.Refresh();
        _campaignEditor?.Refresh();
    }

    public Layout campaignSets { get; set; }

    public CampaignDefinition selectedCampaign { get; private set; }
    public CampaignSet selectedCampaignSet { get; private set; }
    public GameObject container { get; }

    private Editor _editor { get; }
    private Layout _campaigns { get; set; }
    private Layout _campaignEditor { get; set; }

    public CampaignEditor( Editor editor , Vector3 position )
    {
        _editor = editor;
        container = new GameObject( typeof( CampaignEditor ).Name );
        container.transform.SetParent( editor.container.transform );
    }
}
#endif //UNITY_EDITOR