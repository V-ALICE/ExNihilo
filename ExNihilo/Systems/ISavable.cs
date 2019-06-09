namespace ExNihilo.Systems
{
    public interface ISavable
    {
        void Pack(PackedGame game);
        void Unpack(PackedGame game);
    }
}
