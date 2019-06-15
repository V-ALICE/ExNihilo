using System;
using ExNihilo.Menus;

namespace ExNihilo.Systems.Bases
{
    public abstract class Interactive
    {
        public readonly string Name;

        protected Interactive(string name)
        {
            Name = name;
        }

    }

    public class MenuInteractive : Interactive
    {
        public readonly Menu InteractionMenu;

        public MenuInteractive(string name, Menu menu) : base(name)
        {
            InteractionMenu = menu;
        }

    }

    public class ActionInteractive : Interactive
    {
        public readonly Action Function;

        public ActionInteractive(string name, Action action) : base(name)
        {
            Function = action;
        }

    }

}
