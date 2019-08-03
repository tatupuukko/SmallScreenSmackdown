using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class fireBallExplosion : MonoBehaviour
{

    public List<GameObject> targets;
    public float damage;

    private void Awake()
    {
    


    }

    public void Explode(Vector3 center, float radius)
    {

               
        Collider[] hitColliders = Physics.OverlapSphere(center, radius);

        foreach (Collider target in hitColliders)
        {
            GameObject obj = target.gameObject;
            Healthbar bar = obj.GetComponent<Healthbar>();
            if(bar != null&&PhotonNetwork.IsMasterClient)
            {
                bar.photonView.RPC("RpcTakeDamage", RpcTarget.All, damage, true);
            }

            Destroy(gameObject);
        }

    }

}


    //private void OnTriggerEnter(Collider other)
    //{
    //    if(other.gameObject.GetComponent<Healthbar>() != null)
    //    {

    //    }

    //}

