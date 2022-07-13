using UnityEngine;
using Random = UnityEngine.Random;
public class HexParticle : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    [SerializeField]
    private Rigidbody2D rigidBody2D;
    public void ParticleInitialize(int ID)
    {
        var randomScale = Random.Range(0.25f, 0.3f);
        var randomRotation = Random.Range(-60, 60);

        transform.localScale = Vector3.one * randomScale;
        transform.eulerAngles = new Vector3(0, 0, randomRotation);
        transform.position -= Vector3.forward * 4;

        rigidBody2D.velocity = new Vector2(Random.Range(-3f, 3f), Random.Range(-2f, 6f));
        spriteRenderer.color = HexagonManager.Instance.hexagonColors[ID];
        
        Destroy(gameObject,2f);

        transform.parent = CanvasManager.Instance.transform;
    }
}
