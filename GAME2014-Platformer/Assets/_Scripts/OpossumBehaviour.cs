using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Net.Http.Headers;
using UnityEngine;


public enum RampDirection
{
    NONE,
    UP,
    DOWN
}
public class OpossumBehaviour : MonoBehaviour
{
    public float runForce;
    public Rigidbody2D m_rigidbody2D;
    public Transform lookInFrontPoint;
    public Transform lookAheadPoint;
    public LayerMask collisonGroundLayer;
    public LayerMask collisonWallLayer;
    public bool isGroundedAhead;
    public bool onRamp;
    public RampDirection rampDirection;

    // Start is called before the first frame update
    void Start()
    {
        m_rigidbody2D = GetComponent<Rigidbody2D>();
        rampDirection = RampDirection.NONE;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        _LookInFront();
        _LookAhead();
        _Move();
    }

    private void _LookInFront()
    {
        var wallHit = Physics2D.Linecast(transform.position, lookInFrontPoint.position, collisonWallLayer);
        if (wallHit)
        {
            if(!wallHit.collider.CompareTag("Ramps"))
            {
                if(!onRamp && transform.rotation.z == 0)
                {
                    _FlipX();
                }
                rampDirection = RampDirection.DOWN;
            }
            else
            {
                rampDirection = RampDirection.UP;
            }
        }
    }
    private void _LookAhead()
    {
        var groundHit = Physics2D.Linecast(transform.position, lookAheadPoint.position, collisonGroundLayer);
       if (groundHit)
        {
            if(groundHit.collider.CompareTag("Ramps"))
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
    private void _Move()
    {
        if (isGroundedAhead)
        {
            m_rigidbody2D.AddForce(Vector2.left * runForce * Time.deltaTime * transform.localScale.x);
           
            if(onRamp)
            { 
                if(rampDirection == RampDirection.UP)
                {
                    m_rigidbody2D.AddForce(Vector2.up * runForce * 0.5f * Time.deltaTime);
                }
                else
                {
                    m_rigidbody2D.AddForce(Vector2.down * runForce * Time.deltaTime);
                }

                StartCoroutine(Rotate());
            }
            else
            {
                StartCoroutine(Normalize());
            }
            
            m_rigidbody2D.velocity *= 0.90f;
        }

        else
        {
            _FlipX();
        }
       
    }

    IEnumerator Rotate()
    {
        yield return new WaitForSeconds(0.05f);
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, -45.0f);

    }


    IEnumerator Normalize()
    {
        yield return new WaitForSeconds(0.05f);
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
    }


    private void _FlipX()
    {
        transform.localScale = new Vector3(transform.localScale.x * -1.0f, transform.localScale.y, transform.localScale.z);
    }
}
