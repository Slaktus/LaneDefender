using UnityEngine;

public class ConveyorItem : BaseObject
{
    public override void Destroy()
    {
        _conveyor.RemoveItemFromConveyor( this );
        GameObject.Destroy( container );
    }

    public override void Update() => position = new Vector3(position.x, position.y, Mathf.Clamp(position.z - (speed * Time.deltaTime), limit, _conveyor.top.z - (height * 0.5f)));
    public void PowerUp() => label.SetText(type.ToString() + "\n" + ++power);
    public void SetHeld(bool held) => this.held = held;

    public string text => label.text;
    public int index => _conveyor.IndexOf( this );
    public float height => _conveyor.itemHeight;
    public float width => _conveyor.width - _conveyor.itemWidthPadding;
    public Color color { get { return meshRenderer.material.color; } set { meshRenderer.material.color = value; } }
    public override Rect rect => new Rect( position.x - ( width * 0.5f ) , position.z - ( height * 0.5f ) , width , height );
    public bool matchThree => canUpgrade && _conveyor.itemCount > index + 2 && _conveyor.ItemAt( index + 1 ).canUpgrade && _conveyor.ItemAt( index + 2 ).canUpgrade && type == _conveyor.ItemAt( index + 1 ).type && _conveyor.ItemAt( index + 1 ).type == _conveyor.ItemAt( index + 2 ).type && level == _conveyor.ItemAt( index + 1 ).level && _conveyor.ItemAt( index + 1 ).level == _conveyor.ItemAt( index + 2 ).level;
    public bool settled => !held && Mathf.Approximately( position.z , limit );
    public bool canUpgrade => settled && _maxLevel > level;

    public ItemSettings settings { get; }
    public int power { get; private set; }
    public bool held { get; private set; }
    public Definitions.Items type => _definition.type;
    public int damage => settings.damage;
    public int level => settings.level;

    private float speed => _conveyor.speed;
    private float limit => _conveyor.bottom.z + ( height * 0.5f ) + ( ( height + itemSpacing ) * index );
    private float itemSpacing => _conveyor.itemSpacing;

    private Conveyor _conveyor { get; }
    private int _maxLevel { get; } = 3;
    private ItemDefinition _definition { get; }

    public ConveyorItem(Conveyor conveyor, ItemDefinition definition, ItemSettings settings) : base("Conveyor" + definition.type.ToString(), GameObject.CreatePrimitive(PrimitiveType.Quad) )
    {
        _conveyor = conveyor;
        this.settings = settings;
        _definition = definition;
        container.transform.localRotation = Quaternion.Euler( 90 , 0 , 0 );

        body.transform.localRotation = Quaternion.identity;
        body.transform.localScale = new Vector3( width , height , 1 );

        meshRenderer.material.color = Color.white;
        label.SetLocalRotation( Quaternion.identity );
        label.SetText( type.ToString() + "\n" + level );
        position = conveyor.top - ( Vector3.forward * height * 0.5f ) + Vector3.up;
    }
}