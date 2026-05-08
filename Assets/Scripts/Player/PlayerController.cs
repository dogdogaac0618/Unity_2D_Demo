using System.Collections;
using UnityEngine;

/// <summary>
/// 玩家移动控制器
/// 
/// 当前职责：
/// 1. 根据 PlayerInputReader 提供的移动输入控制玩家移动
/// 2. 记录玩家最后一次移动方向
/// 3. 处理玩家冲刺
/// 
/// 注意：
/// 这个脚本不直接读取键盘按键。
/// 按键输入统一交给 PlayerInputReader。
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Move Setting")]
    [SerializeField]private float moveSpeed = 6f;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.1f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private PlayerInputReader inputReader;

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
        inputReader = GetComponent<PlayerInputReader>();

        if (sr != null)
        {
            originalColor = sr.color;
        }
    }

    private void Update()
    {
        UpdateMoveDirection();
        HandleDashInput();
    }

    private void FixedUpdate()
    {
        Move();
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

    /// <summary>
    /// 更新移动方向
    /// </summary>
    private void UpdateMoveDirection()
    {
        if(isDashing)
        {
            return;
        }

        moveDir = inputReader.MoveInput;

        if(moveDir != Vector2.zero)
        {
            lastMoveDir = moveDir;
        }
    }

    /// <summary>
    /// 普通移动
    /// </summary>
    private void Move()
    {
        if(isDashing && !canDash)
        {
            return;
        }

        rb.velocity = moveSpeed * moveDir;
    }

    /// <summary>
    /// 处理冲刺输入
    /// </summary>
    private void HandleDashInput()
    {
        if(inputReader.DashPressed && canDash)
        {
            StartCoroutine(Dash());
        }
    }
}