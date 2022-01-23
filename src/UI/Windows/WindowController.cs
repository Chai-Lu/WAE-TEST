﻿using Rampastring.XNAUI.XNAControls;
using System.Collections.Generic;
using TSMapEditor.Models;
using TSMapEditor.Rendering;
using TSMapEditor.UI.Controls;

namespace TSMapEditor.UI.Windows
{
    public class WindowController
    {
        private List<EditorWindow> Windows { get; } = new List<EditorWindow>();

        public BasicSectionConfigWindow BasicSectionConfigWindow { get; private set; }
        public TaskforcesWindow TaskForcesWindow { get; private set; }
        public ScriptsWindow ScriptsWindow { get; private set; }
        public TeamTypesWindow TeamTypesWindow { get; private set; }
        public TriggersWindow TriggersWindow { get; private set; }
        public PlaceWaypointWindow PlaceWaypointWindow { get; private set; }
        public LocalVariablesWindow LocalVariablesWindow { get; private set; }
        public StructureOptionsWindow StructureOptionsWindow { get; private set; }
        public VehicleOptionsWindow VehicleOptionsWindow { get; private set; }
        public InfantryOptionsWindow InfantryOptionsWindow { get; private set; }
        public HousesWindow HousesWindow { get; private set; }
        public SaveMapAsWindow SaveMapAsWindow { get; private set; }
        public AutoApplyImpassableOverlayWindow AutoApplyImpassableOverlayWindow { get; private set; }

        public void Initialize(XNAControl windowParentControl, Map map, EditorState editorState, ICursorActionTarget cursorActionTarget)
        {
            BasicSectionConfigWindow = new BasicSectionConfigWindow(windowParentControl.WindowManager, map);
            Windows.Add(BasicSectionConfigWindow);

            TaskForcesWindow = new TaskforcesWindow(windowParentControl.WindowManager, map);
            Windows.Add(TaskForcesWindow);

            ScriptsWindow = new ScriptsWindow(windowParentControl.WindowManager, map);
            Windows.Add(ScriptsWindow);

            TeamTypesWindow = new TeamTypesWindow(windowParentControl.WindowManager, map);
            Windows.Add(TeamTypesWindow);

            TriggersWindow = new TriggersWindow(windowParentControl.WindowManager, map, editorState, cursorActionTarget);
            Windows.Add(TriggersWindow);

            PlaceWaypointWindow = new PlaceWaypointWindow(windowParentControl.WindowManager, map, cursorActionTarget.MutationManager, cursorActionTarget.MutationTarget);
            Windows.Add(PlaceWaypointWindow);

            LocalVariablesWindow = new LocalVariablesWindow(windowParentControl.WindowManager, map);
            Windows.Add(LocalVariablesWindow);

            StructureOptionsWindow = new StructureOptionsWindow(windowParentControl.WindowManager, map);
            Windows.Add(StructureOptionsWindow);

            VehicleOptionsWindow = new VehicleOptionsWindow(windowParentControl.WindowManager, map);
            Windows.Add(VehicleOptionsWindow);

            InfantryOptionsWindow = new InfantryOptionsWindow(windowParentControl.WindowManager, map);
            Windows.Add(InfantryOptionsWindow);

            HousesWindow = new HousesWindow(windowParentControl.WindowManager, map);
            Windows.Add(HousesWindow);

            SaveMapAsWindow = new SaveMapAsWindow(windowParentControl.WindowManager, map);
            Windows.Add(SaveMapAsWindow);

            AutoApplyImpassableOverlayWindow = new AutoApplyImpassableOverlayWindow(windowParentControl.WindowManager, map, cursorActionTarget.MutationTarget);
            Windows.Add(AutoApplyImpassableOverlayWindow);

            foreach (var window in Windows)
            {
                windowParentControl.AddChild(window);
                window.Disable();
                window.CenterOnParent();
            }
        }
    }
}
