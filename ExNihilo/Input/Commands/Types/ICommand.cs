using ExNihilo.Sectors;
using ExNihilo.Systems.Backend;
using ExNihilo.Systems.Bases;

namespace ExNihilo.Input.Commands.Types
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

    public abstract class ConsoleCommand : ICommand
    {
        protected readonly ConsoleHandler Receiver;
        protected ConsoleCommand(ConsoleHandler receiver)
        {
            Receiver = receiver;
        }
        public abstract void Activate();
    }

    public abstract class GameplayCommand : ICommand
    {
        protected readonly IPlayer Receiver;
        protected GameplayCommand(IPlayer receiver)
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
