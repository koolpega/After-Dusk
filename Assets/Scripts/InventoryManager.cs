using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public List<GameObject> heldItems = new List<GameObject>();
    public List<GameObject> inventoryIcons = new List<GameObject>();

    private int currentHeldIndex = -1;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddItem(GameObject heldItem, GameObject inventoryIcon)
    {
        if (heldItems.Contains(heldItem))
        {
            return;
        }

        heldItems.Add(heldItem);
        inventoryIcons.Add(inventoryIcon);
        inventoryIcon.SetActive(true);

        EquipItem(heldItems.Count - 1);
    }

    public void RemoveItem(GameObject heldItem)
    {
        int index = heldItems.IndexOf(heldItem);

        if (index != -1)
        {
            heldItems[index].SetActive(false);
            inventoryIcons[index].SetActive(false);

            heldItems.RemoveAt(index);
            inventoryIcons.RemoveAt(index);

            if (currentHeldIndex == index)
                currentHeldIndex = -1;
            else if (currentHeldIndex > index)
                currentHeldIndex--;

            if (heldItems.Count > 0)
                EquipItem(0);
        }
    }

    public void RemoveItemByName(string heldItemName)
    {
        for (int i = 0; i < heldItems.Count; i++)
        {
            if (heldItems[i].name == heldItemName)
            {
                heldItems[i].SetActive(false);
                inventoryIcons[i].SetActive(false);

                heldItems.RemoveAt(i);
                inventoryIcons.RemoveAt(i);

                if (currentHeldIndex == i)
                    currentHeldIndex = -1;
                else if (currentHeldIndex > i)
                    currentHeldIndex--;

                if (heldItems.Count > 0)
                    EquipItem(0);

                return;
            }
        }
    }

    public void EquipItem(int index)
    {
        for (int i = 0; i < heldItems.Count; i++)
        {
            heldItems[i].SetActive(false);
        }

        if (index >= 0 && index < heldItems.Count)
        {
            heldItems[index].SetActive(true);
            currentHeldIndex = index;
        }
    }

    void Update()
    {
        for (int i = 0; i < Mathf.Min(8, heldItems.Count); i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                EquipItem(i);
                return;
            }
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (heldItems.Count == 0) return; 

        if (scroll > 0f)
        {
            int nextIndex = (currentHeldIndex + 1) % heldItems.Count;
            EquipItem(nextIndex);
        }
        else if (scroll < 0f)
        {
            int prevIndex = (currentHeldIndex - 1 + heldItems.Count) % heldItems.Count;
            EquipItem(prevIndex);
        }
    }
}
