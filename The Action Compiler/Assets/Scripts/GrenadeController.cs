using System.Collections;
using UnityEngine;
using System;

public class GrenadeController : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] explosionParticles;

    public static Action<int> GetDamage;

    private int damageToDeal;

    private void OnEnable()
    {
        GetDamage += ReceivedDamage;
    }

    private void OnDisable()
    {
        GetDamage -= ReceivedDamage;
    }

    private void ReceivedDamage(int damage)
    {
        damageToDeal = damage;
    }

    private void DealDamage(int damage)
    {
        Boss.BossTakeDamage?.Invoke(damage);

        StartCoroutine(WaitToDestroy());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Floor")
        {
            foreach (ParticleSystem particles in explosionParticles)
            {
                particles.Play();
            }

            DealDamage(damageToDeal);
        }
    }

    private IEnumerator WaitToDestroy()
    {
        yield return new WaitForSeconds(1f);

        Destroy(gameObject);
    }
}
