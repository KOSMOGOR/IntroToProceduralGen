using System.Collections;
using UnityEngine;

public class CelluralAutomata : MonoBehaviour
{
    public int x = 50, y = 50;
    public int iterations = 50;
    public int rockThreshhold = 4;
    public float rockSize = 0.5f;
    public Rock rockPrefab;
    public float waitSeconds = 0.1f;

    [HideInInspector] public Rock[,] rocks;
    [HideInInspector] public bool isFinished = false;

    void Start() {
        rocks = new Rock[x, y];
        for (int x1 = 0; x1 < x; ++x1) {
            for (int y1 = 0; y1 < y; ++y1) {
                rocks[x1, y1] = Instantiate(rockPrefab, transform);
                rocks[x1, y1].transform.localPosition = Vector3.right * (x1 * rockSize) + Vector3.down * (y1 * rockSize) +
                        Vector3.left * (x * rockSize / 2) + Vector3.up * (y * rockSize / 2);
                rocks[x1, y1].transform.localScale *= rockSize;
                if (Random.value <= 0.5f) rocks[x1, y1].SetRock(true);
            }
        }
        StartCoroutine(IterCoroutine());
    }

    IEnumerator IterCoroutine() {
        for (int i = 0; i < iterations; ++i) {
            yield return Iter();
            print($"After iter {i + 1}");
            yield return new WaitForSeconds(waitSeconds);
        }
        isFinished = true;
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

    IEnumerator Iter() {
        bool[,] rocksStatuses = new bool[x, y];
        for (int x1 = 0; x1 < x; ++x1)
            for (int y1 = 0; y1 < y; ++y1)
                rocksStatuses[x1, y1] = rocks[x1, y1].isRock;
        for (int x1 = 0; x1 < x; ++x1) {
            for (int y1 = 0; y1 < y; ++y1) {
                bool newStatus = CountAroundRocks(rocksStatuses, x1, y1) >= rockThreshhold;
                rocks[x1, y1].SetRock(newStatus);
            }
        }
        yield return null;
    }

    public Rock[,] GetRocks() {
        return rocks;
    }
}
