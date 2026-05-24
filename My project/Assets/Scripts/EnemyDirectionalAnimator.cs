using UnityEngine;

public class EnemyDirectionalAnimator : MonoBehaviour
{
    public Sprite[] spriteUp;
    public Sprite[] spriteDown;
    public Sprite[] spriteLeft;
    public Sprite[] spriteRight;
    public float frameTime = 0.12f;

    private Transform target;
    private SpriteRenderer spriteRenderer;
    private Sprite[] currentSprites;
    private int frameIndex;
    private float timer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentSprites = spriteDown;
        SetFrame(0);
    }

    private void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            target = player.transform;
    }

    private void Update()
    {
        if (target != null)
            PickDirection(target.position - transform.position);

        timer += Time.deltaTime;
        if (timer < frameTime)
            return;

        timer = 0f;
        frameIndex++;
        if (currentSprites != null && currentSprites.Length > 0)
            frameIndex %= currentSprites.Length;
        SetFrame(frameIndex);
    }

    private void PickDirection(Vector3 delta)
    {
        if (delta.sqrMagnitude < 0.001f)
            return;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            ChangeSprites(delta.x > 0f ? spriteRight : spriteLeft);
        else
            ChangeSprites(delta.y > 0f ? spriteUp : spriteDown);
    }

    private void ChangeSprites(Sprite[] sprites)
    {
        if (sprites == null || sprites.Length == 0 || currentSprites == sprites)
            return;

        currentSprites = sprites;
        frameIndex = 0;
        timer = 0f;
        SetFrame(0);
    }

    private void SetFrame(int index)
    {
        if (spriteRenderer == null || currentSprites == null || currentSprites.Length == 0)
            return;

        spriteRenderer.sprite = currentSprites[Mathf.Clamp(index, 0, currentSprites.Length - 1)];
    }
}
