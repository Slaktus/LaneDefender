﻿using System.Collections.Generic;
using UnityEngine;
using System;

public class Lane
{
    public void Update() => Updater?.Invoke();

    public T Add<T>( T laneObject ) where T : LaneObject
    {
        Updater += laneObject.Update;
        objects.Add( laneObject );
        return laneObject;
    }

    public void Remove<T>( T laneObject ) where T : LaneObject
    {
        Updater -= laneObject.Update;
        objects.Remove( laneObject );
    }

    public void Show() => _quad.SetActive( true );
    public void Hide() => _quad.SetActive( false );

    public bool Contains( Vector3 position ) => _rect.Contains( new Vector2( position.x , position.z ) );

    public int Count<T>()
    {
        int count = 0;

        for ( int i = 0 ; objects.Count > i ; i++ )
            if ( objects[ i ] is T )
                count++;

        return count;
    }

    public void Clear<T>()
    {
        List<int> toClear = new List<int>( objects.Count );

        for ( int i = 0 ; objects.Count > i ; i++ )
            if ( objects[ i ] is T )
                toClear.Add( i );

        for ( int i = 0 ; toClear.Count > i ; i++ )
            objects[ toClear[ i ] ].Destroy();
    }

    public void Clear()
    {
        while ( objects.Count > 0 )
            objects[ objects.Count - 1 ].Destroy();
    }

    public void Destroy()
    {
        Clear();
        GameObject.Destroy( _quad );
    }

    public Color color { get { return _meshRenderer.material.color; } set { _meshRenderer.material.color = value; } }
    public Vector3 start => new Vector3( _rect.xMin , 0 , _rect.yMin + ( height * 0.5f ) );
    public Vector3 end => new Vector3( _rect.xMax , 0 , _rect.yMin + ( height * 0.5f ) );
    public float width => _rect.width;
    public float height => _rect.height;
    public float speed => stage.speed;

    public Stage stage { get; }
    public List<LaneObject> objects { get; }

    private Rect _rect { get; set; }
    private GameObject _quad { get; }
    private MeshRenderer _meshRenderer { get; }
    private event Action Updater;

    public Lane( Stage stage , float depth , float width , float height , string name , GameObject parent )
    {
        this.stage = stage;
        objects = new List<LaneObject>();
        _rect = new Rect( 0 , -depth , width , height );

        _quad = GameObject.CreatePrimitive( PrimitiveType.Quad );
        _quad.transform.localRotation = Quaternion.Euler( 90 , 0 , 0 );
        _quad.transform.localPosition = new Vector3( width * 0.5f , 0 , -depth + ( height * 0.5f ) );
        _quad.transform.localScale = new Vector3( width , height , 0 );
        _quad.transform.name = name;

        _quad.GetComponent<MeshCollider>().enabled = false;
        _meshRenderer = _quad.GetComponent<MeshRenderer>();
        _meshRenderer.material = Entry.instance.unlitColor;

        _quad.transform.SetParent( parent.transform );
    }
}
