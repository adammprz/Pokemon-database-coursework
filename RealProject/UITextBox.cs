using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace RealProject
{
    internal class UITextBox: UIRect
    {
        string text;
        Color textColor;
        int alignment;
        bool doDropShadow;
        Color dropShadowColor;
        public UITextBox(float x, float y, string text, Color textColor, int alignment, bool doDropShadow = false, Color? dropShadowColor = null, UIRect parent = null) : base(null, x, y, parent)
        {
            this.text = text;
            this.textColor = textColor;
            this.alignment = alignment;

            this.doDropShadow = (bool)doDropShadow;

            if (dropShadowColor != null)
                this.dropShadowColor = (Color)dropShadowColor;
        }

        public void SetText(string t)
        {
            text = t;
        }

        public override void SetColor(Color c)
        {
            textColor = c;
        }

        public void SetDropShadowColor(Color c)
        {
            dropShadowColor = c;
        }

        float GetStringWidth()
        {
            float pixelWidth = 0;

            pixelWidth += Global.pokeFont.MeasureString(text).X;

            return pixelWidth;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Vector2 textSize = Global.pokeFont.MeasureString(text);
            Vector2 origin = Vector2.Zero;

            // 0 = left, 1 = center, 2 = right
            if (alignment == 1) // center
                origin.X = textSize.X / 2f;
            else if (alignment == 2) // right
                origin.X = textSize.X;

            if (doDropShadow)
            {
                Vector2 shadowOffset1 = new Vector2(Global.pixelsPerUnit, 0);
                Vector2 shadowOffset2 = new Vector2(Global.pixelsPerUnit, Global.pixelsPerUnit);
                Vector2 shadowOffset3 = new Vector2(0, Global.pixelsPerUnit);

                spriteBatch.DrawString(Global.pokeFont, text, pos + shadowOffset1, dropShadowColor, 0, origin, Global.pixelsPerUnit, SpriteEffects.None, 0);
                spriteBatch.DrawString(Global.pokeFont, text, pos + shadowOffset2, dropShadowColor, 0, origin, Global.pixelsPerUnit, SpriteEffects.None, 0);
                spriteBatch.DrawString(Global.pokeFont, text, pos + shadowOffset3, dropShadowColor, 0, origin, Global.pixelsPerUnit, SpriteEffects.None, 0);
            }

            spriteBatch.DrawString(Global.pokeFont, text, pos, textColor, 0, origin, Global.pixelsPerUnit, SpriteEffects.None, 0);
        }
    }
}
