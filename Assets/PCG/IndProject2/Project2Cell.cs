using UnityEngine;

public class Project2Cell : MonoBehaviour
{
    public GameObject emptyObj, rockObj;
    
    public bool isRock = false;
    public int x, y;

    void Awake() {
        SetRock(false);
    }

    public void SetRock(bool isRock) {
        this.isRock = isRock;
        rockObj.SetActive(isRock);
        emptyObj.SetActive(!isRock);
    }
}
