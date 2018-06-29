#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

public class CampaignEditor
{
    public void Update()
    {
        dropdown.Update();
        _campaigns?.Update();
        _campaignSets?.Update();
        _campaignEditor?.Update();
    }

    public void ShowCampaignSets( Vector3 position )
    {
        _campaignSets?.Destroy();
        int count = _editor.campaignData.campaignSets.Count + 1;
        _campaignSets = new Layout( "CampaignSets" , 4 , count , 0.25f , 0.1f , count , container );
        dropdown.AddLayout( _campaignSets );

        List<Button> buttons = new List<Button>( count )
        {
            new Button( "NewCampaignSet" , "New Set" , 4 , 1 , container ,
                Enter: ( Button button ) => button.SetColor( Color.green ) ,
                Stay: ( Button button ) =>
                {
                    if ( Input.GetMouseButtonDown( 0 ) )
                    {
                        HideCampaignSets();
                        ScriptableObjects.Add( ScriptableObject.CreateInstance<CampaignSet>() , _editor.campaignData );
                        ShowCampaignSets( position );
                    }
                } ,
                Exit: ( Button button ) => button.SetColor( Color.white ) )
        };

        for ( int i = 0 ; buttons.Capacity - 1 > i ; i++ )
        {
            int capturedIndex = i;
            buttons.Add( new Button( "CampaignSet" , "Campaign Set" , 4 , 1 , container ,
                Enter: ( Button button ) => button.SetColor( Color.green ) ,
                Stay: ( Button button ) =>
                {
                    if ( Input.GetMouseButtonDown( 0 ) )
                    {
                        selectedCampaignSet = _editor.campaignData.campaignSets[ capturedIndex ];
                        ShowCampaigns( capturedIndex , button.position + new Vector3( button.width * 0.5f , 0 , button.height * 0.5f ) );
                    }
                } ,
                Exit: ( Button button ) => button.SetColor( Color.white ) ) );
        }

        _campaignSets.Add( buttons , true );
        _campaignSets.SetPosition( position + ( Vector3.back * ( _campaignSets.height + dropdown.height ) * 0.5f ) );
    }

    public void HideCampaignSets()
    {
        _campaignSets?.Destroy();
        _campaignSets = null;
    }

    public void ShowCampaigns( int index , Vector3 position )
    {
        _campaigns?.Destroy();
        int count = selectedCampaignSet.campaignDefinitions.Count + 1;
        _campaigns = new Layout( "CampaignLayout" , 4 , count , 0.25f , 0.1f , count , container );
        _campaigns.SetPosition( position + ( Vector3.right * _campaigns.width * 0.5f ) + ( Vector3.back * _campaigns.height * 0.5f ) );
        dropdown.AddLayout( _campaigns );

        List<Button> buttons = new List<Button>( count )
        {
            new Button( "NewCampaign" , "New Campaign" , 4 , 1 , container ,
                Enter: ( Button button ) => button.SetColor( Color.green ) ,
                Stay: ( Button button ) =>
                {
                    if ( Input.GetMouseButtonDown( 0 ) )
                    {
                        HideCampaigns();
                        CampaignDefinition campaignDefinition = ScriptableObject.CreateInstance<CampaignDefinition>();
                        campaignDefinition.Initialize( 20 , 15 , 5 , 5 );
                        ScriptableObjects.Add( campaignDefinition , selectedCampaignSet );
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

                        Hide();
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

        _campaignEditor = new Layout( "CampaignEditor" , 3 , 4 , 0.25f , 0.1f , campaignEditorButtons.Count / 2 , container );
        _campaignEditor.Add( campaignEditorButtons , true );
        _campaignEditor.SetParent( container );
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
        dropdown.RemoveLayouts();
    }

    public void Refresh()
    {
        _campaigns?.Refresh();
        _campaignSets?.Refresh();
        _campaignEditor?.Refresh();
    }

    public CampaignDefinition selectedCampaign { get; private set; }
    public CampaignSet selectedCampaignSet { get; private set; }
    public GameObject container { get; }
    public Dropdown dropdown { get; }

    private NeoEditor _editor { get; }
    private Layout _campaigns { get; set; }
    private Layout _campaignSets { get; set; }
    private Layout _campaignEditor { get; set; }

    public CampaignEditor( NeoEditor editor , Vector3 position )
    {
        _editor = editor;
        container = new GameObject( typeof( CampaignEditor ).Name );
        container.transform.SetParent( editor.container.transform );
        dropdown = new Dropdown( "Campaigns" , "Campaigns" , 4 , 1 , container ,
            Enter: ( Button button ) => button.SetColor( Color.green ) ,
            Stay: ( Button button ) => 
            {
                if ( Input.GetMouseButtonDown( 0 ) )
                    ShowCampaignSets( position );
            } ,
            Exit: ( Button button ) => button.SetColor( Color.white ) , 
            Close: ( Button button ) => 
            {
                if ( Input.GetMouseButtonDown( 0 ) || Input.GetMouseButtonDown( 1 ) )
                {
                    HideCampaigns();
                    HideCampaignSets();
                }
            } );

        dropdown.SetPosition( position );
    }
}
#endif //UNITY_EDITOR