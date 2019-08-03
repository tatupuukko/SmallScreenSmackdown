using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using Photon.Realtime;

public class UnitCreepController : MonoBehaviourPunCallbacks
{
   
    public GameObject currentTarget;

    NavMeshAgent agent;
    Collider attackRange;
    public List<GameObject> targets;
    public Vector3 destination;

    public bool ranged;


    private Animator anim;
    private bool canAttack;
    public float minAttackDistance;
    public float distance;

    public float attackSpeed;
    public float damage;
    public float stoppingDistance;
    GameObject player;

    public float rangedAttackRange;
    public GameObject projectile;

    float attackAnimation;

    


    // Start is called before the first frame update
    void Start()
    {
        if (!PhotonNetwork.IsMasterClient) enabled = false;
        //Get Components
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        attackRange = GetComponentInChildren<SphereCollider>();
        player = GameObject.FindGameObjectWithTag("Player");
        targets = new List<GameObject>();
                
        //Set numbers and bools

        canAttack = true;
        agent.stoppingDistance = stoppingDistance;


        if (ranged)
        {
            minAttackDistance = 4f;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (GetComponent<Healthbar>().stunned)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            return;
        }
        else
        {
            agent.isStopped = false;
        }

        GetComponent<Healthbar>().side = GetComponent<Healthbar>().side;

        currentTarget = null;
        targets.RemoveAll(item => item == null);
        if(currentTarget != null)
        {
            distance = Vector3.Distance(agent.transform.position, player.transform.position);
        }
       

        //sets the target if there is something in the collider
        if (targets.Count >= 1 && targets[0] != null && currentTarget == null)
        {

            currentTarget = targets[0];
            agent.destination = currentTarget.transform.localPosition;

            //agent.stoppingDistance = currentTarget.GetComponent<PlayerMukkelis>().enemyRadius -.2f;
            if (ranged)
            {
                agent.stoppingDistance = 5f;
            }
            else
            {
                agent.stoppingDistance = 1.5f;
            }

        }
        else if (targets.Count < 1 && currentTarget == null)
        {
            agent.destination = destination;
            distance = 0f;
        }

        if (currentTarget != null)
        {
            distance = Vector3.Distance(agent.transform.position, currentTarget.transform.position);

            if (distance < minAttackDistance && canAttack)
            {
                if(ranged)
                {
                    RangedAttack();
                }
                else
                {
                    Attack();
                }
            }
        }
        //Movement Animation controls
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            anim.SetBool("IsMoving", false);
        }
        else
        {
            anim.SetBool("IsMoving", true);
        }


    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.isTrigger) return;
        Healthbar other = collider.GetComponent<Healthbar>();

        if (other != null && other.side != GetComponent<Healthbar>().side)
        {
            targets.Add(collider.gameObject);
        }
    }

    private void OnTriggerExit(Collider collider)
    {

        if (collider.isTrigger) return;
        Healthbar other = collider.GetComponent<Healthbar>();

        if (targets.Contains(collider.gameObject))
        {
            targets.Remove(collider.gameObject);
        }
    }

    

    void AttackCoolDown()
    {
        canAttack = true;
    }
    

    void Attack()
    {
        if (PhotonNetwork.IsMasterClient == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }


        attackAnimation = Random.Range(0, 10);
        canAttack = false;
        FaceTarget();
        if (attackAnimation >= 5)
        {
            anim.Play("Attack1");
            Invoke("AttackCoolDown", attackSpeed);
            currentTarget.GetComponent<Healthbar>().photonView.RPC("RpcTakeDamage", RpcTarget.All, damage, false);
        }
        if (attackAnimation < 5)
        {
            anim.Play("Attack2");
            Invoke("AttackCoolDown", attackSpeed);
            currentTarget.GetComponent<Healthbar>().photonView.RPC("RpcTakeDamage", RpcTarget.All, damage, false);
        }
    }

    void RangedAttack()
    {
        FaceTarget();
  
        
        if(distance >= 2f)
        {

            anim.SetTrigger("Attack1");
            photonView.RPC("RpcShoot", RpcTarget.All, damage -2f, currentTarget.GetComponent<PhotonView>().ViewID);
            canAttack = false;
            Invoke("AttackCoolDown", attackSpeed);
        }
        else
        {
            anim.SetTrigger("Attack2");
            currentTarget.GetComponent<Healthbar>().photonView.RPC("RpcTakeDamage", RpcTarget.All, damage, false);
            canAttack = false;
            Invoke("AttackCoolDown", attackSpeed);
        }
    }
    [PunRPC]
    public void RpcShoot(float damage, int targetID)
    {
        GameObject target = PhotonView.Find(targetID).gameObject;
        GameObject obj = Instantiate(projectile, transform.position, Quaternion.LookRotation(target.transform.position - transform.position));
        obj.GetComponent<ProjectileScript>().SetTarget(target, damage);
    }

    void FaceTarget()
    {
        Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 100f);
    }
}
