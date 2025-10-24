using UnityEngine;

public class DiffusionLimitedAggregation : MonoBehaviour
{
    public int cells = 100;
    public float minDist = 1, maxDist = 10;
    public DiffusionCell cellPrefab;

    void Start() {
        Apply();
    }

    void Apply() {
        DiffusionCell center = Instantiate(cellPrefab, transform.position, Quaternion.identity, transform);
        center.partOfCluster = true;
        center.center = center;
        for (int i = 0; i < cells; ++i) {
            Vector3 pos = Vector3.forward;
            pos = Quaternion.Euler(0, Random.Range(0f, 360f), 0) * pos * Random.Range(minDist, maxDist);
            DiffusionCell cell = Instantiate(cellPrefab, pos, Quaternion.identity, transform);
            cell.center = center;
            cell.transform.localScale *= Random.Range(0.5f, 2f);
        }
    }
}
