using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementWaypointsHandler : MonoBehaviour {

    private const float MOVE_SPEED = 5f;

    private Character_Base characterBase;
    private List<Vector3> waypointList;
    private int waypointIndex;

    private void Awake() {
        characterBase = GetComponent<Character_Base>();
    }

    private void Update() {
        Vector3 moveDir = Vector3.zero;

        if (waypointList != null && waypointList.Count > 0 && waypointIndex < waypointList.Count) {
            moveDir = (waypointList[waypointIndex] - transform.position).normalized;
            transform.position += moveDir * MOVE_SPEED * Time.deltaTime;

            float reachedWaypointDistance = .1f;
            if (Vector3.Distance(transform.position, waypointList[waypointIndex]) < reachedWaypointDistance) {
                waypointIndex++;
            }
        }

        characterBase.PlayMoveAnim(moveDir);
    }

    public void SetWaypointList(List<Vector3> waypointList) {
        this.waypointList = waypointList;
        waypointIndex = 0;
    }

}
