﻿//
// HealthDrop HEALTH ITEM DROP FROM ENEMIES AND HUMANS
// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
// This is GameManager Class which spawn enemy and boss at beginning of each around and create pool of prefabs
// This class is Unity Singleton.
// 
// AUT University - 2020 - Dan Yoo, Yuki Liyanage, Kelly Luo
// 
// Revision History
// ~~~~~~~~~~~~~~~~
// 20.05.2020 Creation date

//
// .NET support packages
// ~~~~~~~~~~~~~~~~~~~~~
using IEGame.FiniteStateMachine;
using System;
using System.Collections;
using System.Collections.Generic;
//
// Unity support packages
// ~~~~~~~~~~~~~~~~~~~~~
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    public GameObject bossFog;

    private ObjectStats bossStats;

    public List<Transform> SpawnPoints { get; set; } = new List<Transform>();

    private static GameManager instance = null;
    public static GameManager Instance
    {
        get { return instance; }
    }

    [Header("Enemy Create Info")]

    public int numberOfAlien;

    public int numberOfHuman;

    public int experienceScore;

    public float spawnDeley = 2f;

    private int roundNo = 1;
    public int RoundNo
    {
        get { return roundNo; }
    }
    public const int maxRound = 5;

    public int maxHuman = 30;
    public int maxAlien = 5;
    private const int LOGVALUESPAWNRATE = 20;

    public bool ishand = false;

    [Header("Root Object Pool")]
    public GameObject moneyBillPrefab;
    public int maxMoneyBillPool = 500; //Number limit of Money objects simultaneously allows in one scene.
    public List<GameObject> moneyBillPool = new List<GameObject>();

    public GameObject ammoPrefab;
    public int maxAmmoPool = 500;
    public List<GameObject> ammoPool = new List<GameObject>();

    public GameObject healthPrefab;
    public int maxHealthPool = 500;
    public List<GameObject> healthPool = new List<GameObject>();

    private MobFactory EnemyFactory;

    [Header("Prefab of Direct Projectile Pool")]
    public GameObject[] directProjectilePrefabs;
    public int numberOfEachDirectProjectile = 3;
    private int directProjectileIdx = 0;
    public List<GameObject> directProjectilePool = new List<GameObject>();
    [Header("Prefab of Straight Down Projectile Pool")]
    public GameObject[] straightDownProjectilePrefabs;
    public int numberOfEachStraightDownProjectile = 3;
    private int straightDownProjectileIdx = 0;
    public List<GameObject> straightDownProjectilePool = new List<GameObject>();
    [Header("Prefab of Normal Projectile Pool")]
    public GameObject[] normalProjectilePrefabs;
    public int numberOfEachNormalProjectile = 3;
    private int normalProjectileIdx = 0;
    public List<GameObject> normalProjectilePool = new List<GameObject>();
    [Header("Prefab of explosive Projectile Pool")]
    public GameObject[] explosiveProjectilePrefabs;
    public int numberOfEachExplosiveProjectile = 5;
    private int explosiveProjectileIdx = 0;
    public List<GameObject> explosiveProjectilePool = new List<GameObject>();

    [Header("Particle Explosive effect pull")]
    public GameObject explosiveEffectPrefab;
    public int maxExplosiveEffectPool = 30;
    public float explosiveEffectDelay = 30f;
    public List<GameObject> explosiveEffectPool = new List<GameObject>();
    public Vector3 explosiveEffectOffset = new Vector3(0, 0.4f, 0);

    [Header("Particle Bullet Hit Effect pull")]
    public GameObject bulletHitEffectPrefab;
    public int maxBulletHitEffectPool = 40;
    public float bulletHitEffectDelay = 0.3f;
    public List<GameObject> bulletHitEffectPool = new List<GameObject>();

    [Header("Particle ItemPopUp effect pull")]
    public GameObject itemPopUpEffectPrefab;
    public int maxItemPopUpEffectPool;
    public float itemPopUpEffectDelay = 0.3f;
    public List<GameObject> itemPopUpEffectPool = new List<GameObject>();
    public Vector3 itemPopUpEffectOffset = new Vector3(0, 1f, 0);


    public IUnityServiceManager UnityService { get; set; } = UnityServiceManager.Instance;

    private bool isScoreSet;
    private bool clearRound;

    public bool bossRound { get; set; } = false ;


 
    private const int LOGVALUESCORE = 50;
    private int requiredScore = 50;
    public int RequiredScore
    {
        get { return requiredScore; }
    }

    public bool ClearRound
    {
        get { return clearRound; }
        set 
        {
            if(value)
            {
                if(roundNo != maxRound) roundNo++;
                if (roundNo >= maxRound && !bossRound)
                {
                    bossRound = true;
                    SpawnBoss();
                }
                else if(!bossRound)
                {
                    maxAlien = (int)CalculateLog(maxAlien, LOGVALUESPAWNRATE); //Increase Enemy size logarithmically 
                    maxHuman = (int)CalculateLog(maxHuman, LOGVALUESPAWNRATE); //Increase Enemy size logarithmically 
                    requiredScore = (int)CalculateLog(requiredScore, LOGVALUESCORE); //Increase round score requirement logarithmically
                }
            }
            
        }
    }

    private float CalculateLog(int original, int log)
    {
        return (float)original * Mathf.Log10(log);
    }

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        //If the instance has different instance of this class, it means new class.
        // if a game flows to next scene and comeback to this scene and Awake() again then delete the new game manager.
        else if (instance != this)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);

        CreateMoneyBillPooling();
        CreateAmmoPooling();
        CreateHealthPooling();
        CreateDirectProjectilePrefabsPooling();
        CreateStraightDownProjectilePrefabsPooling();
        CreateNormalProjectilePrefabsPooling();
        CreateExplosiveProjectilePrefabsPooling();
        CreateExplosiveEffectPooling();
        CreateBulletHitEffectPooling();
        CreateItemPopUpEffectPooling();

    }

    void Start()
    {
        EnemyFactory = GetComponent<EnemyMobFactory>();
        var group = GameObject.Find("EnemySpawnPoints");
        if (group != null)
        {
            group.GetComponentsInChildren<Transform>(SpawnPoints);
            //removing the waypoint folder
            SpawnPoints.RemoveAt(0);
        }

        if (SpawnPoints.Count > 0)
        {
            StartCoroutine(this.CreateEnemy());
        }

    }

    void Update()
    {

    }

    private IEnumerator timerWait()
    {
        yield return new WaitForSeconds(7f);
        bossRound = false;
        SceneManager.LoadScene("MainMenuV2", LoadSceneMode.Single);
        Destroy(this.gameObject);
    }

    #region Enemy Spawn method
    //Coruotine of  Creating enmey till the max number we specified 
    private IEnumerator CreateEnemy()
    {
        while (!ClearRound)
        {
            yield return new WaitForSeconds(spawnDeley);
            numberOfAlien = (int)GameObject.FindGameObjectsWithTag("Enemy").Length;
            numberOfHuman = (int)GameObject.FindGameObjectsWithTag("Human").Length;
            if (numberOfHuman <= maxHuman)
            {
                SpawnHuman();
            }
            else if (numberOfAlien <= maxAlien)
            {
                SpawnAlien();
            }
            else
            {
                yield return null;
            }
        }


    }
    
    void SpawnHuman()
    {
        EnemyFactory.CreateMob(GetRandomSpawnPoint());
    }

    void SpawnAlien()
    {
        EnemyFactory.CreateMobWithWeapon(GetRandomSpawnPoint());
    }

    void SpawnBoss()
    {
       GameObject boss =  EnemyFactory.CreateBoss(GetRandomSpawnPoint());
       bossStats = boss.GetComponent<MonsterController>().Stats;

       bossFog.SetActive(true);
    }

    Vector3 GetRandomSpawnPoint()
    {
        int randomIdx = UnityService.Range(0, SpawnPoints.Count);
        return SpawnPoints[randomIdx].position;
    }

    #endregion
    //Method that spawns effect on the position where got it from the caller classes.
    #region SpawnEffect
    public void SpawnExplosiveEffectObject(Vector3 position, Quaternion angle)
    {
        for (int i = 0; i < explosiveEffectPool.Count; i++)
        {
            if (explosiveEffectPool[i].activeSelf == false)
            {
                explosiveEffectPool[i].transform.position = position + explosiveEffectOffset;
                explosiveEffectPool[i].transform.rotation = angle;
                StartCoroutine(SetActiveForDelayAmount(explosiveEffectPool[i], explosiveEffectDelay));
                break;
            }
        }
    }

    public void SpawnBulletHitObject(Vector3 position, Quaternion angle)
    {
        for (int i = 0; i < bulletHitEffectPool.Count; i++)
        {
            if (bulletHitEffectPool[i].activeSelf == false)
            {
                bulletHitEffectPool[i].transform.position = position;
                bulletHitEffectPool[i].transform.rotation = angle;
                StartCoroutine(SetActiveForDelayAmount(bulletHitEffectPool[i], bulletHitEffectDelay));
                break;
            }
        }
    }

    public void SpawnItemPopUpEffectObject(Vector3 position, Quaternion angle)
    {
        for (int i = 0; i < itemPopUpEffectPool.Count; i++)
        {
            if (itemPopUpEffectPool[i].activeSelf == false)
            {
                itemPopUpEffectPool[i].transform.position = position + itemPopUpEffectOffset;
                itemPopUpEffectPool[i].transform.rotation = angle;
                StartCoroutine(SetActiveForDelayAmount(itemPopUpEffectPool[i], itemPopUpEffectDelay));
                break;
            }
        }
    }

    private IEnumerator SetActiveForDelayAmount(GameObject gameObject,float delay)
    {
        gameObject.SetActive(true);
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }


    #endregion
    //Method Get the Prefaber object from the 
    #region GetPoolObject
    //since every I want show there is alot of projectiles so I iterate the pool list using own index(idx)
    public GameObject GetDirectProjectileObject()
    {
        for (int i = 0; i < directProjectilePool.Count; i++)
        {
            directProjectileIdx %= directProjectilePool.Count;
            if (directProjectilePool[directProjectileIdx].activeSelf == false)
            {
                return directProjectilePool[directProjectileIdx++];
            }
            directProjectileIdx++;
        }
        return null;
    }

    //since every I want show there is alot of projectiles so I iterate the pool list using own index(idx)
    public GameObject GetStraightDownProjectileObject()
    {
        for (int i = 0; i < straightDownProjectilePool.Count; i++)
        {
            straightDownProjectileIdx %= straightDownProjectilePool.Count;
            if (straightDownProjectilePool[straightDownProjectileIdx].activeSelf == false)
            {
                return straightDownProjectilePool[straightDownProjectileIdx++];
            }
            straightDownProjectileIdx++;
        }
        return null;
    }

    //since every I want show there is alot of projectiles so I iterate the pool list using own index(idx)
    public GameObject GetNormalProjectileObject()
    {
        for (int i = 0; i < normalProjectilePool.Count; i++)
        {
            normalProjectileIdx %= normalProjectilePool.Count;
            if (normalProjectilePool[normalProjectileIdx].activeSelf == false)
            {
                return normalProjectilePool[normalProjectileIdx++];
            }
            normalProjectileIdx++;
        }
        return null;
    }

    //since every I want show there is alot of projectiles so I iterate the pool list using own index(idx)
    public GameObject GetExplosiveProjectileObject()
    {
        for (int i = 0; i < explosiveProjectilePool.Count; i++)
        {
            explosiveProjectileIdx %= explosiveProjectilePool.Count;
            if (explosiveProjectilePool[explosiveProjectileIdx].activeSelf == false)
            {
                return explosiveProjectilePool[explosiveProjectileIdx++];
            }
            explosiveProjectileIdx++;
        }
        return null;
    }

    #endregion
    //create pool at beginning of game so we dont have to initiate many time and Keep the Resouce and GC stable. 
    #region CreatingPooling
    public GameObject GetMoneyBillObject()
    {
        for (int i = 0; i < moneyBillPool.Count; i++)
        {
            if (moneyBillPool[i].activeSelf == false)
            {
                return moneyBillPool[i];
            }
        }
        return null;

    }

    public void CreateMoneyBillPooling()
    {
        GameObject objectPools = new GameObject("MoneyBillPools");

        for (int i = 0; i < maxMoneyBillPool; i++)
        {
            var obj = Instantiate<GameObject>(moneyBillPrefab, objectPools.transform);
            obj.name = "MoneyBill_" + i.ToString("00");

            obj.SetActive(false);

            moneyBillPool.Add(obj);
        }
    }

    public GameObject GetAmmoObject()
    {
        for (int i = 0; i < ammoPool.Count; i++)
        {
            if (ammoPool[i].activeSelf == false)
            {
                return ammoPool[i];
            }
        }
        return null;
    }

    public void CreateAmmoPooling()
    {
        GameObject ammoPools = new GameObject("AmmoPools");

        for (int i = 0; i < maxAmmoPool; i++)
        {
            var obj = Instantiate<GameObject>(ammoPrefab, ammoPools.transform);
            obj.name = "Ammo_" + i.ToString("00");

            obj.SetActive(false);

            ammoPool.Add(obj);
        }
    }

    //
    // GetHealthObject()
    // ~~~~~~~~~~~~~~~~~
    // This method loops through the pool of health drops and returns the first not active object
    //
    // returns      the first available health drop object that is not set active yet
    //
    public GameObject GetHealthObject()
    {
        for (int i = 0; i < healthPool.Count; i++)
        {
            if (healthPool[i].activeSelf == false)
            {
                return healthPool[i];
            }
        }
        return null;
    }

    //
    // CreateHealthPooling()
    // ~~~~~~~~~~~~~~~~~~~~~
    // This method instantiates the pool of health objects and sets them not active by default
    //
    public void CreateHealthPooling()
    {
        GameObject healthPools = new GameObject("HealthPools");

        for (int i = 0; i < maxHealthPool; i++)
        {
            var obj = Instantiate<GameObject>(healthPrefab, healthPools.transform);
            obj.name = "Health_" + i.ToString("00");

            obj.SetActive(false);

            healthPool.Add(obj);
        }
    }

    public void CreateExplosiveEffectPooling()
    {
        GameObject objectPools = new GameObject("ExplosiveEffectPools");

        for (int i = 0; i < maxExplosiveEffectPool; i++)
        {
            var obj = Instantiate<GameObject>(explosiveEffectPrefab, objectPools.transform);
            obj.name = "ExplosiveEffect_" + i.ToString("00");

            obj.SetActive(false);

            explosiveEffectPool.Add(obj);
        }
    }
    public void CreateBulletHitEffectPooling()
    {
        GameObject objectPools = new GameObject("BulletHitPools");

        for (int i = 0; i < maxBulletHitEffectPool; i++)
        {
            var obj = Instantiate<GameObject>(bulletHitEffectPrefab, objectPools.transform);
            obj.name = "BulletHit_" + i.ToString("00");

            obj.SetActive(false);

            bulletHitEffectPool.Add(obj);
        }
    }

    public void CreateItemPopUpEffectPooling()
    {
        GameObject objectPools = new GameObject("ItemPopUpEffectPools");

        for (int i = 0; i < maxItemPopUpEffectPool; i++)
        {
            var obj = Instantiate<GameObject>(itemPopUpEffectPrefab, objectPools.transform);
            obj.name = "ItemPopUpEffect_" + i.ToString("00");

            obj.SetActive(false);

            itemPopUpEffectPool.Add(obj);
        }
    }


    public void CreateDirectProjectilePrefabsPooling()
    {
        GameObject objectPools = new GameObject("DirectProjectilePools");
        int j = 0;
        for (int i = 0; i < numberOfEachDirectProjectile; i++)
            foreach (GameObject prefab in directProjectilePrefabs)
            {
                var obj = Instantiate<GameObject>(prefab, objectPools.transform);
                obj.name = "DirectProjectile_" + j.ToString("00");

                obj.SetActive(false);

                directProjectilePool.Add(obj);
                j++;
            }
    }

    public void CreateStraightDownProjectilePrefabsPooling()
    {
        GameObject objectPools = new GameObject("StraightDownProjectilePools");
        int j = 0;
        for (int i = 0; i < numberOfEachStraightDownProjectile; i++)
            foreach (GameObject prefab in straightDownProjectilePrefabs)
            {
                var obj = Instantiate<GameObject>(prefab, objectPools.transform);
                obj.name = "StraightDownProjectile_" + j.ToString("00") ;

                obj.SetActive(false);

                straightDownProjectilePool.Add(obj);
                j++;
            }
    }

    public void CreateNormalProjectilePrefabsPooling()
    {
        GameObject objectPools = new GameObject("NormalProjectilePools");
        int j = 0;
        for (int i = 0; i < numberOfEachNormalProjectile; i++)
            foreach (GameObject prefab in normalProjectilePrefabs)
            {
                var obj = Instantiate<GameObject>(prefab, objectPools.transform);
                obj.name = "NormalProjectile_" + j.ToString("00");

                obj.SetActive(false);

                normalProjectilePool.Add(obj);
                j++;
            }
    }

    public void CreateExplosiveProjectilePrefabsPooling()
    {
        GameObject objectPools = new GameObject("ExplosiveProjectilePools");
        int j = 0;
        for (int i = 0; i < numberOfEachExplosiveProjectile; i++)
            foreach (GameObject prefab in explosiveProjectilePrefabs)
            {
                var obj = Instantiate<GameObject>(prefab, objectPools.transform);
                obj.name = "ExplosiveProjectile_" + j.ToString("00");

                obj.SetActive(false);

                explosiveProjectilePool.Add(obj);
                j++;
            }
    }


    #endregion

}