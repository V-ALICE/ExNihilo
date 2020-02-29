using System;
using System.Linq;
using ExNihilo.Menus;
using ExNihilo.Systems.Game.Items;
using ExNihilo.Util;
using Microsoft.Xna.Framework.Graphics;

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
        private readonly Menu _interactionMenu;

        public MenuInteractive(string name, Menu menu) : base(name)
        {
            _interactionMenu = menu;
        }

        public virtual Menu Access()
        {
            return _interactionMenu;
        }
    }

    public class BoxInteractive : MenuInteractive
    {
        private const int _containerSize = 7, _slotFillChance = 30;
        private readonly Texture2D _closed, _open;
        private readonly ItemInstance[] _contents = new ItemInstance[_containerSize];
        private bool _opened;

        public BoxInteractive(string name, Texture2D open, Texture2D closed, Random rand, int floor) : base(name, BoxMenu.Menu)
        {
            _closed = closed;
            _open = open;
            for (int i = 0; i < _contents.Length; i++)
            {
                if (MathD.Chance(rand, _slotFillChance))
                {
                    _contents[i] = ItemLoader.GetItem(rand, floor);
                }
                else
                {
                    _contents[i] = null;
                }
            }
        }

        public Texture2D GetTexture()
        {
            return _opened ? _open : _closed;
        }

        public override Menu Access()
        {
            _opened = true;
            return base.Access();
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
