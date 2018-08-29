using UnityEngine;

public class HeldWave : BaseObject
{
    public override void Destroy()
    {
        label.Destroy();
        GameObject.Destroy( container );
    }

    public void SetPosition( Vector3 position ) => this.position = new Vector3( position.x , position.y + 1 , position.z );
    public void SetText( string text ) => label.SetText( text );

    public WaveDefinition waveDefinition { get; private set; }

    public HeldWave(Vector3 position, WaveDefinition waveDefinition) : base("Held" + waveDefinition.name, GameObject.CreatePrimitive(PrimitiveType.Quad))
    {
        container.transform.localRotation = Quaternion.Euler( 90 , 0 , 0 );
        body.transform.localRotation = Quaternion.identity;
        body.transform.localScale = new Vector3(2, 1, 1);
        meshRenderer.material.color = Color.yellow;

        label.SetLocalRotation( Quaternion.identity );

        this.position = position;
        this.waveDefinition = waveDefinition;
    }
}