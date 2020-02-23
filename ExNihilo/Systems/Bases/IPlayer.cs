namespace ExNihilo.Systems.Bases
{
    public interface IPlayer
    {
        void PushX(int x);
        void PushY(int y);
        void PushMult(int mult);
        void Touch();
        void ToggleTabMenu();
    }
}
