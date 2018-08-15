using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemEditor : Layout
{
    public void ShowItems()
    {
        HideItems();
        int count = _editor.objectData.itemSets[ (int) Assets.ObjectDataSets.Default ].itemDefinitions.Count;
        Add(_items = new Layout("Items", 3, count, 0.25f, 0.1f, count, container));
        _items.SetViewportPosition(new Vector2(0, 1));
        _items.SetPosition(_items.position + Vector3.up);

        _items.Add(new List<Button>(
            Button.GetButtons(count,
            (int index) => new Button(_editor.objectData.itemSets[ (int) Assets.ObjectDataSets.Default ].itemDefinitions[ index ].name, 3, 1, container, "Item", fontSize: 20,
                Enter: (Button button) => button.SetColor(button.selected ? button.color : Color.green),
                Stay: (Button button) =>
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        _selectedItem = _editor.objectData.itemSets[ (int) Assets.ObjectDataSets.Default ].itemDefinitions[ index ];
                        Debug.Log(_selectedItem);
                        ShowItemLevels(button.position + new Vector3(button.width * 0.5f, 0, button.height * 0.5f));
                        button.SetColor(Color.yellow);
                        button.Select();
                    }
                },
                Exit: (Button button) => button.SetColor(button.selected ? button.color : Color.white),
                Close: (Button button) =>
                {
                    if (button.selected && Input.GetMouseButtonDown(0) && (_itemLevels == null || !_itemLevels.containsMouse))
                    {
                        HideItemLevels();
                        button.Deselect();
                        button.SetColor(Color.white);
                    }
                })
            )), true);
    }

    public void HideItems()
    {
        if (_items != null)
            Remove(_items);

        _items?.Destroy();
        _items = null;
    }

    public void ShowItemLevels(Vector3 position)
    {
        HideItemLevels();
        int count = _selectedItem.levels;
        Add(_itemLevels = new Layout("ItemLevels", 3, count + 1 , 0.25f, 0.1f, count + 1, container));
        _itemLevels.SetPosition(position + (Vector3.right * _itemLevels.width * 0.5f) + (Vector3.back * _itemLevels.height * 0.5f));

        _itemLevels.Add(new List<Button>(
            Button.GetButtons(count,
            (int index) => new Button("Level " + index , 3, 1, container, "Item", fontSize: 20,
                Enter: (Button button) => button.SetColor(button.selected ? button.color : Color.green),
                Stay: (Button button) =>
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        _selectedLevel = index;
                        ShowItemEditor();
                        HideItemLevels();
                    }
                },
                Exit: (Button button) => button.SetColor(button.selected ? button.color : Color.white))))
        {
            new Button( "Add Item Level" , 3 , 1 , container , "NewItemLevel" , fontSize: 20,
                Enter: ( Button button ) => button.SetColor( Color.green ) ,
                Stay: ( Button button ) =>
                {
                    if ( Input.GetMouseButtonDown( 0 ) )
                    {
                        _selectedItem.AddLevel();
                        ShowItemLevels(position);
                    }
                } ,
                Exit: ( Button button ) => button.SetColor( Color.white ) )
        }, true);
    }

    public void HideItemLevels()
    {
        if (_itemLevels != null)
            Remove(_itemLevels);

        _itemLevels?.Destroy();
        _itemLevels = null;
    }

    public void ShowItemEditor()
    {
        //just a bit of positioning here and we be rearin' to gaw
        HideItemEditor();
        Add(_itemEditor = new Layout("ItemEditor", 3, 1 , 0.25f, 0.1f, 1, container));
        _itemEditor.Add(new Label("Damage:", Color.black, 1.25f, 0.5f, container, fontSize: 20, anchor: TextAnchor.MiddleCenter));
        _itemEditor.Add(new Field("Damage", _selectedItem.Damage(_selectedLevel).ToString(), 2, 0.5f, 20, container, Field.ContentMode.Numbers, EndInput: (Field field) =>
        {
            int value;
            int.TryParse(field.label.text, out value);
            _selectedItem.damage[ _selectedLevel ] = value;

            //need to implement refresh
            Refresh();
        }), true);

        int count = _selectedItem.effects[ _selectedLevel ].Count;
        Add(_itemEffects = new Layout("ItemEffects", 3, count + 1, 0.25f, 0.1f, count + 1, container));
        _itemEffects.Add(new List<Button>(Button.GetButtons(count,
            (int index) => new Button(_selectedItem.effects[ _selectedLevel ][ index ].ToString(), 3, 1, container, "Effect", fontSize: 20,
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

    public void HideItemEditor()
    {
        if (_itemEditor != null)
            Remove(_itemEditor);

        _itemEditor?.Destroy();
        _itemEditor = null;

        if (_itemEffects != null)
            Remove(_itemEffects);

        _itemEffects?.Destroy();
        _itemEffects = null;
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
                        Debug.Log(_selectedItem);
                        _selectedItem.Add(_selectedLevel, (Definitions.Effects) index);
                        ShowItemEditor();
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

    private Editor _editor { get; }
    private Layout _items { get; set; }
    private Layout _effects { get; set; }
    private Layout _itemLevels { get; set; }
    private Layout _itemEditor { get; set; }
    private Layout _itemEffects { get; set; }
    private ItemDefinition _selectedItem { get; set; }
    private int _selectedLevel { get; set; }

    public ItemEditor(Editor editor, GameObject parent) : base(typeof(ItemEditor).Name, parent)
    {
        _editor = editor;
    }
}
