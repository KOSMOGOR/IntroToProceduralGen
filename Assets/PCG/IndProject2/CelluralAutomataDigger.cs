using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Clusters = System.Collections.Generic.List<System.Collections.Generic.HashSet<Project2Cell>>;

public class CelluralAutomataDigger : MonoBehaviour
{
    public int x = 50, y = 50;
    public int iterations = 50;
    public int emptyToRockThreshhold = 4; // minimum rocks around for empty cell to become rock
    public int rockToRockThreshhold = 1; // minimum rocks around for rock to stay rock
    public float rockSize = 0.5f;
    public Project2Cell rockPrefab;
    public float waitSeconds = 0.25f;
    public float probChangeDir = 0.05f;
    public float probChangeDirInc = 0.05f;

    [HideInInspector] public Project2Cell[,] cells;

    void Start() {
        StartCoroutine(CreateMap());
    }

    IEnumerator CreateMap() {
        SpawnRocks();
        yield return CellIterCoroutine();
        yield return DiggerIter();
    }

    void SpawnRocks() {
        cells = new Project2Cell[x, y];
        for (int x1 = 0; x1 < x; ++x1) {
            for (int y1 = 0; y1 < y; ++y1) {
                Project2Cell cell = Instantiate(rockPrefab, transform);
                cell.transform.localPosition = Vector3.right * (x1 * rockSize) + Vector3.down * (y1 * rockSize) +
                        Vector3.left * (x * rockSize / 2) + Vector3.up * (y * rockSize / 2);
                cell.transform.localScale *= rockSize;
                cell.x = x1; cell.y = y1;
                cells[x1, y1] = cell;
                if (Random.value <= 0.5f) cells[x1, y1].SetRock(true);
            }
        }
    }

    IEnumerator CellIterCoroutine() {
        for (int i = 0; i < iterations; ++i) {
            yield return CellIter();
            yield return new WaitForSeconds(waitSeconds);
        }
        print("Finished Cellural automata");
    }

    int CountAroundRocks(bool[,] rocks, int x, int y) {
        int n = 0;
        for (int x1 = x - 1; x1 <= x + 1; ++x1) {
            for (int y1 = y - 1; y1 <= y + 1; ++y1) {
                if (x1 >= 0 && y1 >= 0 && x1 < this.x && y1 < this.y && rocks[x1, y1]) ++n;
            }
        }
        return n;
    }

    IEnumerator CellIter() {
        bool[,] rocksStatuses = new bool[x, y];
        for (int x1 = 0; x1 < x; ++x1)
            for (int y1 = 0; y1 < y; ++y1)
                rocksStatuses[x1, y1] = cells[x1, y1].isRock;
        for (int x1 = 0; x1 < x; ++x1) {
            for (int y1 = 0; y1 < y; ++y1) {
                int count = CountAroundRocks(rocksStatuses, x1, y1);
                bool newStatus = false;
                if (!rocksStatuses[x1, y1] && count >= emptyToRockThreshhold) newStatus = true; // if empty - can become rock
                else if (rocksStatuses[x1, y1] && count >= rockToRockThreshhold) newStatus = true; // if rock - can become empty
                cells[x1, y1].SetRock(newStatus);
            }
        }
        yield return null;
    }

    IEnumerator DiggerIter() {
        Clusters clusters = GetClusters();
        float currentProbChangeDir = probChangeDir;
        print(clusters.Count);
        clusters.ForEach(cluster => {
            print(cluster.Count);
            cluster.First().GetComponent<SpriteRenderer>().color = Color.green;
        });
        yield return null;
    }

    Clusters GetClusters() {
        Clusters clusters = new();
        for (int x1 = 0; x1 < x; ++x1) {
            for (int y1 = 0; y1 < y; ++y1) {
                Project2Cell cell = cells[x1, y1];
                if (cell.isRock || IsCellInCluster(clusters, cell)) continue;
                clusters.Add(GetCluster(cell));
            }
        }
        return clusters;
    }

    HashSet<Project2Cell> GetCluster(Project2Cell initCell) {
        HashSet<Project2Cell> cluster = new() {initCell};
        bool changed = true;
        while (changed) {
            changed = false;
            for (int x1 = 0; x1 < x; ++x1) {
                for (int y1 = 0; y1 < y; ++y1) {
                    Project2Cell cell = cells[x1, y1];
                    if (cell.isRock) continue;
                    if (!cluster.Contains(cell) && cluster.Any(cell2 => Mathf.Abs(cell.x - cell2.x) + Mathf.Abs(cell.y - cell2.y) == 1)) {
                        cluster.Add(cell);
                        changed = true;
                    }
                }
            }
        }
        return cluster;
    }

    bool IsCellInCluster(Clusters clusters, Project2Cell cell) {
        return clusters.Any(cluster => cluster.Contains(cell));
    }
}
