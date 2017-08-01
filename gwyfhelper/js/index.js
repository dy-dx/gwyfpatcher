var global = (typeof global === 'undefined' ? {} : global);
// global.UnityEngine = global.UnityEngine || importNamespace('UnityEngine');
global.gwyfhelper = global.gwyfhelper || importNamespace('gwyfhelper');
global.GoFast = global.gwyfhelper.GoFast;
global.Util = global.gwyfhelper.Util;

global.playTas = false;
// global.gwyfhelper.HelperGui.enabled = false;

global.strats = [
  null,
  [{hitForce: 7800, degrees: 104.3}],
  [{hitForce: 6300, degrees: 90}],
  [{hitForce: 5650, degrees: 38.2}],
  // hole 4 - this one never works the first time, idk why
  // [{hitForce: 10500, degrees: 270, min: 43.6, max: 44.4}],
  // alt?
  // [{hitForce: 10500, degrees: 270, min: 40.6, max: 41.4}],
  // safe strat
  [{hitForce: 6600, degrees: 270, min: 68.7, max: 99.7}],

  [{hitForce: 7800, degrees: 71}], // 5
  [{hitForce: 8450, degrees: 180}],
  [{hitForce: 5600, degrees: 180.03}],
  // hole 8
  [{hitForce: 10500, degrees: 175.2, min: 81, max: 99.6}],
  [{hitForce: 7400, degrees: 0}],
  // hole 10
  // [{hitForce: 9900, degrees: 349.2, min: 140, max: 180, min2: 0, max2: 11}],
  // [{hitForce: 10400, degrees: 349.4, min: 142, max: 180, min2: 0, max2: 11}],
  [{hitForce: 9000, degrees: 349, min: 138, max: 180, min2: 168, max2: 180}],

  // hole 11
  // [{hitForce: 10500, degrees: 141}],
  [{hitForce: 10500, degrees: 141.1}],
  // hole 12
  [
    // {hitForce: 10500, degrees: 300},
    // {hitForce: 7800, degrees: 300}
    // safe strat
    {hitForce: 5300, degrees: 30},
    {hitForce: 7600, degrees: 99.3}
  ],
  [{hitForce: 10500, degrees: 93.4}],
  [{hitForce: 10400, degrees: 75.7}],
  [{hitForce: 9720, degrees: 115}],
  // hole 16
  // can't get this one to work:
  // [{hitForce: 10500, degrees: 269.9}],
  // backup strat
  [{hitForce: 10500, degrees: 333.8}],
  [{hitForce: 10500, degrees: 296}],
  // hole 18
  [
    // {hitForce: 10500, degrees: 235.6, min: -146, max: -145.4},
    {hitForce: 10500, degrees: 235.6, min: -126, max: 0},
    // {hitForce: 10500, degrees: 266.1}
    // {hitForce: 10500, degrees: 267.2}
    {hitForce: 10500, degrees: 267.3}
  ],
];

global.OnScriptReload = function OnScriptReload() {
  var GoFast = global.GoFast;
  if (GoFast.ballMovement != null) {
    // GoFast.ResetToSpawn();
  }
}

function currentShot (holeNum) {
  // could possibly be problematic
  var strokeNum = global.GoFast.ballMovement.hitCounter;
  var strat = global.strats[holeNum];
  if (!strat) { return undefined; }
  return strat[strokeNum];
}

global.index = function index () {
  var GoFast = global.GoFast;

  if (!global.playTas) { return; }
  if (!GoFast.CanShoot()) { return; }
  if (GoFast.GetActiveSceneName() != "ForestLevel") { return; }

  var holeNum = GoFast.GetHole();
  var shot = currentShot(holeNum);
  if (!shot) { return; }

  GoFast.SetDegrees(shot.degrees);
  GoFast.SetHitForce(shot.hitForce);

  // prevents inconsistencies with shots during tas mode
  if (GoFast.hitPointDistanceMoved > 0.1) { return; }
  if (holeNum === 7 || holeNum === 13 || holeNum === 18) {
    if (GoFast.hitPointDistanceMoved > 0.001) { return; }
  }
  if (holeNum === 12 || holeNum === 14 || holeNum === 15) {
    if (GoFast.hitPointDistanceMoved > 0.0004) { return; }
  }

  if (shot.min != null && shot.max != null) {
    if (
      GoFast.obstacleTransform != null &&
      GoFast.obstacleMovementAmount >= shot.min &&
      GoFast.obstacleMovementAmount <= shot.max
    ) {
      // just assume we don't need the delay here
      if (shot.min2 == null || shot.max2 == null) {
        // global.Util.LogToScreen("Shooting at " + GoFast.obstacleMovementAmount);
        GoFast.Shoot();
      } else if (
        GoFast.secondObstacleTransform != null &&
        GoFast.secondObstacleMovementAmount >= shot.min2 &&
        GoFast.secondObstacleMovementAmount <= shot.max2
      ) {
        GoFast.Shoot();
      }
    }
  } else {
    GoFast.Shoot();
    GoFast.ShootOnNextFrame();
    // GoFast.ShootWithDelay(0.1);
  }
}
