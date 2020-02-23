using ExNihilo.Systems.Bases;

namespace ExNihilo.Input.Commands.Types
{

    public class InteractWithWorld : GameplayCommand
    {
        public InteractWithWorld(IPlayer game) : base(game)
        {
        }

        public override void Activate()
        {
            Receiver.Touch();
        }
    }

    public class ToggleInventory : GameplayCommand
    {
        public ToggleInventory(IPlayer game) : base(game)
        {
        }

        public override void Activate()
        {
            Receiver.ToggleTabMenu();
        }
    }

    public class DoubleSpeed : GameplayCommand
    {
        public DoubleSpeed(IPlayer player) : base(player)
        {
        }

        public override void Activate()
        {
            Receiver.PushMult(2);
        }
    }

    public class UnDoubleSpeed : GameplayCommand
    {
        public UnDoubleSpeed(IPlayer player) : base(player)
        {
        }

        public override void Activate()
        {
            Receiver.PushMult(1);
        }
    }

    public class TurnRight : GameplayCommand
    {
        public TurnRight(IPlayer player) : base(player)
        {
        }

        public override void Activate()
        {
            Receiver.PushX(1);
        }
    }
    public class TurnLeft : GameplayCommand
    {
        public TurnLeft(IPlayer player) : base(player)
        {
        }

        public override void Activate()
        {
            Receiver.PushX(-1);
        }
    }
    public class TurnUp : GameplayCommand
    {
        public TurnUp(IPlayer player) : base(player)
        {
        }

        public override void Activate()
        {
            Receiver.PushY(-1);
        }
    }
    public class TurnDown : GameplayCommand
    {
        public TurnDown(IPlayer player) : base(player)
        {
        }

        public override void Activate()
        {
            Receiver.PushY(1);
        }
    }
    public class UnTurnRight : GameplayCommand
    {
        public UnTurnRight(IPlayer player) : base(player)
        {
        }

        public override void Activate()
        {
            Receiver.PushX(-1);
        }
    }
    public class UnTurnLeft : GameplayCommand
    {
        public UnTurnLeft(IPlayer player) : base(player)
        {
        }

        public override void Activate()
        {
            Receiver.PushX(1);
        }
    }
    public class UnTurnUp : GameplayCommand
    {
        public UnTurnUp(IPlayer player) : base(player)
        {
        }

        public override void Activate()
        {
           Receiver.PushY(1);
        }
    }
    public class UnTurnDown : GameplayCommand
    {
        public UnTurnDown(IPlayer player) : base(player)
        {
        }

        public override void Activate()
        {
            Receiver.PushY(-1);
        }
    }

}
