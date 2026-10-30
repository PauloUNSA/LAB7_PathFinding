using UnityEngine;
using Pathfinding;

public class EnemyMovement : MonoBehaviour
{
    public AIPath aIPath;
    Vector2 direction;

    void Update()
    {
        faceVelocity();
    }

    void faceVelocity()
    {
        direction = aIPath.desiredVelocity;
        transform.right = direction;
    }
}
