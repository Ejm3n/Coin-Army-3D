using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BG.UI.Main;
using System;

public class MoneyLoot : MonoBehaviour, IMouseEventProxyTarget
{
    public ParticleSystem MoneyParticles;
    public MeshRenderer Renderer;

    private Vector3 _homePosition;
    private Vector3 _randomDirection;
    private Vector3 _randomDirection2;

    private Vector3 _baseScale;

    private float _startAnimT;
    private float _heightOffset;

    private bool _flyingIntoMoneyCounter;

    [NonSerialized]
    public ulong Money;

    public void FlyIntoMoneyCounter(bool sound = false)
    {
        if (_flyingIntoMoneyCounter)
        {
            return;
        }

        _flyingIntoMoneyCounter = true;
        _startAnimT = 0f;
        _homePosition = transform.position;

        if (Money != 0)
        {
            MoneyService.Default.AddTempMoney(Money);
            Money = 0;
        }

        MoneyParticles.Play();

        if (sound)
        {
            SoundHolder.Default.PlayFromSoundPack("LootPickup");
        }
    }

    void Awake()
    {
        _baseScale = transform.localScale;
    }

    void Start()
    {
        UIManager.Default.GetPanel(UIState.Start).GetComponent<StartPanel>().RequestMouseProxy(this, true);
    }

    void OnEnable()
    {
        _homePosition = transform.position;
        transform.localScale = _baseScale;
        _randomDirection = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f) * Vector3.forward;
        _randomDirection2 = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
        transform.rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
        _heightOffset = UnityEngine.Random.Range(0f, Mathf.PI);
        _startAnimT = 0f;
        _flyingIntoMoneyCounter = false;

        if (!LevelSettings.Default.IsFightActive)
        {
            FlyIntoMoneyCounter();
        }
    }

    void Update()
    {
        Renderer.enabled = !_flyingIntoMoneyCounter;

        if (_flyingIntoMoneyCounter)
        {
            //transform.position = Vector3.Lerp(_homePosition, MoneyLootFlyMarker.Default.transform.position, Mathf.SmoothStep(0f, 1f, _startAnimT));

            //transform.Rotate(_randomDirection2 * Time.deltaTime * 1000f);

            //transform.localScale = _baseScale * Mathf.Lerp(1f, 0.25f, _startAnimT);

            if (_startAnimT >= 1f)
            {
                enabled = false;
                UnitManager.LootPool.Add(this);
                UnitManager.DroppedLoot.Remove(this);
            }
        }
        else
        {
            transform.position = Vector3.Lerp(_homePosition, _homePosition + _randomDirection, _startAnimT);
            transform.position += Vector3.up * (0.5f + Mathf.Sin(Time.time * 2f + _heightOffset) * 0.25f * _startAnimT + Mathf.Abs(Mathf.Sin(_startAnimT * Mathf.PI * 2f)) * 2f * (1f - _startAnimT));
        }

        _startAnimT += Time.deltaTime;

        if (_startAnimT > 1f)
        {
            _startAnimT = 1f;
        }

        transform.Rotate(Vector3.up * Time.deltaTime * 200f);
    }

    public bool Exists()
    {
        return this != null && gameObject != null;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public Vector2 GetSize()
    {
        return _flyingIntoMoneyCounter || !gameObject.activeSelf ? new Vector2(0f, 0f) : new Vector2(150f, 150f);
    }

    public void OnPointerDown()
    {
        FlyIntoMoneyCounter(true);
    }

    public void OnPointerDrag()
    {

    }

    public void OnPointerUp()
    {
        
    }
}
