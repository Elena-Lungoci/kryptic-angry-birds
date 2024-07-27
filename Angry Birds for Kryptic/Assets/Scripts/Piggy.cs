using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piggy : MonoBehaviour
{
    [SerializeField] private float maxHealth = 3f;
    [SerializeField] private float damageTreshold = 0.2f;
    [SerializeField] private float currentHealth;
    [SerializeField] private GameObject piggyDeathParticle;
    [SerializeField] private AudioClip deathClip;

    private void Awake() {
        currentHealth=maxHealth;
    }
    public void DamagePiggy(float damageAmount){
        currentHealth -= damageAmount;

        if(currentHealth <= 0f){
            Die();
        }
    }
    private void Die() {
        GameManager.instance.RemovePiggy(this);
        Instantiate(piggyDeathParticle, transform.position, Quaternion.identity);

        AudioSource.PlayClipAtPoint(deathClip, transform.position);
        Destroy(gameObject);
    }
    private void OnCollisionEnter2D(Collision2D other) {
        float impactVelocity = other.relativeVelocity.magnitude;
        if (impactVelocity > damageTreshold){
            DamagePiggy(impactVelocity);
        }
    }
}
