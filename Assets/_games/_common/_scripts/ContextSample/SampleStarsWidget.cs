﻿// Written by Davide Barbieri <davide.barbieri AT ghostshark.it>

using UnityEngine;

namespace EA4S
{
    public class SampleStarsWidget : IStarsWidget
    {
        public void Show(int stars)
        {
            // WARNING: temp hack. Star Flowers must be instanced and active to work.
            // Add Star flowers to Global UI

            StarFlowers.I.gameObject.SetActive(true);
            StarFlowers.I.Show(stars);
        }

        public void Hide()
        {
            StarFlowers.I.gameObject.SetActive(false);
        }
    }
}
