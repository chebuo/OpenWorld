using UnityEngine;
using UnityEngine.UI;
using TMPro;
using R3;

public class PlayerCanvasController : MonoBehaviour
{
    PlayerManager playerManager;
    PlayerController playerController;
    Image[] itemIcons;
    TextMeshProUGUI itemCountText;
    Image digAmountImage;
    Image HPGaugeImage;

    float levelUpUntil=100f;
    float lastLevelUp=0f;

    private void Awake()
    {
        playerManager=GetComponentInParent<PlayerManager>();
        playerController=GetComponentInParent<PlayerController>();

        GameObject iconChild=transform.GetChild(0).gameObject;
        GameObject digAmountChild=transform.GetChild(1).gameObject;
        GameObject HPGaugeChild=transform.GetChild(2).gameObject;
        GameObject iconGrandChild0=iconChild.transform.GetChild(0).gameObject;
        GameObject iconGrandChild1=iconChild.transform.GetChild(1).gameObject;
        GameObject digAmountGrandChild=digAmountChild.transform.GetChild(0).gameObject;
        GameObject HPGaugeGrandChild=HPGaugeChild.transform.GetChild(0).gameObject;
        
        itemIcons=iconGrandChild0.GetComponents<Image>();
        itemCountText=iconGrandChild1.GetComponent<TextMeshProUGUI>();
        digAmountImage=digAmountGrandChild.GetComponent<Image>();
        HPGaugeImage=HPGaugeGrandChild.GetComponent<Image>();
        
        itemCountText.text="";
        digAmountImage.fillAmount=0;
        HPGaugeImage.fillAmount=1;
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
            if(remaining==0)itemCountText.text="";
        }).AddTo(this);

        playerController.SumDigAmount.Subscribe(digAmount =>
        {
            while (digAmount >= levelUpUntil + lastLevelUp)
            {
                lastLevelUp += levelUpUntil;
                Debug.Log($"Level Up! Dig Amount: {digAmount}, Level Up Threshold: {levelUpUntil}");
                levelUpUntil =levelUpUntil*2; // 次のレベルアップまでの量を増やす
                playerController.LevelUp();
            }
            float score=digAmount - lastLevelUp;
            float fillAmount=score/levelUpUntil;
            digAmountImage.fillAmount = Mathf.Clamp01(fillAmount);
            Debug.Log($"Dig Amount: {digAmount}, Fill Amount: {digAmountImage.fillAmount}");
        }).AddTo(this);

        playerManager.HPRatio.Select(ratio => Mathf.Clamp01(ratio)).Subscribe(ratio=>{
            HPGaugeImage.fillAmount=ratio;
        }).AddTo(this);
    }
}