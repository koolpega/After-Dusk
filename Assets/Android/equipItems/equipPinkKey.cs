using UnityEngine;

public class equipPinkKey : MonoBehaviour
{
    public void OnButtonClick()
    {
        for (int i = 0; i < InventoryManager.Instance.heldItems.Count; i++)
        {
            GameObject item = InventoryManager.Instance.heldItems[i];

            if (item.name.Equals("pink-key"))
            {
                InventoryManager.Instance.EquipItem(i);
                return;
            }
        }
    }
}
