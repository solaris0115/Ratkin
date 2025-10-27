using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace NewRatkin
{
    [StaticConstructorOnStartup]
    public static class ColorPatch
    {
        static ColorPatch()
        {
            Harmony harmonyInstance = new Harmony("com.NewRatkin.rimworld.mod");
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

}