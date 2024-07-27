using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngryBird : MonoBehaviour
{
    [SerializeField] private AudioClip hitClip;
    private Rigidbody2D rb;
    private CircleCollider2D circleCollider;
    private bool hasBeenLaunched;
    private bool shouldFaceVelocityDirection;

    private AudioSource audioSource;
    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        audioSource = GetComponent<AudioSource>();

    }
    private void Start(){
        rb.isKinematic = true;
        circleCollider.enabled = false;
    }
    private void FixedUpdate() { //updates after fixed amount of seconds. at the same time as unity physics system -> physics stuff should go here
         if(hasBeenLaunched &&shouldFaceVelocityDirection){
            transform.right= rb.velocity;
         }
    }
   public void LaunchBird(Vector2 direction, float force){
       
        rb.isKinematic = false;
        circleCollider.enabled = true;
        rb.AddForce(direction * force, ForceMode2D.Impulse);
        hasBeenLaunched=true;
        shouldFaceVelocityDirection = true;

   }

    private void OnCollisionEnter2D(Collision2D other) {
        shouldFaceVelocityDirection = false;
        SoundManager.instance.PLayClip(hitClip, audioSource);
        Destroy(this);
   }
}
