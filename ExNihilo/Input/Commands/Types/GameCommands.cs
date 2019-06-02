using ExNihilo.Sectors;

namespace ExNihilo.Input.Commands.Types
{

    public class InteractWithWorld : GameplayCommand
    {
        public InteractWithWorld(UnderworldSector game) : base(game)
        {
        }

        public override void Activate()
        {
            //Receiver.Interact();
        }
    }
    
}
