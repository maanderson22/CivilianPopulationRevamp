using System;
using System.Collections.Generic;
using UnityEngine;
using KSP;

namespace CivilianPopulationRevamp
{
  public class CivilianDockGrowth : CivilianPopulationRegulator
  {
    public override void OnStart (StartState state)
    {
      bool shouldCheckForUpdate = getCheckForUpdate ();
      if (shouldCheckForUpdate) {
        Debug.Log (debuggingClass.modName + this.name + " is Running OnStart()!");
        List<CivilianDockGrowth> partsWithCivies = vessel.FindPartModulesImplementing<CivilianDockGrowth> ();
        foreach (CivilianDockGrowth p in partsWithCivies) {
          if (p.master)
            continue;     //I believe this skips to the next part in the foreach loop, bypassing below.
          p.slave = true; //otherwise, set it as a slave
        }
      } else {
        Debug.Log (debuggingClass.modName + "WARNING: " + this.name + " is skipping OnStart!");
      }
    }

    public override void OnFixedUpdate ()
    {
      int civilianPopulation = 0;
      int nonCivilianPopulation = 0;
      int civilianPopulationSeats = 0;
      double percentCurrentCivilian = 0d;

      List<CivilianDockGrowth> listOfCivilianParts = vessel.FindPartModulesImplementing<CivilianDockGrowth> ();
      if (slave) {                //slave is set during OnStart() for all but the master part.
        CivilianDockGrowth xmaster = getMaster (listOfCivilianParts);//gets master part
        if (xmaster == null) {    //if master part is NOT set (which should not be possible)
          master = true;
          slave = false;
          StartResourceConverter ();
        }
      } else {                   //executes only for master part (aka once per update)
        double dt = GetDeltaTimex ();

        //Section to calculate growth variables
        civilianPopulation = countCiviliansOnShip (listOfCivilianParts);//number of seats taken by civilians in parts using class
        nonCivilianPopulation = countNonCiviliansOnShip (listOfCivilianParts);//number of 
        civilianPopulationSeats = countCivilianSeatsOnShip (listOfCivilianParts);//total seats implementing class
        percentCurrentCivilian = getResourceBudget (debuggingClass.civilianResource);//get current value of Civilian Counter (0.0-1.0)
        percentCurrentCivilianRate = calculateLinearGrowthRate () * getRecruitmentSoIModifier();
        //how much civilianCounter will change on iteration

        if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
          getTaxes (civilianPopulation, dt);

        //Section for creating Civilians
        Debug.Log (debuggingClass.modName + "is adding " + dt * percentCurrentCivilianRate + " to bar!");
        part.RequestResource (debuggingClass.civilianResource, -percentCurrentCivilianRate * dt);
        if ((percentCurrentCivilian > 1) && (civilianPopulationSeats > civilianPopulation + nonCivilianPopulation)) {
          //Debug.Log (debuggingClass.modName + "Can create Civilian:  " + percentCurrentCivilian + ", "
          //+ (civilianPopulationSeats - (civilianPopulation + nonCivilianPopulation)) + " seats currently open.");
          placeNewCivilian (listOfCivilianParts);
          part.RequestResource (debuggingClass.civilianResource, 1.0);
        }//end if condition to create Civilians
      }//end if...else master
    }
// end OnFixedUpdate

    /// <summary>
    /// Calculates the growth rate for civilians taking rides up to the station.
    /// TODO:  
    /// </summary>
    /// <returns>The linear growth rate.</returns>
    public double calculateLinearGrowthRate ()
    {
      double myRate = 0d;//seems to be essential to create a middle variable, else rate does not update (returns to 0)
      myRate = populationGrowthModifier;
      return myRate;
    }

    /// <summary>
    /// Gets the recruitment modifier from being with Kerbin's, Mun's. or Minmus' SoI.  It is easier for competing
    /// programs to get astronauts into Kerbin orbit than it is to Mun/Minus.  None of them are as good as you are.
    /// </summary>
    /// <returns>The recruitment modifier due to.</returns>
    double getRecruitmentSoIModifier ()
    {
      if (!vessel.LandedOrSplashed) {
        ////print(FlightGlobals.currentMainBody.name);
        double recruitmentRateModifier = 0d;
        //if(vessel.situation.ToString == "Orbit")
        switch (FlightGlobals.currentMainBody.name) {
        case "Kerbin":
          //Debug.Log (debuggingClass.modName + "Currently near Kerbin!");
          recruitmentRateModifier = 1.0;
          return recruitmentRateModifier;
        case "Mun":
          //Debug.Log (debuggingClass.modName + "Currently near Mun!");
          recruitmentRateModifier = 0.5;
          return recruitmentRateModifier;
        case "Minmus":
          //Debug.Log (debuggingClass.modName + "Currently near Minmus!");
          recruitmentRateModifier = 0.25;
          return recruitmentRateModifier;
        default:
          //Debug.Log (debuggingClass.modName + "I don't care where I am!");
          recruitmentRateModifier = 0;
          return recruitmentRateModifier;
        }
      }
      //Debug.Log (debuggingClass.modName + "I'm landed!");
      return 0;//else case
    }
  }
}