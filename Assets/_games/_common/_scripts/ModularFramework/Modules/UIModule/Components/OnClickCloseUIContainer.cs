﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace EA4S.Core
{

    public class OnClickCloseUIContainer : MonoBehaviour {

        void OnEnable() {
            // Remove UniRx refactoring request: any reactive interaction within this class must be called manually.
        }

        public void OnMouseDown() {
            UIContainer container = transform.GetComponentInParent<UIContainer>();
            AppManager.Instance.UIModule.HideUIContainer(container.Key);
        }
    }
}