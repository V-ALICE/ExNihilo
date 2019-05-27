using ExNihilo.Input.Commands;

namespace ExNihilo.Input.Controllers
{
    internal interface IController
    {
        CommandHandler Handler { get; set; }
        void UpdateInput();
    }
}
