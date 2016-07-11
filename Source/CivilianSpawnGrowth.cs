using System;
using System.Collections.Generic;
using UnityEngine;
using KSP;

namespace CivilianPopulationRevamp
{
  public class CivilianSpawnGrowth : CivilianPopulationRegulator
  {
    public override void OnStart (StartState state)
    {
      bool shouldCheckForUpdate = getCheckForUpdate ();
      if (shouldCheckForUpdate) {
        Debug.Log (debuggingClass.modName + this.name + " is Running OnStart()!");
        List<CivilianSpawnGrowth> partsWithCivies = vessel.FindPartModulesImplementing<CivilianSpawnGrowth> ();
        foreach (CivilianSpawnGrowth part in partsWithCivies) {
          part.master = false;
          part.slave = true;
        }
        //assign this part as master
        master = true;
        slave = false;
      } else {
        Debug.Log (debuggingClass.modName + "WARNING: " + this.name + " is skipping OnStart!");
      }
    }

    public override void OnUpdate ()
    {
      //if (HighLogic.LoadedSceneIsFlight) {
        int civilianPopulation = 0;
        int nonCivilianPopulation = 0;
        int civilianPopulationSeats = 0;
        double malthusianGrowthParameter = 0d;
        double percentCurrentCivilian = 0d;

        List<CivilianSpawnGrowth> listOfCivilianParts = vessel.FindPartModulesImplementing<CivilianSpawnGrowth> ();
      if (master == true) {                //slave is set during OnStart() for all but the master part.
          double dt = GetDeltaTimex ();

          //Section to calculate growth variables
          civilianPopulation = countCiviliansOnShip (listOfCivilianParts);//number of seats taken by civilians in parts using class
          nonCivilianPopulation = countNonCiviliansOnShip (listOfCivilianParts);//number of 
          civilianPopulationSeats = countCivilianSeatsOnShip (listOfCivilianParts);//total seats implementing class
          malthusianGrowthParameter = getHighestModuleGrowthRate (listOfCivilianParts);
          percentCurrentCivilian = getResourceBudget (debuggingClass.civilianResource);//get current value of Civilian Counter (0.0-1.0)
          percentCurrentCivilianRate = calculateGrowthRate (civilianPopulationSeats, malthusianGrowthParameter, civilianPopulation);
          //how much civilianCounter will change on iteration

          if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
            getTaxes (civilianPopulation, dt);

          //Section for creating Civilians
          part.RequestResource (debuggingClass.civilianResource, -percentCurrentCivilianRate * dt);//increments counter bar
          if ((percentCurrentCivilian > 1) && (civilianPopulationSeats > civilianPopulation + nonCivilianPopulation)) {
            //Debug.Log (debuggingClass.modName + "Can create Civilian:  " + percentCurrentCivilian + ", "
            //+ (civilianPopulationSeats - (civilianPopulation + nonCivilianPopulation)) + " seats currently open.");
            placeNewCivilian (listOfCivilianParts);
            part.RequestResource (debuggingClass.civilianResource, 1.0);
          //}
        }//end if condition to create Civilians
      }else {                   //executes only for master part (aka once per update)
        //Debug.Log(debuggingClass.modName + "Slave updated!");
        return;//end if...else master
    }// end OnFixedUpdate
    }

    /// <summary>
    /// Calculates the growth rate using a logistic function with parameters for:
    /// carrying capacity (maximumSeats)
    /// exponential rate (maximumSteepness)
    /// current value (currentPopulation)
    /// This differential equation is of the form dN/dt = (rN)(1-N/K).
    /// When solved, the population as a function of time is N = K/(1+e^(rt))
    /// </summary>
    /// <returns>The growth rate for N Kerbals.</returns>
    /// <param name="maximumSeats">Maximum seats.</param>
    /// <param name="maximumSteepness">Maximum steepness.</param>
    /// <param name="currentPopulation">Current population.</param>
    public double calculateGrowthRate (double maximumSeats, double steepness, double currentPopulation)
    {
      double populationGrowthRate = 0d;
      populationGrowthRate = (double)steepness * (double)currentPopulation * (1 - ((double)currentPopulation / (double)maximumSeats));
      return populationGrowthRate;
      //}
    }
  }
}

