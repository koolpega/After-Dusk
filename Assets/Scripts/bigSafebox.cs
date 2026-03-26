using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bigSafebox : MonoBehaviour
{
    public GameObject UI_interact;
    public GameObject key;
    public GameObject keyPlayer;
    public GameObject keyImage;
    public bool toggle = true, interactable;
    public Animator safeboxAnim;
    public AudioSource bgm;
    public AudioSource genshinImpactBGM;

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("pink-key"))
        {
            UI_interact.SetActive(true);
            interactable = true;
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("pink-key"))
        {
            UI_interact.SetActive(false);
            interactable = false;
        }
    }
    void Update()
    {
        if (interactable && toggle)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                toggle = !toggle;
                safeboxAnim.ResetTrigger("interact");
                safeboxAnim.SetTrigger("interact");
                InventoryManager.Instance.RemoveItemByName(keyPlayer.name);
                key.SetActive(false);
                keyPlayer.SetActive(false);
                keyImage.SetActive(false);
                UI_interact.SetActive(false);
                bgm.Stop();
                genshinImpactBGM.Play();
                StartCoroutine(Timer());
            }
        }
    }

    IEnumerator Timer()
    {
        yield return new WaitForSeconds(99);
        bgm.Play();
    }
}