using UnityEngine;

public class SwordManager : MonoBehaviour
{
    public GameObject backSword;  // The one on Spine2
    public GameObject handSword;  // The one in RightHand
    private Animator _animator;
    private bool _isSwordOut = false;

    void Start()
    {
        _animator = GetComponent<Animator>();
        // Start: Sword on back, not in hand
        backSword.SetActive(true);
        handSword.SetActive(false);
    }

    void Update()
    {
        // Press E to start the animation
        if (Input.GetKeyDown(KeyCode.Q))
        {
            _animator.SetTrigger("Equip");
        }
        if (_isSwordOut)
    {
        // Left Click = Normal Slash
        if (Input.GetMouseButtonDown(0))
        {
            _animator.SetTrigger("Attack1");
        }

        // Key 1 = 2nd Slash
        if (Input.GetKeyDown(KeyCode.Alpha1)) 
        {
            _animator.SetTrigger("Attack2");
        }

        // Key 2 = 3rd Slash
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _animator.SetTrigger("Attack3");
        }
    }

    }

    // This is the "Magic" function the animation will call
    public void SwapSwords()
    {
        Debug.Log("The SwapSwords function just fired!");
        _isSwordOut = !_isSwordOut;
        
        if (_isSwordOut)
        {
            backSword.SetActive(false);
            handSword.SetActive(true);

            _animator.SetBool("isSwordEquipped", _isSwordOut);
        }
        else
        {
            backSword.SetActive(true);
            handSword.SetActive(false);
        }
    }
}