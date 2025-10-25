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
                    itemData = new HerbItemData(rawData);
                    break;

                case "MINERAL":
                    itemData = new MineralItemData(rawData);
                    break;

                case "Special":
                    itemData = null; // 얜 뭔 타입으로 가야하지
                    break;

                case "WEAPON":
                    itemData = new WeaponItemData(rawData);
                    break;

                case "ARMOR":
                    itemData = new ArmorItemData(rawData);
                    break;

                case "BACKPACK":
                    itemData = new BackpackItemData(rawData);
                    break;

                case "SUBWEAPON":
                    itemData = new SubWeaponItemData(rawData);
                    break;

                case "POTION":
                    itemData = new PotionItemData(rawData);
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
