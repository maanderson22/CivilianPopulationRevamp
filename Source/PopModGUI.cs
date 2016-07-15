using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using KSP;
using KSP.UI.Screens;

//required for ApplicationLauncherButton type

namespace CivilianPopulationRevamp
{
  [KSPAddon (KSPAddon.Startup.EveryScene, false)]

  public class PopModGUI : MonoBehaviour
  {
    static Rect _windowPosition = new Rect(Screen.width / 2 - 250, Screen.height / 2 - 250, 500, 300);
    static bool showPopInfo = false;
    internal static string assetFolder = "CivilianPopulationRevamp/Assets/";
    static bool CivPopGUIOn = false;
    internal bool CivPopTooltip = false;

    private static ApplicationLauncherButton appButton = null;

    /// <summary>
    /// Awake this instance.  Pre-existing method in Unity that runs before KSP loads.  Used to add button to KSP App Bar.
    /// </summary>
    public void Awake ()
    {
      Debug.Log (debuggingClass.modName + this.name + " is starting Awake()!");
      DontDestroyOnLoad (this);
      GameEvents.onGUIApplicationLauncherReady.Add (OnAppLauncherReady);//when AppLauncher can take apps, give it OnAppLauncherReady (mine)
      GameEvents.onGUIApplicationLauncherDestroyed.Add (OnAppLauncherDestroyed);//Not sure what this does
    }

    /// <summary>
    /// OnGUI() is called by the game every time it refreshes the GUI.  I just need it to check if the GUI is
    /// enabled and if it is, show it.
    /// </summary>
    public void OnGUI ()
    {//Executes code whenever screen refreshes.  Extension to enable use of button along main bar on top-right of screen.
      if (CivPopGUIOn) {
        _windowPosition = GUILayout.Window(0,_windowPosition,PopulationManagementGUI,"Civilian Population GUI");//If button is enabled, display rectangle.
      }//end if
    }
    //end OnGui extension

    public void OnAppLauncherDestroyed ()
    {
      if (appButton != null) {
        OnToggleFalse ();
        ApplicationLauncher.Instance.RemoveApplication (appButton);
      }
    }

    /// <summary>
    /// Raises the app launcher ready event.  Method to create an app button on the AppLauncher, as well as tell
    /// what/how the GUI is loaded.
    /// </summary>
    public void OnAppLauncherReady ()
    {
      string iconFile = "CivPopIcon";//This is the name of the file that stores the mod's icon to be used in the appLauncher
      if (HighLogic.LoadedScene == GameScenes.SPACECENTER && appButton == null) {//i.e. if running for the first time
        appButton = ApplicationLauncher.Instance.AddModApplication (
          OnToggleTrue,                           //Run OnToggleTrue() when user clicks button
          OnToggleFalse,                          //Run OnToggleFalse() when user clicks button again
          null, null, null, null,                 //do nothing during hover, exiting, enable/disable
          ApplicationLauncher.AppScenes.FLIGHT,   //When to show applauncher/GUI button (only during flight)
          GameDatabase.Instance.GetTexture (assetFolder + iconFile, false));//where to look for mod applauncher icon
        Debug.Log (debuggingClass.modName + "OnAppLauncherReady; button created!");
      }
      CivPopGUIOn = false;
    }


    /// <summary>
    /// Presumably what to do when the user opens/clicks the button.  Called from OnAppLauncherReady.
    /// </summary>
    private static void OnToggleTrue ()
    {
      CivPopGUIOn = true;//turns on flag for GUI
      Debug.Log (debuggingClass.modName + "Turning on GUI");
    }

    /// <summary>
    /// Presumably what to do when the user closes the button.  Called from OnAppLauncherReady.
    /// </summary>
    private static void OnToggleFalse ()
    {
      CivPopGUIOn = false;//turns off flag for GUI
      Debug.Log (debuggingClass.modName + "Turning off GUI");
    }
      
    /// <summary>
    /// This method controls how the window actually looks when the HUD window is displayed.
    /// </summary>
    public static void PopulationManagementGUI (int windowId)
    {
      string activeVesselName = FlightGlobals.ActiveVessel.vesselName;
      string activeVesselStatus = FlightGlobals.ActiveVessel.situation.ToString ();
      string activeSoI = FlightGlobals.currentMainBody.name;
      if (activeVesselName != null)
        activeVesselName = FlightGlobals.ActiveVessel.vesselName;
      else
        activeVesselName = "[Could not find name]";
      int numCivilians = 0;
      List<ProtoCrewMember> listCivilians = new List<ProtoCrewMember> ();
      foreach (ProtoCrewMember crewMember in FlightGlobals.ActiveVessel.GetVesselCrew()) {
        if (crewMember.trait == debuggingClass.civilianTrait) {
          listCivilians.Add (crewMember);
          numCivilians++;
        }
      }


      //begin GUI
      GUILayout.BeginVertical ();

      //row for ship information
      GUILayout.BeginHorizontal ();
      {
        GUILayout.Label ("Vessel Name:  " + activeVesselName);
        GUILayout.Label ("Status:  " + activeVesselStatus);
        GUILayout.Label ("SoI:  " + activeSoI);
      }
      GUILayout.EndHorizontal ();

      //row for civilian information
      GUILayout.BeginHorizontal ();
      {
        GUILayout.BeginVertical ();
        if (GUILayout.Button ("Show Civilian Info")) {
          showPopInfo = !showPopInfo;
          Debug.Log (debuggingClass.modName + "Civilian Info button pressed. New value:  " + showPopInfo);
        }
        if (showPopInfo) {
          if (listCivilians.FirstOrDefault () != null) {
            foreach (ProtoCrewMember crewMember in listCivilians) {
              GUILayout.BeginHorizontal ();
              {
                GUILayout.Label (crewMember.trait);
                GUILayout.Label (crewMember.name);
                if(GUILayout.Button("EVA")){
                  Debug.Log (debuggingClass.modName + "User pressed button to initiate EVA of " + crewMember.name);
                  FlightEVA.SpawnEVA (crewMember.KerbalRef);
                }
                /*if(GUILayout.Button("Transfer")){
                  Debug.Log (debuggingClass.modName + "User pressed button to initiated transfer of " + crewMember.name);
                  CrewTransfer.Create(crewMember.seat.part,crewMember);
                }Transfer disabled for now; need to figure out how to use highlighting from ShipManifest mod...*/
              }
              GUILayout.EndHorizontal ();
            }
          }
        }
        GUILayout.EndVertical ();
        GUILayout.Label ("Civilians:  " + numCivilians);
        GUILayout.Label ("Crew Capacity:  " + FlightGlobals.ActiveVessel.GetCrewCount () + "/" + FlightGlobals.ActiveVessel.GetCrewCapacity ());
      }
      GUILayout.EndHorizontal ();

      //row for close button
      GUILayout.BeginHorizontal ();
      {
        GUILayout.FlexibleSpace ();
        if (GUILayout.Button ("Close this Window", GUILayout.Width (200f))) {
          Debug.Log (debuggingClass.modName + "Closing CivPopGUI window");
          OnToggleFalse ();
        }
        GUILayout.FlexibleSpace ();
        GUILayout.EndHorizontal ();
      }
      GUILayout.EndVertical ();
      GUI.DragWindow ();
    }
  }
}