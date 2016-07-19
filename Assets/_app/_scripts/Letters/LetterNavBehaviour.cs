﻿using UnityEngine;
using System.Collections;
using DG.Tweening;
using Panda;

namespace EA4S {
    /// <summary>
    /// Add AI Nav logic to letter puppet object.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class LetterNavBehaviour : MonoBehaviour {
        public GameObject WayPointPrefab;
        NavMeshAgent agent;
        Transform wayPoint;
        Vector3 LookAtCameraPosition = new Vector3(0, 0.2f, -19);
        bool lookAtCamera = false;

        void Start() {
            agent = GetComponent<NavMeshAgent>();
            wayPoint = Instantiate<Transform>(WayPointPrefab.transform);

        }

        #region Tasks
        [Task]
        public void SetNavigation(string _stateName) {
            switch (_stateName) {
                case "Stop":
                    agent.Stop();
                    if(lookAtCamera)
                        transform.DOLookAt(LookAtCameraPosition, 0.2f);
                    lookAtCamera = !lookAtCamera;
                    break;
                case "Walk":
                    if (lookAtCamera)
                        setLookAtCamera();
                    else
                        RepositioningWaypoint();
                    lookAtCamera = !lookAtCamera;
                    agent.speed = 3.5f;
                    agent.Resume();
                    break;
                case "Hold":
                    setLookAtCamera();
                    agent.speed = 3.5f;
                    agent.Resume();
                    break;
                case "Run":
                    RepositioningWaypoint();
                    agent.speed = 10f;
                    agent.Resume();
                    break;
                case "Ninja":
                    RepositioningWaypoint();
                    agent.speed = 3f;
                    agent.Resume();
                    break;
                case "GoOut":
                    agent.Stop();
                    transform.position = new Vector3(18, 0, 35);
                    agent.Resume();
                    agent.speed = 10f;
                    agent.Resume();
                    break;
                default:
                    break;
            }
            Task.current.Succeed();
        }
        #endregion

        #region Navigation
        /// <summary>
        /// Debug waypoint.
        /// </summary>
        void OnDrawGizmos() {
            if (wayPoint != null) {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, wayPoint.position);
            }
        }

        /// <summary>
        /// Repositioning waypoint.
        /// </summary>
        void RepositioningWaypoint(int _areaMask = 1) {
            if (!wayPoint)
                return;
            Vector3 randomValidPosition;
            //RandomPoint(Target.position, 10f, out randomValidPosition);
            GameplayHelper.RandomPointInWalkableArea(wayPoint.position, 15f, out randomValidPosition, _areaMask);
            wayPoint.position = randomValidPosition;
            agent.SetDestination(wayPoint.position);
        }

        /// <summary>
        /// Set waypoint to look in camera direction.
        /// </summary>
        void setLookAtCamera() {
            agent.Stop();
            if (!wayPoint)
                return;
            wayPoint.position = LookAtCameraPosition;
            agent.SetDestination(LookAtCameraPosition);
            agent.Resume();
        }

        void OnDestroy() {
            if (!wayPoint)
                return;
            GameObject.Destroy(wayPoint.gameObject);
        }

        #endregion

        #region Collisions
        void OnTriggerEnter(Collider other) {
            //void OnTriggerEnter(Collider other) {
            if (agent && wayPoint && other != wayPoint.GetComponent<Collider>()) {

            } else {
                RepositioningWaypoint();
            }
        }
        #endregion
    }
}
