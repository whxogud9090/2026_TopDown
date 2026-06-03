using UnityEngine;

public class ExperienceGem : MonoBehaviour
{
    public int value = 1;
    public float pickupDistance = 0.55f;
    public float magnetDistance = 3.0f;
    public float magnetSpeed = 7f;
    public float rotateSpeed = 120f;

    private Transform player;

    private void Start()
    {
        var playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
            player = playerObject.transform;
    }

    private void Update()
    {
        if (player == null)
            return;

        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);

        var distance = Vector2.Distance(transform.position, player.position);
        if (distance <= pickupDistance)
        {
            SurvivorsGameManager.Instance.AddExperience(value);
            Destroy(gameObject);
            return;
        }

        if (distance <= magnetDistance)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, magnetSpeed * Time.deltaTime);
        }
    }
}
