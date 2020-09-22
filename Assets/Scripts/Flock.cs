using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour
{
    public FlockManager flockManager;
    private float speed;
    private bool turning = false;

    private void Start ()
    {
        speed = Random.Range(flockManager.minSpeed, flockManager.maxSpeed);
    }

    private void Update ()
    {
        if (flockManager.simpleBehaviour) SimpleUpdate();
        else FullUpdate();
    }

    private void SimpleUpdate()
    {
        ApplyRules();
        transform.Translate(0.0f, 0.0f, speed * Time.deltaTime);
    }

    private void FullUpdate()
    {
        Bounds b = new Bounds(flockManager.transform.position, 2.0f * flockManager.moveLimits);
        RaycastHit hit = new RaycastHit();
        Vector3 direction = Vector3.zero;

        if (!b.Contains(transform.position)) //handle getting out of bounds
        {
            turning = true;
            direction = flockManager.transform.position - transform.position;
        }
        else if (Physics.Raycast(transform.position, this.transform.forward * flockManager.avoidCollidersDistance, out hit)) //handle collision avoidance
        {
            turning = true;
            direction = Vector3.Reflect(this.transform.forward, hit.normal);
            if (flockManager.debug) Debug.DrawRay(this.transform.position, this.transform.forward * flockManager.avoidCollidersDistance, Color.red);
        }
        else turning = false;

        if (turning) //adjust path if needed
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), flockManager.rotationSpeed * Time.deltaTime);
        }
        else //random direction or direction calculated based on the flocking rules
        {
            if (Random.Range(0, 100) < 10)
                speed = Random.Range(flockManager.minSpeed, flockManager.maxSpeed);
            if (Random.Range(0, 100) < 20)
                ApplyRules();
        }

        //move agent forward, along its direction
        transform.Translate(0.0f, 0.0f, speed * Time.deltaTime);
    }

    private void ApplyRules()
    {
        GameObject[] gos;
        gos = flockManager.allAgents;

        Vector3 vcentre = Vector3.zero;
        Vector3 vavoid = Vector3.zero;
        float gSpeed = 0.01f;
        float nDistance;
        int groupSize = 0; //group is a small section of the flock that consists of agents close enough to interact with each other

        foreach (GameObject go in gos)
        {
            if(go != this.gameObject)
            {
                nDistance = Vector3.Distance(go.transform.position, this.transform.position);
                if(nDistance <= flockManager.neighbourDistance)
                {
                    vcentre += go.transform.position;
                    groupSize++;

                    if(nDistance < flockManager.avoidDistance)
                    {
                        vavoid = vavoid + (this.transform.position - go.transform.position);
                    }

                    Flock anotherFlock = go.GetComponent<Flock>();
                    gSpeed = gSpeed + anotherFlock.speed;
                }
            }
        }

        if(groupSize > 0)
        {
            vcentre = vcentre/groupSize + (flockManager.goalPos - this.transform.position);
            speed = gSpeed / groupSize;

            Vector3 direction = (vcentre + vavoid) - transform.position;
            if (direction != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), flockManager.rotationSpeed * Time.deltaTime);
        }
    }
}
