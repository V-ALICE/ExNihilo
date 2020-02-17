using ExNihilo.Systems.Backend;

namespace ExNihilo.Systems.Bases
{
    public interface ISavable
    {
        void Pack(PackedGame game);
        void Unpack(PackedGame game);
    }
}
