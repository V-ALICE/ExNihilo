using ExNihilo.Systems;
using ExNihilo.UI;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Menus
{
    public class CharacterMenu : Menu
    {
/********************************************************************
------->Menu Callbacks
********************************************************************/
        private void CloseMenu(UICallbackPackage package)
        {
            Dead = true;
        }

        private void MakeNewChar(UICallbackPackage package)
        {
            _creatingNewCharacter = true;
            _newCharUI.OnMoveMouse(_lastMousePosition);
        }

/********************************************************************
------->New Character Callbacks
********************************************************************/
        private void CancelNewChar(UICallbackPackage package)
        {
            _creatingNewCharacter = false;
            _panelUI.OnMoveMouse(_lastMousePosition);
        }

        private void ChangeCharacter(UICallbackPackage package)
        {
            var left = package.Caller.EndsWith("Left");
            
            if (package.Caller.StartsWith("Cloth"))
            {
                if (left) _cloth = (_cloth - 1 + _clothCount) % _clothCount;
                else _cloth = (_cloth + 1) % _clothCount;
                var sheet = TextureLibrary.Lookup("Char/cloth/" + (_cloth+1));
                var rect = new Rectangle(0, sheet.Height / 2, sheet.Width, sheet.Height / 4);
                var cloth = new AnimatableTexture(TextureUtilities.GetSubTexture(Container.GraphicsDevice, sheet, rect), 4, 4);
                (_newCharUI.GetElement("CharClothDisplay") as UIDynamicElement)?.ChangeTexture(cloth);
                (_newCharUI.GetElement("CharBodyDisplay") as UIDynamicElement)?.ResetTexture();
                (_newCharUI.GetElement("CharHairDisplay") as UIDynamicElement)?.ResetTexture();
            }
            else if (package.Caller.StartsWith("Hair"))
            {
                if (left) _hair = (_hair - 1 + _hairCount) % _hairCount;
                else _hair = (_hair + 1) % _hairCount;
                var sheet = TextureLibrary.Lookup("Char/hair/" + (_hair+1) + "-" + (_color+1));
                var rect = new Rectangle(0, sheet.Height / 2, sheet.Width, sheet.Height / 4);
                var hair = new AnimatableTexture(TextureUtilities.GetSubTexture(Container.GraphicsDevice, sheet, rect), 4, 4);
                (_newCharUI.GetElement("CharHairDisplay") as UIDynamicElement)?.ChangeTexture(hair);
                (_newCharUI.GetElement("CharBodyDisplay") as UIDynamicElement)?.ResetTexture();
                (_newCharUI.GetElement("CharClothDisplay") as UIDynamicElement)?.ResetTexture();
            }
            else if (package.Caller.StartsWith("Color"))
            {
                if (left) _color = (_color - 1 + _colorCount) % _colorCount;
                else _color = (_color + 1) % _colorCount;
                var sheet = TextureLibrary.Lookup("Char/hair/" + (_hair+1) + "-" + (_color+1));
                var rect = new Rectangle(0, sheet.Height / 2, sheet.Width, sheet.Height / 4);
                var hair = new AnimatableTexture(TextureUtilities.GetSubTexture(Container.GraphicsDevice, sheet, rect), 4, 4);
                (_newCharUI.GetElement("CharHairDisplay") as UIDynamicElement)?.ChangeTexture(hair);
                (_newCharUI.GetElement("CharBodyDisplay") as UIDynamicElement)?.ResetTexture();
                (_newCharUI.GetElement("CharClothDisplay") as UIDynamicElement)?.ResetTexture();
            }
            else if (package.Caller.StartsWith("Body"))
            {
                if (left) _body = (_body - 1 + _bodyCount) % _bodyCount;
                else _body = (_body + 1) % _bodyCount;
                var sheet = TextureLibrary.Lookup("Char/base/" + (_body+1));
                var rect = new Rectangle(0, sheet.Height / 2, sheet.Width, sheet.Height / 4);
                var body = new AnimatableTexture(TextureUtilities.GetSubTexture(Container.GraphicsDevice, sheet, rect), 4, 4);
                (_newCharUI.GetElement("CharBodyDisplay") as UIDynamicElement)?.ChangeTexture(body);
                (_newCharUI.GetElement("CharClothDisplay") as UIDynamicElement)?.ResetTexture();
                (_newCharUI.GetElement("CharHairDisplay") as UIDynamicElement)?.ResetTexture();
            }
        }

/********************************************************************
------->Menu Functions
********************************************************************/
        private readonly UIPanel _panelUI, _newCharUI;
        private int _body, _hair, _cloth, _color;
        private bool _creatingNewCharacter;
        private Point _lastMousePosition;

        private const int _bodyCount = 3, _hairCount = 42, _clothCount = 43, _colorCount = 10;

        public CharacterMenu(GameContainer container) : base(container)
        {
            _panelUI = new UIPanel("this.MenuKing", new Vector2(0.5f, 0.5f), Vector2.One, TextureUtilities.PositionType.Center);
            _newCharUI = new UIPanel("this.MenuKing", new Vector2(0.5f, 0.5f), Vector2.One, TextureUtilities.PositionType.Center);

            var backdrop = new UIElement("Backdrop", "UI/decor/Backdrop", new Vector2(0.5f, 0.5f), Color.White, _panelUI, TextureUtilities.PositionType.Center);
            var backButton = new UIClickable("BackButton", "UI/button/SmallButton", new Coordinate(14, -14), ColorScale.White, backdrop, TextureUtilities.PositionType.BottomLeft, TextureUtilities.PositionType.BottomLeft);
            var backButtonText = new UIText("BackButtonText", new Coordinate(), "Back", ColorScale.Black, backButton, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var currentCharacterSet = new UIElement("CharacterSet", "UI/field/SevenElementSet", new Coordinate(18, -100), ColorScale.White, backdrop, TextureUtilities.PositionType.BottomLeft, TextureUtilities.PositionType.BottomLeft);
            var newCharButton = new UIClickable("NewCharButton", "UI/button/SmallButton", new Coordinate(-50, 50), ColorScale.White, backdrop, TextureUtilities.PositionType.TopRight, TextureUtilities.PositionType.TopRight);
            var newCharButtonText = new UIText("NewCharButtonText", new Coordinate(), "New", ColorScale.Black, newCharButton, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);

            backButton.RegisterCallback(CloseMenu);
            newCharButton.RegisterCallback(MakeNewChar);
            backButton.SetExtraStates("UI/button/SmallButtonDown", "UI/button/SmallButtonOver");
            newCharButton.SetExtraStates("UI/button/SmallButtonDown", "UI/button/SmallButtonOver");

            _panelUI.AddElements(backdrop, currentCharacterSet, backButton, backButtonText, newCharButton, newCharButtonText);

            var backdrop2 = new UIElement("Backdrop2", "UI/decor/Backdrop", new Vector2(0.5f, 0.5f), Color.White, _newCharUI, TextureUtilities.PositionType.Center);
            var cancelButton = new UIClickable("CancelButton", "UI/button/SmallButton", new Coordinate(10, -10), ColorScale.White, backdrop2, TextureUtilities.PositionType.BottomLeft, TextureUtilities.PositionType.BottomLeft);
            var cancelButtonText = new UIText("CancelButtonText", new Coordinate(), "Cancel", ColorScale.Black, cancelButton, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            
            var charDesignPanel = new UIPanel("CharDesignPanel", new Coordinate(-50, 50), new Coordinate(200, 125), backdrop2, TextureUtilities.PositionType.TopRight, TextureUtilities.PositionType.TopRight);
            var charBodyDisplay = new UIDynamicElement("CharBodyDisplay", "null", new Coordinate(), ColorScale.White, charDesignPanel, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var charClothDisplay = new UIDynamicElement("CharClothDisplay", "null", new Coordinate(), ColorScale.White, charDesignPanel, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var charHairDisplay = new UIDynamicElement("CharHairDisplay", "null", new Coordinate(), ColorScale.White, charDesignPanel, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);

            var leftHair = new UIClickable("HairLeft", "UI/button/RedBulb", new Vector2(), ColorScale.White, charDesignPanel, TextureUtilities.PositionType.Center);
            var leftBody = new UIClickable("BodyLeft", "UI/button/GreenBulb", new Vector2(0, 0.33f), ColorScale.White, charDesignPanel, TextureUtilities.PositionType.Center);
            var leftCloth = new UIClickable("ClothLeft", "UI/button/BlueBulb", new Vector2(0, 0.67f), ColorScale.White, charDesignPanel, TextureUtilities.PositionType.Center);
            var leftColor = new UIClickable("ColorLeft", "UI/button/BlackBulb", new Vector2(0, 1), ColorScale.White, charDesignPanel, TextureUtilities.PositionType.Center);

            var rightHair = new UIClickable("HairRight", "UI/button/RedBulb", new Vector2(1, 0), ColorScale.White, charDesignPanel, TextureUtilities.PositionType.Center);
            var rightBody = new UIClickable("BodyRight", "UI/button/GreenBulb", new Vector2(1, 0.33f), ColorScale.White, charDesignPanel, TextureUtilities.PositionType.Center);
            var rightCloth = new UIClickable("ClothRight", "UI/button/BlueBulb", new Vector2(1, 0.67f), ColorScale.White, charDesignPanel, TextureUtilities.PositionType.Center);
            var rightColor = new UIClickable("ColorRight", "UI/button/BlackBulb", new Vector2(1, 1), ColorScale.White, charDesignPanel, TextureUtilities.PositionType.Center);

            var color = new ColorScale(new Color(160, 160, 160, 128));
            var leftArrow1 = new UIElement("Left1", "UI/icon/Left", new Coordinate(), color, leftHair, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var leftArrow2 = new UIElement("Left2", "UI/icon/Left", new Coordinate(), color, leftBody, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var leftArrow3 = new UIElement("Left3", "UI/icon/Left", new Coordinate(), color, leftCloth, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var leftArrow4 = new UIElement("Left4", "UI/icon/Left", new Coordinate(), color, leftColor, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            
            var rightArrow1 = new UIElement("Right1", "UI/icon/Right", new Coordinate(), color, rightHair, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var rightArrow2 = new UIElement("Right2", "UI/icon/Right", new Coordinate(), color, rightBody, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var rightArrow3 = new UIElement("Right3", "UI/icon/Right", new Coordinate(), color, rightCloth, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);
            var rightArrow4 = new UIElement("Right4", "UI/icon/Right", new Coordinate(), color, rightColor, TextureUtilities.PositionType.Center, TextureUtilities.PositionType.Center);

            cancelButton.RegisterCallback(CancelNewChar);
            cancelButton.SetExtraStates("UI/button/SmallButtonDown", "UI/button/SmallButtonOver");
            leftHair.SetExtraStates("UI/button/RedBulbDown", "UI/button/RedBulbOver");
            leftBody.SetExtraStates("UI/button/GreenBulbDown", "UI/button/GreenBulbOver");
            leftCloth.SetExtraStates("UI/button/BlueBulbDown", "UI/button/BlueBulbOver");
            leftColor.SetExtraStates("UI/button/BlackBulbDown", "UI/button/BlackBulbOver");
            rightHair.SetExtraStates("UI/button/RedBulbDown", "UI/button/RedBulbOver");
            rightBody.SetExtraStates("UI/button/GreenBulbDown", "UI/button/GreenBulbOver");
            rightCloth.SetExtraStates("UI/button/BlueBulbDown", "UI/button/BlueBulbOver");
            rightColor.SetExtraStates("UI/button/BlackBulbDown", "UI/button/BlackBulbOver");
            leftHair.RegisterCallback(ChangeCharacter);
            leftBody.RegisterCallback(ChangeCharacter);
            leftCloth.RegisterCallback(ChangeCharacter);
            leftColor.RegisterCallback(ChangeCharacter);
            rightHair.RegisterCallback(ChangeCharacter);
            rightBody.RegisterCallback(ChangeCharacter);
            rightCloth.RegisterCallback(ChangeCharacter);
            rightColor.RegisterCallback(ChangeCharacter);
            charBodyDisplay.SetRules(TextureLibrary.GiantScaleRuleSet);
            charClothDisplay.SetRules(TextureLibrary.GiantScaleRuleSet);
            charHairDisplay.SetRules(TextureLibrary.GiantScaleRuleSet);

            charDesignPanel.AddElements(charBodyDisplay, charClothDisplay, charHairDisplay, leftBody, leftCloth, leftColor, leftHair, rightColor, rightBody, rightCloth, rightHair,
                leftArrow1, leftArrow2, leftArrow3, leftArrow4, rightArrow1, rightArrow2, rightArrow3, rightArrow4);
            _newCharUI.AddElements(backdrop2, cancelButton, cancelButtonText, charDesignPanel);
        }

        public override void Enter(Point point)
        {
            _lastMousePosition = point;
            _panelUI.OnMoveMouse(point);
            Dead = false;
            _creatingNewCharacter = false;
        }

        public override void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            _panelUI.LoadContent(graphics, content);
            _newCharUI.LoadContent(graphics, content);

            _body = _hair = _cloth = _color = 0;
            var sheet = TextureLibrary.Lookup("Char/base/"+(_body+1));
            var rect = new Rectangle(0, sheet.Height/2, sheet.Width, sheet.Height/4);
            var body = new AnimatableTexture(TextureUtilities.GetSubTexture(graphics, sheet, rect), 4, 4);
            var cloth = new AnimatableTexture(TextureUtilities.GetSubTexture(graphics, TextureLibrary.Lookup("Char/cloth/" + (_cloth + 1)), rect), 4, 4);
            var hair = new AnimatableTexture(TextureUtilities.GetSubTexture(graphics, TextureLibrary.Lookup("Char/hair/" + (_hair + 1) + "-" + (_color + 1)), rect), 4, 4);
            (_newCharUI.GetElement("CharBodyDisplay") as UIDynamicElement)?.ChangeTexture(body);
            (_newCharUI.GetElement("CharClothDisplay") as UIDynamicElement)?.ChangeTexture(cloth);
            (_newCharUI.GetElement("CharHairDisplay") as UIDynamicElement)?.ChangeTexture(hair);
        }

        public override void OnResize(GraphicsDevice graphics, Coordinate gameWindow)
        {
            _panelUI.OnResize(graphics, gameWindow);
            _newCharUI.OnResize(graphics, gameWindow);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_creatingNewCharacter) _newCharUI.Draw(spriteBatch);
            else _panelUI.Draw(spriteBatch);
        }

        public override void OnMoveMouse(Point point)
        {
            if (_creatingNewCharacter) _newCharUI.OnMoveMouse(point);
            else _panelUI.OnMoveMouse(point);
        }

        public override bool OnLeftClick(Point point)
        {
            if (_creatingNewCharacter) return _newCharUI.OnLeftClick(point);
            return _panelUI.OnLeftClick(point);
        }

        public override void OnLeftRelease(Point point)
        {
            if (_creatingNewCharacter) _newCharUI.OnLeftRelease(point);
            else _panelUI.OnLeftRelease(point);
        }

        public override void ReceiveInput(string input)
        {
            
        }
    }
}
