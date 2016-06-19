using System;
using UnityEngine;
using KSP;
using Experience;

namespace CivilianPopulationRevamp
{
  /// <summary>
  /// My custom trait to test feasibility of creating new traits.  Right now, it looks like it spawns ~10% of the time in game. 
  /// </summary>
  public class SpecialistClass : Experience.ExperienceEffect
  {
    public SpecialistClass (Experience.ExperienceTrait parent):base(parent)
    //Constructor for CustomClass
    {
      //Debug.Log (debuggingClass.modName + "I'm calling a customClass for");//When the class is created, it should spam the log...right?
    }
  }
}