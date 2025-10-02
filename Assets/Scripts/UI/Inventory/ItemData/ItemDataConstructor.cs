using System;

namespace DBUtility
{
    public static class ItemDataConstructor
    {
        public static ItemData GetItemData(Item_Info_Item rawData)
        {
            if (rawData == null) return null;

            ItemData itemData;

            switch (rawData.type)
            {
                case "HERB":
                    itemData = new HerbItemData(rawData, null);
                    break;

                case "MINERAL":
                    itemData = new MineralItemData(rawData, null);
                    break;

                case "Special":
                    itemData = null; // 얜 뭔 타입으로 가야하지
                    break;

                case "WEAPON":
                    itemData = new WeaponItemData(rawData, null);
                    break;

                case "ARMOR":
                    itemData = new ArmorItemData(rawData, null);
                    break;

                case "BACKPACK":
                    itemData = new BackpackItemData(rawData, null);
                    break;

                case "SUBWEAPON":
                    itemData = new SubWeaponItemData(rawData, null);
                    break;

                case "POTION":
                    itemData = new PotionItemData(rawData, null);
                    break;

                default:
                    return null;
            }

            return itemData;
        }

        public static ItemData GetItemData(int index)
        {
            return GetItemData(Item_Info.Item[index]);
        }

        public static ItemData GetItemData(string id)
        {
            return GetItemData(Array.Find(Item_Info.Item, item => item.id == id));
        }
    }
}
