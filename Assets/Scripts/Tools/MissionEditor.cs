#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MissionEditor
{
    public void Update()
    {
        missions?.Update();
        missionSets?.Update();
        _missionTimeline?.Update();
        _missionEditorLayout?.Update();
        _indicator.transform.position = _editor.mousePosition;

        //should be replaced with a proper layout
        //layouts can ignore constraints now after all
        for ( int i = 0 ; _buttons.Count > i ; i++ )
            _buttons[ i ].Update();
    }

    public void AddWaveToTimeline( WaveDefinition waveDefinition )
    {
        selectedMission.Add( waveDefinition , timelinePosition );

        Button button = new Button( "Wave" , "Wave" , 2 , 1 , container ,
            Enter: ( Button b ) => b.SetColor( b.selected ? b.color : Color.green ) ,
            Stay: ( Button b ) =>
            {
                if ( Input.GetMouseButtonDown( 1 ) )
                {
                    selectedMission.Remove( waveDefinition );
                    _buttons.Remove( b );
                    b.Destroy();
                }
                else if ( !b.selected && Input.GetMouseButtonDown( 0 ) )
                {
                    for ( int i = 0 ; _buttons.Count > i ; i++ )
                    {
                        _buttons[ i ].Deselect();
                        _buttons[ i ].SetColor( Color.white );
                    }

                    _editor.waveEditor.SetSelectedWaveDefinition( waveDefinition );
                    _editor.waveEditor.HideWaveEventButtons();
                    _editor.waveEditor.ShowWaveEventButtons();
                    b.SetColor( Color.yellow );
                    b.Select();
                }
            } ,
            Exit: ( Button b ) => b.SetColor( b.selected ? b.color : Color.white ) );

        _editor.waveEditor.SetSelectedWaveDefinition( waveDefinition );
        _editor.waveEditor.HideWaveEventButtons();
        _editor.waveEditor.ShowWaveEventButtons();

        for ( int i = 0 ; _buttons.Count > i ; i++ )
        {
            _buttons[ i ].Deselect();
            _buttons[ i ].SetColor( Color.white );
        }

        _buttons.Add( button );

        button.SetPosition( new Vector3( _missionTimeline.rect.xMin + ( timelinePosition * _missionTimeline.rect.xMax ) , 0 , _missionTimeline.rect.yMin + 0.5f ) + Vector3.up );
        button.SetColor( Color.yellow );
        button.Select();
    }

    public void AddWaveToTimeline( WaveDefinition waveDefinition , float timelinePosition )
    {
        Button button = new Button( "Wave" , "Wave" , 2 , 1 , container ,
            Enter: ( Button b ) => b.SetColor( b.selected ? b.color : Color.green ) ,
            Stay: ( Button b ) =>
            {
                if ( Input.GetMouseButtonDown( 1 ) )
                {
                    selectedMission.Remove( waveDefinition );
                    _buttons.Remove( b );
                    b.Destroy();
                }
                else if ( !b.selected && Input.GetMouseButtonDown( 0 ) )
                {
                    for ( int i = 0 ; _buttons.Count > i ; i++ )
                    {
                        _buttons[ i ].Deselect();
                        _buttons[ i ].SetColor( Color.white );
                    }

                    _editor.waveEditor.SetSelectedWaveDefinition( waveDefinition );
                    _editor.waveEditor.HideWaveEventButtons();
                    _editor.waveEditor.ShowWaveEventButtons();
                    b.SetColor( Color.yellow );
                    b.Select();
                }
            } ,
            Exit: ( Button b ) => b.SetColor( b.selected ? b.color : Color.white ) );

        button.SetPosition( new Vector3( _missionTimeline.rect.xMin + ( timelinePosition * _missionTimeline.rect.xMax ) , 0 , _missionTimeline.rect.yMin + 0.5f ) + Vector3.up );
        _buttons.Add( button );
    }

    public void ShowMissionTimeline()
    {
        HideMissionTimeline();

        if ( _editor.stage != null )
        {
            _missionTimeline = new Button( "MissionTimeline" , string.Empty , _editor.stage.width , 1 , container ,
            Enter: ( Button button ) => ShowIndicator() ,
            Stay: ( Button button ) =>
            {
                bool overlappingWave = false;

                for ( int i = 0 ; _buttons.Count > i && !overlappingWave ; i++ )
                    overlappingWave = _buttons[ i ].containsMouse;

                if ( overlappingWave || _editor.waveEditor.waveSets != null )
                    HideIndicator();
                else if ( _editor.waveEditor.waveSets == null)
                {
                    ShowIndicator();

                    if ( Input.GetMouseButtonDown( 0 ) )
                    {
                        timelinePosition = Helpers.Normalize( _indicator.transform.position.x , button.rect.xMax , button.rect.xMin );
                        _editor.waveEditor.Show( new Vector3( _indicator.transform.position.x , 0 , button.rect.yMin ) );
                    }
                }
            } ,
            Exit: ( Button button ) => HideIndicator() );

            _missionTimeline.SetPosition( new Vector3( _editor.stage.start + ( _editor.stage.width * 0.5f ) , 0 , Camera.main.ViewportToWorldPoint( new Vector3( 0 , 1 , Camera.main.transform.position.y ) ).z ) + ( Vector3.back * _missionTimeline.height * 0.5f ) );

            if ( selectedMission != null )
                for ( int i = 0 ; selectedMission.waveDefinitions.Count > i ; i++ )
                    AddWaveToTimeline( selectedMission.waveDefinitions[ i ] , selectedMission.waveTimes[ i ] );
        }
    }

    public void HideMissionTimeline()
    {
        _missionTimeline?.Destroy();
        _missionTimeline = null;
    }

    public void ShowMissionSets( int index , Vector3 position )
    {
        HideMissionSets();
        int count = _editor.campaignData.missionSets.Count + 1;
        missionSets = new Layout( "MissionSets" , 4 , count , 0.25f , 0.1f , count , container );
        missionSets.SetPosition( position + ( Vector3.right * missionSets.width * 0.5f ) + ( Vector3.back * missionSets.height * 0.5f ) );

        List<Button> buttons = new List<Button>( count )
        {
            new Button( "NewMissionSet" , "New Set" , 4 , 1 , container ,
                Enter: ( Button button ) => button.SetColor( Color.green ) ,
                Stay: ( Button button ) =>
                {
                    if ( Input.GetMouseButtonDown( 0 ) )
                    {
                        HideMissions();
                        ScriptableObjects.Add( ScriptableObject.CreateInstance<MissionSet>() , _editor.campaignData );
                        ShowMissionSets( index , position );
                    }
                } ,
                Exit: ( Button button ) => button.SetColor( Color.white ) )
        };

        for ( int i = 0 ; buttons.Capacity - 1 > i ; i++ )
        {
            int capturedIndex = i;
            buttons.Add( new Button( "MissionSet" , "Mission Set" , 4 , 1 , container ,
                Enter: ( Button button ) => button.SetColor( selectedMissionSet == _editor.campaignData.missionSets[ capturedIndex ] ? button.color : Color.green ) ,
                Stay: ( Button button ) =>
                {
                    if ( Input.GetMouseButtonDown( 0 ) )
                    {
                        button.SetColor( Color.yellow );
                        selectedMissionSet = _editor.campaignData.missionSets[ capturedIndex ];
                        ShowMissions( index , button.position + new Vector3( button.width * 0.5f , 0 , button.height * 0.5f ) );
                    }
                } ,
                Exit: ( Button button ) => button.SetColor( selectedMissionSet == _editor.campaignData.missionSets[ capturedIndex ] ? button.color : Color.white ) ,
                Close: ( Button button ) =>
                {
                    if ( Input.GetMouseButtonDown( 0 ) && selectedMissionSet == _editor.campaignData.missionSets[ capturedIndex ] && missions != null && !missions.containsMouse )
                    {
                        HideMissions();
                        selectedMissionSet = null;
                        button.SetColor( Color.white );
                    }
                } ) );
        }

        missionSets.Add( buttons );
        missionSets.Refresh();
    }

    public void HideMissionSets()
    {
        selectedMissionSet = null;
        missionSets?.Destroy();
        missionSets = null;
    }

    public void ShowMissions( int index , Vector3 position )
    {
        HideMissions();
        int count = selectedMissionSet.missionDefinitions.Count + 1;
        missions = new Layout( "MissionLayout" , 4 , count , 0.25f , 0.1f , count , container );
        missions.SetPosition( position + ( Vector3.right * missions.width * 0.5f ) + ( Vector3.back * missions.height * 0.5f ) );

        List<Button> buttons = new List<Button>( count )
        {
            new Button( "NewMission" , "New Mission" , 4 , 1 , container ,
                Enter: ( Button button ) => button.SetColor( Color.green ) ,
                Stay: ( Button button ) =>
                {
                    if ( Input.GetMouseButtonDown( 0 ) )
                    {
                        ScriptableObjects.Add( MissionDefinition.Default() , selectedMissionSet );
                        ShowMissions( index , position );
                    }
                } ,
                Exit: ( Button button ) => button.SetColor( Color.white ) )
        };

        for ( int i = 0 ; buttons.Capacity - 1 > i ; i++ )
        {
            int capturedIndex = i;
            buttons.Add( new Button( "Mission" , "Mission" , 4 , 1 , container ,
                Enter: ( Button button ) => button.SetColor( Color.green ) ,
                Stay: ( Button button ) =>
                {
                    if ( Input.GetMouseButtonDown( 0 ) )
                    {
                        selectedMission = selectedMissionSet.missionDefinitions[ capturedIndex ];
                        _editor.campaignEditor.selectedCampaign.Add( selectedMission , index );
                        Button mapButton = _editor.GetMapButton( index );
                        mapButton.SetLabel( selectedMission.name );
                        mapButton.SetColor( Color.white );
                        mapButton.Deselect();
                        HideMissionSets();
                        HideMissions();
                    }
                } ,
                Exit: ( Button button ) => button.SetColor( Color.white ) ) );
        }

        missions.Add( buttons );
        missions.Refresh();
    }

    public void HideMissions()
    {
        missions?.Destroy();
        missions = null;
    }

    public void ShowMissionEditor()
    {
        HideMissionEditor();
        List<Element> missionEditorButtons = new List<Element>()
        {
            new Label( "Duration:" , Color.black , 1.25f , 0.5f , container , fontSize: 20 , anchor: TextAnchor.MiddleCenter ) ,
            new Field( "Duration" , selectedMission.duration.ToString() , 2 , 0.5f , 20 , container , Field.ContentMode.Numbers  , EndInput: ( Field field ) =>
            {
                float.TryParse( field.label.text , out selectedMission.duration );
            } )
        };

        _missionEditorLayout = new Layout( "StageEditor" , 3 , 1 , 0.25f , 0.1f , missionEditorButtons.Count / 2 , container );
        _missionEditorLayout.Add( missionEditorButtons , true );
        _missionEditorLayout.SetPosition( ( _editor.stageEditor.stageEditorLayout != null ? _editor.stageEditor.stageEditorLayout.position : _editor.stageEditor.stageSets.position ) + ( Vector3.back * ( _missionEditorLayout.height + ( _editor.stageEditor.stageEditorLayout != null ? _editor.stageEditor.stageEditorLayout.height : _editor.stageEditor.stageSets.height ) ) * 0.5f ) );
    }

    public void HideMissionEditor()
    {
        _missionEditorLayout?.Destroy();
        _missionEditorLayout = null;
    }

    public void Hide()
    {
        HideMissions();
        HideMissionSets();
        HideIndicator();
    }

    public void SetSelectedMission( MissionDefinition selectedMission ) => this.selectedMission = selectedMission;

    private void ShowIndicator() => _indicator.enabled = true;
    private void HideIndicator() => _indicator.enabled = false;

    public MissionSet selectedMissionSet { get; private set; }
    public MissionDefinition selectedMission { get; private set; }
    public float timelinePosition { get; private set; }
    public Layout missionSets { get; private set; }
    public Layout missions { get; private set; }
    public GameObject container { get; }

    private Editor _editor { get; }
    private MeshRenderer _indicator { get; }
    private List<Button> _buttons { get; set; }
    private Button _missionTimeline { get; set; }
    private Layout _missionEditorLayout { get; set; }

    public MissionEditor( Editor editor )
    {
        _editor = editor;
        _buttons = new List<Button>();
        _indicator = GameObject.CreatePrimitive( PrimitiveType.Sphere ).GetComponent<MeshRenderer>();
        container = new GameObject( typeof( MissionEditor ).Name );
        container.transform.SetParent( editor.container.transform );
        _indicator.transform.SetParent( container.transform );
        _indicator.material.color = Color.yellow;
        HideIndicator();
    }
}
#endif //UNITY_EDITOR

