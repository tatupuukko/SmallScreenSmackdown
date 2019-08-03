using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class fireball : MonoBehaviour
{
    Rigidbody rb;
    ParticleSystem trail;
    Vector3 center;
    float radius;
    public GameObject target, explosion;
    public GameObject fireBallTarget;
    public int speed;

    void Awake()
    {

        rb = GetComponentInChildren<Rigidbody>();
        trail = GetComponentInChildren<ParticleSystem>();
        trail.Play(true);
    }

    private void FixedUpdate()
    {
        if (fireBallTarget == null)
        {
            Destroy(gameObject);
            return;
        }
        if (PhotonNetwork.IsMasterClient == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }

        //distance = Vector3.Distance(gameObject.transform.position, fireBallTarget);
        rb.velocity = (fireBallTarget.transform.position - transform.position).normalized * speed;



    }

    private void OnTriggerEnter(Collider other)
    {
        if (PhotonNetwork.IsMasterClient == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }
        if (other.gameObject == fireBallTarget)
        {
            other.gameObject.GetComponent<fireBallExplosion>().Explode(transform.position, 3.5f);

            GameObject expl = Instantiate(explosion, gameObject.transform.position, gameObject.transform.rotation);
            Destroy(expl, 1f);


            Destroy(gameObject);
        }

    }
    }