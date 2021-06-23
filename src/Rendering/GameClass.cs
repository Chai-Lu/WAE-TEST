﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rampastring.Tools;
using Rampastring.XNAUI;
using System;
using System.IO;
using TSMapEditor.CCEngine;
using TSMapEditor.Models;
using TSMapEditor.UI;
using TSMapEditor.UI.CursorActions;

namespace TSMapEditor.Rendering
{
    public class GameClass : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager graphics;
        private const string GameDirectory = "F:/Pelit/DTA Beta/";

        public GameClass()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            graphics.SynchronizeWithVerticalRetrace = false;
            Window.Title = "DTA Scenario Editor";
        }

        private WindowManager windowManager;

        private readonly char DSC = Path.DirectorySeparatorChar;

        protected override void Initialize()
        {
            base.Initialize();

            AssetLoader.Initialize(GraphicsDevice, Content);
            AssetLoader.AssetSearchPaths.Add(Environment.CurrentDirectory + DSC + "Content" + DSC);

            windowManager = new WindowManager(this, graphics);
            windowManager.Initialize(Content, Environment.CurrentDirectory + DSC + "Content" + DSC);

            windowManager.InitGraphicsMode(1600, 900, false);
            windowManager.SetRenderResolution(1600, 900);
            windowManager.CenterOnScreen();
            windowManager.Cursor.LoadNativeCursor(Environment.CurrentDirectory + DSC + "Content" + DSC + "cursor.cur");

            Components.Add(windowManager);

            InitTest();
        }

        private void InitTest()
        {
            
            IniFile rulesIni = new IniFile(Path.Combine(GameDirectory, "INI/Rules.ini"));
            IniFile firestormIni = new IniFile(Path.Combine(GameDirectory, "INI/Enhance.ini"));
            IniFile artIni = new IniFile(Path.Combine(GameDirectory, "INI/Art.ini"));
            IniFile artFSIni = new IniFile(Path.Combine(GameDirectory, "INI/ArtE.INI"));
            IniFile mapIni = new IniFile(Path.Combine(GameDirectory, "Maps/Missions/stomp.map"));
            //IniFile mapIni = new IniFile(Path.Combine(GameDirectory, "Maps/Default/a_buoyant_city.map"));
            Map map = new Map();
            map.LoadExisting(rulesIni, firestormIni, artIni, artFSIni, mapIni);

            Console.WriteLine();
            Console.WriteLine("Map loaded.");

            Theater theater = new Theater("Temperate", "INI/Tem.ini", "IsoTem.mix", "isotem.pal", "unittem.pal", ".tem", 'A');
            theater.ReadConfigINI(GameDirectory);

            CCFileManager ccFileManager = new CCFileManager();
            ccFileManager.AddSearchDirectory(Path.Combine(GameDirectory, "MIX/"));
            ccFileManager.AddSearchDirectory(Path.Combine(GameDirectory, "Map Editor/"));
            ccFileManager.LoadPrimaryMixFile("Cache.mix");
            ccFileManager.LoadPrimaryMixFile(theater.ContentMIXName);
            ccFileManager.LoadSecondaryMixFile("ECache00.mix");
            ccFileManager.LoadSecondaryMixFile("ECache01.mix");
            ccFileManager.LoadSecondaryMixFile("ECache02.mix");
            ccFileManager.LoadSecondaryMixFile("ECache03.mix");
            ccFileManager.LoadSecondaryMixFile("ECache04.mix");
            ccFileManager.LoadSecondaryMixFile("ECache05.mix");
            ccFileManager.LoadSecondaryMixFile("RampaCache.mix");

            TheaterGraphics theaterGraphics = new TheaterGraphics(GraphicsDevice, theater, ccFileManager, map.Rules);

            mapView = new MapView(windowManager, map, theaterGraphics);
            mapView.Width = windowManager.RenderResolutionX;
            mapView.Height = windowManager.RenderResolutionY;
            windowManager.AddAndInitializeControl(mapView);

            tileSelector = new TileSelector(windowManager, theaterGraphics);
            tileSelector.Width = windowManager.RenderResolutionX;
            tileSelector.Height = 300;
            tileSelector.Y = windowManager.RenderResolutionY - tileSelector.Height;
            windowManager.AddAndInitializeControl(tileSelector);
            tileSelector.TileDisplay.SelectedTileChanged += TileDisplay_SelectedTileChanged;

            terrainPlacementAction = new TerrainPlacementAction();
        }

        MapView mapView;
        TileSelector tileSelector;
        TerrainPlacementAction terrainPlacementAction;

        private void TileDisplay_SelectedTileChanged(object sender, EventArgs e)
        {
            mapView.CursorAction = terrainPlacementAction;
            terrainPlacementAction.Tile = tileSelector.TileDisplay.SelectedTile;
        }
    }
}
