using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
[RequireComponent(typeof(Animator),typeof(Rigidbody),typeof(PhotonView))]
public class PC_LIIKKUU_10 : MonoBehaviourPunCallbacks
{
    public int[] factionSelect;
    public int faction;


    public float speed = 1f;
    public float slerp_time = 25f;
    public float rad = 1f;
    public float dir_mag = 0.01f;

    public Joystick joystick;
    Button attackButton;
    [Header("Dash Parameters")]
    public float dashAmount = 100f;
    public float dashSpeed = 10f;
    public float dashDuration = 0.3f;
    public float dashDamage;
    private float dashtime = 0f;
    private Vector3 dashDirection;
    private bool dashing;





    //Respawn
    public Vector3 respawnPoint;

    //Hero base attack and special damages
    public float attackSpeed;
    public float damage;


    //Special Skills ------------------------------

    //Generic 

    bool specialCoolDown, dashCoolDown;

    public bool channelingSpecial;


    //Skeleton Special
    public List<GameObject> skeletonFriends;
    public GameObject skeletonSacrifice;
    public GameObject healParticles;

    bool skeletonBonus;
    int skeletonBonusToDamage, skeletonHealAmount;

    //Sorceress Special
    [HideInInspector]
    public bool sorceressSpecial, sorceressTargeting, sorceressCasting;
    public GameObject fireballProjectile, targetDestination;
    public float sorceressSpecialDamage;
    Vector3 fireBallTarget;
    [HideInInspector]
    public MeshRenderer fireBallTargetCircle;
    Component fireBallTargetComponent;

    //Angel Special
    public GameObject stunRelease;
    public ParticleSystem channelingSphereParticle;
    public bool channelingSphereOn;

    [HideInInspector]
    public bool dead;
    

    //EXP system
    public int xp;
    public int xpRequired;
    public int level;

    bool canAttack;
    bool hasTarget;

    //Finding enemies
    public List<GameObject> targets;
    public GameObject currentTarget;
    float attackAnimation;

    private Rigidbody rb;
    private Animator ac;

    float distance;
    float minAttackDistance;

    void Start()
    {
        //Special Skills

            //Generic
        specialCoolDown = false;
        channelingSpecial = false;
        dashCoolDown = false;

        //Skeleton
        if (faction == 0)
        {
            skeletonBonus = false;
            skeletonHealAmount = 20;
            skeletonBonusToDamage = 3;
        }

        //Sorceress
        if (faction == 1)
        {
            sorceressTargeting = false;
            fireBallTargetCircle = gameObject.transform.GetChild(4).GetComponent<MeshRenderer>();
            fireBallTargetComponent = gameObject.transform.GetChild(4);
            fireBallTargetCircle.enabled = false;
        }


        //Angel
        if (faction == 2)
        {
            channelingSphereParticle = gameObject.transform.GetChild(5).GetComponent<ParticleSystem>();
            channelingSphereOn = false;
        }

        dashing = false;

        dead = false;
        rb = GetComponent<Rigidbody>();
        ac = GetComponent<Animator>();  
        joystick = FindObjectOfType<Joystick>();
        CameraWork _cameraWork = this.gameObject.GetComponent<CameraWork>();

        minAttackDistance = .5f;
        canAttack = true;
        hasTarget = false;

        
        xp = 0;

        

   



        if (_cameraWork != null)
        {
            if (photonView.IsMine)
            {
                _cameraWork.OnStartFollowing();
            }
        }
        else
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> CameraWork Component on playerPrefab.", this);
        }
    }
    void Update()
    {
        #region Targeting and enabling targetcircle
        //Sets the target if there is something in the collider
        if (targets.Count >= 1 && targets[0] != null && currentTarget == null)
        {
            currentTarget = targets[0];
                currentTarget.GetComponent<Healthbar>().targetCircle.enabled = true;
        }else if (targets.Count >= 1 && targets[0]==null)
        {
            targets.RemoveAll(item => item == null);
        }
        #endregion
        #region XP check
        //Checks if Player has reached the exp required to level up.
        if (xp >= xpRequired)
        {
            LevelUp();
        }
        #endregion
        #region Fireball Targeting
        if (sorceressTargeting)
        {

            gameObject.transform.LookAt(fireBallTarget);
            LayerMask mask = LayerMask.GetMask("Ground");

            Ray myRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(myRay, out hit, 100, mask))
            {
                Debug.Log("RayCasting.");
                fireBallTarget = hit.point;
                fireBallTargetCircle.enabled = true;
                fireBallTargetComponent.transform.position = hit.point+new Vector3(0f,0.03f,0f);
            }
        }
        #endregion
    }

          
    void FixedUpdate()
    {
        if (dashing)
        {
            dashtime += Time.deltaTime;
            if (dashtime >= dashDuration) dashing = false;
        }
        if ((photonView.IsMine == false && PhotonNetwork.IsConnected == true) || dead)
        {
            return;
        }
        if(channelingSpecial || GetComponent<Healthbar>().stunned)
        {
            return;
        }

        float moveHorizontal = joystick.Horizontal;
        float moveVertical = joystick.Vertical;
           
        if ((moveHorizontal != 0 || moveVertical != 0 )&& !dashing)
        {
            ac.SetBool("IsWalking", true);
            ac.SetBool("IsDashing", false);
        }
        else if (dashing)
        {
            ac.SetBool("IsDashing", true);
            ac.SetBool("IsWalking", false);
        }
        else
        {
            ac.SetBool("IsWalking", false);
            ac.SetBool("IsDashing", false);
        }
        Vector3 movement = new Vector3(moveHorizontal * speed * Time.deltaTime, 0, moveVertical * speed * Time.deltaTime);
        //Dash Controls
        if (dashing)
        {
            dashDirection = transform.forward;
            moveHorizontal = dashDirection.x * dashSpeed;
            moveVertical = dashDirection.y * dashSpeed;
            movement = dashDirection * dashSpeed * Time.deltaTime;
            rb.MovePosition(transform.position + movement);
            return;

        }
        // ei käytetä addforcea

        rb.MovePosition(transform.position + movement);

        Vector3 direction = new Vector3(moveHorizontal, 0, moveVertical);
        direction = Vector3.ClampMagnitude(direction, rad);


        if (direction.magnitude > dir_mag)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), slerp_time * Time.deltaTime);
        }
    }

    //Attack and attack cooldown
    public void Attack()
    {
        if ((photonView.IsMine == false && PhotonNetwork.IsConnected == true) || dead)
        {
            return;
        }

        if (canAttack && currentTarget != null && !GetComponent<Healthbar>().stunned)
        {
            attackAnimation = Random.Range(0, 10);

            FaceTarget();

            currentTarget.GetComponent<Healthbar>().photonView.RPC("RpcTakeDamage", RpcTarget.All, damage, true);
            if (attackAnimation >= 5)
            {
                ac.SetTrigger("Attack1");
                Invoke("AttackCoolDown", attackSpeed);
            }
            if (attackAnimation < 5)
            {
                ac.SetTrigger("Attack2");
                Invoke("AttackCoolDown", attackSpeed);
            }

            canAttack = false;
        }
    }
    void AttackCoolDown()
    {
        canAttack = true;
    }

    //Finding enemies
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.isTrigger) return;
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }

        Healthbar other = collider.GetComponent<Healthbar>();

        if (other != null && other.side != GetComponent<Healthbar>().side)
        {

            targets.Add(collider.gameObject);
        }
        else if (faction == 0)
        {
            if(other != null &&other.enemyType=="Creep" &&other.side == GetComponent<Healthbar>().side)
            {

                skeletonFriends.Add(collider.gameObject);
            }
        }

    }
    private void OnTriggerExit(Collider collider)
    {
        if (collider.isTrigger) return;
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }
        Healthbar other = collider.GetComponent<Healthbar>();

        if ( currentTarget!= null && collider.gameObject == currentTarget.gameObject)
        {
            currentTarget.GetComponent<Healthbar>().targetCircle.enabled = false;
            currentTarget = null;
        }

        if (targets.Contains(collider.gameObject))
        {
            targets.Remove(collider.gameObject);
           
        }
        else if (skeletonFriends.Contains(collider.gameObject))
        {
            skeletonFriends.Remove(collider.gameObject);
        }
    }

    
    public void Dash()
    {
        if(!dashCoolDown)
        {
            dashing = true;
            dashCoolDown = true;
            //rb.MovePosition(transform.position + transform.forward * dashAmount);
            Debug.Log("Dashing!");
            dashtime = 0f;
            Invoke("DashCD", 8f);
        }

    }

    void DashCD()
    {
        dashCoolDown = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (PhotonNetwork.IsMasterClient && dashing)
        {
            Healthbar hb = collision.collider.GetComponent<Healthbar>();
            if (hb != null)
            {
                hb.photonView.RPC("RpcTakeDamage", RpcTarget.All, dashDamage, true);
            }
        }
    }


    public void SpecialSkill()
    {

        switch (faction)
        {
            case 0://Skeleton
                {
                    if(!specialCoolDown && skeletonFriends.Count > 0)
                    { 
                    specialCoolDown = true;

                        Invoke("StopSpecial", 0.1f);
                        ac.SetBool("IsDoingSpecial", true);
                        skeletonSacrifice = skeletonFriends[0];
                        if (skeletonSacrifice.GetComponent<Healthbar>().side == gameObject.GetComponent<Healthbar>().side)
                        {
                            FaceTheSacrifice();

                            GameObject heal = Instantiate(healParticles, gameObject.transform.position, healParticles.transform.rotation);
                            Destroy(heal, 1f);


                            skeletonSacrifice.GetComponent<Healthbar>().health = 0;
                            Invoke("SpecialCD", 15);
                            SkeletonSpecialBonus();
                        }
                    }
                }
                
                break;
            case 1://Sorceress
                if(!specialCoolDown && !sorceressCasting)
                {
                    ac.SetBool("IsDoingSpecial", true);
                    specialCoolDown = true;
                    sorceressCasting = true;
                    sorceressTargeting = true;

                }
                else if(sorceressCasting)
                {
                    ac.SetBool("IsDoingSpecial", false);
                    ac.Play("Special2");

                    photonView.RPC("RpcFireBall", RpcTarget.All, fireBallTarget);



                    Invoke("SpecialCD", 15);

                }


                    break;
            case 2://Angel
                if(!specialCoolDown)
                {
                    specialCoolDown = true;
                    channelingSpecial = true;
                    ChannelingSphere();
                    ac.SetBool("IsDoingSpecial",true);


                }
                
                break;
        }
        
    }

    [PunRPC]
    public void RpcFireBall(Vector3 fireBallTarget)
    {
        GameObject obj = Instantiate(fireballProjectile, gameObject.transform.position, gameObject.transform.rotation);



        GameObject target = Instantiate(targetDestination, fireBallTarget, gameObject.transform.rotation);
        target.GetComponent<fireBallExplosion>().damage = sorceressSpecialDamage;
        obj.GetComponent<fireball>().fireBallTarget = target;


        sorceressCasting = false;
        sorceressTargeting = false;
        fireBallTargetCircle.enabled = false;
        fireBallTargetComponent.transform.localPosition = new Vector3(0f, 0f, 0f);
    }

    void StopSpecial()
    {
        ac.SetBool("IsDoingSpecial", false);
    }
    void SpecialCD()
    {
        specialCoolDown = false;
    }

    public void AngelSpecial(float channelingTime)
    {
        if(!channelingSpecial)
        {
            return;
        }
        Invoke("SpecialCD", 10);
        ChannelingSphere();
        GameObject channelingBoom = Instantiate(stunRelease, gameObject.transform.position, stunRelease.transform.rotation);
        Destroy(stunRelease, 2f);
        
        channelingSpecial = false;
        ac.SetBool("IsDoingSpecial", false);

        List<UnitCreepController> angelFriends = new List<UnitCreepController>();
        Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, 5f);

        GameObject enemyHero = null;

        foreach (Collider target in hitColliders)
        {
            GameObject obj = target.gameObject;
            Debug.Log("Osui" + obj.name);
            Healthbar bar = target.GetComponent<Healthbar>();

            if (bar != null)
            {
                if(bar.enemyType == "Hero" && bar.side != GetComponent<Healthbar>().side)
                {
                    float stunTime = channelingTime / 2;
                    bar.photonView.RPC("RpcStun", RpcTarget.All, stunTime);
                    enemyHero = bar.gameObject;
                }
                else if(bar.enemyType == "Creep" && bar.side != GetComponent<Healthbar>().side)
                {
                    float stunTime = channelingTime;
                    bar.photonView.RPC("RpcStun", RpcTarget.All, stunTime);
                }
                else if (bar.enemyType == "Creep" && bar.side == GetComponent<Healthbar>().side && angelFriends.Count <3)
                {
                    angelFriends.Add(bar.GetComponent<UnitCreepController>());
                }

            }
        }
        if(enemyHero !=null)
        {
            foreach (UnitCreepController item in angelFriends)
            {
                item.currentTarget = enemyHero;
            }
        }

    }
    
    void SkeletonSpecialBonus()
    {
        if(!skeletonBonus)
        {
            damage += skeletonBonusToDamage;
            GetComponent<Healthbar>().health += skeletonHealAmount;
            skeletonBonus = true;
            Invoke("SkeletonSpecialBonus", 10);
        }
        else
        {
            damage -= skeletonBonusToDamage;
            skeletonBonus = false;
        }
    }
    public void ChannelingSphere()
    {
        if(!channelingSphereOn)
        {
            channelingSphereOn = true;
            channelingSphereParticle.Play();
        }
        else
        {
            channelingSphereOn = false;
            channelingSphereParticle.Clear();
            channelingSphereParticle.Stop();
        }

    }
    //LevelUp mechanics
    void LevelUp()
    {
        switch (level)
        {
            case 0:
                damage = 6;
                xpRequired = 36;
                //dash avautuu levelillä yksi
                break;
            case 1:
                damage += 2;
                dashDamage = 2;
                gameObject.GetComponent<Healthbar>().maxHealth += 15;
                xpRequired = 96;
                //Special avautuu levelillä 2
                if (faction == 0)
                {
                    skeletonBonusToDamage = 3;
                    skeletonHealAmount = 8;

                }
                else if (faction == 1)
                {
                    sorceressSpecialDamage = 12;
                }
                else if (faction == 2)
                {
                    gameObject.GetComponent<Healthbar>().damageReduction = 0.6f;
                }
                break;
            case 2:
                damage += 2;
                dashDamage += 2;
                gameObject.GetComponent<Healthbar>().maxHealth += 15;
                xpRequired = 156;
                if (faction == 0)
                {
                    skeletonBonusToDamage = 3;
                    skeletonHealAmount += 2;

                }
                else if (faction == 1)
                {
                    sorceressSpecialDamage += 2;
                }
                else if (faction == 2)
                {
                    gameObject.GetComponent<Healthbar>().damageReduction = 0.5f;
                }
                break;
            case 3:
                damage += 2;
                dashDamage += 2;
                gameObject.GetComponent<Healthbar>().maxHealth += 15;
                xpRequired = 216;
                if (faction == 0)
                {
                    skeletonBonusToDamage += 3;
                    skeletonHealAmount += 2;

                }
                else if (faction == 1)
                {
                    sorceressSpecialDamage += 2;
                }
                else if (faction == 2)
                {
                    gameObject.GetComponent<Healthbar>().damageReduction = 0.4f;
                }
                break;
            case 4:
                damage += 2;
                dashDamage += 2;
                gameObject.GetComponent<Healthbar>().maxHealth += 15;
                if (faction == 0)
                {
                    skeletonBonusToDamage = 3;
                    skeletonHealAmount += 2;

                }
                else if (faction == 1)
                {
                    sorceressSpecialDamage += 2;
                }
                else if (faction == 2)
                {
                    gameObject.GetComponent<Healthbar>().damageReduction = 0.3f;
                }
                break;
        }
        level += 1;


    }

    public void GainXp(int xpAmount)
    {
        if(dead != true)
        {
            xp += xpAmount;
        }

    }

    void FaceTarget()
    {
        Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 100f);

    }

    void FaceTheSacrifice()
    {
        Vector3 direction = (skeletonSacrifice.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 100f);
    }
}
