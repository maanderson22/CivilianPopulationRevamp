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
      if (shouldCheckForUpdate) {   //if master/slaves not set, flight status...should only be run once
        Debug.Log (debuggingClass.modName + this.name + " is running OnStart()!");
        List<CivilianDockGrowth> partsWithCivies = vessel.FindPartModulesImplementing<CivilianDockGrowth> ();
        setMasterSlaves (partsWithCivies);
      } else {                    //if master/slave set or flight status fail.  Should be run n-1 times where n = #parts
        Debug.Log (debuggingClass.modName + "WARNING: " + this.name + " is skipping OnStart!");
      }
    }

    public void FixedUpdate ()
    {
      if (!HighLogic.LoadedSceneIsFlight)
        return;

      List<CivilianDockGrowth> listOfCivilianParts = vessel.FindPartModulesImplementing<CivilianDockGrowth> ();

      if ((!master && !slave) || (master && slave)) {
        setMasterSlaves (listOfCivilianParts);
        return;
      }
      
      int civilianPopulation = 0;
      int nonCivilianPopulation = 0;
      int civilianPopulationSeats = 0;
      double percentCurrentCivilian = 0d;
      //Debug.Log (debuggingClass.modName + "Master Status:  " + master + "Slave Status:  " + slave);

      if (master == true) {                //master is set during OnStart()
        double dt = Time.deltaTime;

        //Section to calculate growth variables
        civilianPopulation = countCiviliansOnShip (listOfCivilianParts);//number of seats taken by civilians in parts using class
        nonCivilianPopulation = countNonCiviliansOnShip (listOfCivilianParts);//number of 
        civilianPopulationSeats = countCivilianSeatsOnShip (listOfCivilianParts);//total seats implementing class
        percentCurrentCivilian = getResourceBudget (debuggingClass.civilianResource);//get current value of Civilian Counter (0.0-1.0)
        percentCurrentCivilianRate = calculateLinearGrowthRate () * getRecruitmentSoIModifier ();
        //how much civilianCounter will change on iteration

        if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
          getTaxes (civilianPopulation, dt);

        //Section to create Civilians
        part.RequestResource (debuggingClass.civilianResource, -percentCurrentCivilianRate * dt * TimeWarp.CurrentRate);
        if ((percentCurrentCivilian > 1.0) && (civilianPopulationSeats > civilianPopulation + nonCivilianPopulation)) {
          placeNewCivilian (listOfCivilianParts);
          part.RequestResource (debuggingClass.civilianResource, 1.0);
        }//end if condition to create Civilians
      }
      //Debug.Log (debuggingClass.modName + "Finished FixedUpdate!");
    }
// end FixedUpdate

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
          Debug.Log (debuggingClass.modName + "Problem finding SoI body!");
          recruitmentRateModifier = 0;
          return recruitmentRateModifier;
        }
      }
      //Debug.Log (debuggingClass.modName + "I'm landed!");
      return 0;//else case
    }
  }
}