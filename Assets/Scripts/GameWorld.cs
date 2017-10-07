using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.UI;

public class GameWorld : MonoBehaviour {

    #region notes
    /*
     - Have flashlight stutter/ stop working when an enemy is close
     - Record audio for hostiles
        - Seperate for patrolling and aggro'd
    */
    #endregion
    public static readonly int blockSize = 10;
    public static readonly Vector3 hostileOffset = new Vector3(9.11f, 3.6f, -0.73f);

    public static Dictionary<string, MapPoint> map;
    public static Dictionary<Block, Direction[]> blockFaces;

    static Dictionary<Block, float> spawnChance;

    readonly string[] blockNames = new string[] { "Corridor_Hor", "Corridor_Ver",
            "Corner_NorthLeft", "Corner_NorthRight", "Corner_SouthLeft", "Corner_SouthRight",
            "TSplit_North", "TSplit_South",
            "End_North", "End_East", "End_South", "End_West",
			"Pit_Hor", "Pit_Ver" };
    readonly Vector3 carpetOffset = new Vector3(9f, 1.02f, -1f), lanternOffset = new Vector3(9.4f, 5.7f, -0.65f);

    List<GameObject> decorations;
    List<GameObject> hostiles;
    List<Point> openPoints;
    Text positionLbl, mapSizeLbl;
    Block currentBlock;

    // Use this for initialization
    void Start () {
        map = new Dictionary<string, MapPoint>();
        decorations = new List<GameObject>();
        hostiles = new List<GameObject>();

        blockFaces = new Dictionary<Block, Direction[]>();
        InitiateBlockFaces();

        spawnChance = new Dictionary<Block, float>();
        InitiateSpawnChance();

        openPoints = new List<Point>();

        GameController.player = GameObject.Find("FPSController");
        positionLbl = GameObject.Find("PositionLabel").GetComponent<Text>();
        mapSizeLbl = GameObject.Find("MapSizeLabel").GetComponent<Text>();

        Spawn(0, 0, Block.Corridor_Ver, Direction.North);
        currentBlock = Block.Corridor_Ver;
    }

    // Update is called once per frame
    void Update() {
        if(GameController.playerEnabled) {
            Point playerPos = GetPlayerPosition();
            positionLbl.text = playerPos.ToString();
        }

        /*
        Debug.Log("Playertransform: " + player.transform.position + " Playerlocation: " + playerPos.ToString());

        if(!map.ContainsKey(playerPos.ToString())) {
            Spawn(playerPos.x, playerPos.y, Block.Corridor_Ver);
        }

        if (!map.ContainsKey(playerPos.ToString())) {
            Block[] validBlocks = GetValidBlocks(currentBlock);
            Spawn(playerPos.x, playerPos.y, validBlocks[UnityEngine.Random.Range(0, validBlocks.Length)]);
        }

        currentBlock = map[playerPos.ToString()].block;
        Debug.Log("Current Block: " + blockNames[(int)currentBlock]);
        */

        // openPoint contains position of blocks that aren't enclosed by other blocks
        if (openPoints.Count > 0) {
            Point thisPoint = openPoints.First();
            MapPoint target = map[thisPoint.ToString()];
            Direction[] targetDirections = target.GetDirections();
            foreach (Direction targetDir in targetDirections) {
                List<string> spawnedBlocks = new List<string>();
                Point newPoint = GetPointDirection(thisPoint, targetDir);
                Direction oppositeTarget = GetOppositeDirection(targetDir);
                if (!map.ContainsKey(newPoint.ToString())) {
                    Spawn(newPoint.x, newPoint.y, SelectBlock(GetValidBlocks(oppositeTarget)), oppositeTarget);
                    spawnedBlocks.Add(newPoint.ToString());

                    if(UnityEngine.Random.value < .05f && !blockNames[(int)map[newPoint.ToString()].block].Contains("Pit_")) {
                        SpawnCarpet(newPoint.x, newPoint.y);
                    }

                    if(UnityEngine.Random.value < .02f) {
                        SpawnLantern(newPoint.x, newPoint.y);
                    }
                } else {
                    MapPoint existingBlock = map[newPoint.ToString()];
                    if(!existingBlock.GetDirections().Contains(oppositeTarget)) {
                        foreach(string spawn in spawnedBlocks) {
                            MapPoint spawnedPoint = map[spawn];
                            GameObject.Destroy(spawnedPoint.obj);
                            map.Remove(spawn);
                        }

                        GameObject.Destroy(target.obj);
                        map.Remove(thisPoint.ToString());
                        Spawn(thisPoint.x, thisPoint.y, GetEndPointByDirection(GetOppositeDirection(target.spawnDirection)), oppositeTarget);
                        break;
                    }
                }
            }

            openPoints.Remove(thisPoint);

            //Debug.Log("Map size: " + map.Count + ". Open points: " + openPoints.Count);
            mapSizeLbl.text = map.Count.ToString();
            if(Input.GetKeyDown(KeyCode.T)) {
                openPoints.Clear();
            }

            if(map.Count > 150) {
                GameController.RemoveSplashScreen();
            }
        } else {
            if (Input.GetKeyDown(KeyCode.R) || map.Count < 150) {
                DestroyWorld();
                Spawn(0, 0, Block.Corridor_Ver, Direction.North);
            } else if(hostiles.Count < map.Count / 100) {
				//SpawnHostile(0, 0);

				string[] spawnPoints = map.Keys.ToArray();
				MapPoint spawnPoint = map[spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)]];

				if (!blockNames[(int)spawnPoint.block].Contains("End_")) {
					SpawnHostile(spawnPoint.point.x, spawnPoint.point.y);
				}
			}
        }
    }

    void DestroyWorld() {
        foreach (MapPoint mp in map.Values) {
            GameObject.Destroy(mp.obj);
        }

        map.Clear();

        foreach(GameObject dec in decorations) {
            GameObject.Destroy(dec);
        }

        decorations.Clear();

        foreach (GameObject hos in hostiles) {
            GameObject.Destroy(hos);
        }

        hostiles.Clear();
    }

    #region spawning
    void Spawn(int x, int y, Block block, Direction spawnDirection) {
        GameObject spawn = GameObject.Instantiate(Resources.Load("Prefabs/Blocks/" + blockNames[(int)block])) as GameObject;

        spawn.transform.position = new Vector3(x * blockSize, 0, y * blockSize);
        map.Add(x + ", " + y, new MapPoint(new Point(x, y), spawnDirection, block, spawn));

        if (!blockNames[(int)block].Contains("End_")) {
            openPoints.Add(new Point(x, y));
        }
    }

    void SpawnCarpet(int x, int y) {
        GameObject carpet = GameObject.Instantiate(Resources.Load("Prefabs/Carpet_Red")) as GameObject;

        carpet.transform.position = new Vector3(x * blockSize, 0, y * blockSize);
        carpet.transform.position += carpetOffset;

        decorations.Add(carpet);
    }

    void SpawnLantern(int x, int y) {
        GameObject lantern = GameObject.Instantiate(Resources.Load("Prefabs/Lantern")) as GameObject;

        lantern.transform.position = new Vector3(x * blockSize, 0, y * blockSize);
        lantern.transform.position += lanternOffset;

        decorations.Add(lantern);
    }

    void SpawnHostile(int x, int y) {
        GameObject hostile = GameObject.Instantiate(Resources.Load("Prefabs/Hostile")) as GameObject;

        hostile.transform.position = new Vector3(x * blockSize, 0, y * blockSize);
        hostile.transform.position += hostileOffset;

        hostiles.Add(hostile);
    }
    #endregion

    #region initialization
    void InitiateBlockFaces() {
        blockFaces.Add(Block.Corridor_Hor, new Direction[] { Direction.East, Direction.West });
        blockFaces.Add(Block.Corridor_Ver, new Direction[] { Direction.North, Direction.South });
        blockFaces.Add(Block.Corner_NorthLeft, new Direction[] { Direction.West, Direction.South });
        blockFaces.Add(Block.Corner_NorthRight, new Direction[] { Direction.East, Direction.South });
        blockFaces.Add(Block.Corner_SouthLeft, new Direction[] { Direction.North, Direction.West });
        blockFaces.Add(Block.Corner_SouthRight, new Direction[] { Direction.North, Direction.East });
        blockFaces.Add(Block.TSplit_North, new Direction[] { Direction.East, Direction.West, Direction.South });
        blockFaces.Add(Block.TSplit_South, new Direction[] { Direction.East, Direction.West, Direction.North });
        blockFaces.Add(Block.End_North, new Direction[] { Direction.South });
        blockFaces.Add(Block.End_East, new Direction[] { Direction.West });
        blockFaces.Add(Block.End_South, new Direction[] { Direction.North });
        blockFaces.Add(Block.End_West, new Direction[] { Direction.East });
		blockFaces.Add(Block.Pit_Hor, new Direction[] { Direction.East, Direction.West });
		blockFaces.Add(Block.Pit_Ver, new Direction[] { Direction.North, Direction.South });
	}

    void InitiateSpawnChance() {
        spawnChance.Add(Block.Corridor_Hor, 1f);
        spawnChance.Add(Block.Corridor_Ver, 1f);
        spawnChance.Add(Block.Corner_NorthLeft, 1f);
        spawnChance.Add(Block.Corner_NorthRight, 1f);
        spawnChance.Add(Block.Corner_SouthLeft, 1f);
        spawnChance.Add(Block.Corner_SouthRight, 1f);
        spawnChance.Add(Block.TSplit_North, 1.5f);
        spawnChance.Add(Block.TSplit_South, 1.5f);
        spawnChance.Add(Block.End_North, .005f);
        spawnChance.Add(Block.End_East, .005f);
        spawnChance.Add(Block.End_South, .005f);
        spawnChance.Add(Block.End_West, .005f);
		spawnChance.Add(Block.Pit_Hor, .4f);
		spawnChance.Add(Block.Pit_Ver, .4f);
	}
    #endregion

    #region objects
    public enum Block {
        Corridor_Hor,
        Corridor_Ver,
        Corner_NorthLeft,
        Corner_NorthRight,
        Corner_SouthLeft,
        Corner_SouthRight,
        TSplit_North,
        TSplit_South,
        End_North,
        End_East,
        End_South,
        End_West,
		Pit_Hor,
		Pit_Ver
    }

    public enum Direction {
        North,
        East,
        South,
        West
    }

    public class Point {
        public int x, y;

        public Point(int x, int y) {
            this.x = x;
            this.y = y;
        }

        public bool Equals(Point point) {
            return this.x == point.x && this.y == point.y;
        }

        public override string ToString() {
            return x + ", " + y;
        }
    }

    public class MapPoint {
        public Point point;
        public Direction spawnDirection;
        public Block block;
        public GameObject obj;

        public MapPoint(Point point, Direction spawnDirection, Block block, GameObject obj) {
            this.point = point;
            this.spawnDirection = spawnDirection;
            this.block = block;
            this.obj = obj;
        }

        public Direction[] GetDirections() {
            return blockFaces[block];
        }
    }

    Point GetPointDirection(Point input, Direction target) {
        //North = negative Z/Y
        //East = negative X

        if (target == Direction.North) {
            return new Point(input.x, input.y - 1);
        }

        if (target == Direction.East) {
            return new Point(input.x - 1, input.y);
        }

        if (target == Direction.South) {
            return new Point(input.x, input.y + 1);
        }

        if (target == Direction.West) {
            return new Point(input.x + 1, input.y);
        }

        return input;
    }

    public static Direction GetOppositeDirection(Direction input) {
        if (input == Direction.North) {
            return Direction.South;
        }

        if (input == Direction.East) {
            return Direction.West;
        }

        if (input == Direction.South) {
            return Direction.North;
        }

        if (input == Direction.West) {
            return Direction.East;
        }

        return Direction.North;
    }

    Block GetEndPointByDirection(Direction input) {
        if (input == Direction.North) {
            return Block.End_North;
        }

        if (input == Direction.East) {
            return Block.End_East;
        }

        if (input == Direction.South) {
            return Block.End_South;
        }

        return Block.End_West;
    }

    Block[] GetValidBlocks(Direction inputDirection) {
        List<Block> result = new List<Block>();

        foreach (KeyValuePair<Block, Direction[]> face in blockFaces) {
            if (face.Value.Contains(inputDirection)) {
                result.Add(face.Key);
            }
        }

        return result.ToArray();
    }

    Block SelectBlock(Block[] inputBlocks) {
        float spawnRange = 0;
        foreach (Block block in inputBlocks) {
            spawnRange += spawnChance[block];
        }

        float spawnValue = UnityEngine.Random.Range(0f, spawnRange);
        float spawnCheck = 0;
        foreach (Block block in inputBlocks) {
            spawnCheck += spawnChance[block];
            if (spawnValue < spawnCheck) {
                return block;
            }
        }

        return inputBlocks[UnityEngine.Random.Range(0, inputBlocks.Length)];
    }

    Point GetPlayerPosition() {
        return new Point(Mathf.FloorToInt((GameController.player.transform.position.x - 5) / blockSize), Mathf.FloorToInt((GameController.player.transform.position.z + 5.75f) / blockSize));
    }

    bool MatchOneDirection(Direction[] input, Direction[] target) {
        foreach (Direction inp in input) {
            foreach (Direction tar in target) {
                if (tar == inp) {
                    return true;
                }
            }
        }

        return false;
    }
    #endregion
}
