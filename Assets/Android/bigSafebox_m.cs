using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class bigSafebox_m : MonoBehaviour
{
    public GameObject UI_interact_m;
    public GameObject key;
    public GameObject keyPlayer;
    public GameObject keyImage;
    public bool toggle = true, interactable;
    private Button interactButton;
    public Animator safeboxAnim;
    public AudioSource bgm;
    public AudioSource genshinImpactBGM;

    void Start()
    {
        if (Application.isMobilePlatform)
        {
            UI_interact_m.SetActive(false);
            interactButton = UI_interact_m.GetComponent<Button>();
            if (interactButton != null)
            {
                interactButton.onClick.AddListener(OnInteractButtonClicked);
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (Application.isMobilePlatform && other.CompareTag("pink-key"))
        {
            UI_interact_m.SetActive(true);
            interactable = true;
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (Application.isMobilePlatform && other.CompareTag("pink-key"))
        {
            UI_interact_m.SetActive(false);
            interactable = false;
        }
    }
    void OnInteractButtonClicked()
    {
        if (interactable && toggle)
        {
            toggle = !toggle;
            safeboxAnim.ResetTrigger("interact");
            safeboxAnim.SetTrigger("interact");
            InventoryManager.Instance.RemoveItemByName(keyPlayer.name);
            key.SetActive(false);
            UI_interact_m.SetActive(false);
            bgm.Stop();
            genshinImpactBGM.Play();
            StartCoroutine(Timer());
            Debug.Log("big safe box m");
        }
    }

    IEnumerator Timer()
    {
        yield return new WaitForSeconds(99);
        bgm.Play();
    }
}
