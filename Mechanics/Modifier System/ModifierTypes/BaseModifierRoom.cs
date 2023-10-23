using Dungeon.Rooms;
using Modifiers.Data;
using Modifiers.ModifierTypes;

namespace Modifiers.Modifier_Effects.ModifierType
{
    [System.Serializable]
    public abstract class BaseModifierRoom : BaseModifier
    {
        protected Dungeon.Rooms.Room currentRoom;


        protected BaseModifierRoom(ModifierData data, Dungeon.Rooms.Room currentRoom) : base(data)
        {
        }
    }
}