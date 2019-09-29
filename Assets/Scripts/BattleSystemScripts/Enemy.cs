﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    public enum States
    {
        IDLE,
        ATTACKED,
        STATES
    };

    public States currentState;

    public Transform sprite;
    public Transform player;
    public Coroutine currentCoroutine;

    public float moveSpeed;
    public float initSNSpeed;
    public float hopHeight;
    
    private Vector3 initPos;

    private Rigidbody rb;

    //Controls degree of squish
    private float squashDegree;
    private float initY;

    //Controls speed of squishing eff
    private float SNSpeed;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();

        initPos = transform.position;

        initY = sprite.localPosition.y;

        Reset();
    }
	
	// Update is called once per frame
	void Update ()
    {
        switch (currentState)
        {
            case States.IDLE:
                float offset = sinFunc(squashDegree, 2.0f, 0.05f);

                sprite.localScale = new Vector3(1.0f - (offset / 2.0f), 1.0f + offset, 1.0f);
                sprite.localPosition = new Vector3(sprite.localPosition.x, initY + offset, sprite.localPosition.z);

                squashDegree += Time.deltaTime;
                break;
            default:
                break;
        }   
    }

    private void Reset()
    {
        //Reset values for before enemy is hit
        squashDegree = 0.0f;

        SNSpeed = initSNSpeed;

        sprite.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        sprite.localPosition = new Vector3(sprite.localPosition.x, initY, sprite.localPosition.z);

        currentState = States.IDLE;
    }

    float sinFunc(float x, float freq, float amp)
    {
        float theta = (freq * x) + (Mathf.PI / 2.0f);

        return (Mathf.Sin(theta) * amp) - amp;
    }

    public IEnumerator SquashNStretch()
    {
        //While loop for Squish Animation
        while (SNSpeed > 0.0f)
        {
            float offset = sinFunc(squashDegree, 6.0f, 0.1f);

            sprite.localScale = new Vector3(1.0f, 1.0f + offset, 1.0f);
            sprite.transform.localPosition = new Vector3(sprite.localPosition.x, initY + offset, sprite.localPosition.z);
            
            squashDegree += Time.deltaTime * SNSpeed;

            SNSpeed -= Mathf.Pow(Time.deltaTime, 0.5f);

            SNSpeed = Mathf.Clamp(SNSpeed, 0.0f, 10.0f);

            yield return null;
        }

        //Post squish returning to something close to reset state
        while (sprite.localScale.y < 0.95f)
        {
            float offset = sinFunc(squashDegree, 6.0f, 0.1f);

            sprite.localScale = new Vector3(1.0f, 1.0f + offset, 1.0f);
            sprite.transform.localPosition = new Vector3(sprite.localPosition.x, initY + offset, sprite.localPosition.z);

            squashDegree += Time.deltaTime;

            yield return null;
        }

        //Properly reset
        Reset();
    }

    public IEnumerator BumpedInto()
    {
        rb.velocity = new Vector3(moveSpeed, hopHeight, rb.velocity.z);

        float n = moveSpeed;

        while (n > 0)
        {
            rb.velocity = new Vector3(n, rb.velocity.y, rb.velocity.z);

            n -= 5.0f * Time.deltaTime;

            yield return null;
        }

        // Reel back to charge up
        rb.velocity = -Vector3.right * moveSpeed;

        //Wait until back in the reset position
        yield return new WaitUntil(() => Vector3.Distance(transform.position, initPos) < moveSpeed * 0.02f);

        transform.position = initPos;

        rb.velocity = Vector3.zero;

        Reset();
    }

    private void OnCollisionEnter(Collision collision)
    {
        //On collision trigger the squish effect
        if (collision.gameObject.tag == "Player")
        {
            Reset();

            currentState = States.ATTACKED;

            Vector3 collisionNormal =  collision.contacts[0].normal;
            //Cancel current coroutine if one is active
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
                currentCoroutine = null;
            }

            if (Vector3.Dot(collisionNormal, -Vector3.up) == 1.0f)
            {
                currentCoroutine = StartCoroutine(SquashNStretch());
            }
            else if (Vector3.Dot(collisionNormal, Vector3.right) == 1.0f)
            {
                currentCoroutine = StartCoroutine(BumpedInto());
            }
        }
    }
}
