using UnityEngine;
using UnityEngine.UI;
using R3;

public class PlayerCanvasController : MonoBehaviour
{
    PlayerController playerController;
    Canvas canvas;
    Image[] itemIcons;

    private void Awake()
    {
        playerController=GetComponentInParent<PlayerController>();
        canvas=this.GetComponent<Canvas>();
        GameObject child=transform.GetChild(0).gameObject;
        GameObject grandChild=child.transform.GetChild(0).gameObject;
        itemIcons=grandChild.GetComponents<Image>();
        Debug.Log(itemIcons.Length);
        Debug.Log(itemIcons[0].gameObject.name);
        playerController.CurrentItem.Subscribe(item=>{
            if(item==null)
            {
                itemIcons[0].sprite=null;
                Color color=itemIcons[0].color;
                color.a=0;
                itemIcons[0].color=color;
                return;
            }
            itemIcons[0].sprite=item.icon;
            Color iconColor=itemIcons[0].color;
            iconColor.a=1;
            itemIcons[0].color=iconColor;
        }).AddTo(this);
    }    
}