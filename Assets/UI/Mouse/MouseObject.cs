using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MouseObject
{
    public abstract void Destroy();

    public string text => textMesh.text;
    public Color color { get { return meshRenderer.material.color; } set { meshRenderer.material.color = value; } }
    public Vector3 position { get { return container.transform.position; } protected set { container.transform.position = value; } }

    protected GameObject quad { get; }
    protected TextMesh textMesh { get; }
    protected GameObject container { get; }
    protected MeshRenderer meshRenderer { get; }

    public MouseObject( string name )
    {
        container = new GameObject( name );
        quad = GameObject.CreatePrimitive( PrimitiveType.Quad );
        quad.transform.SetParent( container.transform );
        meshRenderer = quad.GetComponent<MeshRenderer>();
        meshRenderer.material = Entry.instance.unlitColor;
        textMesh = new GameObject( name ).AddComponent<TextMesh>();
        textMesh.transform.SetParent( container.transform );
        textMesh.alignment = TextAlignment.Center;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.characterSize = 0.15f;
        textMesh.color = Color.black;
        textMesh.fontSize = 35;
    }
}