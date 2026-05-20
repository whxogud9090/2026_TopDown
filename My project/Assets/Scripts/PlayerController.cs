using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 1f;
    public Sprite[] spriteUp;
    public Sprite[] spriteDown;
    public Sprite[] spriteLeft;
    public Sprite[] spriteRight;
    public float frameTime = 0.2f;
    public bool followCamera = true;
    public float cameraFollowSpeed = 8f;
    public Vector3 cameraOffset = new Vector3(0f, 0f, -10f);

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Camera mainCamera;
    private Vector2 input;
    private Vector2 velocity;
    private Sprite[] currentSprites;
    private int frameIndex = 0;
    private float timer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;

        currentSprites = spriteDown;
        SetFrame(0);
    }

    public void OnMove(InputValue value)
    {
        input = value.Get<Vector2>();
        velocity = input.normalized * moveSpeed;

        if (input.sqrMagnitude > 0.01f)
        {
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                if (input.x > 0)
                    ChangeSprites(spriteRight);
                else
                    ChangeSprites(spriteLeft);
            }
            else
            {
                if (input.y > 0)
                    ChangeSprites(spriteUp);
                else
                    ChangeSprites(spriteDown);
            }
        }
    }

    private void Update()
    {
        if (input.sqrMagnitude <= 0.01f)
        {
            frameIndex = 0;
            SetFrame(frameIndex);
            return;
        }

        timer += Time.deltaTime;

        if (timer >= frameTime)
        {
            timer = 0f;
            frameIndex++;

            if (currentSprites != null && frameIndex >= currentSprites.Length)
                frameIndex = 0;

            SetFrame(frameIndex);
        }
    }

    private void FixedUpdate()
    {
        if (rb == null)
            return;

        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    private void LateUpdate()
    {
        if (!followCamera || mainCamera == null)
            return;

        Vector3 targetPosition = transform.position + cameraOffset;
        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position,
            targetPosition,
            cameraFollowSpeed * Time.deltaTime);
    }

    private void ChangeSprites(Sprite[] newSprites)
    {
        if (currentSprites == newSprites)
            return;

        currentSprites = newSprites;
        frameIndex = 0;
        timer = 0f;
        SetFrame(frameIndex);
    }

    private void SetFrame(int index)
    {
        if (sr == null || currentSprites == null || currentSprites.Length == 0)
            return;

        sr.sprite = currentSprites[Mathf.Clamp(index, 0, currentSprites.Length - 1)];
    }
}
