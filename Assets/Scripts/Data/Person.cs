using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Person {
    public static Material defaultMat;
    public static Material possessedMat;
    public static GameObject possessedEffect;

    public GameObject obj { get; private set; }
    public Transform objTransform { get; private set; }
    public GameObject objChild { get; private set; }
    public MeshRenderer objRenderer { get; private set; }

    private PathAStar pathfinding;

    public Vector2 curr { get; private set; }
    public Vector2 next { get; private set; }
    public Vector2 dest { get; private set; }

    private float distToNext;
    private float movePercent;

    public Vector3 pos { get; private set; }

    private Texture2D[][] anims;
    private float timeSinceLastAnim = 0;
    private float animTime = 0.1f;
    private int animClip;
    private int animPhase;
    public int dir { get; private set; }

    public PersonState globalState { get; private set; }
    public PersonState lastState { get; private set; }
    public PersonState currState { get; private set; }

    public List<Item> inventory { get; private set; }

    public int speed { get; private set; }
    public int strength { get; private set; }
    public int resilience { get; private set; }

    public int health { get; private set; }

    public bool isPossessed { get; private set; }

    private AudioClip[] hurtSounds;

    public bool attacks { get; private set; }

    public Person(GameObject obj, Texture2D[][] anims, PersonState globalState, PersonState currState, List<Item> inventory, int speed, int strength, int resilience, int health, AudioClip[] hurtSounds, bool attacks) {
        this.obj = obj;
        objTransform = obj.transform;
        objChild = objTransform.GetChild(0).gameObject;
        objRenderer = objChild.GetComponent<MeshRenderer>();
        if (objRenderer == null) {
            Debug.Log("ObjRenderer is null");
        }
        pos = objTransform.position;

        this.anims = anims;
        animPhase = 0;
        animClip = 0;
        dir = 2;

        this.globalState = globalState;
        this.globalState.EnterState(this);
        this.currState = currState;
        this.currState.EnterState(this);
        this.inventory = inventory;
        this.speed = speed;
        this.strength = strength;
        this.resilience = resilience;
        this.health = health;
        this.hurtSounds = hurtSounds;
        this.attacks = attacks;

        SetPossessed(false);
    }

    public void Update(float deltaTime) {
        timeSinceLastAnim += deltaTime;
        if (timeSinceLastAnim >= animTime) {
            timeSinceLastAnim = 0;
            animPhase++;
        }

        objRenderer.material.mainTexture = anims[animClip][animPhase % anims[animClip].Length];
    }

    public void FixedUpdate(float deltaTime, float speed) {
        globalState.ExecuteState(this);
        currState.ExecuteState(this);

        if (curr == dest) {
            pathfinding = null;
            return;
        }
        if ((next.x == -1 && next.y == -1) || next == curr) {
            if (pathfinding == null || pathfinding.Length() == 0) {
                pathfinding = new PathAStar(curr, dest);
                if (pathfinding.Length() == 0) {
                    // Debug.Log(obj.name  + ": No path to (" + dest.x + ", " + dest.y + ").");
                    pathfinding = null;
                    dest = curr;
                    return;
                }
            }
            next = pathfinding.Dequeue();
            distToNext = CityManager.Instance.Distance(curr, next);
        }
        float moveThisFrame = speed * deltaTime;
        float percThisFrame = moveThisFrame / distToNext;
        movePercent += percThisFrame;
        if (movePercent >= 1) {
            curr = next;
            movePercent = 0;
        }
        Move(Vector2.Lerp(curr, next, movePercent));
    }

    public void Move(Vector2 pos, bool isWalking = true, bool collide = true) {
        if (!isWalking && isPossessed) {
            PlayerManager.Instance.pos = new Vector2(-1, -1);
        }

        if (isWalking) {
            if (this.pos.x < pos.x && !Mathf.Approximately(this.pos.x, pos.x)) {
                SetAnimClip(1);
                SetDir(0);
            } else if (this.pos.y < pos.y && !Mathf.Approximately(this.pos.y, pos.y)) {
                SetAnimClip(1);
                SetDir(1);
            } else if (this.pos.x > pos.x && !Mathf.Approximately(this.pos.x, pos.x)) {
                SetAnimClip(1);
                SetDir(2);
            } else if (this.pos.y > pos.y && !Mathf.Approximately(this.pos.y, pos.y)) {
                SetAnimClip(1);
                SetDir(3);
            } else {
                SetAnimClip(0);
            }
        }

        /* float sortingPos = (BuildingInteriorManager.Instance.isInside ? (BuildingInteriorManager.Instance.Width() - BuildingInteriorManager.Instance.InsidePos(pos).y) : (CityManager.Instance.Width() - pos.y));
        for(int i = 0; i < inventory.Count; i++) {
			MeshRenderer item = ItemManager.Instance.FindItemRendererInWorld(inventory[i].id);
			Vector3 itemPos = item.transform.localPosition;
			if(dir == 0) {
				item.material.mainTextureScale = new Vector2(1, 1);
				itemPos.x = itemPos.x + 0.42f;
			} else if(dir == 2) {
				item.material.mainTextureScale = new Vector2(-1, 1);
				itemPos.x = itemPos.x - 0.42f;
			}
			item.transform.localPosition = itemPos;
			for(int j = 0; j < item.transform.childCount; j++) {
				Transform itemChild = item.transform.GetChild(j);
				itemPos = itemChild.localPosition;
				if(dir == 0) {
					itemChild.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(1, 1);
					itemPos.x = itemPos.x + 0.42f;
				} else if(dir == 2) {
					itemChild.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(-1, 1);
					itemPos.x = itemPos.x - 0.42f;
				}
				itemChild.localPosition = itemPos;
			}
		} */

        if (collide) {
            obj.GetComponent<Rigidbody2D>().MovePosition(pos);
        } else {
            objTransform.position = pos;
        }

        if (isWalking) {
            this.pos = objTransform.position;
        } else {
            this.pos = pos;
        }

        objTransform.position = new Vector3(this.pos.x, this.pos.y, (this.pos.y / 10) + 0.001f);
    }

    public void SetDir(int dir) {
        if (dir == 0) {
            objRenderer.material.mainTextureScale = new Vector2(-1, 1);
        } else if (dir == 1) {
        } else if (dir == 2) {
            objRenderer.material.mainTextureScale = new Vector2(1, 1);
        } else if (dir == 3) {
        }

        for (int i = 0; i < inventory.Count; i++) {
            MeshRenderer item = ItemManager.Instance.FindItemRendererInWorld(inventory[i].id);
            Vector3 itemPos = item.transform.localPosition;
            if (dir == 0) {
                objRenderer.material.mainTextureScale = new Vector2(-1, 1);
                itemPos.x = ItemManager.Instance.GetItemTypeInfo(ItemManager.Instance.FindItemInWorld(inventory[i].id).type).offset.x + 0.37f;
            } else if (dir == 2) {
                objRenderer.material.mainTextureScale = new Vector2(1, 1);
                itemPos.x = ItemManager.Instance.GetItemTypeInfo(ItemManager.Instance.FindItemInWorld(inventory[i].id).type).offset.x;
            }
            item.transform.localPosition = itemPos;
            for (int j = 0; j < item.transform.childCount; j++) {
                Transform itemChild = item.transform.GetChild(j);
                // itemPos = itemChild.localPosition;
                if (dir == 0) {
                    itemChild.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(-1, 1);
                    // itemPos.x = itemPos.x + 0.42f;
                } else if (dir == 2) {
                    itemChild.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(1, 1);
                    // itemPos.x = itemPos.x - 0.42f;
                }
                // itemChild.localPosition = itemPos;
            }
        }

        this.dir = dir;
    }

    public void SetAnimClip(int animClip) {
        if (this.animClip != animClip) {
            timeSinceLastAnim = 0;
            animPhase = 0;
            this.animClip = animClip;
        }
    }

    public void SetDest(Vector2 dest) {
        this.dest = dest;
        pathfinding = null;
    }

    public void SetState(PersonState state) {
        lastState = currState;
        if (lastState != null) {
            lastState.ExitState(this);
        }
        currState = state;
        if (currState != null) {
            currState.EnterState(this);
        }
    }

    public void SetPossessed(bool isPossessed) {
        this.isPossessed = isPossessed;
        // TODO: change color when possessed
        ParticleSystem particleSystem = obj.GetComponentInChildren<ParticleSystem>(true);
        if (isPossessed) {
            if (particleSystem != null) {
                particleSystem.gameObject.SetActive(true);
            } else {
                GameObject particleSystemObj = GameObject.Instantiate(possessedEffect, objChild.transform);
                particleSystemObj.transform.localPosition = new Vector3(0f, 0.5f, 0f);
                particleSystemObj.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
            }
        } else {
            if (particleSystem != null) {
                particleSystem.gameObject.SetActive(false);
            }
            curr = new Vector2(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
            next = curr;
            dest = curr;
        }
    }

    public void AddHealth(int value, MonoBehaviour behavior = null) {
        Debug.Log("AddHealth " + isPossessed + " | " + value + " | " + health);
        health += value;
        if (value < 0) {
            AudioClip hurtSound = hurtSounds[Random.Range(0, hurtSounds.Length)];
            AudioManager.Instance.PlaySound(hurtSound, hurtSound.length, 0f, 0.55f);
        }
        if (isPossessed) {
            PlayerManager.Instance.AddHealth(value);
        } else {
            if (value < 0) {
                CameraManager.Instance.Shake(0.2f);
                Flash(PlayerManager.Instance.enemyHitColor, behavior, 0.3f);
                if (behavior != null) {
                    behavior.StartCoroutine(Sleep(0.2f));
                }
            }
        }
        if (health <= 0) {
            Die(behavior);
        }
    }

    public void Die(MonoBehaviour behavior) {
        if (!isPossessed) {
            CameraManager.Instance.Shake(0.1f);
        }
        ItemManager.Instance.DeleteItems(inventory);
        PeopleManager.Instance.DeletePerson(this);
        GameObject.Destroy(obj);
        if (behavior != null) {
            behavior.StartCoroutine(Sleep(0.3f));
        }
    }

    public void Flash(Color color, MonoBehaviour behavior, float flashTime = 0.5f) {
        if (behavior != null) {
            behavior.StartCoroutine(FlashActual(color, flashTime));
        }
    }

    private IEnumerator FlashActual(Color color, float flashTime) {
        Color orgColor = objRenderer.material.color;
        objRenderer.material.color = color;

        yield return new WaitForSecondsRealtime(flashTime);

        objRenderer.material.color = orgColor;
    }

    private IEnumerator Sleep(float sleepAmt) {
        Time.timeScale = 0;

        yield return new WaitForSecondsRealtime(sleepAmt);

        Time.timeScale = 1;
    }
}
