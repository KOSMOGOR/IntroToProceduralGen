using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProjDiamondSquare : MonoBehaviour
{
    public MeshFilter plane;

    public GameObject objectPrefab;
    public GameObject treePrefab;
    public float chanceSpawnTree = 0.2f;
    public float dist;
    public float randMin = -1, randMax = 5;
    public int iterations = 1;
    public bool spawnDebugObjects = true;

    int size;

    void Start() {
        GeneratePlane();
    }

    public void GeneratePlane() {
        transform.Cast<Transform>().ToList().ForEach(t => Destroy(t.gameObject)); // removing all children, if regeneration is used
        size = (int)Mathf.Pow(2, iterations) + 1; // size of height matrix
        float[,] heights = new float[size, size]; // height matrix and starting values
        heights[0, 0] = Rand();
        heights[0, size - 1] = Rand();
        heights[size - 1, 0] = Rand();
        heights[size - 1, size - 1] = Rand();
        // performing all Diamond and Square iterations
        for (int i = 0; i < iterations; ++i) {
            DiamondStep(heights, i);
            SquareStep(heights, i);
        }
        List<Vector3> newVertices = new(); // vertices list for new mesh
        List<int> newTriangles = new(); // triangle list for new mesh
        for (int x = 0; x < size; ++x) {
            for (int y = 0; y < size; ++y) {
                // calculating position for new point
                Vector3 pos = transform.position + Vector3.right * (x * dist) + Vector3.forward * (y * dist) + Vector3.up * heights[x, y];
                // spawning debug sphere if needed
                if (spawnDebugObjects) Instantiate(objectPrefab, pos, Quaternion.identity, transform);
                // spawn tree if right height and random is good
                if (0 <= heights[x, y] && heights[x, y] <= 3 && Random.Range(0f, 1f) <= chanceSpawnTree) Instantiate(treePrefab, pos, Quaternion.Euler(new(0, Random.Range(0f, 360f), 0)), transform);
                newVertices.Add(pos); // adding new vertex to list
            }
        }
        // calculating triangles based on vertices
        for (int x = 0; x < size - 1; ++x) {
            for (int y = 0; y < size - 1; ++y) {
                int ind = x + y * size;
                newTriangles.AddRange(new int[] {ind, ind + 1, ind + size + 1});
                newTriangles.AddRange(new int[] {ind, ind + size + 1, ind + size});
            }
        }
        // initializing new mesh with calculated vertices and triangles
        plane.mesh = new() {
            vertices = newVertices.ToArray(),
            triangles = newTriangles.ToArray()
        };
        plane.mesh.RecalculateNormals();
        plane.mesh.RecalculateBounds();
        Material material = plane.GetComponent<MeshRenderer>().material;
        // material.SetFloat("_MinHeight", heights.Cast<float>().Min());
        // material.SetFloat("_MaxHeight", heights.Cast<float>().Max());
    }

    // shortcut for random function with parametres
    float Rand() {
        return Random.Range(randMin, randMax);
    }

    // Diamond step
    void DiamondStep(float[,] heights, int iter) {
        int step = (size - 1) / (int)Mathf.Pow(2, iter); // number of steps
        int numOfSteps = (size - 1) / step; // size of steps
        // calculating new height depending on existing ones
        for (int x = 0; x < numOfSteps; ++x) {
            for (int y = 0; y < numOfSteps; ++y) {
                int newX = step * x + (step / 2), newY = step * y + (step / 2);
                float h = (heights[newX - step / 2, newY - step / 2] + heights[newX - step / 2, newY + step / 2] +
                            heights[newX + step / 2, newY - step / 2] + heights[newX + step / 2, newY + step / 2]) / 4;
                heights[newX, newY] = h + Rand() / (iter + 2);
            }
        }
    }

    // Square step
    void SquareStep(float[,] heights, int iter) {
        int xStep = (size - 1) / (int)Mathf.Pow(2, iter + 1); // number of steps in x direction
        int numOfXSteps = (size - 1) / xStep + 1; // size of steps in x direction
        int yStep = xStep * 2; // number of steps in y direction
        int numOfYSteps = (size - 1) / yStep + 1; // size of steps in y direction
        // calculating adjacent points and new height
        for (int x = 0; x < numOfXSteps; ++x) {
            for (int y = x % 2 == 0 ? 1 : 0; y < numOfYSteps; ++y) {
                int newX = x * xStep, newY = y * yStep - (x % 2 == 0 ? yStep / 2 : 0);
                List<float> currentHeights = new();
                AddIfValid(currentHeights, heights, newX - xStep, newY); 
                AddIfValid(currentHeights, heights, newX + xStep, newY); 
                AddIfValid(currentHeights, heights, newX, newY - xStep); 
                AddIfValid(currentHeights, heights, newX, newY + xStep);
                heights[newX, newY] = currentHeights.Average() + Rand() / (iter + 2);
            }
        }
    }

    // utility function for adding height in given list if coords are valid
    void AddIfValid(List<float> list, float[,] height, int x, int y) {
        if (x >= 0 && x < size && y >= 0 && y < size) list.Add(height[x, y]);
    }
}
