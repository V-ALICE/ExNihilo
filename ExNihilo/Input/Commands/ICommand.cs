using ExNihilo.Sectors;

namespace ExNihilo.Input.Commands
{
    public interface ICommand
    {
        void Activate();
    }

    public abstract class SuperCommand : ICommand
    {
        protected readonly GameContainer Receiver;
        protected SuperCommand(GameContainer receiver)
        {
            Receiver = receiver;
        }
        public abstract void Activate();
    }

    public abstract class MenuCommand : ICommand
    {
        protected readonly Sector Receiver;
        protected MenuCommand(Sector receiver)
        {
            Receiver = receiver;
        }
        public abstract void Activate();
    }

    public abstract class GameCommand : ICommand
    {
        protected readonly UnderworldSector Receiver;
        protected GameCommand(UnderworldSector receiver)
        {
            Receiver = receiver;
        }
        public abstract void Activate();
    }

    /*public abstract class PlayerCommand : ICommand
    {
        protected PlayerEntity Receiver { get; private set; }
        protected PlayerCommand(PlayerEntity receiver)
        {
            Receiver = receiver;
        }
        public abstract void Activate();
    }*/

    public class Uncommand : ICommand
    {
        public void Activate()
        {
        }
    }
}
