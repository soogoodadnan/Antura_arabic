﻿using UnityEngine;

namespace EA4S.Utilities {

    public class SingletonMonoBehaviour<T> : MonoBehaviour where T : Component
    {
        // Static singleton property
        public static T I { get; private set; }
        public string TypeName { get; private set; }

        protected bool IsDuplicatedInstance = false;

        protected virtual void Awake() {
            TypeName = typeof(T).FullName;

            // checks if there is already another instance of this type.
            if (I != null) {
                if (I != this) {
                    // destroys immediately to break the chain of events associated to this object.
                    //DestroyImmediate(gameObject);
                    IsDuplicatedInstance = true;
                    Destroy(gameObject);
                }
                return;             
            }

            // Here we save our singleton instance
            I = this as T;

            Initialise();
        }

        void OnDestroy() {
            if (I == this)
                Finalise();
        }

        protected virtual void Initialise() {
        }

        protected virtual void Finalise() {
        }
    }
}