namespace ExNihilo.Input.Commands
{
    public interface ICommand
    {
        void Activate();
    }

    public abstract class GameCommand : ICommand
    {
        protected GameContainer Receiver { get; private set; }
        protected GameCommand(GameContainer receiver)
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
