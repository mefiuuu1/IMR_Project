using UnityEngine;

public class SwordDamage : MonoBehaviour
{
    public float baseDamage = 10f;
    public float velocityMultiplier = 1.2f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        var enemy = collision.collider.GetComponent<EnemyHealth>();
        if (enemy == null) return;

        float speed = rb.velocity.magnitude;
        float finalDamage = baseDamage + speed * velocityMultiplier;

        enemy.TakeDamage(finalDamage);

        if (collision.rigidbody != null)
        {
            Vector3 forceDir = collision.contacts[0].normal * -1f;
            collision.rigidbody.AddForce(forceDir * speed, ForceMode.Impulse);
        }
    }
}
