//using Inventory.ReadOnly;
//using NUnit.Framework.Constraints;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;




//namespace Inventory
//{
//    [Serializable]
//    public class InventorySlotData
//    {
//        public string ItemId;
//        public int Amount;
//    }

//    public class InventoryGridData
//    {
//        public string OwnerId;
//        public List<InventorySlotData> Slot;
//        public Vector2Int Size;
//    }
//    public interface IReadOnlyInventory
//    {
//        event Action<string, int> ItemsAdded;
//        event Action<string, int> ItemsRemoved;

//        string Owner { get; }
//        int GetAmount(string itemId);
//        bool Has(string itemId, int amount);
//    }
//    public interface IReadOnlyInventoryGrid : IReadOnlyInventory
//    {
//        event Action<Vector2Int> SizeChanged;
//        Vector2Int Size { get; }
//        IReadOnlyInventorySlot[,] GetSlots;
//    }
//}
//namespace Inventory.ReadOnly
//{
//    public interface IReadOnlyInventorySlot
//    {
//        event Action<string> ItemIdChanged;
//        event Action<int> ItemAmountChanged;

//        string ItemId { get; }
//        int Amount { get; }
//        bool IsEmpty { get; }
//    }
//}

//namespace Inventory.Scripts
//{
//    public class InventorySlot : IReadOnlyInventorySlot
//    {

//    }
//}

