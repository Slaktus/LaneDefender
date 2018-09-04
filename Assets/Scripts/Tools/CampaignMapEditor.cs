using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampaignMapEditor : Layout
{
    private void SetSelectedMission(MissionDefinition missionDefinition) => _editor.missionEditor.SetSelectedMission(missionDefinition);
    private MissionDefinition GetMission(int index) => selectedCampaign.GetMissionDefinition(index);
    private bool HasMission(int index) => _editor.campaignEditor.selectedCampaign.Has(index);

    public override void Update()
    {
        if (_dummyContainer != null)
        {
            _dummyContainer.transform.localScale = new Vector3(0.2f, Vector3.Distance(_dummyContainer.transform.position, mousePos), 0.2f);
            Quaternion rotation = Quaternion.LookRotation(Vector3.down, (mousePos - _dummyContainer.transform.position).normalized);
            _dummyContainer.transform.rotation = rotation;
        }

        base.Update();
    }

    public void ShowCampaignMap()
    {
        HideCampaignMap();
        Vector3 offset = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, Camera.main.transform.position.y)) + (Vector3.left * selectedCampaign.width * 0.5f) + (Vector3.forward * selectedCampaign.height * 0.5f);
        campaignMap = new CampaignMap(selectedCampaign.width, selectedCampaign.height, selectedCampaign.columns, selectedCampaign.rows, offset);

        for (int i = 0; campaignMap.tileMap.count > i; i++)
        {
            int index = i;
            bool mission = selectedCampaign.Has(index);
            RenameableButton button = new RenameableButton(mission ? GetMission(index).name : index.ToString(), mission ? campaignMap.tileMap.tileWidth - 1 : 1 , mission ? campaignMap.tileMap.tileHeight * 0.5f : 1, container,
                fontSize: 20,
                EndInput: (Field field) => 
                {
                    GetMission(index).name = field.label.text;
                    ShowCampaignMap();
                },
                Enter: (Button b) => b.SetColor(b.selected || missionSets != null ? b.color : Color.green),
                Stay: (Button b) =>
                {
                    if (missionSets == null)
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            if (selectedCampaign.Has(index))
                            {
                                SetSelectedMission(selectedCampaign.GetMissionDefinition(index));

                                if (selectedMission.stageDefinition != null)
                                {
                                    _editor.stageEditor.SetSelectedStageDefinition(selectedMission.stageDefinition);
                                    _editor.ShowStage(selectedMission.stageDefinition);
                                }

                                _editor.stageEditor.Show();
                                _editor.missionEditor.ShowMissionEditor();
                                _editor.timelineEditor.ShowMissionTimeline();

                                _editor.campaignEditor.Hide();
                                _editor.timelineEditor.Hide();
                                _editor.missionEditor.Hide();
                                HideCampaignMap();
                            }
                            else
                            {
                                _editor.missionEditor.ShowMissionSets(index, b.position + new Vector3(b.width * 0.5f, 0, b.height * 0.5f));
                                b.SetColor(Color.yellow);
                                b.Select();
                            }
                        }

                        if (Input.GetMouseButtonDown(1) && selectedCampaign.Has(index))
                        {
                            //ok, so the problem is that it's the same damn instance!
                            //that's ... uh, gonna be a bit of a bitch to work around
                            //some kind of double bookkeeping required that I'm too tired to handle now
                            //but at least hey, that's it -- when looking up the index and removing the instance, it finds the other identical instance, because it's the same in both
                            //duh

                            for (int j = 0; selectedCampaign.connections.Count > j; j++)
                            {
                                Connection connection = selectedCampaign.connections[ j ];

                                if (connection.fromIndex == index || connection.toIndex == index)
                                    selectedCampaign.Remove(connection);
                            }

                            selectedCampaign.Remove(selectedCampaign.GetMissionDefinition(index));
                            ShowCampaignMap();
                        }
                    }
                },
                Exit: (Button b) => b.SetColor(b.selected ? b.color : Color.white),
                Close: (Button b) =>
                {
                    if (b.selected && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && (missionSets == null || !missionSets.containsMouse) && (missions == null || !missions.containsMouse))
                    {
                        b.Deselect();
                        b.SetColor(Color.white);
                        _editor.missionEditor.HideMissions();
                        _editor.missionEditor.HideMissionSets();
                    }
                });

            if (!mission)
                button.DisableField();

            button.SetPosition(campaignMap.tileMap.PositionOf(index));
            _campaignMapButtons.Add(button);
            Add(button);

            if (selectedCampaign.Has(index))
            {
                ShowConnectorAndTerminal(index, button);
                ShowFirstMissionButton(index, button);
                ShowFinalMissionButton(index, button);
            }
        }

        ShowConnections();
    }

    public void HideCampaignMap()
    {
        for (int i = 0; _campaignMapButtons.Count > i; i++)
        {
            Remove(_campaignMapButtons[ i ]);
            _campaignMapButtons[ i ].Destroy();
        }

        _campaignMapButtons.Clear();
        HideConnectorsAndTerminals();
        HideFinalMissionButtons();
        HideFirstMissionButtons();
        HideConnections();
    }

    private void ShowConnectorAndTerminal(int index, RenameableButton button)
    {
        Button connector = new Button("+", 0.5f, 0.5f, container, "Connector+",
            Enter: (Button butt) => butt.SetColor(butt.selected ? butt.color : Color.green),
            Stay: (Button butt) =>
            {
                if (Input.GetMouseButtonDown(0))
                {
                    butt.Select();
                    butt.SetColor(Color.yellow);
                    _selectedConnectorIndex = index;
                    _dummyContainer = new GameObject("DummyContainer");
                    _dummyConnector = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    _dummyConnector.transform.SetParent(_dummyContainer.transform);
                    _dummyConnector.transform.localPosition += Vector3.up * 0.5f;
                    _dummyContainer.transform.position = butt.position + Vector3.up;
                }
            },
            Exit: (Button butt) => butt.SetColor(butt.selected ? butt.color : Color.white),
            Close: (Button butt) =>
            {
                if (Input.GetMouseButtonUp(0))
                {
                    butt.Deselect();
                    butt.SetColor(Color.white);

                    if (_dummyContainer != null)
                    {
                        butt.Deselect();
                        GameObject.Destroy(_dummyContainer);
                        _dummyContainer = null;
                        _dummyConnector = null;
                    }
                }
            });

        Button terminator = new Button("-", 0.5f, 0.5f, container, "Terminator-",
            Enter: (Button butt) => butt.SetColor(_selectedConnectorIndex >= 0 ? Color.yellow : Color.green),
            Stay: (Button butt) =>
            {
                if (_selectedConnectorIndex >= 0 && _selectedConnectorIndex != index && Input.GetMouseButtonUp(0))
                {
                    ScriptableObjects.Add(ScriptableObject.CreateInstance<Connection>().Initialize(_selectedConnectorIndex, index), selectedCampaign);
                    _selectedConnectorIndex = -1;
                    butt.SetColor(Color.white);
                    ShowConnections();
                }
            },
            Exit: (Button butt) => butt.SetColor(Color.white));

        Add(connector);
        Add(terminator);
        _connectorsAndTerminators.Add(connector);
        _connectorsAndTerminators.Add(terminator);

        connector.SetPosition(new Vector3(button.rect.xMax + 0.25f, button.position.y, button.rect.center.y));
        terminator.SetPosition(new Vector3(button.rect.xMin - 0.25f, button.position.y, button.rect.center.y));
    }

    public void HideConnectorsAndTerminals()
    {
        for (int i = 0; _connectorsAndTerminators.Count > i; i++)
        {
            Remove(_connectorsAndTerminators[ i ]);
            _connectorsAndTerminators[ i ].Destroy();
        }

        _connectorsAndTerminators.Clear();
    }

    public void ShowFirstMissionButton(int index, RenameableButton button)
    {
        Button butt = new Button("1st", 1, 0.5f, container, "First",
            fontSize: 20,
            Enter: (Button b) => b.SetColor(b.selected ? b.color : Color.green),
            Stay: (Button b) =>
            {
                if (Input.GetMouseButtonDown(0))
                {
                    b.Select();
                    b.SetColor(Color.yellow);
                    selectedCampaign.SetFirstMissionIndex(index);
                }
            },
            Exit: (Button b) => b.SetColor(b.selected ? b.color : Color.white),
            Close: (Button b) =>
            {
                if (Input.GetMouseButtonDown(0) && index != selectedCampaign.firstMissionIndex)
                {
                    b.Deselect();
                    b.SetColor(Color.white);
                }
            });

        if (index == selectedCampaign.firstMissionIndex)
        {
            butt.Select();
            butt.SetColor(Color.yellow);

        }

        Add(butt);
        _firstMissionButtons.Add(butt);
        butt.SetPosition(new Vector3(button.rect.center.x, button.position.y, button.rect.yMax + 0.25f));
    }

    public void HideFirstMissionButtons()
    {
        for (int i = 0; _firstMissionButtons.Count > i; i++)
        {
            Remove(_firstMissionButtons[ i ]);
            _firstMissionButtons[ i ].Destroy();
        }

        _firstMissionButtons.Clear();
    }

    public void ShowFinalMissionButton(int index, RenameableButton button)
    {
        Button butt = new Button("Final", 1, 0.5f, container, "Final",
            fontSize: 20,
            Enter: (Button b) => b.SetColor(b.selected ? b.color : Color.green),
            Stay: (Button b) =>
            {
                if (Input.GetMouseButtonDown(0))
                {
                    b.Select();
                    b.SetColor(Color.yellow);
                    selectedCampaign.AddFinalMissionIndex(index);
                }

                if (Input.GetMouseButtonDown(1) && selectedCampaign.HasFinalMissionIndex(index))
                {
                    b.Deselect();
                    b.SetColor(Color.white);
                    selectedCampaign.RemoveFinalMissionIndex(index);
                }
            },
            Exit: (Button b) => b.SetColor(b.selected ? b.color : Color.white));

        if (selectedCampaign.HasFinalMissionIndex(index))
        {
            butt.Select();
            butt.SetColor(Color.yellow);
        }

        Add(butt);
        _finalMissionButtons.Add(butt);
        butt.SetPosition(new Vector3(button.rect.center.x, button.position.y, button.rect.yMin - 0.25f));
    }

    public void HideFinalMissionButtons()
    {
        for (int i = 0; _finalMissionButtons.Count > i; i++)
        {
            Remove(_finalMissionButtons[ i ]);
            _finalMissionButtons[ i ].Destroy();
        }

        _finalMissionButtons.Clear();
    }

    private void ShowConnections()
    {
        HideConnections();

        for (int i = 0; selectedCampaign.connections.Count > i; i++)
        {
            Connection connection = selectedCampaign.connections[ i ];

            GameObject container = new GameObject("Connector");
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.transform.SetParent(container.transform);
            quad.transform.localPosition += Vector3.up * 0.5f;

            Vector3 fromPosition = campaignMap.tileMap.PositionOf(connection.fromIndex) + (Vector3.right * ((campaignMap.tileMap.tileWidth * 0.5f)));
            Vector3 toPosition = campaignMap.tileMap.PositionOf(connection.toIndex) + (Vector3.left * ((campaignMap.tileMap.tileWidth * 0.5f)));

            container.transform.position = fromPosition + Vector3.up;
            container.transform.localScale = new Vector3(0.2f, Vector3.Distance(fromPosition, toPosition), 0.2f);
            Quaternion rotation = Quaternion.LookRotation(Vector3.down, (toPosition - fromPosition).normalized);
            container.transform.rotation = rotation;

            _connectors.Add(container);
        }
    }

    private void HideConnections()
    {
        for (int i = 0; _connectors.Count > i; i++)
            GameObject.Destroy(_connectors[ i ]);

        _connectors.Clear();
    }

    public CampaignMap campaignMap { get; private set; }

    private MissionDefinition selectedMission => _editor.missionEditor.selectedMission;
    private CampaignDefinition selectedCampaign => _editor.campaignEditor.selectedCampaign;
    private Layout missionSets => _editor.missionEditor.missionSets;
    private Layout missions => _editor.missionEditor.missions;
    private int _selectedConnectorIndex { get; set; } = -1;
    private List<Button> _connectorsAndTerminators { get; }
    private List<Button> _finalMissionButtons { get; }
    private List<Button> _firstMissionButtons { get; }
    private List<RenameableButton> _campaignMapButtons { get; }
    private GameObject _dummyConnector { get; set; }
    private GameObject _dummyContainer { get; set; }
    private List<GameObject> _connectors { get; }
    private Editor _editor { get; }

    public CampaignMapEditor(Editor editor, GameObject parent) : base(typeof(CampaignMapEditor).Name, parent)
    {
        _editor = editor;
        _connectors = new List<GameObject>();
        _firstMissionButtons = new List<Button>();
        _finalMissionButtons = new List<Button>();
        _connectorsAndTerminators = new List<Button>();
        _campaignMapButtons = new List<RenameableButton>();
    }
}
