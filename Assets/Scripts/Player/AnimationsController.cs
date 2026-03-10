using UnityEngine;

public class AnimationsController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    
    public void ExecuteAnimationOfWeapon(Animator animatorOfWeapon)
    {
        animatorOfWeapon.SetTrigger("Attack");
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
