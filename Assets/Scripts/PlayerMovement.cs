using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    Rigidbody2D rb;
    [Header("Raycast Checkers")]
    public Transform Ground;
    public Transform Sticky_L;
    public Transform Sticky_R;
    public LayerMask GroundLayer;    public float CheckRadius;
    public bool isGround;

    [Header("States")]
    public int sticky;
    public int ledge;
    public bool hold;
    public bool isSticky;
    public bool AirControl;
    public int state;

    [Header("Properties")]
    public float h_speed;
    public float v_speed;
    public float JumpForce;

    [Header("Bounce")]
    public float bounce;
    public bool await_bounce = false;
    public float BouncyModifier;
    float m_bouncymod = 1;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        state = 0;
    }

    private void Update()
    {
        GetGround();
        GetSticky();
    }

    void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool stateChange = Input.GetButtonDown("State_Change");
        bool jump = Input.GetButtonDown("Jump");
        hold = Input.GetButton("Hold");

        if (isGround || AirControl) Move(horizontal, jump);

        if (stateChange) doState();

        if (state == 0) {
            DoSticky(horizontal);
            if (sticky != 0 && vertical != 0 && hold) Climb(vertical);
            if (isSticky && jump) WallJump(horizontal, vertical);
        } else
        {
            DoBounce();
            if (!AirControl)
            {
                StartCoroutine(Regain_AirControl());
            }
        }
    }

    private void Move(float direction, bool jump)
    {
        rb.velocity = new Vector2(direction * h_speed * m_bouncymod, rb.velocity.y);
        if (jump)
        {
            if (isGround) rb.velocity = new Vector2(rb.velocity.x, JumpForce * m_bouncymod);
            if (state == 1) bounce = 1;
            AirControl = true;
        }
    }

    private void GetGround()
    {
        Collider2D col_g = Physics2D.OverlapCircle(Ground.position, CheckRadius, GroundLayer);
        isGround = (col_g != null) ? true : false;

        if (state == 1)
        {

        }
    }

    private void DoBounce()
    {
        if (await_bounce && isGround)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * -0.75f);
            await_bounce = false;
        } else if (await_bounce && sticky != 0)
        {
            AirControl = false;
            rb.velocity = new Vector2(rb.velocity.x * - 0.75f, rb.velocity.y);
            await_bounce = false;
        }
        else if (!isGround) await_bounce = true;
    }


    private void GetSticky()
    {
        Collider2D col_l = Physics2D.OverlapCircle(Sticky_L.position, CheckRadius, GroundLayer);
        Collider2D col_r = Physics2D.OverlapCircle(Sticky_R.position, CheckRadius, GroundLayer);

        if (col_l != null) sticky = 1;
        else if (col_r != null) sticky = 2;
        else sticky = 0;
    }

    private void Climb(float vert)
    {
        rb.velocity = new Vector2(rb.velocity.x, vert * v_speed);
    }

    private void DoSticky(float direction)
    {
        if ((hold && sticky != 0) || (direction > 0 && sticky == 2) || (direction < 0 && sticky == 1))
        {
            hold = true;
            isSticky = true;
            rb.velocity = new Vector2(0, 0);
            rb.gravityScale = 0;
        }
        else
        {
            isSticky = false;
            rb.gravityScale = 1;
        }
    }

    private void WallJump(float hor, float vert)
    {
        hold = false;
        sticky = 0;
        DoSticky(hor);
        Vector2 direction = new Vector3(hor, vert);
        direction.Normalize();
        rb.velocity = direction * JumpForce;
    }

    private void doState ()
    {
        if (state == 0) {
            state = 1;
            m_bouncymod = BouncyModifier;
        } else
        {
            bounce = 0;
            state = 0;
            m_bouncymod = 1;
        }
    }

    private IEnumerator Regain_AirControl()
    {
        yield return new WaitForSeconds(0.5f);
        AirControl = true;
    }
}
