using System.Collections.Generic;
using UnityEngine;

public interface IBuildingInteriorManager {
	Vector2 SpawnPos(Vector2 pos);
	List<Person> GenerateInterior(int seed, Vector2 pos);
	void DisplayInterior();

	int[,] Map();
	int Width();
	int Height();
	bool IsWall(int x, int y);
	bool IsWall(int x, int y, int dirX, int dirY);
	Vector2[] Neighbors(int x, int y);
}