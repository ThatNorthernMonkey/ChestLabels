using Storm.ExternalEvent;
using Storm.StardewValley;
using Storm.StardewValley.Event;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using xTile;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storm.StardewValley.Accessor;
using Storm.StardewValley.Wrapper;
using Microsoft.Xna.Framework.Input;
using Storm.StardewValley.Proxy;
using Storm.Collections;

namespace ChestLabels
{
    [Mod]
    public class ChestLabels : DiskResource
    {
        private int MousePosX { get; set; }
        private int MousePosY { get; set; }
        private int CursorPosX { get; set; }
        private int CursorPosY { get; set; }
        public bool MouseOverChest { get; set; }
        public List<ObjectItem> ChestsInLocation { get; set; }
        public string ChestTooltipText { get; set; }
        public string CurrentLocation { get; set; }
        public bool LocationHasChanged { get; set; }
        public int TimeLastUpdatedChests { get; set; }
        public int TimeToUpdateChests { get; set; }
        public bool HasLoadedGame { get; set; }
        public WrappedProxyList<GameLocationAccessor, GameLocation> Locations { get; set; }
        public ValueProxyDictionary<Vector2, ObjectAccessor, ObjectItem> FarmObjects { get; set; }
        public int TimeOfDay { get; set; }
        public string LiveLocationName { get; set; }

        public ChestLabels()
        {
            MouseOverChest = false;
            ChestsInLocation = new List<ObjectItem>();
        }

        [Subscribe]
        public void TestTextboxCreation(KeyPressedEvent @e)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.L))
            {

            }
        }

        [Subscribe]
        public void PostRenderCallback(PreUIRenderEvent @e)
        {
            if (MouseOverChest)
            {

                var textLen = ChestTooltipText.Length;
                var width = (textLen * 10);

                var root = @e.Root;
                var batch = root.SpriteBatch;
                var font = root.SmoothFont;
                var pos = new Vector2 { X = CursorPosX - 40, Y = CursorPosY - 28 };
                var bg = new Rectangle(CursorPosX - 40, CursorPosY - 25, width, 20);
                var tex = new Texture2D(@e.Root.Graphics.GraphicsDevice, width, 20);
                var bgColour = Color.Black;
                Color[] data = new Color[width * 20];
                for (int i = 0; i < data.Length; ++i) data[i] = bgColour;


                tex.SetData(data);
                batch.Draw(tex, bg, Color.Black);
                batch.DrawString(font, ChestTooltipText, pos, Color.DarkOrange);
                MouseOverChest = false;
            }
        }

        [Subscribe]
        public void OnGameLoad(PostGameLoadedEvent @e)
        {
            Locations = @e.Root.Locations;
            UpdateChests();
        }

        [Subscribe]
        public void UpdateCallback(PostUpdateEvent @e)
        {
            HasLoadedGame = @e.Root.HasLoadedGame;

            if (HasLoadedGame)
            {
                TimeOfDay = @e.Root.TimeOfDay;
                LiveLocationName = @e.Location.Name;

                CheckTimeForNewSave();

                //Get the current mouse cursors tile position
                var mouseManager = Mouse.GetState();

                CursorPosX = mouseManager.X;
                CursorPosY = mouseManager.Y;

                var viewport = @e.Root.Viewport;

                int xTile = (viewport.X + mouseManager.X) / @e.Root.TileSize;
                int yTile = (viewport.Y + mouseManager.Y) / @e.Root.TileSize;

                var mouseTilePos = new Vector2 { X = xTile, Y = yTile };

                MousePosX = xTile;
                MousePosY = yTile;

                // See if our mouse is hovering over a chest, if so update the draw label
                foreach (var c in ChestsInLocation)
                {
                    if ((c.BoundingBox.X / @e.Root.TileSize) == (MousePosX) && (c.BoundingBox.Y / @e.Root.TileSize) == (MousePosY))
                    {

                        MouseOverChest = true;

                        var haveName = false;
                        var labelName = "Empty";
                        var items = c.As<ObjectAccessor, Chest>().Items;
                        for (int i = 0; i < items.Count; i++)
                        {
                            if (items[i].Is<ObjectAccessor>() && !haveName)
                            {
                                labelName = items[i].As<ObjectAccessor, ObjectItem>().Name;
                                haveName = true;

                                int category = items[i].As<ObjectAccessor, ObjectItem>().Category;                              
                                var categoryName = GetCategoryName(category);
                                if (categoryName != "")
                                {
                                    labelName += " - ";
                                    labelName += categoryName;
                                }
                            }
                        }
                        ChestTooltipText = labelName;
                    }
                }
            }
        }

        public void CheckTimeForNewSave()
        {
            if (HasLoadedGame)
            {
                if (TimeLastUpdatedChests == 0)
                {
                    TimeLastUpdatedChests = TimeOfDay;
                    TimeToUpdateChests = TimeLastUpdatedChests + 10;
                }

                if (TimeOfDay == 600)
                {
                    UpdateChests();
                    TimeLastUpdatedChests = TimeOfDay;
                    TimeToUpdateChests = TimeLastUpdatedChests + 10;
                }

                if (TimeOfDay >= TimeToUpdateChests && !IsTimeWonky())
                {
                    UpdateChests();
                    TimeLastUpdatedChests = TimeOfDay;
                    TimeToUpdateChests = TimeLastUpdatedChests + 10;
                }

                // See if we have changed locations
                if (CurrentLocation != LiveLocationName)
                {
                    CurrentLocation = LiveLocationName;
                    LocationHasChanged = true;
                }
            }
        }
        public void UpdateChests()
        {
            // Update our reference of chests incase things have changed. Temporary until we have access to Chest events.
            if (LocationHasChanged == true && CurrentLocation == "Farm" || CurrentLocation == "FarmHouse" || CurrentLocation == "Greenhouse" || CurrentLocation == "Barn" || CurrentLocation == "Coop")
            {
                ChestsInLocation.RemoveAll(c => c.Name == "Chest");
                
                for (int i = 0; i < @Locations.Count; i++)
                {
                    var loc = Locations[i];
                    FarmObjects = loc.Objects;
                    foreach (var f in FarmObjects.Keys)
                    {
                        if (FarmObjects[f].Name == "Chest")
                        {
                            if (FarmObjects[f].As<ObjectAccessor, Chest>().PlayerChest)
                            {
                                if (!ChestsInLocation.Contains(FarmObjects[f]))
                                {
                                    ChestsInLocation.Add(FarmObjects[f]);
                                }
                            }
                        }
                    }
                }

                LocationHasChanged = false;
            }
        }

        public bool IsTimeWonky()
        {
            //Bad method name - time is ALWAYS wonky in SV.
            var time = TimeToUpdateChests.ToString();
            int hour = (int)(TimeToUpdateChests.ToString()[0]);
            time = time.Remove(0, 1);
            var newTime = Int32.Parse(time);
            if (newTime >= 60)
            {
                hour++;
                newTime = 00;
                string timeString = hour.ToString();
                timeString += newTime.ToString();
                TimeToUpdateChests = Int32.Parse(timeString);
                return true;
            }
            return false;
        }

        public string GetCategoryName(int id)
        {
            // Hard coded until getCategoryName is implemented.
            switch (id)
            {
                case -81:
                    return "Forage";
                case -80:
                    return "Flower";
                case -79:
                    return "Fruit";
                case -78:
                case -77:
                case -76:
                    break;
                case -75:
                    return "Vegetable";
                case -74:
                    return "Seed";
                default:
                    switch (id)
                    {
                        case -28:
                            return "Monster Loot";
                        case -27:
                        case -26:
                            return "Artisan Goods";
                        case -25:
                            return "Cooking";
                        case -24:
                            return "Decor";
                        case -22:
                            return "Fishing Tackle";
                        case -21:
                            return "Bait";
                        case -20:
                            return "Trash";
                        case -19:
                            return "Fertilizer";
                        case -18:
                        case -14:
                        case -5:
                            return "Animal Product";
                        case -16:
                        case -15:
                            return "Resource";
                        case -12:
                        case -2:
                            return "Mineral";
                        case -8:
                            return "Crafting";
                        case -7:
                            return "Cooking";
                        case -6:
                            return "Animal Product";
                        case -4:
                            return "Fish";
                    }
                    break;
            }
            return "";
        }


        private class ClickMenuTest : ClickableMenuDelegate
        {
            public override void Draw(SpriteBatch b)
            {

            }

            public override void EmergencyShutDown()
            {
                throw new NotImplementedException();
            }

            public override void PerformHoverAction(int x, int y)
            {
                throw new NotImplementedException();
            }

            public override bool ReadyToClose()
            {
                throw new NotImplementedException();
            }

            public override void ReceiveGamePadButton(Buttons b)
            {
                throw new NotImplementedException();
            }

            public override void ReceiveKeyPress(Keys key)
            {
                throw new NotImplementedException();
            }

            public override void ReceiveLeftClick(int x, int y, bool playSound = true)
            {
                throw new NotImplementedException();
            }

            public override void ReceiveRightClick(int x, int y, bool playSound = true)
            {
                throw new NotImplementedException();
            }

            public override void ReceiveScrollWheelAction(int direction)
            {
                throw new NotImplementedException();
            }

            public override void Update(GameTime time)
            {
                throw new NotImplementedException();
            }
        }

        /*
        Add chest to json file on open. Fire some event for chest opening? 
        Work out how to add a name. TextBox. 
        Save labels to json file.
        Location of chest with that name Vector2.
        Get mouse position Vector2, see if it is over a Chest tile located in json.
        OnMouseOver event to call the sprite render above tile.
        OnMouseOut event to remove the sprite render.
        */
    }
}
