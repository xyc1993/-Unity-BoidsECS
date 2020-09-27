using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    [HideInInspector] public BoidsManager boidsManager;

    private float speed;
    private bool turning = false;

    private void Start ()
    {
        speed = Random.Range(boidsManager.minSpeed, boidsManager.maxSpeed);
    }

    public void SimpleUpdate()
    {
        ApplyRules();
        transform.Translate(0.0f, 0.0f, speed * Time.deltaTime);
    }

    public void FullUpdate()
    {
        Vector3 direction = Vector3.zero;

        if (!boidsManager.bounds.Contains(transform.position)) //handle getting out of bounds
        {
            turning = true;
            direction = boidsManager.transform.position - transform.position;
        }
        else if (Physics.Raycast(transform.position, transform.forward * boidsManager.avoidCollidersDistance, out RaycastHit hit)) //handle collision avoidance
        {
            turning = true;
            direction = Vector3.Reflect(transform.forward, hit.normal);
        }
        else turning = false;

        if (turning) //adjust path if needed
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), boidsManager.rotationSpeed * Time.deltaTime);
        }
        else //random direction or direction calculated based on the flocking rules
        {
            if (Random.Range(0, 100) < 10)
                speed = Random.Range(boidsManager.minSpeed, boidsManager.maxSpeed);
            if (Random.Range(0, 100) < 20)
                ApplyRules();
        }

        //move agent forward, along its direction
        transform.Translate(0.0f, 0.0f, speed * Time.deltaTime);
    }

    private void ApplyRules()
    {
        Vector3 centre = Vector3.zero;
        Vector3 avoid = Vector3.zero;
        float groupSpeed = 0.01f;
        float neighbourDistance;
        int groupSize = 0; //group is a small section of the flock that consists of agents close enough to interact with each other

        foreach (Boid boid in boidsManager.Boids)
        {
            if (boid != this)
            {
                neighbourDistance = Vector3.Distance(boid.transform.position, transform.position);
                if (neighbourDistance <= boidsManager.neighbourDistance)
                {
                    groupSize++;
                    groupSpeed += boid.speed;
                    centre += boid.transform.position;
                    if (neighbourDistance < boidsManager.avoidDistance)
                    {
                        avoid += (transform.position - boid.transform.position);
                    }
                }
            }
        }

        if (groupSize > 0)
        {
            centre = centre/groupSize + (boidsManager.goalPos - transform.position);
            speed = groupSpeed / groupSize;

            Vector3 direction = (centre + avoid) - transform.position;
            if (direction != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), boidsManager.rotationSpeed * Time.deltaTime);
        }
    }
}