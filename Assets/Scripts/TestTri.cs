using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTri : MonoBehaviour
{
    public float jumpPower = 0;
    public bool isJumpReady;
    public float powerSpeed;
    public float maxJumpPower;

    Rigidbody2D rig;
    LineRenderer line;
    private void Awake()
    {
        rig = GetComponent<Rigidbody2D>();
        line = GetComponentInChildren<LineRenderer>();
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            jumpPower += Time.deltaTime * powerSpeed;
            if (jumpPower > maxJumpPower) jumpPower = 0f;

            PredictTrajectory(transform.position, (Vector3.right * 3f + Vector3.up) * jumpPower);

            isJumpReady = true;
        }
        else if (isJumpReady && Input.GetMouseButtonUp(0))
        {
            rig.AddForce((Vector3.right * 3f + Vector3.up) * jumpPower,ForceMode2D.Impulse);
            jumpPower = 0;
            isJumpReady = false;
        }
    }

    void PredictTrajectory(Vector3 startPos, Vector3 vel)
    {
        int step = 60;
        float deltaTime = Time.fixedDeltaTime;
        Vector3 gravity = Physics.gravity*rig.gravityScale;

        Vector3 position = startPos;
        Vector3 velocity = vel/rig.mass;

        line.positionCount = step;

        for (int i = 0; i < step; i++)
        {
            position += velocity * deltaTime + 0.5f * gravity * deltaTime * deltaTime;
            velocity += gravity * deltaTime;

            line.SetPosition(i, position);
        }
    }
}
