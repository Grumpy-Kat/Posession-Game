using System.Collections.Generic;
using UnityEngine;

public class ItemSpawnPoint : MonoBehaviour {
    [SerializeField] private string itemName;

    [SerializeField] private Vector2 spawnRangeX;
    [SerializeField] private Vector2 spawnRangeY;
    [SerializeField] private int spawnMin = 0;
    [SerializeField] private int spawnMax = 2;
    [SerializeField] private float spawnRate = 45;
    private float currTime = 0;

    private void Start() {
        // Object is placed and parameters to modified to determine what objects to spawn at the location of the object
        SpawnItems();
    }

    private void Update() {
        if (spawnRate > 0) {
            currTime -= Time.deltaTime;
            if (currTime <= 0) {
                SpawnItems();
            }
        }
    }

    private void SpawnItems() {
        int spawnAmt = Random.Range(spawnMin, spawnMax + 1);
        for (int i = 0; i < spawnAmt; i++) {
            List<Item> items = ItemManager.Instance.GenerateItems(new List<string>(new string[] { itemName }), null);
            Vector3 pos = new Vector3(transform.position.x + Random.Range(spawnRangeX.x, spawnRangeX.y), transform.position.y + Random.Range(spawnRangeY.x, spawnRangeY.y), CityManager.EFFECTS - 1);
            GameObject obj = ItemManager.Instance.FindItemObjInWorld(items[0].id);
            obj.transform.position = pos;
            obj.SetActive(true);
            MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
            renderer.sortingLayerName = "Furniture";
        }
        currTime = spawnRate;
    }
}

