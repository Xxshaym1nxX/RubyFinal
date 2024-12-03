using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class powerUp : MonoBehaviour
{
    public ParticleSystem powerUpParticle;
    void OnTriggerEnter2D(Collider2D other)
    {
        RubyController controller = other.GetComponent<RubyController>();

        if (controller != null)
        {
            Instantiate(powerUpParticle, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            controller.ActivatePowerUp();
            Destroy(gameObject);
        }
    }
}
