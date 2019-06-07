using System;
using ExNihilo.UI;
using ExNihilo.Util;
using Microsoft.Xna.Framework;

namespace ExNihilo.Menus
{
    public class TitleMenu : Menu
    {
        private void DoThing(string s)
        {
            Container.Console.ForceMessage("", "<"+s+" pressed>");
        }

        public TitleMenu(GameContainer container) : base(container)
        {
            var newPanel = new UIPanel("ButtonPanel", new Vector2(0.5f, 1), new Vector2(0, 0.5f), UIElement.PositionType.CenterBottom);

            var button1 = new UIClickable("Button1", "UI/BigButtonUp", new Vector2(0, 0), newPanel, UIElement.PositionType.CenterTop, "UI/BigButtonDown");
            var button2 = new UIClickable("Button2", "UI/BigButtonUp", new Coordinate(0, 10), button1, UIElement.PositionType.CenterTop, UIElement.PositionType.CenterBottom, "UI/BigButtonDown");
            var button3 = new UIClickable("Button3", "UI/BigButtonUp", new Coordinate(0, 10), button2, UIElement.PositionType.CenterTop, UIElement.PositionType.CenterBottom, "UI/BigButtonDown");
            var button4 = new UIClickable("Button4", "UI/BigButtonUp", new Coordinate(0, 10), button3, UIElement.PositionType.CenterTop, UIElement.PositionType.CenterBottom, "UI/BigButtonDown");

            button1.RegisterCallback(DoThing);
            button2.RegisterCallback(DoThing);
            button3.RegisterCallback(DoThing);
            button4.RegisterCallback(DoThing);

            newPanel.AddElements(button1, button2, button3, button4);
            newPanel.AddElements(new UIText("Button1Text", new Coordinate(), "Button1", new[] { Color.Black }, button1, UIElement.PositionType.Center, UIElement.PositionType.Center));
            newPanel.AddElements(new UIText("Button2Text", new Coordinate(), "Button2", new[] { Color.Black }, button2, UIElement.PositionType.Center, UIElement.PositionType.Center));
            newPanel.AddElements(new UIText("Button3Text", new Coordinate(), "Button3", new[] { Color.Black }, button3, UIElement.PositionType.Center, UIElement.PositionType.Center));
            newPanel.AddElements(new UIText("Button4Text", new Coordinate(), "Button4", new[] { Color.Black }, button4, UIElement.PositionType.Center, UIElement.PositionType.Center));

            _menuUI.AddElements(newPanel);
        }

        public override void Enter()
        {
            
        }

        public override bool TryBackOut()
        {
            return true;
        }

        protected override void MenuDown()
        {
            
        }

        protected override void MenuUp()
        {
            
        }

        protected override void MenuLeft()
        {
            
        }

        protected override void MenuRight()
        {
            
        }

        protected override void Select()
        {
            
        }

        public override void Update()
        {

        }

    }
}
