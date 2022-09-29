using System.Collections.Generic;
using UnityEngine;

namespace FPS.Inventory
{
    public class InventoryManager : MonoBehaviour
    {
        public GameObject searchStartPoint = null;
        private IdentifiableGameObject[] idObjects = null;
        [SerializeField] private string onBackKey = "OnBack";

        private List<IdentifiableObject> inventoryItems = new List<IdentifiableObject>();

        public void EquipItem(IdentifiableObject ido)
        {
            if (idObjects == null) idObjects = searchStartPoint.GetComponentsInChildren<IdentifiableGameObject>();

            // Unequip the old items
            UnequipItems();
            // Add the new item to the list if its not already in it
            if (inventoryItems.Contains(ido) == false) inventoryItems.Add(ido);

            foreach (var item in idObjects)
            {
                item.gameObject.SetActive(item.idObj == ido && item.uniqueToGameObject != onBackKey);
            }
        }

        public void UnequipItems()
        {
            if (idObjects == null) idObjects = searchStartPoint.GetComponentsInChildren<IdentifiableGameObject>();

            foreach (var item in idObjects)
            {
                item.gameObject.SetActive(item.uniqueToGameObject == onBackKey && inventoryItems.Contains(item.idObj));
            }
        }
    }
}