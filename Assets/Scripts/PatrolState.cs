using UnityEngine;

public class PatrolState : IState
{
    private HunterNPC _hunter;
    private int _currentWaypointIndex = 0;
    private float _spawnTimer = 0f;

    public PatrolState(HunterNPC hunter) { _hunter = hunter; }

    public void Enter() { }

    public void Execute()
    {
        if (_hunter.GetClosestBoid(requiresDead: true) != null)
        {
            _hunter.FSM.ChangeState(_hunter.GatherState);
            return;
        }

        if (_hunter.TimeSinceLastAttack >= _hunter.TBA && _hunter.GetClosestBoid(requiresDead: false) != null)
        {
            _hunter.FSM.ChangeState(_hunter.AttackState);
            return;
        }

        Transform targetWP = _hunter.Waypoints[_currentWaypointIndex];
        _hunter.transform.position = Vector3.MoveTowards(_hunter.transform.position, targetWP.position, _hunter.Speed * Time.deltaTime);

        if (Vector3.Distance(_hunter.transform.position, targetWP.position) < 0.5f)
        {
            _currentWaypointIndex = (_currentWaypointIndex + 1) % _hunter.Waypoints.Length; 
        }

        _spawnTimer += Time.deltaTime;
        if (_spawnTimer >= 5f && _hunter.ActiveObjects.Count < 5)
        {
            _spawnTimer = 0;
            GameObject newObj = GameObject.Instantiate(_hunter.ObjectOfInterestPrefab, _hunter.transform.position, Quaternion.identity);
            _hunter.ActiveObjects.Add(newObj);
        }
    }

    public void Exit() { }
}