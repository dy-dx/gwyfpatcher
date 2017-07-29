// var UnityEngine = UnityEngine || importNamespace('UnityEngine');
var gwyfhelper = gwyfhelper || importNamespace('gwyfhelper');
var GoFast = gwyfhelper.GoFast;
var Util = gwyfhelper.Util;

var playTas = false;

var strats = [
  null,
  // [[10500, 100.2]],
  [[7800, 104.3]],
  [[6300, 90]],
  [[5650, 38.2]],
  // [[10500, 270, 42]],
  [[10500, 270, 43]],
  [[7700, 71]], // 5
  [[8450, 180]],
  [[5600, 180.03]],
  [[10500, 175.2, 95]],
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

function OnScriptReload () {
  if (GoFast.ballMovement != null) {
    GoFast.ResetToSpawn();
  }
}

function currentShot () {
  var ballMovement = GoFast.ballMovement;
  var holeNum = GoFast.GetHole();
  // could possibly be problematic
  var strokeNum = ballMovement.hitCounter;
  var strat = strats[holeNum];
  if (!strat) { return undefined; }
  var shot = strat[strokeNum];
  if (!shot) { return undefined; }
  return {
    hitForce: shot[0],
    degrees: shot[1],
    obstacleDegreesDesired: shot[2],
  };
}

function index () {
  if (!playTas) { return; }
  if (!GoFast.CanShoot()) { return; }
  if (GoFast.GetActiveSceneName() != "ForestLevel") { return; }
  if (GoFast.OnNewHole()) { return; }
  // if (GoFast.ballMovement.currentVelocity > 0.01) { return; }

  var shot = currentShot();
  if (!shot) { return; }

  GoFast.SetHitForce(shot.hitForce);
  GoFast.SetDegrees(shot.degrees);

  if (shot.obstacleDegreesDesired != null) {
    if (
      GoFast.obstacleTransform != null &&
      (Math.abs(GoFast.obstacleRotationDegrees - shot.obstacleDegreesDesired) < 1)
    ) {
      // just assume we don't need the delay here
      GoFast.Shoot();
    }
  } else {
    GoFast.ShootOnNextFrame();
  }
}

