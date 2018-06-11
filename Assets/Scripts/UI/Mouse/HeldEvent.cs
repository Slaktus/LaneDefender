using UnityEngine;

public class HeldEvent : MouseObject
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

    public HeldEvent( Vector3 position , WaveEventDefinition waveEventDefinition , int laneIndex ) : base( "Held" + waveEventDefinition.type.ToString() )
    {
        container.transform.localRotation = Quaternion.Euler( 90 , 0 , 0 );
        quad.transform.localRotation = Quaternion.identity;
        meshRenderer.material.color = Color.white;

        label.SetLocalRotation( Quaternion.identity );

        this.position = position;
        this.laneIndex = laneIndex;
        this.waveEventDefinition = waveEventDefinition;
    }
}