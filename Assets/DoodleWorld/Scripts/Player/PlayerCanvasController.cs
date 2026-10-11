using UnityEngine;
using UnityEngine.UI;
using TMPro;
using R3;

public class PlayerCanvasController : MonoBehaviour
{
    PlayerController playerController;
    Image[] itemIcons;
    TextMeshProUGUI itemCountText;

    private void Awake()
    {
        playerController=GetComponentInParent<PlayerController>();
        GameObject child=transform.GetChild(0).gameObject;
        GameObject grandChild0=child.transform.GetChild(0).gameObject;
        GameObject grandChild1=child.transform.GetChild(1).gameObject;
        itemIcons=grandChild0.GetComponents<Image>();
        itemCountText=grandChild1.GetComponent<TextMeshProUGUI>();
        itemCountText.text="";
        Debug.Log(itemIcons.Length);
        Debug.Log(itemIcons[0].gameObject.name);
        playerController.CurrentItem.Subscribe(item=>{
            if(item==null)
            {
                itemIcons[0].sprite=null;
                Color color=itemIcons[0].color;
                color.a=0;
                itemIcons[0].color=color;
                itemCountText.text="";
                return;
            }
            itemIcons[0].sprite=item.icon;
            Color iconColor=itemIcons[0].color;
            iconColor.a=1;
            itemIcons[0].color=iconColor;
        }).AddTo(this);

        playerController.CurrentItemRemaining.Subscribe(remaining=>{
            itemCountText.text=remaining.ToString();
        }).AddTo(this);
    }
}