/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using CodeMonkey;
using System;
public class TestingDOTSEvents : MonoBehaviour {

    private void Start() {
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<RacingEnemyMoveSystem_Done>().OnRacingEnemyPassed += TestingDOTSEvents_OnRacingEnemyPassed;
    }

    private void TestingDOTSEvents_OnRacingEnemyPassed(object sender, System.EventArgs e) {
        CMDebug.TextPopup("Ding!", new Vector3(.5f, .2f));
    }

}
