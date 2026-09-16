using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidAgent : MonoBehaviour
{
    [Header("Atributos")]
    public float MaxSpeed = 4f;
    public float MaxForce = 2f;
    public float Health = 100f;
    public bool IsDead { get; private set; } = false;

    [Header("Percepción")]
    public float CohesionAlignmentRadius = 5f;
    public float SeparationRadius = 2f;
    public float EvadeRadius = 15f;
    public float ObjectPerceptionRadius = 10f;

    [Header("Referencias")]
    public HunterNPC Hunter;

    private Vector3 _velocity;
    private MeshRenderer _renderer;
    private float _interactionTimer;

    private void Start()
    {
        _velocity = Random.insideUnitSphere * MaxSpeed;
        _velocity.y = 0;
        _renderer = GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        if (IsDead) return;

        Vector3 acceleration = Vector3.zero;

        if (Vector3.Distance(transform.position, Hunter.transform.position) < EvadeRadius)
        {
            acceleration += Evade(Hunter.transform.position) * 1.5f;
        }
        else
        {
            ObjectOfInterest closestObj = GetClosestObjectInRadius();

            if (closestObj != null)
            {
                acceleration += Arrive(closestObj.transform.position);
                InteractWithObject(closestObj);
            }
            else
            {
                acceleration += Separation() * 1.5f;
                acceleration += Alignment() * 1f;
                acceleration += Cohesion() * 1f;
            }
        }

        if (Vector3.Distance(Vector3.zero, transform.position) > 24f)
        {
            acceleration += Seek(Vector3.zero) * 3f;
        }

        acceleration.y = 0;
        _velocity += acceleration * Time.deltaTime;
        _velocity = Vector3.ClampMagnitude(_velocity, MaxSpeed);
        transform.position += _velocity * Time.deltaTime;

        if (_velocity != Vector3.zero)
            transform.forward = _velocity.normalized;
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        Health -= amount;
        if (Health <= 0)
        {
            Health = 0;
            Die();
        }
    }

    private ObjectOfInterest GetClosestObjectInRadius()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, ObjectPerceptionRadius);
        ObjectOfInterest closest = null;
        float minDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            ObjectOfInterest obj = hit.GetComponent<ObjectOfInterest>();
            if (obj != null)
            {
                float dist = Vector3.Distance(transform.position, obj.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = obj;
                }
            }
        }
        return closest;
    }

    private void InteractWithObject(ObjectOfInterest obj)
    {
        if (Vector3.Distance(transform.position, obj.transform.position) < 2f)
        {
            _interactionTimer += Time.deltaTime;
            if (_interactionTimer >= 1f)
            {
                obj.TakeDamage(10f);
                _interactionTimer = 0f;
            }
        }
    }

    private Vector3 Evade(Vector3 target)
    {
        Vector3 desiredVelocity = (transform.position - target).normalized * MaxSpeed;
        return Vector3.ClampMagnitude(desiredVelocity - _velocity, MaxForce);
    }

    private Vector3 Arrive(Vector3 target)
    {
        Vector3 desiredVelocity = target - transform.position;
        float distance = desiredVelocity.magnitude;
        
        if (distance < 1.5f)
        {
            desiredVelocity = desiredVelocity.normalized * MaxSpeed * (distance / 1.5f);
        }
        else
        {
            desiredVelocity = desiredVelocity.normalized * MaxSpeed;
        }

        return Vector3.ClampMagnitude(desiredVelocity - _velocity, MaxForce);
    }

    private Vector3 Seek(Vector3 target)
    {
        Vector3 desiredVelocity = (target - transform.position).normalized * MaxSpeed;
        return Vector3.ClampMagnitude(desiredVelocity - _velocity, MaxForce);
    }

    private Vector3 Separation()
    {
        Vector3 steering = Vector3.zero;
        int count = 0;
        Collider[] hits = Physics.OverlapSphere(transform.position, SeparationRadius);
        foreach (var hit in hits)
        {
            if (hit.gameObject != gameObject && hit.CompareTag("Boid"))
            {
                Vector3 diff = transform.position - hit.transform.position;
                steering += diff.normalized / diff.magnitude;
                count++;
            }
        }
        if (count > 0)
        {
            steering /= count;
            steering = steering.normalized * MaxSpeed;
            steering -= _velocity;
            steering = Vector3.ClampMagnitude(steering, MaxForce);
        }
        return steering;
    }

    private Vector3 Alignment()
    {
        Vector3 sum = Vector3.zero;
        int count = 0;
        Collider[] hits = Physics.OverlapSphere(transform.position, CohesionAlignmentRadius);
        foreach (var hit in hits)
        {
            if (hit.gameObject != gameObject && hit.CompareTag("Boid"))
            {
         
                sum += hit.GetComponent<BoidAgent>()._velocity;
                count++;
            }
        }
        if (count > 0)
        {
            sum /= count;
            sum = sum.normalized * MaxSpeed;
            Vector3 steer = sum - _velocity;
            return Vector3.ClampMagnitude(steer, MaxForce);
        }
        return Vector3.zero;
    }

    private Vector3 Cohesion()
    {
        Vector3 sum = Vector3.zero;
        int count = 0;
        Collider[] hits = Physics.OverlapSphere(transform.position, CohesionAlignmentRadius);
        foreach (var hit in hits)
        {
            if (hit.gameObject != gameObject && hit.CompareTag("Boid"))
            {
                sum += hit.transform.position;
                count++;
            }
        }
        if (count > 0)
        {
            sum /= count;
            return Arrive(sum);
        }
        return Vector3.zero;
    }

    public void Die()
    {
        IsDead = true;
        _renderer.material.color = Color.gray;
    }

    public void Collect()
    {

        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(5f);

        transform.position = new Vector3(Random.Range(-20f, 20f), 0, Random.Range(-20f, 20f));
        Health = 100f;
        IsDead = false;

        _renderer.material.color = Color.white;

        GetComponent<MeshRenderer>().enabled = true;
        GetComponent<Collider>().enabled = true;
    }
}