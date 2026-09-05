using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System;
public class Truck : MonoBehaviour
{
    private HashSet<GameObject> _objectsInside = new HashSet<GameObject>();
    public int Count => _objectsInside.Count;
    public GameObject explosion;
    public Transform door;
    public UI ui;
    public Collider killBox;
    private float targetDoorPosition;
    public bool exploded;
    private bool movementRunning;
    private Vector3 targetPosition;
    private float targetRotation;
    private RaycastHit groundHit;
    private Vector3 groundVector;
    private Quaternion groundAngle;
    private Vector3 flatPos;
    private bool onRoad;
    private float moveSpeed;
    public bool loaded;
    public bool docked;
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
        exploded = false;
        docked = false;
        loaded = false;
        targetDoorPosition = 1;
    }
    public void Update()
    {
        if(exploded) return;

        Debug.Log("Box Count: " + _objectsInside.Count);
        Debug.DrawRay(transform.position, groundVector * 100, Color.purple);
        if(!movementRunning)
        {
            StartCoroutine(Movement());
        }
        Physics.Raycast(transform.position, Vector3.down, out groundHit, 100);
        groundVector = Vector3.ProjectOnPlane(transform.forward, groundHit.normal).normalized;
        groundAngle = Quaternion.LookRotation(groundVector, groundHit.normal);
        
        
        if(Vector3.Distance(transform.position, targetPosition) > 3 && onRoad)
        {
            flatPos = Vector3.MoveTowards(transform.position, 
                new Vector3(targetPosition.x, transform.position.y, targetPosition.z), moveSpeed * Time.deltaTime);
            
            flatPos.y = groundHit.point.y + 0.25f;
            transform.position = flatPos;

            transform.rotation = groundAngle;
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position,
                targetPosition, moveSpeed);
            flatPos.y = targetPosition.y;
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, Quaternion.Euler(new Vector3(0, targetRotation, 0)),
                15 * Time.deltaTime);
        }
        door.localScale = new Vector3(1,
            Mathf.MoveTowards(door.localScale.y, targetDoorPosition, 0.025f), 1);

        if(killBox.bounds.Contains(ui.transform.position) && onRoad)
            StartCoroutine(ui.DeathScreen("splat"));
    }
    IEnumerator Movement()
    {
        movementRunning = true;
        onRoad = true;

        moveSpeed = 15;
        targetPosition = new Vector3(0, 2.3f, 0);
        while(Vector3.Distance(transform.position, targetPosition) > 3) yield return null;

        onRoad = false;
        moveSpeed = 0.04f;
        targetRotation = -270;
        targetPosition = new Vector3(-10, 2.3f, 10.5f);
        yield return new WaitForSeconds(4);
        targetPosition = new Vector3(-25, 2.3f, 9);
        targetRotation = -275;
        yield return new WaitForSeconds(2);
        targetRotation = -270;

        docked = true;
        targetDoorPosition = 0.1f;
        while(!(Count >= 5)) yield return null;
        loaded = true;
        targetDoorPosition = 1;

        yield return new WaitForSeconds(2);

        foreach (GameObject obj in _objectsInside)
        {
            if(obj != null)
                Destroy(obj);
        }
        _objectsInside.Clear();

        targetRotation = -275;
        targetPosition = new Vector3(-10, 2.3f, 10.5f);
        yield return new WaitForSeconds(4);
        targetRotation = -270;
        yield return new WaitForSeconds(3);
        targetRotation = -180;
        targetPosition = new Vector3(0, 2.5f, -5);
        yield return new WaitForSeconds(8);
        moveSpeed = 15;
        onRoad = true;
        targetPosition = new Vector3(0, 10f, -200);
        yield return new WaitForSeconds(5);
        GameObject newExplosion = Instantiate(explosion, transform);
        exploded = true;
    }
}
