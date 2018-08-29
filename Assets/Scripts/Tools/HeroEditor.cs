#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroEditor : Layout
{
    public void ShowHeroes()
    {
        HideHeroes();
        int count = _editor.objectData.heroSets[ (int) Assets.ObjectDataSets.Default ].heroDefinitions.Count;
        Add(_heroes = new Layout("Heroes", 3, count, 0.25f, 0.1f, count, container));

        _heroes.SetViewportPosition(new Vector2(0.5f, 1));
        _heroes.SetPosition(_heroes.position + Vector3.up + Vector3.back);

        _heroes.Add(new List<Button>(
            Button.GetButtons(count,
            (int index) => new Button(_editor.objectData.heroSets[ (int) Assets.ObjectDataSets.Default ].heroDefinitions[ index ].name, 3, 1, container, "Hero", fontSize: 20,
                Enter: (Button button) => button.SetColor(button.selected ? button.color : Color.green),
                Stay: (Button button) =>
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        _selectedHero = _editor.objectData.heroSets[ (int) Assets.ObjectDataSets.Default ].heroDefinitions[ index ];
                        ShowHeroLevels(button.position + new Vector3(button.width * 0.5f, 0, button.height * 0.5f));
                        button.SetColor(Color.yellow);
                        button.Select();
                    }
                },
                Exit: (Button button) => button.SetColor(button.selected ? button.color : Color.white),
                Close: (Button button) =>
                {
                    if (button.selected && Input.GetMouseButtonDown(0) && (_heroLevels == null || !_heroLevels.containsMouse))
                    {
                        HideHeroLevels();
                        button.Deselect();

                        if (_selectedLevel != index)
                        {
                            button.SetColor(Color.white);
                            _selectedHero = null;
                        }
                    }
                })
            )), true);
    }

    public void HideHeroes()
    {
        if (_heroes != null)
            Remove(_heroes);

        _heroes?.Destroy();
        _heroes = null;
    }

    public void ShowHeroLevels(Vector3 position)
    {
        HideHeroLevels();
        int count = _selectedHero.levels.Count;
        Add(_heroLevels = new Layout("HeroLevels", 3, count + 1 , 0.25f, 0.1f, count + 1, container));
        _heroLevels.SetPosition(position + (Vector3.right * _heroLevels.width * 0.5f) + (Vector3.back * _heroLevels.height * 0.5f));

        _heroLevels.Add(new List<Button>(
            Button.GetButtons(count,
            (int index) => new Button("Level " + index , 3, 1, container, "Item", fontSize: 20,
                Enter: (Button button) => button.SetColor(button.selected ? button.color : Color.green),
                Stay: (Button button) =>
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        _selectedLevel = index;
                        ShowHeroEditor();
                        HideHeroLevels();
                    }
                },
                Exit: (Button button) => button.SetColor(button.selected ? button.color : Color.white))))
        {
            new Button( "Add Hero Level" , 3 , 1 , container , "AddHeroLevel" , fontSize: 20,
                Enter: ( Button button ) => button.SetColor( Color.green ) ,
                Stay: ( Button button ) =>
                {
                    if ( Input.GetMouseButtonDown( 0 ) )
                    {
                        _selectedHero.AddLevel();
                        ShowHeroLevels(position);
                    }
                } ,
                Exit: ( Button button ) => button.SetColor( Color.white ) )
        }, true);
    }

    public void HideHeroLevels()
    {
        if (_heroLevels != null)
            Remove(_heroLevels);

        _heroLevels?.Destroy();
        _heroLevels = null;
    }

    public void ShowHeroEditor()
    {
        //just a bit of positioning here and we be rearin' to gaw
        HideHeroEditor();
        Add(_heroEditor = new Layout("HeroEditor", 3, 3 , 0.25f, 0.1f, 3, container));
        _heroEditor.SetPosition(_heroes.position + (Vector3.back * (_heroes.height + _heroEditor.height) * 0.5f));
        _heroEditor.Add(new List<Element>()
        {
            new Label("Value:", Color.black, 1.25f, 0.5f, container, fontSize: 20, anchor: TextAnchor.MiddleCenter),
            new Field("Value", _selectedHero.Value(_selectedLevel).ToString(), 2, 0.5f, 20, container, Field.ContentMode.Numbers, EndInput: (Field field) =>
            {
                int value;
                int.TryParse(field.label.text, out value);
                _selectedHero.SetValue(_selectedLevel, value);
                field.label.SetText(value.ToString());
                //need to implement refresh
                Refresh();
            }),
            new Label("Health:", Color.black, 1.25f, 0.5f, container, fontSize: 20, anchor: TextAnchor.MiddleCenter),
            new Field("Health", _selectedHero.Health(_selectedLevel).ToString(), 2, 0.5f, 20, container, Field.ContentMode.Numbers, EndInput: (Field field) =>
            {
                int value;
                int.TryParse(field.label.text, out value);
                _selectedHero.SetHealth(_selectedLevel, value);
                field.label.SetText(value.ToString());
                //need to implement refresh
                Refresh();
            }),
            new Label("Damage:", Color.black, 1.25f, 0.5f, container, fontSize: 20, anchor: TextAnchor.MiddleCenter),
            new Field("Damage", _selectedHero.Damage(_selectedLevel).ToString(), 2, 0.5f, 20, container, Field.ContentMode.Numbers, EndInput: (Field field) =>
            {
                int value;
                int.TryParse(field.label.text, out value);
                _selectedHero.SetDamage(_selectedLevel, value);
                field.label.SetText(value.ToString());
                //need to implement refresh
                Refresh();
            })
        }, true);

        int count = _selectedHero.levels[ _selectedLevel ].effects.Count;
        Add(_heroEffects = new Layout("HeroEffects", 3, count + 1, 0.25f, 0.1f, count + 1, container));
        _heroEffects.SetPosition(_heroEditor.position + (Vector3.back * ( ((_heroEffects.height + _heroEditor.height) * 0.5f))));
        _heroEffects.Add(new List<Button>(Button.GetButtons(count,
            (int index) => new Button(_selectedHero.levels[ _selectedLevel ].effects[ index ].ToString(), 3, 1, container, "Effect", fontSize: 20,
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

    public void HideHeroEditor()
    {
        if (_heroEditor != null)
            Remove(_heroEditor);

        _heroEditor?.Destroy();
        _heroEditor = null;

        if (_heroEffects != null)
            Remove(_heroEffects);

        _heroEffects?.Destroy();
        _heroEffects = null;
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
                        _selectedHero.Add(_selectedLevel, (Definitions.Effects) index);
                        ShowHeroEditor();
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
        HideHeroes();
        HideEffects();
        HideHeroLevels();
        HideHeroEditor();
        base.Hide();
    }

    private Editor _editor { get; }
    private Layout _heroes { get; set; }
    private Layout _effects { get; set; }
    private Layout _heroLevels { get; set; }
    private Layout _heroEditor { get; set; }
    private Layout _heroEffects { get; set; }
    private HeroDefinition _selectedHero { get; set; }
    private int _selectedLevel { get; set; }

    public HeroEditor(Editor editor, GameObject parent) : base(typeof(HeroEditor).Name, parent)
    {
        _editor = editor;
        _selectedLevel = -1;
    }
}
#endif //UNITY_EDITOR