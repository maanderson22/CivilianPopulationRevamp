using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP;

namespace CivilianPopulationRevamp
{
  public class CivilianPopulationRegulator : BaseConverter
  {
    /// <summary>
    /// The current rate at which a civilian is created.  Typically around 1E-8 to start.
    /// </summary>
    [KSPField (isPersistant = true, guiActive = true, guiName = "Current Growth Rate")]
    public double percentCurrentCivilianRate = 0d;

    [KSPField (isPersistant = true, guiActive = false)]
    public double populationGrowthModifier;

    /// <summary>
    /// The time until taxes; once each day
    /// </summary>
    [KSPField (isPersistant = true, guiActive = true, guiName = "Time until Rent payment")]
    public double TimeUntilTaxes = 21600.0;

    /// <summary>
    /// The last time since calculateRateOverTime() was run.  Used to calculate time steps (?)
    /// </summary>
    [KSPField (isPersistant = true, guiActive = false)]
    public float lastTime;
    //only one part with this can be the master on any vessel.
    //this prevents duplicating the population calculation
    public bool master = false;
    public bool slave = false;

    /// <summary>
    /// Gets the first part within the vessel implementing Civilian Population and assigns it as the master.  Also
    /// sets all other parts implementing Civilian Population as slaves.
    /// </summary>
    /// <returns>The master part.</returns>
    public growthRate getMaster<growthRate> (List<growthRate> partsWithCivies) where growthRate: CivilianPopulationRegulator
    {
      growthRate foundMaster = null;
      foreach (growthRate p in partsWithCivies) {
        if (p.master) {               //initially only executes if master is set in OnStart()
          if (foundMaster != null) {  //if this is NOT the first time executing; seems to never execute
            p.slave = true;
            p.master = false;
            Debug.Log (debuggingClass.modName + "Master part found; set to slave");
          } else {
            foundMaster = p;
            Debug.Log (debuggingClass.modName + "Master part set");
          }
        }
      }
      return foundMaster;//first part containing Civilian Population resource
    }

    /// <summary>
    /// Checks status of on scene, vessel, and pre-initiliazation of craft.
    /// </summary>
    /// <returns><c>true </c>, if active flight and no master/slave detected in part, <c>false</c> otherwise.</returns>
    public bool getCheckForUpdate ()
    {
      if (!HighLogic.LoadedSceneIsFlight) {//only care about running on flights because master/slaves are being set once
        return false;
      }
      if (this.vessel == null) {          //Make sure vessel is not empty (likely will cause error)
        return false;
      }
      if (master || slave) {              //If (for whatever reason) master/slaves already assigned (such as previous flight)
        return false;
      }
      return true;
    }

    /// <summary>
    /// Counts Civilians within parts implementing CivilianPopulationRegulator class.  This should be limited to only
    /// Civilian Population Parts.  It also only counts Kerbals with Civilian Population trait.  Iterates first over each
    /// part implementing CivilianPopulationRegulator, and then iterates over each crew member within that part.      
    /// </summary>
    /// <returns>The number of Civilians on the ship</returns>
    /// <param name="listOfMembers">List of members.</param>
    public int countCiviliansOnShip<growthRate> (List<growthRate> listOfMembers) where growthRate: CivilianPopulationRegulator
    //to get current ship, use this.vessel.protoVessel
    {
      int numberCivilians = 0;
      foreach (growthRate myRegulator in listOfMembers) {//check for each part implementing CivilianPopulationRegulator
        if (myRegulator.part.protoModuleCrew.Count > 0) {
          foreach (ProtoCrewMember kerbalCrewMember in myRegulator.part.protoModuleCrew) {//check for each crew member within each part above
            if (kerbalCrewMember.trait == debuggingClass.civilianTrait) {
              numberCivilians++;
            }//end if civilian
          }//end foreach kerbalCrewMember
        }//end if crew capacity
      }//end foreach part implementing class
      return numberCivilians;//number of Kerbals with trait: debuggingClass.civilianTrait -> Civilian
    }

    public int countNonCiviliansOnShip<growthRate> (List<growthRate> listOfMembers) where growthRate: CivilianPopulationRegulator
    {
      int numberNonCivilians = 0;
      foreach (growthRate myRegulator in listOfMembers) {//check for each part implementing CivilianPopulationRegulator
        if (myRegulator.part.protoModuleCrew.Count > 0) {
          foreach (ProtoCrewMember kerbalCrewMember in myRegulator.part.protoModuleCrew) {//check for each crew member within each part above
            if (kerbalCrewMember.trait != debuggingClass.civilianTrait) {
              numberNonCivilians++;
            }//end if nonCivilian
          }//end foreach kerbalCrewMember
        }//end if crew capacity
      }//end foreach part implementing class
      return numberNonCivilians;//number of Kerbals without trait: debuggingClass.civilianTrait -> Civilian
    }

    /// <summary>
    /// Counts the civilian seats on ship.
    /// </summary>
    /// <returns>The civilian seats on ship.</returns>
    /// <param name="listOfMembers">List of members.</param>
    public int countCivilianSeatsOnShip<growthRate> (List<growthRate> listOfMembers) where growthRate: CivilianPopulationRegulator
    {
      int numberPossibleSeats = 0;
      foreach (growthRate myRegulator in listOfMembers) {
        numberPossibleSeats += myRegulator.part.CrewCapacity;
      }
      return numberPossibleSeats;
    }

    /// <summary>
    /// Calculates the rent based on the number of Kerbals within a ship.
    /// TODO:  Implement changing values after mod successfully altered
    /// </summary>
    /// <returns>The total rent.</returns>
    /// <param name="numberOfCivilians">Number of civilians.</param>
    int calculateRent (int numberOfCivilians)
    {
      int rentRate = 200;
      int totalRent = 0;
      totalRent = numberOfCivilians * rentRate;//Use fixed value for testing

      return totalRent;
    }

    /// <summary>
    /// Gets the highest module growth rate of all modules on the craft.  Growth rates come from the part's .cfg files.
    /// </summary>
    /// <returns>The highest module growth rate.</returns>
    /// <param name="listOfMembers">List of members.</param>
    public double getHighestModuleGrowthRate<growthRate> (List<growthRate> listOfMembers) where growthRate: CivilianPopulationRegulator
    {
      double exponentialRate = 0d;//the malthusian parameter used to calculate the population growth.  Taken as largest value on vessel. 
      foreach (CivilianPopulationRegulator myRegulator in listOfMembers) {
        if (myRegulator.populationGrowthModifier > exponentialRate) {
          exponentialRate = myRegulator.populationGrowthModifier;
        }
      }
      return exponentialRate;//returns the largest rate among parts using CivilianPopulationRegulator class.
    }

    /// <summary>
    /// This method will place a new civilian in a part containing CivlianPopulationRegulator.  It should only
    /// be called when there are seat positions open in onesuch part.  Perhaps in the future, there will be a specific
    /// part that generates Civilians.
    /// </summary>
    /// <param name="listOfMembers">List of members.</param>
    public void placeNewCivilian<growthRate> (List<growthRate> listOfMembers) where growthRate : CivilianPopulationRegulator
    {
      ProtoCrewMember newCivilian = createNewCrewMember (debuggingClass.civilianTrait);
      bool civPlaced = false;
      foreach (growthRate currentPart in listOfMembers) {
        if (currentPart.part.CrewCapacity > currentPart.part.protoModuleCrew.Count () && !civPlaced) {
          if (currentPart.part.AddCrewmember (newCivilian)) {
            Debug.Log (debuggingClass.modName + newCivilian.name + " has been placed successfully by placeNewCivilian");
            civPlaced = true;
          }
        }
      }
      if (civPlaced == false)
        Debug.Log (debuggingClass.modName + "ERROR:  " + newCivilian.name + " could not be placed in method placeNewCivilian");
    }

    /// <summary>
    /// Creates the new crew member of trait kerbalTraitName.  It must be of type Crew because they seem to be the only
    /// type of Kerbal that can keep a trait.
    /// </summary>
    /// <returns>The new crew member.</returns>
    /// <param name="kerbalTraitName">Kerbal trait name.</param>
    ProtoCrewMember createNewCrewMember (string kerbalTraitName)
    {
      KerbalRoster roster = HighLogic.CurrentGame.CrewRoster;
      ProtoCrewMember newKerbal = roster.GetNewKerbal (ProtoCrewMember.KerbalType.Crew);
      KerbalRoster.SetExperienceTrait (newKerbal, kerbalTraitName);//Set the Kerbal as the specified role (kerbalTraitName)
      Debug.Log (debuggingClass.modName + "Created " + newKerbal.name + ", a " + newKerbal.trait);
      return newKerbal;//returns newly-generated Kerbal
    }

    /// <summary>
    /// Gets the delta time of the physics (?) update.  First it confirms the game is in a valid state.  Then it calculats
    /// the time between physics update by comparing with Planetarium.GetUniversalTime() and GetMaxDeltaTime().
    /// </summary>
    /// <returns>The delta time.</returns>
    protected double GetDeltaTimex ()
    {
      if (Time.timeSinceLevelLoad < 1.0f || !FlightGlobals.ready) {
        //Error:  Not sure what this error is for...maybe not enough time since load?
        Debug.Log(debuggingClass.modName + "ERROR:  check timeSinceLevelLoad/FlightGlobals");
        Debug.Log(debuggingClass.modName + "timeSinceLevelLoad = " + Time.timeSinceLevelLoad);
        Debug.Log(debuggingClass.modName + "FlightGlobals.ready = " + !FlightGlobals.ready);
        return -1;
      }

      if (Math.Abs (lastUpdateTime) < float.Epsilon) {
        //Error:  Just started running
        Debug.Log(debuggingClass.modName + "ERROR:  check lastUpdateTime");
        Debug.Log(debuggingClass.modName + "lastUpdateTime = " + lastUpdateTime);
        lastUpdateTime = Planetarium.GetUniversalTime();
        return -1;
      }

      var deltaTime = Math.Min (Planetarium.GetUniversalTime () - lastUpdateTime, ResourceUtilities.GetMaxDeltaTime ());
      return deltaTime;

      //why is deltaTime == 0?
      //return deltaTime;
    }

    public void getTaxes (int numCivilians, double reduceTime)
    {
      int rentAcquired = 0;
      TimeUntilTaxes -= reduceTime;
      if (TimeUntilTaxes <= 0) {
        rentAcquired = calculateRent (numCivilians);
        Funding.Instance.AddFunds (rentAcquired, TransactionReasons.Vessels);
        TimeUntilTaxes = 21600;
      }
    }

    /// <summary>
    /// Looks over vessel to find amount of a given resource matching name.  In this project's scope, it is used
    /// in order to determine how far along the civilian growth counter is towards creating a new Kerbal.
    /// </summary>
    /// <returns>The amount of resource matching name.</returns>
    /// <param name="name">Name.</param>
    public double getResourceBudget (string name)
    {
      if (this.vessel != null) {
        var resources = vessel.GetActiveResources ();
        for (int i = 0; i < resources.Count; i++) {
          if (resources [i].info.name == name) {
            return (double)resources [i].amount;
          }
        }
      }
      return 0;
    }

    //Anything below this, I don't know what it does but it is essential to keep from seeing
    //"No Resource definition found for RESOURCE" error message in OnFixedUpdate.

    [KSPField]
    public string RecipeInputs = "";

    [KSPField]
    public string RecipeOutputs = "";

    [KSPField]
    public string RequiredResources = "";


    public ConversionRecipe Recipe {
      get { return _recipe ?? (_recipe = LoadRecipe ()); }
    }

    private ConversionRecipe _recipe;

    protected override ConversionRecipe PrepareRecipe (double deltatime)
    {

      if (_recipe == null)
        _recipe = LoadRecipe ();
      UpdateConverterStatus ();
      if (!IsActivated)
        return null;
      return _recipe;
    }

    private ConversionRecipe LoadRecipe ()
    {
      var r = new ConversionRecipe ();
      try {

        if (!String.IsNullOrEmpty (RecipeInputs)) {
          var inputs = RecipeInputs.Split (',');
          for (int ip = 0; ip < inputs.Count (); ip += 2) {
            print (String.Format ("[REGOLITH] - INPUT {0} {1}", inputs [ip], inputs [ip + 1]));
            r.Inputs.Add (new ResourceRatio {
              ResourceName = inputs [ip].Trim (),
              Ratio = Convert.ToDouble (inputs [ip + 1].Trim ())
            });
          }
        }

        if (!String.IsNullOrEmpty (RecipeOutputs)) {
          var outputs = RecipeOutputs.Split (',');
          for (int op = 0; op < outputs.Count (); op += 3) {
            print (String.Format ("[REGOLITH] - OUTPUTS {0} {1} {2}", outputs [op], outputs [op + 1],
              outputs [op + 2]));
            r.Outputs.Add (new ResourceRatio {
              ResourceName = outputs [op].Trim (),
              Ratio = Convert.ToDouble (outputs [op + 1].Trim ()),
              DumpExcess = Convert.ToBoolean (outputs [op + 2].Trim ())
            });
          }
        }

        if (!String.IsNullOrEmpty (RequiredResources)) {
          var requirements = RequiredResources.Split (',');
          for (int rr = 0; rr < requirements.Count (); rr += 2) {
            print (String.Format ("[REGOLITH] - REQUIREMENTS {0} {1}", requirements [rr], requirements [rr + 1]));
            r.Requirements.Add (new ResourceRatio {
              ResourceName = requirements [rr].Trim (),
              Ratio = Convert.ToDouble (requirements [rr + 1].Trim ()),
            });
          }
        }
      } catch (Exception) {
        print (String.Format ("[REGOLITH] Error performing conversion for '{0}' - '{1}' - '{2}'", RecipeInputs, RecipeOutputs, RequiredResources));
      }
      return r;
    }
  }
}