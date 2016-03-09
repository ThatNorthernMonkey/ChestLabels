using Storm.ExternalEvent;
using Storm.StardewValley;
using Storm.StardewValley.Event;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ChestLabels
{
    [Mod]
    public class ChestLabels : DiskResource
    {

        public Vector2 MousePosition { get; set; }

        [Subscribe]
        public void UpdateCallback(PreUpdateEvent @event)
        {
            var mouseManager = Microsoft.Xna.Framework.Input.Mouse.GetState();           
            this.MousePosition = new Vector2(mouseManager.X, mouseManager.Y);
            //Console.WriteLine("X: " + MousePosition.X.ToString() + "Y: " + MousePosition.Y.ToString());

        }


        /*
        [Subscribe]
        public void PostRender(PreUIRenderEvent ev)
        {
            var font = ev.Root.DialogueFont;
            var batch = ev.Root.SpriteBatch;

            var testText = "X: " + MousePosition.X.ToString() + "Y: " + MousePosition.Y.ToString();
            var position = new Vector2();
            var textColour = new Color(150, 0, 0, 200);

            position.X = 200;
            position.Y = 200;
            batch.DrawString(
                font,
                testText,
                position,
                textColour
                );
        }

        */
        /*
        Add chest to json file on open. Fire some event for chest opening? 
        Work out how to add a name. TextBox. Location of chest with that name Vector2.
        Get mouse position Vector2, see if it is over a Chest tile.
        Save labels to json file.
        OnMouseOver event to call the sprite render above tile.
        OnMouseOut event to remove the sprite render.

        */
    }
}
