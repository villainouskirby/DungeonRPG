using System;

namespace ItemDataExtensions
{
    public class ThrowableItemDataExtension : ItemDataExtension
    {
        public int MaxRegisterCount => _throwItemData.max_register_count;
        public float Damage => _throwItemData.damage;
        public int Distance => _throwItemData.use_distance;
        public float SoundRange => _throwItemData.sound_range;

        public Item_Info_ThrowableItem ThrowItemData => _throwItemData;

        private Item_Info_ThrowableItem _throwItemData;

        public ThrowableItemDataExtension(string id)
        {
            _throwItemData = Array.Find(Item_Info.ThrowableItem, info => info.id == id);
        }
    }
}
