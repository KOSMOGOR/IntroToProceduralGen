using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Rock : MonoBehaviour
{
    public Color rockColor = Color.black, emptyColor = Color.white;
    
    SpriteRenderer sprite;
    public bool isRock = false;

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
