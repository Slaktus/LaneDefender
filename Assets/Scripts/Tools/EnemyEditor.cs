#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEditor : Layout
{
    public void ShowEnemies()
    {
        HideEnemies();
        int count = _editor.objectData.enemySets[ (int) Assets.ObjectDataSets.Default ].enemyDefinitions.Count;
        Add(_enemies = new Layout("Enemies", 3, count, 0.25f, 0.1f, count, container));

        _enemies.SetViewportPosition(new Vector2(0.25f, 1));
        _enemies.SetPosition(_enemies.position + Vector3.up + Vector3.back);

        _enemies.Add(new List<Button>(
            Button.GetButtons(count,
            (int index) => new Button(_editor.objectData.enemySets[ (int) Assets.ObjectDataSets.Default ].enemyDefinitions[ index ].name, 3, 1, container, "Enemy", fontSize: 20,
                Enter: (Button button) => button.SetColor(button.selected ? button.color : Color.green),
                Stay: (Button button) =>
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        ShowEnemyLevels(_editor.objectData.enemySets[ (int) Assets.ObjectDataSets.Default ].enemyDefinitions[ index ], button.position + new Vector3(button.width * 0.5f, 0, button.height * 0.5f));
                        button.SetColor(Color.yellow);
                        button.Select();
                    }
                },
                Exit: (Button button) => button.SetColor(button.selected ? button.color : Color.white),
                Close: (Button button) =>
                {
                    if (button.selected && Input.GetMouseButtonDown(0) && (_enemyLevels == null || !_enemyLevels.containsMouse))
                    {
                        HideEnemyLevels();
                        button.Deselect();

                        if (_selectedLevel != index)
                            button.SetColor(Color.white);
                    }
                })
            )), true);
    }

    public void HideEnemies()
    {
        if (_enemies != null)
            Remove(_enemies);

        _enemies?.Destroy();
        _enemies = null;
    }

    public void ShowEnemyLevels(EnemyDefinition definition, Vector3 position)
    {
        HideEnemyLevels();
        int count = definition.levels.Count;
        Add(_enemyLevels = new Layout("HeroLevels", 3, count + 1 , 0.25f, 0.1f, count + 1, container));
        _enemyLevels.SetPosition(position + (Vector3.right * _enemyLevels.width * 0.5f) + (Vector3.back * _enemyLevels.height * 0.5f));

        _enemyLevels.Add(new List<Button>(
            Button.GetButtons(count,
            (int index) => new Button("Level " + index , 3, 1, container, "Item", fontSize: 20,
                Enter: (Button button) => button.SetColor(button.selected ? button.color : Color.green),
                Stay: (Button button) =>
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        _selectedEnemy = definition;
                        _selectedLevel = index;
                        ShowEnemyEditor();
                        HideEnemyLevels();
                    }
                },
                Exit: (Button button) => button.SetColor(button.selected ? button.color : Color.white))))
        {
            new Button( "Add Enemy Level" , 3 , 1 , container , "AddEnemyLevel" , fontSize: 20,
                Enter: ( Button button ) => button.SetColor( Color.green ) ,
                Stay: ( Button button ) =>
                {
                    if ( Input.GetMouseButtonDown( 0 ) )
                    {
                        definition.AddLevel();
                        ShowEnemyLevels(definition,position);
                    }
                } ,
                Exit: ( Button button ) => button.SetColor( Color.white ) )
        }, true);
    }

    public void HideEnemyLevels()
    {
        if (_enemyLevels != null)
            Remove(_enemyLevels);

        _enemyLevels?.Destroy();
        _enemyLevels = null;
    }

    public void ShowEnemyEditor()
    {
        //just a bit of positioning here and we be rearin' to gaw
        HideEnemyEditor();
        Add(_enemyEditor = new Layout("EnemyEditor", 3, 6 , 0.25f, 0.1f, 6, container));
        _enemyEditor.SetPosition(_enemies.position + (Vector3.back * (_enemies.height + _enemyEditor.height) * 0.5f));
        _enemyEditor.Add(new List<Element>()
        {
            new Label("Value:", Color.black, 1.25f, 0.5f, container, fontSize: 20, anchor: TextAnchor.MiddleCenter),
            new Field("Value", _selectedEnemy.Value(_selectedLevel).ToString(), 2, 0.5f, 20, container, Field.ContentMode.Numbers, EndInput: (Field field) =>
            {
                int value;
                int.TryParse(field.label.text, out value);
                _selectedEnemy.SetValue(_selectedLevel, value);
                field.label.SetText(value.ToString());
                //need to implement refresh
                Refresh();
            }),
            new Label("Health:", Color.black, 1.25f, 0.5f, container, fontSize: 20, anchor: TextAnchor.MiddleCenter),
            new Field("Health", _selectedEnemy.Health(_selectedLevel).ToString(), 2, 0.5f, 20, container, Field.ContentMode.Numbers, EndInput: (Field field) =>
            {
                int value;
                int.TryParse(field.label.text, out value);
                _selectedEnemy.SetHealth(_selectedLevel, value);
                field.label.SetText(value.ToString());
                //need to implement refresh
                Refresh();
            }),
            new Label("Damage:", Color.black, 1.25f, 0.5f, container, fontSize: 20, anchor: TextAnchor.MiddleCenter),
            new Field("Damage", _selectedEnemy.Damage(_selectedLevel).ToString(), 2, 0.5f, 20, container, Field.ContentMode.Numbers, EndInput: (Field field) =>
            {
                int value;
                int.TryParse(field.label.text, out value);
                _selectedEnemy.SetDamage(_selectedLevel, value);
                field.label.SetText(value.ToString());
                //need to implement refresh
                Refresh();
            }),
            new Label("Speed:", Color.black, 1.25f, 0.5f, container, fontSize: 20, anchor: TextAnchor.MiddleCenter),
            new Field("Speed", _selectedEnemy.Speed(_selectedLevel).ToString(), 2, 0.5f, 20, container, Field.ContentMode.Numbers, EndInput: (Field field) =>
            {
                int value;
                int.TryParse(field.label.text, out value);
                _selectedEnemy.SetSpeed(_selectedLevel, value);
                field.label.SetText(value.ToString());
                //need to implement refresh
                Refresh();
            }),
            new Label("Width:", Color.black, 1.25f, 0.5f, container, fontSize: 20, anchor: TextAnchor.MiddleCenter),
            new Field("Width", _selectedEnemy.width.ToString(), 2, 0.5f, 20, container, Field.ContentMode.Numbers, EndInput: (Field field) =>
            {
                float value;
                float.TryParse(field.label.text, out value);
                _selectedEnemy.width = value;
                field.label.SetText(value.ToString());
                //need to implement refresh
                Refresh();
            }),
            new Label("Padding:", Color.black, 1.25f, 0.5f, container, fontSize: 20, anchor: TextAnchor.MiddleCenter),
            new Field("Padding", _selectedEnemy.laneHeightPadding.ToString(), 2, 0.5f, 20, container, Field.ContentMode.Numbers, EndInput: (Field field) =>
            {
                float value;
                float.TryParse(field.label.text, out value);
                _selectedEnemy.laneHeightPadding = value;
                field.label.SetText(value.ToString());
                //need to implement refresh
                Refresh();
            })

        }, true);

        int count = _selectedEnemy.levels[ _selectedLevel ].effects.Count;
        Add(_enemyEffects = new Layout("EnemyEffects", 3, count + 1, 0.25f, 0.1f, count + 1, container));
        _enemyEffects.SetPosition(_enemyEditor.position + (Vector3.back * ( ((_enemyEffects.height + _enemyEditor.height) * 0.5f))));
        _enemyEffects.Add(new List<Button>(Button.GetButtons(count,
            (int index) => new Button(_selectedEnemy.levels[ _selectedLevel ].effects[ index ].ToString(), 3, 1, container, "Effect", fontSize: 20,
                Enter: (Button button) => button.SetColor(Color.green),
                Stay: (Button button) =>
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                    }
                },
                Exit: (Button button) => button.SetColor(Color.white))))
        {
            new Button( "Add Effect" , 3 , 1 , container , "AddEffect" , fontSize: 20,
                Enter: ( Button button ) => button.SetColor( Color.green ) ,
                Stay: ( Button button ) =>
                {
                    if ( Input.GetMouseButtonDown( 0 ) )
                    {
                        ShowEffects(button.position + new Vector3(button.width * 0.5f, 0, button.height * 0.5f));
                    }
                } ,
                Exit: ( Button button ) => button.SetColor( Color.white ) ),
        }, true);
    }

    public void HideEnemyEditor()
    {
        if (_enemyEditor != null)
            Remove(_enemyEditor);

        _enemyEditor?.Destroy();
        _enemyEditor = null;

        if (_enemyEffects != null)
            Remove(_enemyEffects);

        _enemyEffects?.Destroy();
        _enemyEffects = null;
    }

    public void ShowEffects( Vector3 position)
    {
        HideEffects();
        int count = ( int ) Definitions.Effects.Count;
        Add(_effects = new Layout("Effects", 3, count, 0.25f, 0.1f, count, container));
        _effects.SetPosition(position + (Vector3.right * _effects.width * 0.5f) + (Vector3.back * _effects.height * 0.5f));

        _effects.Add(new List<Button>(
            Button.GetButtons(count,
            (int index) => new Button((( Definitions.Effects) index ).ToString(), 3, 1, container, "Effect", fontSize: 20,
                Enter: (Button button) => button.SetColor(button.selected ? button.color : Color.green),
                Stay: (Button button) =>
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        _selectedEnemy.Add(_selectedLevel, (Definitions.Effects) index);
                        ShowEnemyEditor();
                    }
                },
                Exit: (Button button) => button.SetColor(button.selected ? button.color : Color.white),
                Close: (Button button) =>
                {
                    if (Input.GetMouseButtonDown(0) && (_effects == null || !_effects.containsMouse))
                        HideEffects();
                })
            )), true);
    }

    public void HideEffects()
    {
        if (_effects != null)
            Remove(_effects);

        _effects?.Destroy();
        _effects = null;
    }

    public override void Hide()
    {
        HideEnemies();
        HideEffects();
        HideEnemyLevels();
        HideEnemyEditor();
        base.Hide();
    }

    private Editor _editor { get; }
    private Layout _enemies { get; set; }
    private Layout _effects { get; set; }
    private Layout _enemyLevels { get; set; }
    private Layout _enemyEditor { get; set; }
    private Layout _enemyEffects { get; set; }
    private EnemyDefinition _selectedEnemy { get; set; }
    private int _selectedLevel { get; set; }

    public EnemyEditor(Editor editor, GameObject parent) : base(typeof(HeroEditor).Name, parent)
    {
        _editor = editor;
        _selectedLevel = -1;
    }
}
#endif //UNITY_EDITOR