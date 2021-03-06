﻿#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

public class CampaignEditor : Layout
{
    private void ShowCampaignMap() => _editor.campaignMapEditor.ShowCampaignMap();
    private CampaignSet GetCampaignSet(int index) => _editor.campaignData.GetCampaignSet(index);
    private CampaignDefinition GetCampaign(int index) => selectedCampaignSet.campaignDefinitions[ index ];
    public void SetSelectedCampaign(CampaignDefinition selectedCampaign) => this.selectedCampaign = selectedCampaign;

    public void ShowCampaignSets()
    {
        HideCampaignSets();
        int count = _editor.campaignData.campaignSets.Count;
        Add(campaignSets = new Layout("CampaignSets", 4, count + 1, 0.25f, 0.1f, count + 1, container));

        campaignSets.SetViewportPosition(new Vector2(1, 1));
        campaignSets.SetPosition(campaignSets.position + Vector3.up + (Vector3.left * campaignSets.width) + (Vector3.back * 0.5f));

        campaignSets.Add(new List<RenameableDeletableButton>(
            RenameableDeletableButton.GetButtons(count,
            (int index) => new RenameableDeletableButton(GetCampaignSet(index).name, 4, 1, container, 
                fontSize: 20,
                DeleteStay: ( Button button ) =>
                {
                    if (Input.GetMouseButtonUp(0))
                    {
                        if (button.selected)
                            HideCampaigns();

                        _editor.campaignData.Remove(GetCampaignSet(index));
                        ShowCampaignSets();
                    }
                },
                EndInput: (Field field) => 
                {
                    GetCampaignSet(index).name = field.label.text;
                    field.SetColor(Color.white);
                },
                Enter: (Button button) => button.SetColor(_campaigns != null && selectedCampaignSet == GetCampaignSet( index ) ? button.color : Color.green),
                Stay: (Button button) =>
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        selectedCampaignSet = GetCampaignSet(index);
                        ShowCampaigns(index, button.position );
                        button.SetColor(Color.yellow);
                        button.Select();
                    }
                },
                Exit: (Button button) => button.SetColor(_campaigns != null && selectedCampaignSet == GetCampaignSet( index ) ? button.color : Color.white),
                Close: (Button button) =>
                {
                    if (button.selected && Input.GetMouseButtonDown(0) && (_campaigns == null || !_campaigns.containsMouse))
                    {
                        HideCampaigns();
                        button.Deselect();
                        selectedCampaignSet = null;
                        button.SetColor(Color.white);
                    }
                })
            )));

        campaignSets.Add(new Button("New Set", 4, 1, container, "NewCampaignSet",
            fontSize: 20,
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
            Exit: (Button button) => button.SetColor(Color.white)), true);
    }

    public void HideCampaignSets()
    {
        if (campaignSets != null)
            Remove(campaignSets);

        campaignSets?.Destroy();
        campaignSets = null;
    }

    public void ShowCampaigns( int index , Vector3 position )
    {
        HideCampaigns();
        int count = selectedCampaignSet.campaignDefinitions.Count;
        Add(_campaigns = new Layout( "CampaignLayout" , 4 , count + 1 , 0.25f , 0.1f , count + 1 , container ));
        _campaigns.SetPosition(position + (Vector3.left * (campaignSets.width - 0.5f)) + (Vector3.back * ((_campaigns.height * 0.5f) - 0.5f)));

        _campaigns.Add(new List<RenameableDeletableButton>(
            RenameableDeletableButton.GetButtons(count,
            (int capturedIndex) => new RenameableDeletableButton(GetCampaign(capturedIndex).name, 4, 1, container,
                fontSize: 20,
                DeleteStay: (Button button) =>
                {
                    if (Input.GetMouseButtonUp(0))
                    {
                        selectedCampaignSet.Remove(GetCampaign(index));
                        ShowCampaigns(index,position);
                        ShowCampaignSets();
                    }
                },
                EndInput: (Field field) =>
                {
                    GetCampaign(capturedIndex).name = field.label.text;
                    field.SetColor(Color.white);
                },
                Enter: (Button button) => button.SetColor(Color.green),
                Stay: (Button button) =>
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        selectedCampaign = GetCampaign( capturedIndex);

                        HideCampaigns();
                        HideCampaignSets();

                        ShowCampaignMap();
                        ShowCampaignSets();
                        ShowCampaignEditor();
                    }
                },
                Exit: (Button button) => button.SetColor(Color.white)))) );

        _campaigns.Add(new Button("New Campaign", 4, 1, container, "NewCampaign",
            fontSize: 20,
            Enter: (Button button) => button.SetColor(Color.green),
            Stay: (Button button) =>
            {
                if (Input.GetMouseButtonDown(0))
                {
                    ScriptableObjects.Add(CampaignDefinition.Default(), selectedCampaignSet);
                    ShowCampaigns(index, position);
                }
            },
            Exit: (Button button) => button.SetColor(Color.white)), true);
    }

    public void HideCampaigns()
    {
        if (_campaigns != null)
            Remove(_campaigns);

        _campaigns?.Destroy();
        _campaigns = null;
    }

    public void ShowCampaignEditor()
    {
        HideCampaignEditor();

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

            Add(_campaignEditor = new Layout("CampaignEditor", 4, 4, 0.25f, 0.1f, campaignEditorButtons.Count / 2, container));
            _campaignEditor.SetPosition( campaignSets.position + ( Vector3.back * ( ( campaignSets.height + _campaignEditor.height ) * 0.5f ) ) );
            _campaignEditor.Add(campaignEditorButtons, true);
            _campaignEditor.SetParent(container);
        }
    }

    public void HideCampaignEditor()
    {
        if (_campaignEditor != null)
            Remove(_campaignEditor);

        _campaignEditor?.Destroy();
        _campaignEditor = null;
    }

    public override void Hide()
    {
        HideCampaigns();
        HideCampaignSets();
        HideCampaignEditor();
    }

    public CampaignDefinition selectedCampaign { get; private set; }
    public CampaignSet selectedCampaignSet { get; private set; }
    public Layout campaignSets { get; set; }

    private Editor _editor { get; }
    private Layout _campaigns { get; set; }
    private Layout _campaignEditor { get; set; }

    public CampaignEditor( Editor editor , Vector3 position , GameObject parent ) : base(typeof(CampaignEditor).Name , parent)
    {
        _editor = editor;
    }
}
#endif //UNITY_EDITOR