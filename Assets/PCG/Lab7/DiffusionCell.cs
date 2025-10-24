using UnityEngine;

public class DiffusionCell : MonoBehaviour
{
    public float speed = 0.5f;

    public DiffusionCell center;
    public bool partOfCluster = false;

    void FixedUpdate() {
        if (partOfCluster || !center) return;
        Vector3 dir = (center.transform.position - transform.position).normalized;
        dir = Quaternion.Euler(0, Random.Range(-90f, 90f), 0) * dir * (Random.Range(speed / 10, speed) * Time.fixedDeltaTime);
        transform.Translate(dir);
    }

    void OnTriggerEnter(Collider other) {
        DiffusionCell otherCell = other.GetComponent<DiffusionCell>();
        print("trigger");
        if (!otherCell || partOfCluster) return;
        if (otherCell.partOfCluster) partOfCluster = true;
    }
}
