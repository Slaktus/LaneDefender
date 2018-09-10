using UnityEngine;

public class HeldEvent : BaseObject
{
    public override void Destroy()
    {
        label.Destroy();
        GameObject.Destroy( container );
    }

    public void SetPosition( Vector3 position ) => this.position = new Vector3( position.x , position.y + 1 , position.z );
    public void SetText( string text ) => label.SetText( text );

    public WaveEventDefinition waveEventDefinition { get; private set; }
    public int laneIndex { get; }

    public HeldEvent(Vector3 position, float width , float height , Color color, WaveEventDefinition waveEventDefinition, int laneIndex) : base("Held" + waveEventDefinition.type.ToString(), GameObject.CreatePrimitive(PrimitiveType.Quad))
    {
        container.transform.localRotation = Quaternion.Euler( 90 , 0 , 0 );
        body.transform.localScale = new Vector3(width, height, 1);
        body.transform.localRotation = Quaternion.identity;
        meshRenderer.material.color = color;

        label.SetLocalRotation( Quaternion.identity );

        this.position = position;
        this.laneIndex = laneIndex;
        this.waveEventDefinition = waveEventDefinition;
    }
}