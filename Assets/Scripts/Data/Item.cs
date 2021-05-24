using System;
using UnityEngine;

[Serializable]
public class Item {
    private static int lastIdGiven = 0;
    public int id { get; protected set; }

    public string name;
    public string description;
    public Sprite icon;
    public ItemType type;

    [Space(20)]
    public Sprite[] effects;

    // All attributes and methods may not be used for every item
    [Space(20)]
    public int damageAmt;
    public int range;
    public float cooldown;
    public GameObject bullet;
    public Color glowColor;

    [Space(20)]
    public Vector2 horizOffset;
    public Vector2 vertOffset;
    public AudioClip[] soundEffects;

    public float cooldownTime = 0;

    public Item(Item item) {
        this.name = item.name;
        this.description = item.description;
        this.icon = item.icon;
        this.type = item.type;
        this.effects = item.effects;
        this.damageAmt = item.damageAmt;
        this.range = item.range;
        this.cooldown = item.cooldown;
        this.bullet = item.bullet;
        this.horizOffset = item.horizOffset;
        this.vertOffset = item.vertOffset;
        this.glowColor = item.glowColor;
        this.soundEffects = item.soundEffects;
        id = lastIdGiven;
        lastIdGiven++;
    }

    public void OnCreated(GameObject obj) {
        switch (type) {
            case ItemType.Artifact:
                OnCreatedArtifact(obj);
                break;
        }
    }

    private void OnCreatedArtifact(GameObject obj) {
        obj.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = glowColor;
    }

    public void OnUpdate(float deltaTime) {
        switch (type) {
            case ItemType.RangedWeapon:
                OnUpdateRangedWeapon(deltaTime);
                break;
        }
    }

    private void OnUpdateRangedWeapon(float deltaTime) {
        cooldownTime += deltaTime;
    }

    public void OnAction(GameObject obj, bool isPlayer = true) {
        switch (type) {
            case ItemType.RangedWeapon:
                OnActionRangedWeapon(obj, isPlayer);
                break;
            case ItemType.MeleeWeapon:
            default:
                OnActionMeleeWeapon(obj, isPlayer);
                break;
        }
    }

    private void OnActionRangedWeapon(GameObject obj, bool isPlayer) {
        if (cooldownTime < cooldown) {
            return;
        }

        Person person = PeopleManager.Instance.FindPerson(obj.transform.parent.parent.gameObject);
        GameObject bulletObj = null;
        Vector3 pos = person.obj.transform.position;

        switch (person.dir) {
            case 0:
                pos.x += horizOffset.x;
                pos.y += horizOffset.y;
                bulletObj = GameObject.Instantiate(bullet, pos, Quaternion.Euler(new Vector3(0, 0, 0)));
                break;
            case 1:
                pos.y += vertOffset.y;
                bulletObj = GameObject.Instantiate(bullet, pos, Quaternion.Euler(new Vector3(0, 0, 90)));
                break;
            case 2:
                pos.x -= horizOffset.x;
                pos.y += horizOffset.y;
                bulletObj = GameObject.Instantiate(bullet, pos, Quaternion.Euler(new Vector3(0, 0, 180)));
                break;
            case 3:
                pos.y -= vertOffset.x;
                bulletObj = GameObject.Instantiate(bullet, pos, Quaternion.Euler(new Vector3(0, 0, 270)));
                break;
        }

        cooldownTime = 0;

        if (bulletObj != null) {
            bulletObj.GetComponent<Bullet>().SetInfo(damageAmt, range, person);
        }

        CameraManager.Instance.Flash(obj.transform.GetChild(0).gameObject, 0.05f);
        PeopleManager.Instance.SendMsg(new MessageAttack(pos, 17, false));

        AudioClip soundEffect = soundEffects[UnityEngine.Random.Range(0, soundEffects.Length)];
        AudioManager.Instance.PlaySound(soundEffect, soundEffect.length);

        if (isPlayer) {
            CameraManager.Instance.Shake(0.15f);
        }
    }

    private void OnActionMeleeWeapon(GameObject obj, bool isPlayer) {
        AudioClip soundEffect = soundEffects[UnityEngine.Random.Range(0, soundEffects.Length)];
        AudioManager.Instance.PlaySound(soundEffect, soundEffect.length);

        if (isPlayer) {
            CameraManager.Instance.Shake(0.07f);
        }

        BoxCollider2D[] cols = new BoxCollider2D[10];
        obj.GetComponent<BoxCollider2D>().GetContacts(cols);

        Person orgPerson = PeopleManager.Instance.FindPerson(obj.transform.parent.parent.gameObject);

        for (int i = 0; i < cols.Length; i++) {

            if (cols[i] == null) {
                continue;
            }

            Person person = PeopleManager.Instance.FindPerson(cols[i].gameObject);
            if (person != null && person != orgPerson) {
                switch (orgPerson.dir) {
                    case 0:
                        person.Move(person.pos - new Vector3(-0.1f, 0, 0), false);
                        break;
                    case 1:
                        person.Move(person.pos - new Vector3(0, -0.1f, 0), false);
                        break;
                    case 2:
                        person.Move(person.pos - new Vector3(0.1f, 0, 0), false);
                        break;
                    case 3:
                        person.Move(person.pos - new Vector3(0, 0.1f, 0), false);
                        break;
                }
                person.AddHealth(-damageAmt, PeopleManager.Instance);
                PeopleManager.Instance.SendMsg(new MessageAttack(cols[i].transform.position, 5, true));
            }

        }
    }
}

