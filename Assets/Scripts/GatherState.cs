using UnityEngine;

public class GatherState : IState
{
    private HunterNPC _hunter;
    private BoidAgent _target;
    private float _collectTimer = 0f;

    public GatherState(HunterNPC hunter) { _hunter = hunter; }

    public void Enter()
    {
        _collectTimer = 0f;
        _target = null;
    }

    public void Execute()
    {
        if (_target == null || !_target.IsDead)
        {
            _target = _hunter.GetClosestBoid(requiresDead: true);
        }

        if (_target == null)
        {
            _hunter.FSM.ChangeState(_hunter.PatrolState);
            return;
        }

        float distance = Vector3.Distance(_hunter.transform.position, _target.transform.position);

        if (distance > 1f)
        {
            _hunter.transform.position = Vector3.MoveTowards(_hunter.transform.position, _target.transform.position, _hunter.Speed * Time.deltaTime);
        }
        else
        {
            _collectTimer += Time.deltaTime;
            if (_collectTimer >= 2f) 
            {
                _target.Collect();
                _hunter.FSM.ChangeState(_hunter.PatrolState);
            }
        }
    }

    public void Exit() { }
}