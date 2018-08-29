#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimelineEditor : Layout
{
    public override void Update()
    {
        _indicator.transform.position = mousePos + Vector3.up;
        base.Update();
    }

    public void AddWaveToTimeline(WaveDefinition waveDefinition)
    {
        _editor.missionEditor.selectedMission.Add(waveDefinition, timelinePosition);

        Button button = new Button("Wave", 2, 1, container, "Wave",
            Enter: (Button b) => b.SetColor(b.selected ? b.color : Color.green),
            Stay: (Button b) =>
            {
                if (Input.GetMouseButtonDown(1))
                {
                    _editor.missionEditor.selectedMission.Remove(waveDefinition);
                    _buttons.Remove(b);
                    b.Destroy();
                }
                else if (!b.selected && Input.GetMouseButtonDown(0))
                {
                    for (int i = 0; _buttons.Count > i; i++)
                    {
                        _buttons[ i ].Deselect();
                        _buttons[ i ].SetColor(Color.white);
                    }

                    _editor.waveEditor.SetSelectedWaveDefinition(waveDefinition);
                    _editor.waveEditor.HideWaveEventButtons();
                    _editor.waveEditor.ShowWaveEventButtons();
                    b.SetColor(Color.yellow);
                    b.Select();
                }
            },
            Exit: (Button b) =>
            {
                if (_editor.stage.conveyor == null || !_editor.stage.conveyor.showing)
                {
                    if (_editor.waveEditor.heldWaveEvent == null && heldWave == null && Input.GetMouseButton(0))
                    {
                        heldWave = new HeldWave(b.rect.position, waveDefinition);
                        heldWave.SetText("Wave");
                    }

                    b.SetColor(b.selected ? b.color : Color.white);
                }
            });

        _editor.waveEditor.SetSelectedWaveDefinition(waveDefinition);
        _editor.waveEditor.HideWaveEventButtons();
        _editor.waveEditor.ShowWaveEventButtons();

        for (int i = 0; _buttons.Count > i; i++)
        {
            _buttons[ i ].Deselect();
            _buttons[ i ].SetColor(Color.white);
        }

        _buttons.Add(button);
        Add(button);

        button.SetPosition(new Vector3(missionTimeline.rect.xMin + (timelinePosition * missionTimeline.rect.xMax), 0, missionTimeline.rect.yMin + 0.5f) + Vector3.up);
        button.SetColor(Color.yellow);
        button.Select();
    }

    public void AddWaveToTimeline(WaveDefinition waveDefinition, float timelinePosition)
    {
        Button button = new Button("Wave", 2, 1, container, "Wave",
            Enter: (Button b) =>
            {
                if (_editor.stage.conveyor == null || !_editor.stage.conveyor.showing)
                    b.SetColor(b.selected ? b.color : Color.green);
            },
            Stay: (Button b) =>
            {
                if (_editor.stage.conveyor == null || !_editor.stage.conveyor.showing)
                {
                    if (Input.GetMouseButtonDown(1))
                    {
                        _editor.missionEditor.selectedMission.Remove(waveDefinition);
                        _buttons.Remove(b);
                        b.Destroy();
                    }
                    else if (!b.selected && Input.GetMouseButtonDown(0))
                    {
                        for (int i = 0; _buttons.Count > i; i++)
                        {
                            _buttons[ i ].Deselect();
                            _buttons[ i ].SetColor(Color.white);
                        }

                        _editor.waveEditor.SetSelectedWaveDefinition(waveDefinition);
                        _editor.waveEditor.HideWaveEventButtons();
                        _editor.waveEditor.ShowWaveEventButtons();
                        b.SetColor(Color.yellow);
                        b.Select();
                    }
                }
            },
            Exit: (Button b) =>
            {
                if (_editor.stage.conveyor == null || !_editor.stage.conveyor.showing)
                {
                    if ( _editor.waveEditor.heldWaveEvent == null && heldWave == null && Input.GetMouseButton(0))
                    {
                        heldWave = new HeldWave(b.rect.position, waveDefinition);
                        heldWave.SetText("Wave");
                    }

                    b.SetColor(b.selected ? b.color : Color.white);
                }
            });

        button.SetPosition(new Vector3(missionTimeline.rect.xMin + (timelinePosition * missionTimeline.rect.xMax), 0, missionTimeline.rect.yMin + 0.5f) + (Vector3.up * (missionTimeline.position.y + 1)));
        _buttons.Add(button);
        Add(button);
    }

    public void RemoveWaveFromTimeline(WaveDefinition waveDefinition)
    {
        int index = _editor.missionEditor.selectedMission.waveDefinitions.IndexOf(waveDefinition);
        Remove(_buttons[ index ]);
        _buttons[ index ].Destroy();
        _buttons.RemoveAt(index);
    }

    public void ShowMissionTimeline()
    {
        HideMissionTimeline();

        if (_editor.stage != null)
        {
            Add(missionTimeline = new Button(string.Empty, _editor.stage.width, 1, container, "MissionTimeline",
                Stay: (Button button) =>
                {
                    bool overlappingWave = false;

                    for (int i = 0; _buttons.Count > i && !overlappingWave; i++)
                        overlappingWave = _buttons[ i ].containsMouse;

                    if (overlappingWave || heldWave != null || _editor.waveEditor.waveSets != null || (_editor.stage.conveyor != null && _editor.stage.conveyor.showing))
                        HideIndicator();
                    else if (_editor.waveEditor.waveSets == null )
                    {
                        ShowIndicator();

                        if (Input.GetMouseButtonDown(0) && (_editor.stage.conveyor != null && !_editor.stage.conveyor.showing))
                        {
                            timelinePosition = Helpers.Normalize(_indicator.transform.position.x, button.rect.xMax, button.rect.xMin);
                            _editor.waveEditor.Show();
                        }
                    }
                },
                Exit: (Button button) => HideIndicator()));

            missionTimeline.SetPosition(new Vector3(_editor.stage.start + (_editor.stage.width * 0.5f), 0, Camera.main.ViewportToWorldPoint(new Vector3(0, 1, Camera.main.transform.position.y)).z) + (Vector3.back * missionTimeline.height * 0.5f) + Vector3.up);

            if (_editor.missionEditor.selectedMission != null)
                for (int i = 0; _editor.missionEditor.selectedMission.waveDefinitions.Count > i; i++)
                    AddWaveToTimeline(_editor.missionEditor.selectedMission.waveDefinitions[ i ], _editor.missionEditor.selectedMission.waveTimes[ i ]);
        }
    }

    public void HideMissionTimeline()
    {
        for (int i = 0; _buttons.Count > i; i++)
        {
            Remove(_buttons[ i ]);
            _buttons[ i ].Destroy();
        }

        _buttons.Clear();

        if (missionTimeline != null)
            Remove(missionTimeline);

        missionTimeline?.Destroy();
        missionTimeline = null;
    }

    public override void Hide()
    {
        HideIndicator();
    }

    private void ShowIndicator() => _indicator.enabled = true;
    private void HideIndicator() => _indicator.enabled = false;

    public Vector3 indicatorPosition => new Vector3(_indicator.transform.position.x, 0, missionTimeline.rect.yMin);

    public float timelinePosition { get; private set; }
    public Button missionTimeline { get; set; }
    public HeldWave heldWave { get; set; }

    private Editor _editor { get; }
    private MeshRenderer _indicator { get; }
    private List<Button> _buttons { get; set; }

    public TimelineEditor(Editor editor, GameObject parent = null) : base(typeof(TimelineEditor).Name, parent)
    {
        _editor = editor;
        _buttons = new List<Button>();
        _indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere).GetComponent<MeshRenderer>();
        _indicator.transform.SetParent(container.transform);
        _indicator.material.color = Color.yellow;

        HideIndicator();
    }
}
#endif //UNITY_EDITOR