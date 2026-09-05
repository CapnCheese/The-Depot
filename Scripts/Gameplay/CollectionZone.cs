using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System;
public class CollectionZone : MonoBehaviour
{
    private HashSet<GameObject> _objectsInside = new HashSet<GameObject>();
    public int Count => _objectsInside.Count;
    public UI ui;
    public void Register(GameObject obj) => _objectsInside.Add(obj);
    public void Unregister(GameObject obj) => _objectsInside.Remove(obj);

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Box"))
            _objectsInside.Add(other.gameObject);
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Box"))
            _objectsInside.Remove(other.gameObject);
    }
    public void Start()
    {
        ui = FindFirstObjectByType<UI>();
    }
    public void Update()
    {
        Debug.Log("Box Count: " + _objectsInside.Count);
    }
}