using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HostileBehaviour : MonoBehaviour {

    readonly float hoverRange = .2f, hoverSpeed = .005f,
        rotateSpeed = 30f, moveSpeed = 2f;

    // Rotate around Y. 0 = west, 90 = north etc.
    GameWorld.Direction myDirection;
    float hoverHeight;
    int hoverDirection, rotateTimer;
    Vector3 targetRotation;
    bool rotating;

	// Use this for initialization
	void Start () {
        myDirection = SelectDirection(GameWorld.blockFaces[GetCurrentMapPoint().block]);
        this.transform.eulerAngles = GetEulerByDirection(myDirection);

        hoverHeight = 0;
        hoverDirection = 1;
        rotateTimer = 0;

        rotating = false;
	}
	
	// Update is called once per frame
	void Update () {
        Hover();

        if (!rotating) {
            this.transform.position += GetForwardByDirection(myDirection) * moveSpeed * Time.deltaTime;
            rotateTimer++;

            Vector3 position = this.transform.position - GameWorld.hostileOffset;
            if(rotateTimer > 60 && Mathf.Abs(position.x) % GameWorld.blockSize < .5f && Mathf.Abs(position.z) % GameWorld.blockSize < .5f) {
                Rotate();
                rotateTimer = 0;
            }
        } else {
            Rotate();
        }
    }

    void Rotate() {
        if(!rotating) {
            GameWorld.Direction newDirection = SelectDirection(GameWorld.blockFaces[GetCurrentMapPoint().block]);
            if(newDirection != myDirection) {
                targetRotation = GetEulerByDirection(newDirection);
                myDirection = newDirection;

                rotating = true;
			}
        } else if(this.transform.eulerAngles != targetRotation) {
            if(Mathf.Abs(this.transform.eulerAngles.y - targetRotation.y) < rotateSpeed * Time.deltaTime) {
                this.transform.eulerAngles = targetRotation;
                rotating = false;
            } else {
                this.transform.eulerAngles += new Vector3(0, rotateSpeed * GetRotateDirection() * Time.deltaTime, 0);
            }
        } else {
            rotating = false;
        }
    }

    void Hover () {
        float currentSpeed = hoverSpeed - (Mathf.Abs(hoverHeight) / 50f);

        if((hoverDirection == 1 && hoverHeight < hoverRange) || (hoverDirection == -1 && hoverHeight > hoverRange * -1)) {
            this.transform.position += new Vector3(0, currentSpeed * hoverDirection, 0);
            hoverHeight += currentSpeed * hoverDirection;
        } else {
            hoverDirection *= -1;
        }
    }

    int GetRotateDirection() {
		if ((targetRotation.y != 0 && this.transform.eulerAngles.y != 0) || targetRotation.y == 90) {
			if (targetRotation.y > this.transform.eulerAngles.y) {
				return 1;
			}

			return -1;
		}

		float negative = this.transform.eulerAngles.y - targetRotation.y,
            positive = targetRotation.y + 360 - this.transform.eulerAngles.y;

        if(positive < negative) {
            return 1;
		}

		return -1;
	}

    Vector3 GetForwardByDirection(GameWorld.Direction targetDirection) {
        if (targetDirection == GameWorld.Direction.North) {
            return new Vector3(0, 0, -1);
        } else if (targetDirection == GameWorld.Direction.East) {
            return new Vector3(-1, 0, 0);
        } else if (targetDirection == GameWorld.Direction.South) {
            return new Vector3(0, 0, 1);
        } else {
            return new Vector3(1, 0, 0);
        }
    }

    Vector3 GetEulerByDirection(GameWorld.Direction targetDirection) {
        int rotation = 0;

        if(targetDirection == GameWorld.Direction.North) {
            rotation = 90;
        } else if(targetDirection == GameWorld.Direction.East) {
            rotation = 180;
        } else if (targetDirection == GameWorld.Direction.South) {
            rotation = 270;
        }

        return new Vector3(0, rotation, 90);
    }

    GameWorld.Direction SelectDirection(GameWorld.Direction[] inputDirections) {
		if (inputDirections.Length == 1) {
            return inputDirections[0];
        } else {
            List<GameWorld.Direction> possibleDirections = new List<GameWorld.Direction>();
			string debugDirs = "";
			foreach (GameWorld.Direction direction in inputDirections) {
                if(direction != GameWorld.GetOppositeDirection(myDirection)) {
                    possibleDirections.Add(direction);
					debugDirs += direction.ToString() + ", ";
				}
            }
			
			return possibleDirections[Random.Range(0, possibleDirections.Count)];
        }
    }

    GameWorld.MapPoint GetCurrentMapPoint() {
        return GameWorld.map[GetMyPosition().ToString()];
    }

    GameWorld.Point GetMyPosition() {
        Vector3 position = (this.transform.position - GameWorld.hostileOffset) / GameWorld.blockSize;

		return new GameWorld.Point((int)(position.x), (int)(position.z));
    }
}
