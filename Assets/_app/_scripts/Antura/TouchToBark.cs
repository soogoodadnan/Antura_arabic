﻿using EA4S.Audio;
using UnityEngine;

namespace EA4S.Antura
{
    public class TouchToBark : MonoBehaviour
    {
        float animationTimer = 0;

        public void OnMouseDown()
        {
            if (animationTimer > 0)
                return;

            var rnd = UnityEngine.Random.value;

            if (rnd < 0.3f)
            {
                GetComponent<AnturaAnimationController>().DoSniff(null, () => { Audio.AudioManager.I.PlaySound(Sfx.DogSnorting); });
            }
            else if (rnd < 0.5f)
            {
                GetComponent<AnturaAnimationController>().State = AnturaAnimationStates.digging;
                animationTimer = 2;
            }
            else if (rnd < 0.7f)
            {
                GetComponent<AnturaAnimationController>().State = AnturaAnimationStates.sheeping;
                animationTimer = 2;
            }
            else
                GetComponent<AnturaAnimationController>().DoShout(() => { AudioManager.I.PlaySound(Sfx.DogBarking); });
        }

        public void Update()
        {
            if (animationTimer > 0)
            {
                animationTimer -= Time.deltaTime;
                if (animationTimer <= 0)
                {
                    GetComponent<AnturaAnimationController>().State = AnturaAnimationStates.sitting;
                }
            }
        }
    }
}