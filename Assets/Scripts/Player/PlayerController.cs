using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Move Settings")]
    public float moveSpeed = 6f;

    [Header("Dash Settings")]
    public float dashSpeed = 12f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.1f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private Vector2 input;
    private Vector2 moveDir;
    private Vector2 lastMoveDir = Vector2.right;
    public Vector2 LastMoveDir => lastMoveDir;

    private bool isDashing = false;
    private bool canDash = true;

    private Color originalColor;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            originalColor = sr.color;
        }
    }

    private void Update()
    {
        if (!isDashing)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            moveDir = input.normalized;

            if (moveDir != Vector2.zero)
            {
                lastMoveDir = moveDir;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) && canDash)
        {
            StartCoroutine(Dash());
        }
    }

    private void FixedUpdate()
    {
        if (!isDashing)
        {
            rb.velocity = moveDir * moveSpeed;
        }
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        if (sr != null)
        {
            sr.color = Color.cyan;
        }

        rb.velocity = lastMoveDir * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
        rb.velocity = Vector2.zero;

        if (sr != null)
        {
            sr.color = originalColor;
        }

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }
}