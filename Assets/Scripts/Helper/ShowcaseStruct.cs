using System;
using Niantic.Lightship.AR.LocationAR;
using UnityEngine;
using UnityEngine.Serialization;

namespace Helper
{
    [Serializable]
    public struct Showcase
    {
        public ARLocation arLocation;
        public GameObject[] variants;
    }
}
