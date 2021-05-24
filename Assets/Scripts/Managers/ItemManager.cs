using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour {
    public static ItemManager Instance { get; private set; }

    [SerializeField] private Transform itemParent;

    [Space(20)]
    [SerializeField] private ItemTypeInfo[] itemTypes;
    private Dictionary<ItemType, ItemTypeInfo> itemTypesMap;

    [Space(20)]
    [SerializeField] private Item emptyItem;

    [Space(20)]
    [SerializeField] private List<Item> items;

    private Dictionary<int, Item> itemsInWorld;
    private Dictionary<int, GameObject> itemObjsInWorld;
    private Dictionary<int, MeshRenderer> itemRenderersInWorld;

    private void Awake() {
        Instance = this;

        GenerateItemTypesMap();

        itemsInWorld = new Dictionary<int, Item>();
        itemObjsInWorld = new Dictionary<int, GameObject>();
        itemRenderersInWorld = new Dictionary<int, MeshRenderer>();

        InputManager.Instance.RegisterQKeyClicked(OnQKeyClicked);
    }

    private void Update() {
        foreach (int id in itemsInWorld.Keys) {
            if (itemObjsInWorld[id].activeInHierarchy) {
                itemsInWorld[id].OnUpdate(Time.deltaTime);
            }
        }
    }

    private void GenerateItemTypesMap() {
        itemTypesMap = new Dictionary<ItemType, ItemTypeInfo>();
        for (int i = 0; i < itemTypes.Length; i++) {
            itemTypesMap.Add(itemTypes[i].type, itemTypes[i]);
        }
    }

    private void OnQKeyClicked(int isClicked) {
        if (isClicked != 0) {
            return;
        }

        foreach (int id in itemsInWorld.Keys) {
            if (itemObjsInWorld[id].activeInHierarchy && PlayerManager.Instance.possessed.inventory.Contains(itemsInWorld[id])) {
                itemsInWorld[id].OnAction(itemObjsInWorld[id], true);
            }
        }
    }

    public List<Item> GenerateItems(int amt, Transform parent, List<ItemType> exclude = null) {
        List<Item> newItems = new List<Item>();
        for (int i = 0; i < amt; i++) {
            Item item = items[Random.Range(0, items.Count)];
            if (exclude != null) {
                while (exclude.Contains(item.type)) {
                    item = items[Random.Range(0, items.Count)];
                }
            }
            newItems.Add(GenerateItem(item, parent));
        }
        return newItems;
    }

    public List<Item> GenerateItems(List<string> itemNames, Transform parent) {
        List<Item> newItems = new List<Item>();
        for (int i = 0; i < itemNames.Count; i++) {
            newItems.Add(GenerateItem(FindItem(itemNames[i]), parent));
        }
        return newItems;
    }

    private Item GenerateItem(Item copyItem, Transform parent) {
        if (copyItem == null) {
            Debug.Log("Item not found.");
            return null;
        }

        Item item = new Item(copyItem);
        itemsInWorld.Add(item.id, item);

        ItemTypeInfo info = itemTypesMap[item.type];

        GameObject obj = GameObject.Instantiate(info.prefab, (parent == null ? itemParent : parent));
        obj.transform.localPosition = new Vector3(info.offset.x, info.offset.y, -0.02f);
        obj.name = item.name;
        obj.SetActive(false);

        CityManager.Instance.BuildMesh(obj, 1, 1);
        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        renderer.material.mainTexture = item.icon.texture;
        renderer.material.shader = Shader.Find("Transparent/Diffuse");

        for (int i = 0; i < item.effects.Length; i++) {
            GameObject objChild = obj.transform.GetChild(i).gameObject;
            objChild.transform.position = new Vector3(objChild.transform.position.x, objChild.transform.position.y, CityManager.EFFECTS);
            CityManager.Instance.BuildMesh(objChild, 1, 1);
            MeshRenderer childRenderer = objChild.GetComponent<MeshRenderer>();
            childRenderer.material.mainTexture = item.effects[i].texture;
            childRenderer.material.shader = Shader.Find("Transparent/Diffuse");
        }

        itemObjsInWorld.Add(item.id, obj);
        itemRenderersInWorld.Add(item.id, renderer);
        item.OnCreated(obj);
        return item;
    }

    public void DeleteItems(List<Item> items) {
        for (int i = 0; i < items.Count; i++) {
            itemsInWorld.Remove(items[i].id);
            itemObjsInWorld.Remove(items[i].id);
            itemRenderersInWorld.Remove(items[i].id);
        }
    }

    public Item FindItem(string itemName) {
        if (itemName == emptyItem.name) {
            return emptyItem;
        }
        for (int i = 0; i < items.Count; i++) {
            if (itemName.ToLower().Equals(items[i].name.ToLower())) {
                return items[i];
            }
        }
        return null;
    }

    public Item FindItemInWorld(int itemId) {
        if (itemsInWorld.ContainsKey(itemId)) {
            return itemsInWorld[itemId];
        }
        return null;
    }

    public GameObject FindItemObjInWorld(int itemId) {
        if (itemObjsInWorld.ContainsKey(itemId)) {
            return itemObjsInWorld[itemId];
        }
        return null;
    }

    public MeshRenderer FindItemRendererInWorld(int itemId) {
        if (itemRenderersInWorld.ContainsKey(itemId)) {
            return itemRenderersInWorld[itemId];
        }
        return null;
    }

    public ItemTypeInfo GetItemTypeInfo(ItemType type) {
        return itemTypesMap[type];
    }

    public List<string> GetEmptyItem() {
        return new List<string> { emptyItem.name };
    }
}

