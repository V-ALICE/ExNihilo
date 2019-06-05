using ExNihilo.UI;
using Microsoft.Xna.Framework;

namespace ExNihilo.Menus
{
    public class TitleMenu : Menu
    {
        public TitleMenu()
        {
            var newPanel = new UIPanel(new Vector2(0.5f, 1), new Vector2(0, 0.5f), UIElement.PositionType.CenterBottom);
            newPanel.AddElements(new UIClickable("UI/BigButtonUp", new Vector2(0, 0), "UI/BigButtonDown"));
            newPanel.AddElements(new UIClickable("UI/BigButtonUp", new Vector2(0, 0.23f), "UI/BigButtonDown"));
            newPanel.AddElements(new UIClickable("UI/BigButtonUp", new Vector2(0, 0.46f), "UI/BigButtonDown"));
            newPanel.AddElements(new UIClickable("UI/BigButtonUp", new Vector2(0, 0.69f), "UI/BigButtonDown"));
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
