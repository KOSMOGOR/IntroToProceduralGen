using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class EvolutionaryDungeon : MonoBehaviour
{
    public GameObject tilePrefab;
    public int x = 10, y = 10;
    public int iterations = 100;
    public int populationSize = 100;
    public float selectBest = 0.8f;

    bool[,] tiles;
    List<bool[,]> population;

    void Start() {
        CreateDungeon();
        DrawDungeon();
    }

    void CreateDungeon() {
        population = new();
        for (int i = 0; i < populationSize; ++i) {
            bool[,] arr = new bool[x, y];
            for (int x1 = 0; x1 < x; x1++)
                for (int y1 = 0; y1 < y; y1++)
                    arr[x1, y1] = Random.value <= 0.5;
        }
        for (int iter = 0; iter < iterations; ++iter) {
            population.OrderBy(arr => Mathf.Abs(CountClusters(arr) - 3));
            
        }
    }

    int CountClusters(bool[,] arr) {
        List<List<Vector2Int>> clusters = new();
        for (int x1 = 0; x1 < x; x1++)
            for (int y1 = 0; y1 < y; y1++)
                if (arr[x1, y1] && !clusters.Any(cl => cl.Contains(new(x1, y1)))) clusters.Add(GetCluster(arr, x1, y1));
        return clusters.Count;
    }

    List<Vector2Int> GetCluster(bool[,] arr, int x1, int y1) {
        List<Vector2Int> cluster = new() {new(x1, y1)};
        HashSet<Vector2Int> toCheck = new() {cluster[0]};
        while (toCheck.Count > 0) {
            foreach (Vector2Int point in toCheck) {
                toCheck.Remove(point);
                if (cluster.Contains(point)) continue;
                foreach (Vector2Int point1 in new List<Vector2Int>() {point + Vector2Int.left, point + Vector2Int.right, point + Vector2Int.up, point + Vector2Int.down}) {
                    if (point1.x < 0 || point1.x >= x || point1.y < 0 || point1.y >= y || !arr[point1.x, point1.y] || cluster.Contains(point1)) continue;
                    cluster.Add(point1);
                    toCheck.Add(point1);
                }
            }
        }
        return cluster;
    }

    private void DrawDungeon() {
        throw new NotImplementedException();
    }
}