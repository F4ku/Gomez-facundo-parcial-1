using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HunterNPC : MonoBehaviour
{
    public StateMachine FSM { get; private set; }
    public PatrolState PatrolState { get; private set; }
    public AttackState AttackState { get; private set; }
    public GatherState GatherState { get; private set; }

    [Header("Parámetros FSM")]
    public float TBA = 3f;
    public float TimeSinceLastAttack;
    public float RangeAttackRadius = 10f;
    public float MeleeAttackRadius = 2f;
    public float PerceptionRadius = 15f;

    [Header("Patrullaje y Objetos")]
    public Transform[] Waypoints;
    public float Speed = 5f;
    public GameObject ObjectOfInterestPrefab;
    public List<GameObject> ActiveObjects = new List<GameObject>();

    [Header("Feedback Visual")]
    public TextMeshProUGUI StateText;

    private void Awake()
    {
        FSM = new StateMachine();
        PatrolState = new PatrolState(this);
        AttackState = new AttackState(this);
        GatherState = new GatherState(this);
        TimeSinceLastAttack = TBA; 
    }

    private void Start()
    {
        FSM.ChangeState(PatrolState);
    }

    private void Update()
    {
        TimeSinceLastAttack += Time.deltaTime;
        FSM.Update();


        ActiveObjects.RemoveAll(item => item == null);

        if (StateText != null)
            StateText.text = $"State: {FSM.CurrentState.GetType().Name}";
    }

    public BoidAgent GetClosestBoid(bool requiresDead)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, PerceptionRadius);
        BoidAgent closest = null;
        float minDistance = Mathf.Infinity;

        foreach (var hit in hits)
        {
            BoidAgent boid = hit.GetComponent<BoidAgent>();
            if (boid != null && boid.IsDead == requiresDead)
            {
                float dist = Vector3.Distance(transform.position, boid.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closest = boid;
                }
            }
        }
        return closest;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, PerceptionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, RangeAttackRadius);
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, MeleeAttackRadius);
    }
}