using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseObject
{
    public virtual bool Contains(Vector3 position) => rect.Contains(new Vector2(position.x, position.z));

    public virtual void Update() { }
    public abstract void Destroy();

    public float top => rect.yMax;
    public float bottom => rect.yMin;
    public virtual float back => rect.xMin;
    public virtual float front => rect.xMax;
    public Vector3 topPoint => new Vector3(rect.center.x, 0, top);
    public Vector3 backPoint => new Vector3(back, 0, rect.center.y);
    public Vector3 frontPoint => new Vector3(front, 0, rect.center.y);
    public Vector3 bottomPoint => new Vector3(rect.center.x, 0, bottom);

    public bool valid => container != null;

    public virtual Rect rect => new Rect(position.x - (scale.x * 0.5f), position.z - (scale.z * 0.5f), scale.x, scale.z);
    public virtual Vector3 scale { get { return body.transform.localScale; } protected set { body.transform.localScale = value; } }
    public virtual Vector3 position { get { return container.transform.position; } protected set { container.transform.position = value; } }

    protected Label label { get; }
    protected GameObject body { get; }
    protected GameObject container { get; }
    protected MeshRenderer meshRenderer { get; }

    public BaseObject(string name, GameObject body)
    {
        container = new GameObject(name);
        body.transform.SetParent(container.transform);
        this.body = body;

        meshRenderer = body.GetComponent<MeshRenderer>();
        label = new Label(string.Empty, Color.black, 1, 1, container);
        label.SetLocalRotation(Quaternion.Euler(90, 0, 0));
    }
}
