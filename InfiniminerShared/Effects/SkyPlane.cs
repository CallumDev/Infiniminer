﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Infiniminer.Effects
{
    using System;
    
    public class SkyPlane
    {
        static Effect[] variants;
        private static bool iscompiled = false;
        public static Effect Get(ShaderFeatures features)
        {
            AllShaders.Compile();
            return variants[0];
        }
        public static Effect Get()
        {
            AllShaders.Compile();
            return variants[0];
        }
        internal static void Compile(string sourceBundle)
        {
            if (iscompiled)
            {
                return;
            }
            iscompiled = true;
            Effect.Log("Compiling SkyPlane");
            variants = new Effect[1];
            // No GL4 variants detected
            variants[0] = Effect.Compile(sourceBundle.Substring(1851, 450), sourceBundle.Substring(2301, 744));
        }
    }
}
