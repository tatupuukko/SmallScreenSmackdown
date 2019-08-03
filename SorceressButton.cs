using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SorceressButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public int side;
    public GameObject player;





    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");


    }
     
    
    //Detect if a click occurs
    public void OnPointerDown(PointerEventData pointerEventData)
    {
        player.GetComponent<PC_LIIKKUU_10>().sorceressSpecial = true;
        player.GetComponent<PC_LIIKKUU_10>().SpecialSkill();

    }
    public void OnPointerUp(PointerEventData pointerEventData)
    {
        player.GetComponent<PC_LIIKKUU_10>().SpecialSkill();
        player.GetComponent<PC_LIIKKUU_10>().sorceressSpecial = false;

    }
}

