#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimelineEditor : Layout
{
    private void SetSelectedWaveDefinition(WaveDefinition waveDefinition) => _editor.waveEditor.SetSelectedWaveDefinition(waveDefinition);
    private void HideWaveEventButtons() => _editor.waveEditor.HideWaveEventButtons();
    private void ShowWaveEventButtons() => _editor.waveEditor.ShowWaveEventButtons();
    private void HideIndicator() => _indicator.enabled = false;
    private void ShowIndicator() => _indicator.enabled = true;

    public override void Update()
    {
        _indicator.transform.position = mousePos + Vector3.up;
        HandleTimelineHover();
        base.Update();
    }

    public void HandleTimelineHover()
    {
        if (heldWave != null)
        {
            heldWave.SetPosition(mousePos + (Vector3.up * 2));

            if (Input.GetMouseButtonUp(0))
            {
                if (containsMouse)
                {
                    selectedMission.waveTimes[ heldWaveIndex ] = Helpers.Normalize(mousePos.x, missionTimeline.rect.xMax, missionTimeline.rect.xMin);
                    ShowMissionTimeline();
                }

                heldWave.Destroy();
                heldWave = null;
            }
        }
    }

    public void AddWaveToTimeline(WaveDefinition waveDefinition, float timelinePosition, bool addToMission = false )
    {
        if ( addToMission )
            selectedMission.Add(waveDefinition, timelinePosition);

        Button button = new Button("Wave", 1, 1, container, "Wave",
            fontSize: 20,
            Enter: (Button b) =>
            {
                if (_editor.conveyor == null || !_editor.conveyor.showing)
                    b.SetColor(b.selected ? b.color : Color.green);
            },
            Stay: (Button b) =>
            {
                if (_editor.conveyor == null || !_editor.conveyor.showing)
                {
                    if (Input.GetMouseButtonDown(1))
                    {
                        selectedMission.Remove(waveDefinition);
                        _buttons.Remove(b);
                        Remove(b);
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
                if (_editor.conveyor == null || !_editor.conveyor.showing)
                {
                    if ( _editor.waveEditor.heldWaveEvent == null && heldWave == null && Input.GetMouseButton(0))
                    {
                        heldWaveIndex = selectedMission.waveTimes.IndexOf(timelinePosition);
                        heldWave = new HeldWave(b.rect.position, waveDefinition);
                        heldWave.SetPosition(mousePos + (Vector3.up * 2));
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
        int index = selectedMission.waveDefinitions.IndexOf(waveDefinition);
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

                    if (overlappingWave || heldWave != null || _editor.waveEditor.waveSets != null || (_editor.conveyor != null && _editor.conveyor.showing))
                        HideIndicator();
                    else if (_editor.waveEditor.waveSets == null )
                    {
                        ShowIndicator();

                        if (Input.GetMouseButtonDown(0) && (_editor.conveyor != null && !_editor.conveyor.showing))
                        {
                            timelinePosition = Helpers.Normalize(_indicator.transform.position.x, button.rect.xMax, button.rect.xMin);
                            _editor.waveEditor.Show();
                        }
                    }
                },
                Exit: (Button button) => HideIndicator()));

            missionTimeline.SetPosition( new Vector3(_editor.stage.start + (_editor.stage.width * 0.5f), 0, Camera.main.ViewportToWorldPoint(new Vector3(0, 1, Camera.main.transform.position.y)).z) + (Vector3.back * missionTimeline.height * 0.5f) + Vector3.up);

            if (selectedMission != null)
                for (int i = 0; selectedMission.waveDefinitions.Count > i; i++)
                    AddWaveToTimeline(selectedMission.waveDefinitions[ i ], selectedMission.waveTimes[ i ], false);
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

    public override void Hide() => HideIndicator();

    public Vector3 indicatorPosition => new Vector3(_indicator.transform.position.x, 0, missionTimeline.rect.center.y);

    public float timelinePosition { get; private set; }
    public Button missionTimeline { get; private set; }
    public HeldWave heldWave { get; private set; }
    public int heldWaveIndex { get; private set; }

    private MissionDefinition selectedMission => _editor.missionEditor.selectedMission;
    private HeldEvent heldWaveEvent => _editor.waveEditor.heldWaveEvent;

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