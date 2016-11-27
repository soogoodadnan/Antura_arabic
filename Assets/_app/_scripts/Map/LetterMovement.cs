﻿using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using EA4S;
using EA4S.TestE;
using System.Collections.Generic;
using DG.Tweening;

namespace EA4S
{
    public class LetterMovement : MonoBehaviour
    {

        public MiniMap miniMapScript;

        [Header("Materials")]
        public Material black;
        public Material red;

        public int pos;

        float distanceNextDotToHitPoint;
        float distanceBeforelDotToHitPoint;
        float distanceActualDotToHitPoint;
        int numDots;
        int posDotMiniMapScript, dotCloser;
        Rope ropeSelected;
        Collider colliderRaycast;

        int learningblock;
        int playSession;
        int stage;

        void Start()
        {
            Floating();
            learningblock = AppManager.I.Player.CurrentJourneyPosition.LearningBlock;
            playSession = AppManager.I.Player.CurrentJourneyPosition.PlaySession;
            stage = AppManager.I.Player.CurrentJourneyPosition.Stage;
        }
        void Floating()
        {
            transform.DOBlendableMoveBy(new Vector3(0, 1, 0), 1).SetLoops(-1, LoopType.Yoyo);
        }

        void Update()
        {
            /* Debug.Log(AppManager.Instance.Player.CurrentJourneyPosition.Stage);
             Debug.Log(AppManager.Instance.Player.CurrentJourneyPosition.LearningBlock);
             Debug.Log(AppManager.Instance.Player.CurrentJourneyPosition.PlaySession);*/

            // transform.position = Vector3.MoveTowards(transform.position, new Vector3(posDot.x, transform.position.y, posDot.z), speed * Time.deltaTime);
            if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.tag == "Rope")
                    {
                        if (hit.transform.parent.gameObject.GetComponent<Rope>().dots[1].activeInHierarchy)//All dots available of all ropes
                        {
                            int numDotsRope = hit.transform.parent.transform.gameObject.GetComponent<Rope>().dots.Count;
                            float distaceHitToDot = 1000;
                            float distanceHitBefore = 0;
                            dotCloser = 0;

                            for (int i = 0; i < numDotsRope; i++)
                            {
                                distanceHitBefore = Vector3.Distance(hit.point,
                                    hit.transform.parent.transform.gameObject.GetComponent<Rope>().dots[i].transform.position);
                                if (distanceHitBefore < distaceHitToDot)
                                {
                                    distaceHitToDot = distanceHitBefore;
                                    dotCloser = i;
                                }
                            }
                        }
                        else
                        {
                            dotCloser = 0;
                        }


                        posDotMiniMapScript = hit.transform.parent.transform.gameObject.GetComponent<Rope>().dots[dotCloser].GetComponent<Dot>().pos;

                        ChangeMaterialDotToBlack(miniMapScript.posDots[pos]);
                        pos = posDotMiniMapScript;
                        ChangeMaterialDotToRed(miniMapScript.posDots[pos]);

                        ropeSelected = hit.transform.parent.transform.gameObject.GetComponent<Rope>();

                        transform.position = miniMapScript.posDots[posDotMiniMapScript].transform.position;
                        transform.LookAt(miniMapScript.posPines[hit.transform.parent.transform.gameObject.GetComponent<Rope>().learningBlockRope]);

                        colliderRaycast = hit.collider;
                    }
                    if (hit.collider.tag == "Pin")
                    {
                        ChangeMaterialDotToBlack(miniMapScript.posDots[pos]);
                        colliderRaycast = hit.collider;
                        if (hit.transform.gameObject.GetComponent<MapPin>().Number < miniMapScript.posPines.Length - 1)
                            transform.LookAt(miniMapScript.posPines[hit.transform.gameObject.GetComponent<MapPin>().Number + 1]);
                        transform.position = colliderRaycast.transform.position;
                    }
                }
            }

            if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject() && (colliderRaycast != null))
            {
                if (colliderRaycast.tag == "Rope")
                {
                    transform.position = miniMapScript.posDots[posDotMiniMapScript].transform.position;

                    AppManager.I.Player.CurrentJourneyPosition.PlaySession = ropeSelected.dots[dotCloser].GetComponent<Dot>().playSessionActual;
                    AppManager.I.Player.CurrentJourneyPosition.LearningBlock = ropeSelected.dots[dotCloser].GetComponent<Dot>().learningBlockActual;
                    LookAtRightPin();
                    UpdateCurrenJourneyPosition();

                }
                if (colliderRaycast.tag == "Pin")
                {
                    transform.position = colliderRaycast.transform.position;
                    pos = colliderRaycast.transform.gameObject.GetComponent<MapPin>().posBefore;

                    AppManager.I.Player.CurrentJourneyPosition.PlaySession = 100;
                    AppManager.I.Player.CurrentJourneyPosition.LearningBlock = colliderRaycast.transform.gameObject.GetComponent<MapPin>().Number;
                    if (colliderRaycast.transform.gameObject.GetComponent<MapPin>().Number < miniMapScript.posPines.Length - 1)
                        transform.LookAt(miniMapScript.posPines[AppManager.I.Player.CurrentJourneyPosition.LearningBlock + 1]);
                    UpdateCurrenJourneyPosition();
                }

            }
        }


        public void MoveToTheRightDot()
        {
            if ((AppManager.I.Player.CurrentJourneyPosition.PlaySession == 2) && (miniMapScript.posPines[AppManager.I.Player.CurrentJourneyPosition.LearningBlock].tag == "Pin"))
            {
                ChangeMaterialDotToBlack(miniMapScript.posDots[pos]);
                transform.position = miniMapScript.posPines[AppManager.I.Player.CurrentJourneyPosition.LearningBlock].transform.position;
                AppManager.I.Player.CurrentJourneyPosition.PlaySession = 100;
                UpdateCurrenJourneyPosition();
                if (AppManager.I.Player.CurrentJourneyPosition.LearningBlock != miniMapScript.posPines.Length - 1)
                    transform.LookAt(miniMapScript.posPines[AppManager.I.Player.CurrentJourneyPosition.LearningBlock + 1]);
            }
            else if ((AppManager.I.Player.CurrentJourneyPosition.PlaySession == 100) && (pos < (miniMapScript.posMax - 1)))
            {
                if (pos % 2 != 0)
                    pos++;
                transform.position = miniMapScript.posDots[pos].transform.position;
                ChangeMaterialDotToRed(miniMapScript.posDots[pos]);
                AppManager.I.Player.CurrentJourneyPosition.PlaySession = miniMapScript.posDots[pos].GetComponent<Dot>().playSessionActual;
                AppManager.I.Player.CurrentJourneyPosition.LearningBlock++;
                UpdateCurrenJourneyPosition();
                LookAtRightPin();
            }
            else
            {
                if (pos < (miniMapScript.posMax - 1))
                {
                    ChangeMaterialDotToBlack(miniMapScript.posDots[pos]);
                    pos++;
                    ChangeMaterialDotToRed(miniMapScript.posDots[pos]);
                    transform.position = miniMapScript.posDots[pos].transform.position;
                    AppManager.I.Player.CurrentJourneyPosition.PlaySession = miniMapScript.posDots[pos].GetComponent<Dot>().playSessionActual;
                    UpdateCurrenJourneyPosition();
                    LookAtRightPin();
                }
            }
        }
        public void MoveToTheLeftDot()
        {
            if (AppManager.I.Player.CurrentJourneyPosition.PlaySession == 1)
            {
                if (pos > 0)
                {
                    ChangeMaterialDotToBlack(miniMapScript.posDots[pos]);
                    transform.position = miniMapScript.posPines[AppManager.I.Player.CurrentJourneyPosition.LearningBlock - 1].transform.position;
                    AppManager.I.Player.CurrentJourneyPosition.PlaySession = 100;
                    AppManager.I.Player.CurrentJourneyPosition.LearningBlock--;

                    UpdateCurrenJourneyPosition();
                    LookAtLeftPin();
                }
            }
            else if (AppManager.I.Player.CurrentJourneyPosition.PlaySession == 100)
            {
                if (pos % 2 == 0)
                    pos--;
                transform.position = miniMapScript.posDots[pos].transform.position;
                ChangeMaterialDotToRed(miniMapScript.posDots[pos]);
                AppManager.I.Player.CurrentJourneyPosition.PlaySession = miniMapScript.posDots[pos].GetComponent<Dot>().playSessionActual;

                UpdateCurrenJourneyPosition();
                LookAtLeftPin();
            }
            else
            {
                if (pos > 0)
                {
                    ChangeMaterialDotToBlack(miniMapScript.posDots[pos]);
                    pos--;
                    transform.position = miniMapScript.posDots[pos].transform.position;
                    ChangeMaterialDotToRed(miniMapScript.posDots[pos]);
                    LookAtLeftPin();
                }
                AppManager.I.Player.CurrentJourneyPosition.PlaySession = miniMapScript.posDots[pos].GetComponent<Dot>().playSessionActual;
                AppManager.I.Player.CurrentJourneyPosition.LearningBlock = miniMapScript.posDots[pos].GetComponent<Dot>().learningBlockActual;
                UpdateCurrenJourneyPosition();
            }
        }

        public void ResetPosLetter()
        {
            if (AppManager.I.Player.CurrentJourneyPosition.PlaySession == 100)//Letter is on a pin
            {
                transform.position = miniMapScript.posPines[AppManager.I.Player.CurrentJourneyPosition.LearningBlock].transform.position;
                pos = miniMapScript.posPines[AppManager.I.Player.CurrentJourneyPosition.LearningBlock].GetComponent<MapPin>().posBefore;
            }
            else  //Letter is on a dot
            {
                transform.position = miniMapScript.ropes[AppManager.I.Player.CurrentJourneyPosition.LearningBlock - 1].GetComponent<Rope>().dots
                    [AppManager.I.Player.CurrentJourneyPosition.PlaySession - 1].transform.position;
                pos = miniMapScript.ropes[AppManager.I.Player.CurrentJourneyPosition.LearningBlock - 1].GetComponent<Rope>().dots
                    [AppManager.I.Player.CurrentJourneyPosition.PlaySession - 1].GetComponent<Dot>().pos;
                miniMapScript.posDots[pos].GetComponent<Renderer>().material = red;
            }
            LookAtRightPin();
        }
        public void ResetPosLetterAfterChangeStage()
        {
            pos = 0;
            transform.position = miniMapScript.posDots[pos].transform.position;
            miniMapScript.posDots[pos].GetComponent<Renderer>().material = red;
            AppManager.I.Player.CurrentJourneyPosition.LearningBlock = 1;
            AppManager.I.Player.CurrentJourneyPosition.PlaySession = 1;
            LookAtRightPin();
            UpdateCurrenJourneyPosition();
        }
        void UpdateCurrenJourneyPosition()
        {
            AppManager.I.Player.SetActualJourneyPosition(new JourneyPosition(AppManager.I.Player.CurrentJourneyPosition.Stage,
             AppManager.I.Player.CurrentJourneyPosition.LearningBlock,
              AppManager.I.Player.CurrentJourneyPosition.PlaySession), true);
        }
        public void ChangeMaterialDotToBlack(GameObject dot)
        {
            dot.GetComponent<Renderer>().material = black;
        }
        void ChangeMaterialDotToRed(GameObject dot)
        {
            dot.GetComponent<Renderer>().material = red;
        }
        void LookAtRightPin()
        {
            //Debug.Log("LOOKRIGHT");
            transform.LookAt(miniMapScript.posPines[AppManager.I.Player.CurrentJourneyPosition.LearningBlock]);
        }
        void LookAtLeftPin()
        {
            transform.LookAt(miniMapScript.posPines[AppManager.I.Player.CurrentJourneyPosition.LearningBlock - 1]);
        }
    }
}

