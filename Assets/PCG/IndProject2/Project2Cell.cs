using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Project2Cell : MonoBehaviour
{
    public Color rockColor = Color.black, emptyColor = Color.white;
    
    SpriteRenderer sprite;
    public bool isRock = false;
    public int x, y;

    void Awake() {
        sprite = GetComponent<SpriteRenderer>();
        sprite.color = emptyColor;
    }

    public void SetRock(bool isRock) {
        this.isRock = isRock;
        if (isRock) sprite.color = rockColor;
        else sprite.color = emptyColor;
    }
}
