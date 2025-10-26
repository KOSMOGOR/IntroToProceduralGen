using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

using Clusters = System.Collections.Generic.List<System.Collections.Generic.HashSet<Project2Cell>>;

public class CelluralAutomataDigger : MonoBehaviour
{
    [Header("Automata")]
    public int x = 50;
    public int y = 50;
    public float rockSpawnChance = 0.5f;
    public int automataIterations = 5;
    public int emptyToRockThreshhold = 4; // minimum rocks around for empty cell to become rock
    public int rockToRockThreshhold = 1; // minimum rocks around for rock to stay rock
    public float cellSize = 0.5f;
    public Project2Cell cellPrefab;
    public float waitSecondsAutomata = 0.25f;
    [Header("Diggers")]
    public int diggerIterations = 3;
    public float waitSecondsDigger = 0.1f;
    public float probChangeDir = 0.05f;
    public float probChangeDirInc = 0.05f;
    public float probDig = 0.25f;
    public int maxDiggerMoves = 20;

    [HideInInspector] public Project2Cell[,] cells;

    void Start() {
        StartCoroutine(CreateMap());
    }

    IEnumerator CreateMap() {
        SpawnRocks();
        yield return new WaitForSeconds(waitSecondsAutomata);
        for (int i = 0; i < automataIterations; ++i) {
            yield return AutomataIter();
            yield return new WaitForSeconds(waitSecondsAutomata);
        }
        print("Finished automata");
        for (int i = 0; i < diggerIterations; ++i) {
            GenerateClusters();
            if (clusters.Count <= 1) break;
            yield return DoDiggers();
            yield return AutomataIter();
        }
        print("Finished diggers");
        yield return new WaitForSeconds(waitSecondsAutomata);
        for (int i = 0; i < automataIterations; ++i) {
            yield return AutomataIter();
            yield return new WaitForSeconds(waitSecondsAutomata);
        }
        print("Finished automata 2");
    }

    void SpawnRocks() {
        cells = new Project2Cell[x, y];
        for (int x1 = 0; x1 < x; ++x1) {
            for (int y1 = 0; y1 < y; ++y1) {
                Project2Cell cell = Instantiate(cellPrefab, transform);
                cell.transform.localPosition = transform.right * (x1 * cellSize) - transform.forward * (y1 * cellSize) - // main position
                        transform.right * (x * cellSize / 2) + transform.forward * (y * cellSize / 2); // makes cell(x / 2, y / 2) in center
                cell.transform.localScale *= cellSize;
                cell.x = x1; cell.y = y1;
                cells[x1, y1] = cell;
                if (Random.value <= rockSpawnChance) cells[x1, y1].SetRock(true);
            }
        }
    }

    int CountAroundRocks(bool[,] rocks, int x, int y) {
        int n = 0;
        for (int x1 = x - 1; x1 <= x + 1; ++x1) {
            for (int y1 = y - 1; y1 <= y + 1; ++y1) {
                if (!(x1 >= 0 && y1 >= 0 && x1 < this.x && y1 < this.y) || rocks[x1, y1]) ++n;
            }
        }
        return n;
    }

    IEnumerator AutomataIter() {
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

    Clusters clusters;
    IEnumerator DoDiggers() {
        float currentProbChangeDir = probChangeDir;
        List<Coroutine> coroutines = new();
        foreach (var cluster in clusters.Select((c, i) => new {c, i})) {
            // cluster.c.First().GetComponent<SpriteRenderer>().color = Color.green;
            coroutines.Add(StartCoroutine(DiggerIter(cluster.c, cluster.i)));
        }
        foreach (var i in coroutines) yield return i;
    }

    readonly Vector2Int[] possibleMoveDirs = { new(1, 0), new(0, 1), new(-1, 0), new(0, -1) };
    readonly Vector2Int[] digDirs = { new(1, 0), new(0, 1), new(-1, 0), new(0, -1) };
    IEnumerator DiggerIter(HashSet<Project2Cell> cluster, int startIndex) {
        Project2Cell startCell = cluster.SelectRandom();
        int x = startCell.x, y = startCell.y;
        float cProbChangeDir = probChangeDir;
        int dirInd = Random.Range(0, possibleMoveDirs.Length);
        Vector2Int dir = possibleMoveDirs[dirInd];
        int moves = 0;
        while (true) {
            // dig
            cells[x, y].SetRock(false);
            foreach (var i in digDirs) {
                Project2Cell cell = TryGetCell(x + i.x, y + i.y);
                if (cell == null || Random.value > probDig) continue;
                cell.SetRock(false);
                // cell.GetComponent<SpriteRenderer>().color = Color.red;
            }
            // move
            if (x + dir.x < 0 || x + dir.x >= this.x) dir.x = -dir.x;
            if (y + dir.y < 0 || y + dir.y >= this.y) dir.y = -dir.y;
            x += dir.x; y += dir.y;
            // stop condition
            if (++moves == maxDiggerMoves) yield break;
            int newInd = GetClusterIndByCell(cells[x, y]);
            if (newInd != -1 && newInd != startIndex) yield break;
            // try change dir
            if (Random.value <= cProbChangeDir) { // dir changed
                cProbChangeDir = probChangeDir;
                int indAdd = (new[] {1, -1})[Random.Range(0, 2)];
                dirInd = (dirInd + indAdd + possibleMoveDirs.Length) % possibleMoveDirs.Length;
                dir = possibleMoveDirs[dirInd];
            } else { // dir not changed
                cProbChangeDir += probChangeDirInc;
            }
            // wait
            yield return new WaitForSeconds(waitSecondsDigger);
        }
    }

    void GenerateClusters() {
        clusters = new();
        for (int x1 = 0; x1 < x; ++x1) {
            for (int y1 = 0; y1 < y; ++y1) {
                Project2Cell cell = cells[x1, y1];
                if (cell.isRock || IsCellInCluster(cell)) continue;
                clusters.Add(GetCluster(cell));
            }
        }
    }

    HashSet<Project2Cell> GetCluster(Project2Cell initCell) {
        HashSet<Project2Cell> cluster = new() {initCell};
        HashSet<Project2Cell> needToCheck = new() {initCell};
        while (needToCheck.Count > 0) {
            foreach (var cell in needToCheck.ToList()) {
                needToCheck.Remove(cell);
                List<Project2Cell> potentialCells = new() {TryGetCell(cell.x + 1, cell.y), TryGetCell(cell.x - 1, cell.y), TryGetCell(cell.x, cell.y + 1), TryGetCell(cell.x, cell.y - 1)};
                List<Project2Cell> cells = potentialCells.Where(cell => cell != null && !cluster.Contains(cell) && !cell.isRock).ToList();
                needToCheck.AddRange(cells);
                cluster.AddRange(cells);
            }
        }
        return cluster;
    }

    bool IsCellInCluster(Project2Cell cell) {
        return clusters.Any(cluster => cluster.Contains(cell));
    }

    int GetClusterIndByCell(Project2Cell cell) {
        for (int i = 0; i < clusters.Count; ++i) if (clusters[i].Contains(cell)) return i;
        return -1;
    }

    Project2Cell TryGetCell(int x, int y) {
        if (x >= 0 && x < this.x && y >= 0 && y < this.y) return cells[x, y];
        return null;
    }
}

static class Ext {
    public static T SelectRandom<T>(this IEnumerable<T> enumerable) {
        List<T> list = enumerable.ToList();
        return list[Random.Range(0, list.Count)];
    }
}