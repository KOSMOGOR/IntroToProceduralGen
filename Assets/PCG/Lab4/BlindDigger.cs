using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlindDigger : MonoBehaviour
{
    public int numOfSteps = 10;
    public float stepSize = 1f;
    public GameObject roomPrefab;
    public Color roomColor;

    public float probChangeDir = 0.05f;
    public float probChangeDirInc = 0.05f;
    public float probPlaceRoom = 0.05f;
    public float probPlaceRoomInc = 0.05f;
    public float roomSizeMin = 3f, roomSizeMax = 7f;
    public float roomZ = -1f;

    Vector2 dir = new(1, 0);
    Vector2[] possibleDirs = {
        new(1, 0),
        new(-1, 0),
        new(0, 1),
        new(0, -1)
    };

    void Start() {
        ApplyBlindDig();
    }

    void ApplyBlindDig() {
        System.Random dirRand = new();
        System.Random roomRand = new();
        for (int i = 0; i < numOfSteps; ++i) {
            if (dirRand.NextDouble() < probChangeDir) {
                dir = possibleDirs.Where(x => x != dir).OrderBy(_ => UnityEngine.Random.value).First();
                probChangeDir = 0;
            } else {
                probChangeDir += probChangeDirInc;
            }
            if (roomRand.NextDouble() < probPlaceRoom) {
                GameObject room = Instantiate(roomPrefab, transform.position + new Vector3(0, 0, roomZ), Quaternion.identity);
                room.transform.localScale = new(
                    UnityEngine.Random.Range(roomSizeMin, roomSizeMax),
                    UnityEngine.Random.Range(roomSizeMin, roomSizeMax),
                    room.transform.localScale.z
                );
                room.name = $"Room {transform.position.x} {transform.position.y}";
                room.GetComponent<SpriteRenderer>().color = roomColor;
                probPlaceRoom = 0;
            } else {
                probPlaceRoom += probPlaceRoomInc;
            }
            GameObject path = Instantiate(roomPrefab, transform.position, Quaternion.identity);
            path.name = $"Path {transform.position.x} {transform.position.y}";
            transform.position += (Vector3)dir;
        }
    }
}
