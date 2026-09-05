using UnityEngine;

public class Nextbot : MonoBehaviour
{
    private UI ui;
    public float speed;
    private Vector3 lookDirection;
    public Timer timer;
    void Start()
    {
        ui = FindFirstObjectByType<UI>();
        timer = FindAnyObjectByType<Timer>();
    }
    void Update()
    {
        if(timer.time > 0) return;

        lookDirection = new Vector3(transform.position.x - 
            ui.transform.position.x, 0, transform.position.z - ui.transform.position.z);
        transform.rotation = Quaternion.LookRotation(lookDirection);

        transform.position = Vector3.MoveTowards(transform.position, 
            new Vector3(ui.transform.position.x, transform.position.y,
            ui.transform.position.z), speed);
        transform.position = new Vector3(transform.position.x,
            ui.transform.position.y - 2, transform.position.z);

        if(Physics.Raycast(transform.position + Vector3.up,
            (ui.transform.position - transform.position).normalized,
            out RaycastHit hit, 2f))
        {
            Debug.Log(hit.transform.name);
            IDamageable damageable = hit.transform.GetComponentInParent<IDamageable>();
            damageable?.Damage(100);
        }
    }
}
