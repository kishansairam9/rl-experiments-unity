using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacles: MonoBehaviour
{
    public float velocityChangeProbability = 0.003f;
    public float maxSpeed = 0.4f;
    private Vector2 velocity = Vector2.zero;
    private Rigidbody rigidBody;
    public void Start()
    {
        rigidBody = transform.GetComponent<Rigidbody>();
        rigidBody.centerOfMass = Vector3.zero;
        rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
    }

    public void Update()
    {
        if (Random.Range(0f, 1f) < velocityChangeProbability)
        {
            SetRandomVelocity();
        }
        Move();
    }

    public void Move()
    {
        rigidBody.velocity = new Vector3(velocity.x, 0f, velocity.y);
    }

    public void SetRandomVelocity()
    {
        var theta = Random.Range(-Mathf.PI, Mathf.PI);
        var direction = new Vector2(Mathf.Cos(theta), Mathf.Sin(theta));
        var speed = Random.Range(0, maxSpeed);
        this.velocity = direction * maxSpeed;
        print(gameObject.name + " " + velocity);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name != "Floor")
        {
            SetRandomVelocity();
            print(gameObject.name + " collided with " + collision.gameObject.name); 
        }
    }
}
