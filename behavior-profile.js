window.PARK_BEHAVIOR = {
  "version": 1,
  "durations": {"sleep": [60,180], "sit": [20,60], "groom": [15,30], "wander": [5,15], "knead": [8,14], "settle": [3,5], "rub": [5,8], "cuddle": [60,120], "spar": [5,10]},
  "playCooldown": 30,
  "socialCooldown": 45,
  "groupBalance": {"quietThreshold":0.5,"minimumEnergyInfluence":0.35,"multipliers":{"sleep":0.12,"sit":0.35,"wander":4,"groom":1.4,"knead":1.3,"social":1.6,"environment":1.8}},
  "clearanceX": 26,
  "clearanceY": 14,
  "restAvoid": [{"x":237,"y":118,"rx":20,"ry":13},{"x":219,"y":108,"rx":28,"ry":17},{"x":134,"y":118,"rx":14,"ry":9},{"x":182,"y":118,"rx":14,"ry":9},{"x":58,"y":133,"rx":17,"ry":10}],
  "pairSeparation": {"rub": 18.75, "cuddle": 16.25, "spar": 18.75},
  "pairLimitCompanion": 1,
  "pairLimitParkFour": 1,
  "pairLimitParkEight": 2,
  "weightsCompanion": {"sleep": 5, "sit": 5, "groom": 2, "wander": 2, "knead": 1, "social": 1},
  "weightsPark": {"sleep": 3, "sit": 3, "groom": 2, "wander": 3, "knead": 1, "social": 2, "environment": 2}
};
