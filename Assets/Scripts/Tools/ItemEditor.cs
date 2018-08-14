using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemEditor : Layout
{
    public void ShowItems()
    {
        HideItems();
        int count = ( int ) Definitions.Items.Count;
        Add(_items = new Layout("Items", 3, count + 1, 0.25f, 0.1f, count + 1, container));
        _items.SetViewportPosition(new Vector2(0, 1));
        _items.SetPosition(_items.position + Vector3.up);

        _items.Add(new List<Button>(
            Button.GetButtons(count,
            (int index) => new Button(((Definitions.Items) index).ToString(), 3, 1, container, "Item", fontSize: 20,
                Enter: (Button button) => button.SetColor(_itemLevels != null /*&& _selectedItem == _editor.campaignData.campaignSets[ index ] */? button.color : Color.green),
                Stay: (Button button) =>
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        _selectedItem = null;//_editor.campaignData.campaignSets[ index ];
                        ShowItemLevels(index, button.position + new Vector3(button.width * 0.5f, 0, button.height * 0.5f));
                        button.SetColor(Color.yellow);
                        button.Select();
                    }
                },
                Exit: (Button button) => button.SetColor(_itemLevels != null /*&& _selectedItem == _editor.campaignData.campaignSets[ index ]*/ ? button.color : Color.white),
                Close: (Button button) =>
                {
                    if (button.selected && Input.GetMouseButtonDown(0) && (_itemLevels == null/* || !_itemLevels.containsMouse*/))
                    {
                        HideItemLevels();
                        button.Deselect();
                        _selectedItem = null;
                        button.SetColor(Color.white);
                    }
                })
            ))
        {
            new Button("New Item", 4, 1, container, "NewItem", fontSize: 20,
                Enter: (Button button) => button.SetColor(Color.green),
                Stay: (Button button) =>
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        ScriptableObjects.Add(ScriptableObject.CreateInstance<CampaignSet>(), _editor.campaignData);
                        ShowItems();
                    }
                },
                Exit: (Button button) => button.SetColor(Color.white))
        }, true);
    }

    public void HideItems()
    {
        if (_items != null)
            Remove(_items);

        _items?.Destroy();
        _items = null;
    }

    public void ShowItemLevels(int index, Vector3 position) { }
    public void HideItemLevels() { }

    private Editor _editor { get; }
    private Layout _items { get; set; }
    private Layout _itemLevels { get; set; }
    private ItemDefinition _selectedItem { get; set; }

    public ItemEditor(Editor editor, GameObject parent) : base(typeof(ItemEditor).Name, parent)
    {
        _editor = editor;
    }
}
