using System.Collections.Generic;
using UnityEngine;

public class DiamondSquare : MonoBehaviour
{
    public GameObject objectPrefab;
    public float dist;
    public float randMax;
    public int iterations = 1;

    int size;

    void Start() {
        size = 2 * iterations + 1;
        GameObject[,] objects = new GameObject[size, size];
        objects[0, 0] = Instantiate(objectPrefab, new(0, Rand(), 0), Quaternion.identity);
        objects[0, size - 1] = Instantiate(objectPrefab, new(0, Rand(), dist), Quaternion.identity);
        objects[size - 1, 0] = Instantiate(objectPrefab, new(dist, Rand(), 0), Quaternion.identity);
        objects[size - 1, size - 1] = Instantiate(objectPrefab, new(dist, Rand(), dist), Quaternion.identity);
        for (int i = 0; i < iterations; ++i) {
            DiamondStep(objects, i + 1);
            SquareStep(objects, i + 1);
        }
    }

    float Rand() {
        return Random.Range(-randMax, randMax);
    }

    float IndToCoord(int ind) {
        return ind * dist / (size - 1);
    }

    void DiamondStep(GameObject[,] objects, int iter) {
        int step = size / iter;
        for (int x = 0; x < iter; ++x) {
            for (int y = 0; y < iter; ++y) {
                int newX = step * x + (step / 2), newY = step * y + (step / 2);
                float h = (objects[newX - step / 2, newY - step / 2].transform.position.y + objects[newX - step / 2, newY + step / 2].transform.position.y +
                            objects[newX + step / 2, newY - step / 2].transform.position.y + objects[newX + step / 2, newY + step / 2].transform.position.y) / 4;
                objects[newX, newY] = Instantiate(objectPrefab, new(IndToCoord(newX), h + Rand(), IndToCoord(newY)), Quaternion.identity);
            }
        }
    }

    void SquareStep(GameObject[,] objects, int iter) {
        int step = size / iter;
        for (int x = 0; x < size / step; ++x) {
            for (int y = x % 2 == 0 ? 1 : 0; y < size / step; ++y) {
                int newX = x * step, newY = y * step;
                print($"{newX} {newY}");
            }
        }
    }
}
