using UnityEngine;
using StarterAssets;

public class PlayerAttackController : MonoBehaviour
{
    private Animator _animator;
    private StarterAssetsInputs _input;
    private int _attackStep = 0; // Tracks which punch we are on
    private float _lastAttackTime;
    public float comboResetTime = 1.0f; // Reset combo if you wait too long

    void Start()
    {
        _animator = GetComponent<Animator>();
        _input = GetComponent<StarterAssetsInputs>();
    }

    void Update()
    {
        // Reset combo to the first punch if the player waits too long
        if (Time.time - _lastAttackTime > comboResetTime)
        {
            _attackStep = 0;
        }

        if (_input.attack)
        {
            PerformAttack();
            _input.attack = false;
        }
    }

    void PerformAttack()
    {
        _lastAttackTime = Time.time;

        _animator.SetInteger("AttackIndex", _attackStep);
        _animator.SetTrigger("Attack");

        _attackStep++;

        // CHANGE THIS LINE: 
        // We now have 3 moves (0, 1, and 2), so reset if it goes above 2.
        if (_attackStep > 2) 
        {
            _attackStep = 0;
        }
    }
}