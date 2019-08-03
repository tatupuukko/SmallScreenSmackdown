using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackFromUI : MonoBehaviour
{
     public GameObject player;
    

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if(player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
    }

    public void HeroAttack()
    {

        player.GetComponentInChildren<PC_LIIKKUU_10>().Attack();

        
    }
    public void Dash()
    {
        player.GetComponentInChildren<PC_LIIKKUU_10>().Dash();
    }
    public void SpecialSkill()
    {
        player.GetComponent<PC_LIIKKUU_10>().SpecialSkill();
    }
}
