using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerBehaviour : MonoBehaviour
{

    [Header("Controls")]
    public Joystick joystick;
    public int score;
    public float joystickHorizontalSensitivity;
    public float joystickVerticalSensitivity;
    public float horizontalForce;
    public float verticalForce;

    [Header("Checks")]
    public bool isGrounded;
    public bool isJumping;
    public bool isCrouching;
    public bool isInWater;
    public bool isRamp;
    public bool dmgTaken;
    private bool victory;
    private bool atkAvail;

    private bool isAttacking;
    [Header("Platform Detection")]
    public RampDirection rampDirection;
    public bool isGroundedAhead;
    public bool onRamp;
    public bool isPaused;
    public Transform lookAheadPoint;
    public Transform lookInFrontPoint;
    public Transform maxMelee;
    public GameObject maxMeleeObj;
    public LayerMask collisonGroundLayer;
    public LayerMask collisonWallLayer;

    [Header("Spawn Point")]
    public Transform spawnPoint;

    private Rigidbody2D m_rigidBody2D;
    private SpriteRenderer m_spriteRenderer;
    private Animator m_animator;

    [Header("Texts")]
    public Text saveText;
    public Text lifeText;
    public Text scoreText;
    private bool dispText;


    [Header("Timers")]
    private float timeRemaining = 5, anotherTimeRemaining = 5, attackTiming = 0.4f;
    private int life;
    public BarController healthBar;

    [Header("Particle Effects")]
    public ParticleSystem dustTrail;
    public Color dustTrailColour;

    [Header("Audios")]
    public AudioSource[] Audios;
    // Start is called before the first frame update
    void Start()
    {
        m_rigidBody2D = GetComponent<Rigidbody2D>();
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_animator = GetComponent<Animator>();
        rampDirection = RampDirection.NONE;
        dustTrail = GetComponentInChildren<ParticleSystem>();
        dispText = false;
        life = 3;
        victory = false;
        isPaused = false;
        score = 0;
        isAttacking = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        lifeText.text = "Life : " + life;
        scoreText.text = "Score : " + score;
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
        }
        else
        {
            dispText = false;
            timeRemaining = 5;
        }
        if (dispText)
        {
            saveText.gameObject.SetActive(true);
        }
        else
        {
            saveText.gameObject.SetActive(false);
        }

        if(life == 0)
        {
            SceneManager.LoadScene("EndScene(Lose)");
        }

        if(victory)
        {
            if (anotherTimeRemaining > 0)
            {
                anotherTimeRemaining -= Time.deltaTime;
            }
            else
            {
                SceneManager.LoadScene("EndScene(Win)");
            }
        }

        if(isAttacking)
        {
            if(attackTiming > 0)
            {
               attackTiming -= Time.deltaTime;
            }
            else
            {
                m_animator.SetInteger("AnimState", (int)PlayerAnimationType.IDLE);
                attackTiming = 0.4f;
                isAttacking = false;
            }
        }
        attackCheck();
        _LookInFront();
        _LookAhead();
        _Move();
    }

    void _Move()
    {

        if (isGrounded || isRamp)
        {
            if(!isJumping && !isCrouching && !isAttacking)
            {
                if (joystick.Horizontal > joystickHorizontalSensitivity)
                {
                    // move right
                    CreateDustTrail();
                    m_rigidBody2D.AddForce(Vector2.right * horizontalForce * Time.deltaTime);
                    m_spriteRenderer.flipX = false;
                    m_animator.SetInteger("AnimState", (int)PlayerAnimationType.RUN);
                    lookAheadPoint.localPosition = new Vector3(Mathf.Abs(lookAheadPoint.localPosition.x), lookAheadPoint.localPosition.y, lookAheadPoint.localPosition.z);
                    lookInFrontPoint.localPosition = new Vector3(Mathf.Abs(lookInFrontPoint.localPosition.x), lookInFrontPoint.localPosition.y, lookInFrontPoint.localPosition.z);
                    maxMelee.localPosition = new Vector3(Mathf.Abs(maxMelee.localPosition.x), maxMelee.localPosition.y, maxMelee.localPosition.z);

                }
                else if (joystick.Horizontal < -joystickHorizontalSensitivity)
                {
                    // move left
                    CreateDustTrail();
                    m_rigidBody2D.AddForce(Vector2.left * horizontalForce * Time.deltaTime);
                    m_spriteRenderer.flipX = true;
                    m_animator.SetInteger("AnimState", (int)PlayerAnimationType.RUN);
                    lookAheadPoint.localPosition = new Vector3(-Mathf.Abs(lookAheadPoint.localPosition.x), lookAheadPoint.localPosition.y, lookAheadPoint.localPosition.z);
                    lookInFrontPoint.localPosition = new Vector3(-Mathf.Abs(lookInFrontPoint.localPosition.x), lookInFrontPoint.localPosition.y, lookInFrontPoint.localPosition.z);
                    maxMelee.localPosition = new Vector3(-Mathf.Abs(maxMelee.localPosition.x), maxMelee.localPosition.y, maxMelee.localPosition.z);

                }
                else
                {
                    m_animator.SetInteger("AnimState", (int)PlayerAnimationType.IDLE);
                }
            }

            if (onRamp)
            {
                if (rampDirection == RampDirection.UP)
                {
                    m_rigidBody2D.AddForce(Vector2.up * verticalForce * Time.deltaTime);
                }
                else
                {
                    m_rigidBody2D.AddForce(Vector2.down * verticalForce * Time.deltaTime);
                }

            }

            if ((joystick.Vertical > joystickVerticalSensitivity) && (!isJumping))
            {
                CreateDustTrail();
                m_rigidBody2D.AddForce(Vector2.up * verticalForce);
                m_animator.SetInteger("AnimState", (int)PlayerAnimationType.JUMP);
                isJumping = true;
                Audios[1].Play();
            }
            else
            {
                isJumping = false;
            }

            if ((joystick.Vertical < -joystickVerticalSensitivity) && (!isCrouching))
            {
                // Crouch
                m_animator.SetInteger("AnimState", (int)PlayerAnimationType.CROUCH);
                isCrouching = true;
            }
            else
            {
                isCrouching = false;
            }
            
        }

        if (isInWater)
        {
            if (joystick.Horizontal > joystickHorizontalSensitivity)
            {
                m_rigidBody2D.AddForce(Vector2.right * horizontalForce / 5.0f * Time.deltaTime);
                m_spriteRenderer.flipX = false;
            }
            else if (joystick.Horizontal < -joystickHorizontalSensitivity)
            {
                // move left
                m_rigidBody2D.AddForce(Vector2.left * horizontalForce / 5.0f * Time.deltaTime);
                m_spriteRenderer.flipX = true;
            }

                if (joystick.Vertical > joystickVerticalSensitivity)
            {
                m_rigidBody2D.AddForce(Vector2.up * verticalForce / 50.0f);
                m_animator.SetInteger("AnimState", (int)PlayerAnimationType.JUMP);
            }
        }

        

    }

    private void attackCheck()
    {
        if(maxMeleeObj.GetComponent<MaxMeleeCheck>().ableToAttack)
        {
            atkAvail = true;
        }
        else
        {
            atkAvail = false;
        }
    }
    private void _LookAhead()
    {
        var groundHit = Physics2D.Linecast(transform.position, lookAheadPoint.position, collisonGroundLayer);
        if (groundHit)
        {
            if (groundHit.collider.CompareTag("Ramps"))
            {
                // up left
                onRamp = true;
            }

            if (groundHit.collider.CompareTag("Platforms"))
            {
                onRamp = false;
            }
            isGroundedAhead = true;
        }
        else
        {
            isGroundedAhead = false;
        }
    }

    private void _LookInFront()
    {
        var wallHit = Physics2D.Linecast(transform.position, lookInFrontPoint.position, collisonWallLayer);
        if (wallHit)
        {
            if (!wallHit.collider.CompareTag("Ramps"))
            {
                rampDirection = RampDirection.DOWN;
            }
            else
            {
                rampDirection = RampDirection.UP;
            }
        }
    }

    public void _Attack()
    {
        if (!isJumping && !isInWater && !isAttacking)
        {
            m_animator.SetInteger("AnimState", (int)PlayerAnimationType.ATTACK);
            isAttacking = true;
            Audios[4].Play();
            if(maxMeleeObj.GetComponent<MaxMeleeCheck>().enemyObj != null && atkAvail)
            {
                maxMeleeObj.GetComponent<MaxMeleeCheck>().enemyObj.SetActive(false);
                score = score + 100;
                Audios[0].Play();
            }
        }
    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Platforms"))
        {
            isGrounded = true;
        }

        if(other.gameObject.CompareTag("Ramps"))
        {
            isRamp = true;
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Platforms"))
        {
            isGrounded = false;
        }
        if (other.gameObject.CompareTag("Ramps"))
        {
            isRamp = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // respawn
        if (other.gameObject.CompareTag("DeathPlane"))
        {
            transform.position = spawnPoint.position;
            dmgTaken = true;
        }
        if (other.gameObject.CompareTag("Water"))
        {
            m_rigidBody2D.gravityScale = 0.0001f;
            isInWater = true;
        }
        if (other.gameObject.CompareTag("Enemy"))
        {
            transform.position = spawnPoint.position;
            dmgTaken = true;
        }
        if (other.gameObject.CompareTag("SavePoint") && !other.gameObject.GetComponent<SavePoint>().savePointEnabled)
        {
            other.gameObject.GetComponent<SavePoint>().savePointEnabled = true;
            spawnPoint.position = other.gameObject.transform.position;
            dispText = true;
        }

        if (other.gameObject.CompareTag("Goal"))
        {
            Audios[3].Stop();
            Audios[2].Play();
            victory = true;
        }

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Water"))
        {
            m_rigidBody2D.gravityScale = 1.0f;
            isInWater = false;
        }
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (dmgTaken)
            {
                Audios[5].Play();
                life--;
                healthBar.SetValue(life);
                dmgTaken = false;
            }
        }

        if (collision.gameObject.CompareTag("DeathPlane"))
        {
            if (dmgTaken)
            {
                Audios[5].Play();
                life--;
                healthBar.SetValue(life);
                dmgTaken = false;
            }
        }
    }

    private void CreateDustTrail()
    {
        dustTrail.GetComponent<Renderer>().material.SetColor("_Color", dustTrailColour);

        dustTrail.Play();
    }
}
