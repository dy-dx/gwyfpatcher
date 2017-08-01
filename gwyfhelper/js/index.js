(function() {

var global = (typeof global === 'undefined' ? {} : global);
// global.UnityEngine = global.UnityEngine || importNamespace('UnityEngine');
global.gwyfhelper = global.gwyfhelper || importNamespace('gwyfhelper');
global.GoFast = global.gwyfhelper.GoFast;
global.Util = global.gwyfhelper.Util;

global.playTas = false;

global.strats = [
  null,
  // [[10500, 100.2]],
  [[7800, 104.3]],
  [[6300, 90]],
  [[5650, 38.2]],
  [[10500, 270, 42, 44]],
  [[7700, 71]], // 5
  [[8450, 180]],
  [[5600, 180.03]],
  [[10500, 175.2, 94, 96]],
  [[7400, 0]],
  [[9900, 349.2]], // 10
  // [[10500, 141]],
  [[10500, 141.1]],
  [[10500, 300], [7800, 300]],
  [[10500, 93.4]],
  [[10400, 75.7]],
  [[9720, 115]], // 15
  [[10500, 269.9]],
  [[10500, 296]],
  [[10500, 235.6], [10500, 266.0]],
];

global.OnScriptReload = function OnScriptReload() {
  var GoFast = global.GoFast;
  if (GoFast.ballMovement != null) {
    GoFast.ResetToSpawn();
  }
}

function currentShot () {
  var GoFast = global.GoFast;

  var ballMovement = GoFast.ballMovement;
  var holeNum = GoFast.GetHole();
  // could possibly be problematic
  var strokeNum = ballMovement.hitCounter;
  var strat = global.strats[holeNum];
  if (!strat) { return undefined; }
  var shot = strat[strokeNum];
  if (!shot) { return undefined; }
  return {
    hitForce: shot[0],
    degrees: shot[1],
    obstacleMovementMin: shot[2],
    obstacleMovementMax: shot[3],
  };
}

global.index = function index () {
  var GoFast = global.GoFast;

  if (!global.playTas) { return; }
  if (!GoFast.CanShoot()) { return; }
  if (GoFast.GetActiveSceneName() != "ForestLevel") { return; }
  if (GoFast.OnNewHole()) { return; }
  // if (GoFast.ballMovement.currentVelocity > 0.01) { return; }

  var shot = currentShot();
  if (!shot) { return; }

  GoFast.SetHitForce(shot.hitForce);
  GoFast.SetDegrees(shot.degrees);

  if (shot.obstacleMovementMin != null && shot.obstacleMovementMax != null) {
    if (
      GoFast.obstacleTransform != null &&
      GoFast.obstacleMovementAmount >= shot.obstacleMovementMin &&
      GoFast.obstacleMovementAmount <= shot.obstacleMovementMax
    ) {
      // just assume we don't need the delay here
      GoFast.Shoot();
    }
  } else {
    GoFast.ShootOnNextFrame();
  }
}

})();
