using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

using Clusters = System.Collections.Generic.List<System.Collections.Generic.HashSet<Project2Cell>>;

public class CelluralAutomataDigger : MonoBehaviour
{
    [Header("Automata")]
    public int x = 50; // grid size in x direction
    public int y = 50; // grid size in y directon
    public float rockSpawnChance = 0.5f; // chance of each cell at start to become rock (otherwise it will be empty)
    public int automataIterations = 5; // number of Cellural Automata iteration before and after all Blind Diggers iterations
    public int emptyToRockThreshhold = 4; // minimum rocks around for empty cell to become rock
    public int rockToRockThreshhold = 1; // minimum rocks around for rock to stay rock
    public float cellSize = 0.5f; // multiplies for cell size
    public Project2Cell cellPrefab; // prefab of cell
    public float waitSecondsAutomata = 0.25f; // seconds passed between Cellural Automata iterations
    [Header("Diggers")]
    public int diggerIterations = 3; // maximum Blind Diggers iterations
    public float waitSecondsDigger = 0.1f; // seconds passed betweem each Blind Digger moves
    public float probChangeDir = 0.05f; // start probability of Blind Digger change direction
    public float probChangeDirInc = 0.05f; // increase probability of Blide Digger change direction after every failed turn
    public float probDig = 0.25f; // probability for Blind Digger to mine orthogonal cells (calculated independently for every cell)
    public int maxDiggerMoves = 20; // maximum moves that each Blind Digger can make

    [HideInInspector] public Project2Cell[,] cells; // matrix with all cells

    void Start() {
        StartCoroutine(CreateMap()); // starting algotithm
    }

    IEnumerator CreateMap() {
        SpawnRocks();
        yield return new WaitForSeconds(waitSecondsAutomata); // waiting
        for (int i = 0; i < automataIterations; ++i) {
            yield return AutomataIter();
            yield return new WaitForSeconds(waitSecondsAutomata); // waiting
        }
        print("Finished automata");
        for (int i = 0; i < diggerIterations; ++i) {
            GenerateClusters();
            if (clusters.Count <= 1) break;
            yield return DoDiggers();
            yield return AutomataIter();
        }
        print("Finished diggers");
        yield return new WaitForSeconds(waitSecondsAutomata); // waiting
        for (int i = 0; i < automataIterations; ++i) {
            yield return AutomataIter();
            yield return new WaitForSeconds(waitSecondsAutomata); // waiting
        }
        print("Finished automata 2");
    }

    // function for generating for rocks at start
    void SpawnRocks() {
        cells = new Project2Cell[x, y]; // new matrix
        for (int x1 = 0; x1 < x; ++x1) {
            for (int y1 = 0; y1 < y; ++y1) {
                Project2Cell cell = Instantiate(cellPrefab, transform); // instantiating new cell
                cell.transform.localPosition = transform.right * (x1 * cellSize) - transform.forward * (y1 * cellSize) - // main position
                        transform.right * (x * cellSize / 2) + transform.forward * (y * cellSize / 2); // makes cell(x / 2, y / 2) in center
                cell.transform.localScale *= cellSize; // applying scale
                cell.x = x1; cell.y = y1; // assigning coordinates
                cells[x1, y1] = cell;
                if (Random.value <= rockSpawnChance) cells[x1, y1].SetRock(true); // with chance changing type to rock
            }
        }
    }

    // function for counting rocks around some cell (this cell also counts)
    int CountAroundRocks(bool[,] rocks, int x, int y) {
        int n = 0;
        // for every adjucent cell
        for (int x1 = x - 1; x1 <= x + 1; ++x1) {
            for (int y1 = y - 1; y1 <= y + 1; ++y1) {
                if (!(x1 >= 0 && y1 >= 0 && x1 < this.x && y1 < this.y) || rocks[x1, y1]) ++n; // if coordinates are out of screen or this is a rock - add 1
            }
        }
        return n;
    }

    // function for running Cellural Automata iteration
    IEnumerator AutomataIter() {
        // filling cell statuses before changing cell types
        bool[,] rocksStatuses = new bool[x, y];
        for (int x1 = 0; x1 < x; ++x1)
            for (int y1 = 0; y1 < y; ++y1)
                rocksStatuses[x1, y1] = cells[x1, y1].isRock;
        // applying rules for every cell
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
    // function for doing diggers iterations
    IEnumerator DoDiggers() {
        float currentProbChangeDir = probChangeDir;
        List<Coroutine> coroutines = new();
        foreach (var cluster in clusters.Select((c, i) => new {c, i})) {
            coroutines.Add(StartCoroutine(DiggerIter(cluster.c, cluster.i))); // starting Blind Digger for every cluster
        }
        foreach (var i in coroutines) yield return i; // waiting for all to complete
    }

    readonly Vector2Int[] possibleMoveDirs = { new(1, 0), new(0, 1), new(-1, 0), new(0, -1) }; // directions where Blimd Diggers can move
    readonly Vector2Int[] digDirs = { new(1, 0), new(0, 1), new(-1, 0), new(0, -1) }; // cells which Blind Diggers can change to empty
    IEnumerator DiggerIter(HashSet<Project2Cell> cluster, int startIndex) {
        Project2Cell startCell = cluster.SelectRandom(); // selecting random cell in every cluster
        int x = startCell.x, y = startCell.y; // initializing Blind Digger in this cell
        float cProbChangeDir = probChangeDir;
        int dirInd = Random.Range(0, possibleMoveDirs.Length); // choosing start direction
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

    // funtion for checking for empty cells clusters
    void GenerateClusters() {
        clusters = new(); // creating new cluster List
        for (int x1 = 0; x1 < x; ++x1) {
            for (int y1 = 0; y1 < y; ++y1) {
                Project2Cell cell = cells[x1, y1];
                if (cell.isRock || IsCellInCluster(cell)) continue; // if cell is not a rock and not in cluster already - it is in new cluste
                clusters.Add(GetCluster(cell)); // adding new cluster
            }
        }
    }

    // function for finding cluster that includes given cell
    HashSet<Project2Cell> GetCluster(Project2Cell initCell) {
        HashSet<Project2Cell> cluster = new() {initCell}; // initializing cluster
        HashSet<Project2Cell> needToCheck = new() {initCell}; // set of cellls to check adjacent cells
        while (needToCheck.Count > 0) { // if we have cell to check
            foreach (var cell in needToCheck.ToList()) { // for every cell to check
                needToCheck.Remove(cell);
                // getting orthogonal cells that are rocks
                List<Project2Cell> potentialCells = new() {TryGetCell(cell.x + 1, cell.y), TryGetCell(cell.x - 1, cell.y), TryGetCell(cell.x, cell.y + 1), TryGetCell(cell.x, cell.y - 1)};
                List<Project2Cell> cells = potentialCells.Where(cell => cell != null && !cluster.Contains(cell) && !cell.isRock).ToList();
                needToCheck.AddRange(cells); // adding them to cells to check in next iteration
                cluster.AddRange(cells); // adding them to cluster
            }
        }
        return cluster;
    }

    // function for checking if given cell in any cluster
    bool IsCellInCluster(Project2Cell cell) {
        return clusters.Any(cluster => cluster.Contains(cell));
    }

    // function for getting index of cluster given cell belongs
    int GetClusterIndByCell(Project2Cell cell) {
        for (int i = 0; i < clusters.Count; ++i) if (clusters[i].Contains(cell)) return i;
        return -1;
    }

    // function for getting cell if coordinates are correct
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