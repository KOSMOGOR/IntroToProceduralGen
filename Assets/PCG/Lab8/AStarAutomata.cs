using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AStarAutomata : MonoBehaviour
{
    public int x1 = 0, y1 = 0;
    public int x2 = 49, y2 = 49;
    public CelluralAutomata automata;

    bool constructed = false;

    void Update() {
        if (constructed || !automata.isFinished) return;
        constructed = true;
        List<Vector2> path = PerformAStar(automata, x1, y1, x2, y2);
        print($"Path size: {path.Count}");
        if (path.Count == 0) return;
        foreach (Vector2 v in path) {
            automata.rocks[(int)v.x, (int)v.y].GetComponent<SpriteRenderer>().color = Color.green;
        }
    }

    static int PathCost(int xFrom, int yFrom, int xTo, int yTo) {
        int min = Mathf.Min(Mathf.Abs(xFrom - xTo), Mathf.Abs(yFrom - yTo));
        int max = Mathf.Max(Mathf.Abs(xFrom - xTo), Mathf.Abs(yFrom - yTo));
        return min * 14 + (max - min) * 10;
    }

    static int PathCost(Vector2 v1, Vector2 v2) {
        return PathCost((int)v1.x, (int)v1.y, (int)v2.x, (int)v2.y);
    }

    static int H(int x, int y, int x2, int y2) {
        return PathCost(x, y, x2, y2);
    }

    static List<Vector2> ReconstructPath(Vector2 current, Dictionary<Vector2, Vector2> cameFrom) {
        List<Vector2> totalPath = new() {current};
        while (cameFrom.ContainsKey(current)) {
            current = cameFrom[current];
            totalPath.Add(current);
        }
        totalPath.Reverse();
        return totalPath;
    }

    static List<Vector2> PerformAStar(CelluralAutomata automata, int x1, int y1, int x2, int y2) {
        Dictionary<Vector2, Vector2> cameFrom = new();
        Rock[,] rocks = automata.rocks;
        HashSet<Vector2> openSet = new() {new(x1, y1)};
        Dictionary<Vector2, int> gScore = new() { [new(x1, y1)] = 0 };
        Dictionary<Vector2, int> fScore = new() { [new(x1, y1)] = H(x1, y1, x2, y2) };
        int I = 0;
        while (openSet.Count > 0) {
            Vector2 current = openSet.OrderBy(vec => fScore[vec]).ToList()[0];
            if (current.x == x2 && current.y == y2) return ReconstructPath(current, cameFrom);
            openSet.Remove(current);
            for (int xDelta = -1; xDelta <= 1; ++xDelta) {
                for (int yDelta = -1; yDelta <= 1; ++yDelta) {
                    Vector2 neig = current + new Vector2(xDelta, yDelta);
                    if (neig == current || neig.x < 0 || neig.y < 0 || neig.x >= automata.x || neig.y >= automata.y || rocks[(int)neig.x, (int)neig.y].isRock) continue;
                    int tgScore = gScore[current] + PathCost(current, neig);
                    if (!gScore.ContainsKey(neig) || tgScore < gScore[neig]) {
                        cameFrom[neig] = current;
                        gScore[neig] = tgScore;
                        fScore[neig] = tgScore + H((int)neig.x, (int)neig.y, x2, y2);
                        if (!openSet.Contains(neig)) openSet.Add(neig);
                    }
                }
            }
            if (++I == (Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2)) * 2) break;
        }
        return new();
    }
}
