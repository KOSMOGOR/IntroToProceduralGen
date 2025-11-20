using UnityEngine;

public class MidiNote : MonoBehaviour
{
    public float speed = 10f;
    public float timeToLive = 3f;

    float timeLiving = 0f;

    void FixedUpdate() {
        timeLiving += Time.fixedDeltaTime;
        if (timeLiving >= timeToLive) {
            Destroy(gameObject);
            return;
        }
        transform.position += new Vector3(0, speed * Time.fixedDeltaTime, 0);
    }
}
