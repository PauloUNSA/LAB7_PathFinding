using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerScript : MonoBehaviour
{
    public float speed = 5f;
    public Transform graphics;
    Rigidbody2D body;
    Vector2 movement;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        movement = new Vector2(
            (Input.GetKey(KeyCode.RightArrow) ? 1 : 0) - (Input.GetKey(KeyCode.LeftArrow) ? 1 : 0),
            (Input.GetKey(KeyCode.UpArrow) ? 1 : 0) - (Input.GetKey(KeyCode.DownArrow) ? 1 : 0));
    }

    void FixedUpdate()
    {
        ApplyMovement(movement);
    }

    public void ApplyMovement(Vector2 direction)
    {
        if (body == null) body = GetComponent<Rigidbody2D>();
        direction = Vector2.ClampMagnitude(direction, 1f);
        body.linearVelocity = direction * speed;
        if (direction.sqrMagnitude > 0.001f && graphics != null)
            graphics.right = direction;
    }
}
