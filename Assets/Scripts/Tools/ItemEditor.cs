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
                        _selectedItem = null;
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

    private Editor _editor { get; }
    private Layout _items { get; set; }
    private Layout _itemLevels { get; set; }
    private int _selectedLevel { get; set; }
    private ItemDefinition _selectedItem { get; set; }

    public ItemEditor(Editor editor, GameObject parent) : base(typeof(ItemEditor).Name, parent)
    {
        _editor = editor;
    }
}
