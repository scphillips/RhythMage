using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonBuilder : MonoBehaviour
{
    [Zenject.Inject]
    readonly Settings m_settings;

    [Zenject.Inject]
    Outplay.RandomNumberProvider m_rng;

    [System.Serializable]
    public class Settings
    {
        public GameObject prefabFloor;
        public GameObject prefabWall;
    }

    enum Direction
    {
        Forwards,
        Right,
        Backwards,
        Left
    }

    Dictionary<Direction, Vector3> m_facings = new Dictionary<Direction, Vector3>()
    {
        { Direction.Forwards, new Vector3(0, 0, 1) },
        { Direction.Right, new Vector3(1, 0, 0) },
        { Direction.Backwards, new Vector3(0, 0, -1) },
        { Direction.Left, new Vector3(-1, 0, 0) }
    };

    Dictionary<Direction, Vector3> m_wallOffsetLeft = new Dictionary<Direction, Vector3>()
    {
        { Direction.Forwards, new Vector3(-1, 0, 0) },
        { Direction.Right, new Vector3(0, 0, 1) },
        { Direction.Backwards, new Vector3(1, 0, 0) },
        { Direction.Left, new Vector3(0, 0, -1) }
    };

    Dictionary<Direction, Vector3> m_wallOffsetRight = new Dictionary<Direction, Vector3>()
    {
        { Direction.Forwards, new Vector3(1, 0, 0) },
        { Direction.Right, new Vector3(0, 0, -1) },
        { Direction.Backwards, new Vector3(-1, 0, 0) },
        { Direction.Left, new Vector3(0, 0, 1) }
    };

    Direction ChangeDirectionLeft(Direction currentDirection)
    {
        int dirInt = System.Convert.ToInt32(currentDirection);
        dirInt = (dirInt + m_facings.Count - 1) % m_facings.Count;
        return (Direction)dirInt;
    }

    Direction ChangeDirectionRight(Direction currentDirection)
    {
        int dirInt = System.Convert.ToInt32(currentDirection);
        dirInt = (dirInt + 1) % m_facings.Count;
        return (Direction)dirInt;
    }

    void Start()
    {
        BuildDungeon();
	}
	
	void Update()
    {
		
	}

    void BuildDungeon()
    {
        Direction currentDirection = Direction.Forwards;
        Vector3 currentPosition = Vector3.zero;
        int segments = 5;
        for (int i = 0; i < segments; ++i)
        {
            Vector3 offset;
            m_facings.TryGetValue(currentDirection, out offset);

            int length = m_rng.Next(2, 10);
            for (int j = 0; j < length; ++j)
            {
                currentPosition += offset;
                CreateSegment(currentPosition, currentDirection);
            }

            if (i < segments - 1)
            {
                currentPosition += offset;
                int directionChange = m_rng.Next(2);
                if (directionChange == 0)
                {
                    currentDirection = BuildLeftTurn(currentPosition, currentDirection);
                }
                else if (directionChange == 1)
                {
                    currentDirection = BuildRightTurn(currentPosition, currentDirection);
                }
            }
        }
    }

    Direction BuildLeftTurn(Vector3 position, Direction direction)
    {
        // Create Floor
        var floor = (GameObject)Instantiate(m_settings.prefabFloor);
        floor.transform.SetParent(transform, false);
        floor.transform.localPosition = new Vector3(position.x, position.y - 0.5f, position.z);

        Direction newDirection = ChangeDirectionLeft(direction);
        return newDirection;
    }

    Direction BuildRightTurn(Vector3 position, Direction direction)
    {
        // Create Floor
        var floor = (GameObject)Instantiate(m_settings.prefabFloor);
        floor.transform.SetParent(transform, false);
        floor.transform.localPosition = new Vector3(position.x, position.y - 0.5f, position.z);
        
        Direction newDirection = ChangeDirectionRight(direction);
        return newDirection;
    }

    void CreateSegment(Vector3 position, Direction direction)
    {
        // Create Floor
        var floor = (GameObject)Instantiate(m_settings.prefabFloor);
        floor.transform.SetParent(transform, false);
        floor.transform.localPosition = new Vector3(position.x, position.y - 0.5f, position.z);

        // Create Walls
        Vector3 leftOffset;
        Vector3 rightOffset;
        m_wallOffsetLeft.TryGetValue(direction, out leftOffset);
        m_wallOffsetRight.TryGetValue(direction, out rightOffset);

        var leftWall = (GameObject)Instantiate(m_settings.prefabWall);
        leftWall.transform.SetParent(transform, false);
        leftWall.transform.localPosition = position + leftOffset;

        var rightWall = (GameObject)Instantiate(m_settings.prefabWall);
        rightWall.transform.SetParent(transform, false);
        rightWall.transform.localPosition = position + rightOffset;
    }
}
