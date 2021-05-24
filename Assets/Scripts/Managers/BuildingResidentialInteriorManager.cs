using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildingResidentialInteriorManager : BuildingTypeInteriorManager {
    [Space(20)]
    [SerializeField] private int minHallSpace = 40;
    [SerializeField] private int maxHallSpace = 55;
    [SerializeField] private float minGap = 0.25f;
    [SerializeField] private int minSide = 5;
    [SerializeField] private int maxSide = 9;
    [SerializeField] private float divideChance = 0.4f;

    // Total hallspace
    private int hallSpace;
    // Amount of hallspace currently generated (should be >= hallSpace by the end of GenerateHallways() recursion)
    private int currHallSpace;

    private Dictionary<int, Furniture> furnitureIdMap;
    // Num of each furniture
    private Dictionary<int, int> furnitureTypes;

    public override List<Person> GenerateInterior(int seed, Vector2 pos) {
        base.GenerateInterior(seed, pos);

        int x = 0;
        int y = GetRandPos(0, height);
        while (x < width && y < height) {
            map[x, y] = HALLWAY;
            x++;
        }

        hallSpace = rand.Next(minHallSpace, maxHallSpace + 1);
        currHallSpace = 0;
        GenerateHallway(GetRandPos(0, width), 0, true);

        GenerateRoomAreas();
        GenerateRoomTypes();
        GenerateDoors();
        GenerateFurniture();

        return people;
    }

    private void GenerateHallway(int startX, int startY, bool horiz) {
        if (currHallSpace >= hallSpace || startX < 0 || startY < 0 || startX >= width || startY >= height) {
            return;
        }

        int x = startX;
        int y = startY;

        while (x < width && y < height && map[x, y] != HALLWAY) {
            map[x, y] = HALLWAY;
            if (horiz) {
                y++;
            } else {
                x++;
            }
            currHallSpace++;
        }

        int endX = x;
        int endY = y;

        if (!horiz) {
            x = GetRandPos(startX, endX);
            y++;
        } else {
            x++;
            y = GetRandPos(startY, endY);
        }
        GenerateHallway(x, y, !horiz);

        if (!horiz) {
            x = GetRandPos(startX, endX);
            y = 0;
        } else {
            x = 0;
            y = GetRandPos(startY, endY);
        }
        GenerateHallway(x, y, !horiz);
    }

    private int GetRandPos(int start, int end) {
        int min = (int)(start * (1 + minGap));
        int max = (int)(end * (1 - minGap));
        if (max < min) {
            return (int)((start + end) / 2f);
        }
        return rand.Next(min, max);
    }

    private void GenerateRoomAreas() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (map[x, y] == AIR) {
                    roomAreas.AddRange(GenerateRoomArea(x, y));
                }
            }
        }
    }

    private List<Rect> GenerateRoomArea(int startX, int startY) {
        List<Rect> rects = new List<Rect>();

        int x = startX;
        int y = startY;
        while (x < width && map[x, y] != HALLWAY) {
            y = startY;
            while (y < height && map[x, y] != HALLWAY) {
                map[x, y] = requiredRooms.Count;
                y++;
            }
            x++;
            y--;
        }
        x--;

        Rect rect = Rect.MinMaxRect(startX, startY, x + 1, y + 1);
        Rect tempRect;
        rects.Add(new Rect(rect.x, rect.y, rect.width, rect.height));
        if (rect.width >= maxSide) {
            int num = rects.Count;
            for (int i = 0; i < num; i++) {
                int newWidth = GetRandPos(1, (int)rect.width);
                tempRect = rects[i];
                tempRect.width = newWidth;
                rects[i] = tempRect;
                rects.Add(new Rect(rects[i].xMax, rects[i].y, rect.width - newWidth, rects[i].height));
            }
        } else if (rect.width > minSide && rand.NextDouble() <= divideChance) {
            int num = rects.Count;
            for (int i = 0; i < num; i++) {
                int newWidth = GetRandPos(1, (int)rect.width);
                tempRect = rects[i];
                tempRect.width = newWidth;
                rects[i] = tempRect;
                rects.Add(new Rect(rects[i].xMax, rects[i].y, rect.width - newWidth, rects[i].height));
            }
        }
        if (rect.height >= maxSide) {
            int num = rects.Count;
            for (int i = 0; i < num; i++) {
                int newHeight = GetRandPos(1, (int)rect.height);
                tempRect = rects[i];
                tempRect.height = newHeight;
                rects[i] = tempRect;
                rects.Add(new Rect(rects[i].x, rects[i].yMax, rects[i].width, rect.height - newHeight));
            }
        } else if (rect.height > minSide && rand.NextDouble() <= divideChance) {
            int num = rects.Count;
            for (int i = 0; i < num; i++) {
                int newHeight = GetRandPos(1, (int)rect.height);
                tempRect = rects[i];
                tempRect.height = newHeight;
                rects[i] = tempRect;
                rects.Add(new Rect(rects[i].x, rects[i].yMax, rects[i].width, rect.height - newHeight));
            }
        }

        return rects;
    }

    private void GenerateRoomTypes() {
        List<Room> roomsToAdd = new List<Room>(requiredRooms);
        roomsToAdd.RemoveRange(AIR, HALLWAY + 1);
        roomsToAdd = OrderRoomTypes(roomsToAdd);

        int largest;
        for (int i = roomsToAdd.Count - 1; i >= 0; i--) {
            int index = requiredRooms.IndexOf(roomsToAdd[i]);
            for (int j = 0; j < roomsToAdd[i].MinNum(); j++) {
                largest = GetLargestRoom();
                if (largest < 0) {
                    return;
                }
                Rect largestRoom = roomAreas[largest];
                int placeable = IsRoomPlaceable(largestRoom, roomsToAdd[i]);
                if (placeable == 1) {
                    continue;
                } else if (placeable == -1) {
                    List<int> empty = GetEmptyRooms();
                    for (int k = 0; k < empty.Count; k++) {
                        if (IsRoomPlaceable(roomAreas[empty[k]], roomsToAdd[i]) == 0) {
                            largestRoom = roomAreas[empty[k]];
                            break;
                        }
                    }
                }
                FillRoom(largestRoom, index);
                if (roomTypes.ContainsKey(index)) {
                    roomTypes[index]++;
                } else {
                    roomTypes.Add(index, 1);
                }
            }
            if (roomTypes.ContainsKey(index) && roomTypes[index] >= roomsToAdd[i].MaxNum()) {
                roomsToAdd.RemoveAt(i);
            }
        }

        List<Room> temp = new List<Room>(extraRooms);
        temp.RemoveAt(AIR);
        roomsToAdd.AddRange(temp);

        largest = GetLargestRoom();
        while (largest > -1) {
            Rect largestRoom = roomAreas[largest];
            Room room = GetRandRoom(roomsToAdd);
            do {
                room = GetRandRoom(roomsToAdd);
            } while (IsRoomPlaceable(largestRoom, room) != 0);
            int i = (extraRooms.Contains(room) ? -extraRooms.IndexOf(room) : requiredRooms.IndexOf(room));
            FillRoom(largestRoom, i);
            if (roomTypes.ContainsKey(i)) {
                roomTypes[i]++;
            } else {
                roomTypes.Add(i, 1);
            }
            if (room.MaxNum() <= roomTypes[i]) {
                roomsToAdd.Remove(room);
            }
            largest = GetLargestRoom();
        }
    }

    private void GenerateDoors() {
        for (int i = 0; i < roomAreas.Count; i++) {
            doors.Add(new Dictionary<Vector3, string>());
        }

        for (int i = 0; i < roomAreas.Count; i++) {

            Rect room = roomAreas[i];
            Vector3 key;
            int index;

            if (room.yMin > 0 && map[(int)room.xMin, (int)room.yMin - 1] == HALLWAY) {

                key = new Vector3(room.center.x - 0.5f, room.yMin, 0);
                if (!doors[i].ContainsKey(key)) {
                    doors[i].Add(key, "N");
                }

                index = GetRoom(room.xMin, room.yMin - 1);
                if (index != -1 && !doors[index].ContainsKey(key)) {
                    doors[index].Add(key, "S");
                }

            } else if (room.xMax < width && map[(int)room.xMax, (int)room.yMax - 1] == HALLWAY) {

                key = new Vector3(room.xMax - 0.5f, room.center.y, 90);
                if (!doors[i].ContainsKey(key)) {
                    doors[i].Add(key, "E");
                }

                index = GetRoom(room.xMax, room.yMax - 1);
                if (index != -1 && !doors[index].ContainsKey(key)) {
                    doors[index].Add(key, "W");
                }

            } else if (room.yMax < height && map[(int)room.xMax - 1, (int)room.yMax] == HALLWAY) {

                key = new Vector3(room.center.x - 0.5f, room.yMax, 180);
                if (!doors[i].ContainsKey(key)) {
                    doors[i].Add(key, "S");
                }

                index = GetRoom(room.xMax - 1, room.yMax);
                if (index != -1 && !doors[index].ContainsKey(key)) {
                    doors[index].Add(key, "N");
                }

            } else if (room.xMin > 0 && map[(int)room.xMin - 1, (int)room.yMin] == HALLWAY) {

                key = new Vector3(room.xMin - 0.5f, room.center.y, 270);
                if (!doors[i].ContainsKey(key)) {
                    doors[i].Add(key, "W");
                }

                index = GetRoom(room.xMin - 1, room.yMin);
                if (index != -1 && !doors[index].ContainsKey(key)) {
                    doors[index].Add(key, "E");
                }

            }

            if (doors[i].Count < 1) {
                int type = map[(int)room.xMin, (int)room.yMin];
                if (room.yMin > 0 && map[(int)room.xMin, (int)room.yMin - 1] != type && !doors[i].ContainsValue("N")) {

                    key = new Vector3(room.center.x - 0.5f, room.yMin, 0);
                    if (!doors[i].ContainsKey(key)) {
                        doors[i].Add(key, "N");
                    }

                    index = GetRoom(room.xMin, room.yMin - 1);
                    if (index != -1 && !doors[index].ContainsKey(key)) {
                        doors[index].Add(key, "S");
                    }

                } else if (room.xMax < width && map[(int)room.xMax, (int)room.yMax - 1] != type && !doors[i].ContainsValue("E")) {

                    key = new Vector3(room.xMax - 0.5f, room.center.y, 90);
                    if (!doors[i].ContainsKey(key)) {
                        doors[i].Add(key, "E");
                    }

                    index = GetRoom(room.xMax, room.yMax - 1);
                    if (index != -1 && !doors[index].ContainsKey(key)) {
                        doors[index].Add(key, "W");
                    }

                } else if (room.yMax < height && map[(int)room.xMax - 1, (int)room.yMax] != type && !doors[i].ContainsValue("S")) {

                    key = new Vector3(room.center.x - 0.5f, room.yMax, 180);
                    if (!doors[i].ContainsKey(key)) {
                        doors[i].Add(key, "S");
                    }

                    index = GetRoom(room.xMax - 1, room.yMax);
                    if (index != -1 && !doors[index].ContainsKey(key)) {
                        doors[index].Add(key, "N");
                    }

                } else if (room.xMin > 0 && map[(int)room.xMin - 1, (int)room.yMin] != type && !doors[i].ContainsValue("W")) {

                    key = new Vector3(room.xMin - 0.5f, room.center.y, 270);
                    if (!doors[i].ContainsKey(key)) {
                        doors[i].Add(key, "W");
                    }

                    index = GetRoom(room.xMin - 1, room.yMin);
                    if (index != -1 && !doors[index].ContainsKey(key)) {
                        doors[index].Add(key, "E");
                    }

                }
            }

            GenerateDoor(i);

        }
    }

    private void GenerateDoor(int index) {
        Rect room = roomAreas[index];
        Dictionary<Vector3, string> doorPos = doors[index];
        foreach (KeyValuePair<Vector3, string> pos in doorPos) {
            DisplayDoor(pos.Key.x, pos.Key.y, pos.Key.z);
        }
    }

    private List<Room> OrderRoomTypes(List<Room> rooms) {
        List<Room> ordered = new List<Room>(rooms);
        for (int i = 0; i < ordered.Count; i++) {
            for (int j = i + 1; j < ordered.Count; j++) {
                if (ordered[i].MinArea() > ordered[j].MinArea()) {
                    Room temp = ordered[j];
                    ordered[j] = ordered[i];
                    ordered[i] = temp;
                }
            }
        }
        return ordered;
    }

    private int GetRoom(float orgX, float orgY) {
        int x = (int)orgX;
        int y = (int)orgY;
        for (int i = 0; i < roomAreas.Count; i++) {
            Rect room = roomAreas[i];
            if (x >= room.xMin && x <= room.xMax && y >= room.yMin && y <= room.yMax) {
                return i;
            }
        }
        return -1;
    }

    private Room GetRandRoom(List<Room> roomsToAdd) {
        float total = 0;
        foreach (Room room in roomsToAdd) {
            total += room.SpawnChance();
        }
        float randValue = (float)rand.NextDouble() * total;
        for (int i = 0; i < roomsToAdd.Count; i++) {
            if (randValue < roomsToAdd[i].SpawnChance()) {
                return roomsToAdd[i];
            }
            randValue -= roomsToAdd[i].SpawnChance();
        }
        return roomsToAdd[roomsToAdd.Count - 1];
    }

    private List<int> GetEmptyRooms() {
        List<int> empty = new List<int>();
        for (int i = 0; i < roomAreas.Count; i++) {
            if (map[(int)roomAreas[i].xMin, (int)roomAreas[i].yMin] == requiredRooms.Count) {
                empty.Add(i);
            }
        }
        return empty;
    }

    private int GetLargestRoom() {
        List<int> empty = GetEmptyRooms();
        if (empty.Count == 0) {
            return -1;
        }
        int largest = empty[0];
        for (int i = 1; i < empty.Count; i++) {
            float iArea = roomAreas[empty[i]].width * roomAreas[empty[i]].height;
            float largestArea = roomAreas[largest].width * roomAreas[largest].height;
            if (iArea > largestArea) {
                largest = empty[i];
            }
        }
        return largest;
    }

    private void GenerateFurniture() {
        for (int i = 0; i < roomAreas.Count; i++) {
            furnitureIdMap = new Dictionary<int, Furniture>(GetRoom(map[(int)roomAreas[i].xMin, (int)roomAreas[i].yMin]).GetFurnitureIdMap());

            furnitureTypes = new Dictionary<int, int>();
            foreach (KeyValuePair<int, Furniture> furniture in furnitureIdMap) {
                furnitureTypes.Add(furniture.Key, 0);
            }

            Dictionary<int, Furniture> oldFurnitureIdMap = new Dictionary<int, Furniture>(furnitureIdMap);
            foreach (KeyValuePair<int, Furniture> furniture in oldFurnitureIdMap) {
                if (!furniture.Value.RequiresOtherObject()) {
                    int num = rand.Next(furniture.Value.MinNum(), furniture.Value.MaxNum() + 1);
                    for (int j = 0; j < num; j++) {
                        if (rand.NextDouble() <= furniture.Value.spawnChance) {
                            int x;
                            int y;
                            int attempts = 0;
                            if (furniture.Value.RequiresWall()) {
                                do {
                                    if (rand.NextDouble() < 0.5) {
                                        // Horizontal
                                        x = rand.Next((int)roomAreas[i].xMin, (int)roomAreas[i].xMax);
                                        y = (rand.NextDouble() < 0.5 ? (int)roomAreas[i].yMin : (int)roomAreas[i].yMax - 1);
                                    } else {
                                        // Vertical
                                        x = (rand.NextDouble() < 0.5 ? (int)roomAreas[i].xMin : (int)roomAreas[i].xMax - 1);
                                        y = rand.Next((int)roomAreas[i].yMin, (int)roomAreas[i].yMax);
                                    }
                                    attempts++;
                                } while (!IsFurniturePlaceable(furniture.Value, x, y) && attempts < 5);
                            } else {
                                do {
                                    x = rand.Next((int)roomAreas[i].xMin, (int)roomAreas[i].xMax);
                                    y = rand.Next((int)roomAreas[i].yMin, (int)roomAreas[i].yMax);
                                    attempts++;
                                } while (!IsFurniturePlaceable(furniture.Value, x, y) && attempts < 5);
                            }
                            if (attempts >= 5) {
                                continue;
                            }
                            furnitureMap[x, y] = furniture.Key;
                            furnitureTypes[furniture.Key]++;
                            if (furnitureTypes[furniture.Key] >= furniture.Value.MaxNum()) {
                                furnitureIdMap.Remove(furniture.Key);
                            }
                            FurnitureChanged(furniture.Key, x, y);
                        }
                    }
                }
            }
        }
    }

    private void FurnitureChanged(int id, int orgX, int orgY) {
        for (int x = -1; x <= 1; x++) {
            for (int y = -1; y <= 1; y++) {
                if ((x == 0 && y == 0) || IsWall(orgX, orgY, orgX + x, orgY + y)) {
                    continue;
                }
                Dictionary<int, Furniture> oldFurnitureIdMap = new Dictionary<int, Furniture>(furnitureIdMap);
                foreach (KeyValuePair<int, Furniture> furniture in oldFurnitureIdMap) {
                    if (furniture.Value.RequiresObject(id) && furnitureTypes[furniture.Key] < furniture.Value.MaxNum()) {
                        if (IsFurniturePlaceable(furniture.Value, orgX + x, orgY + y) && rand.NextDouble() <= furniture.Value.spawnChance) {
                            furnitureMap[orgX + x, orgY + y] = furniture.Key;
                            furnitureTypes[furniture.Key]++;
                            if (furnitureTypes[furniture.Key] >= furniture.Value.MaxNum()) {
                                furnitureIdMap.Remove(furniture.Key);
                            }
                            FurnitureChanged(furniture.Key, orgX + x, orgY + y);
                            break;
                        }
                    }
                }
            }
        }
    }
}

