using ExNihilo.Sectors;

namespace ExNihilo.Input.Commands
{

    public class InteractWithWorld : GameCommand
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
