using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CustomSlots : MonoBehaviour
{
    [SerializeField] MoneyLoot moneyLoot;

    [SerializeField] private Animator rouletteAnim;
    [SerializeField] private RectTransform[] reels; // Барабаны (UI RectTransforms)
    [SerializeField] private Sprite[] availableSprites; // Возможные символы
    public int symbolsPerReel = 10; // Количество символов на каждом барабане

    public float spinSpeed = 1000f; // Базовая скорость вращения
    public float deceleration = 500f; // Базовое замедление вращения
    public int symbolStep = 150; // Шаг между символами

    private bool isSpinning = false;
    private float[] reelAccelerations; // Случайное ускорение для каждого барабана

    private void Start()
    {
        InitializeReels();
        InitializeAccelerations();
    }

    private void OnEnable()
    {
        moneyLoot.gameObject.SetActive(false);
        isSpinning = false;
    }

    private void InitializeReels()
    {
        // Генерируем символы для каждого барабана
        for (int i = 0; i < reels.Length; i++)
        {
            Transform[] symbols = reels[i].GetComponentsInChildren<Transform>();

            for (int j = 0; j < symbols.Length; j++)
            {
                if (symbols[j].GetComponent<Image>() != null)
                {
                    symbols[j].GetComponent<Image>().sprite = availableSprites[Random.Range(0, availableSprites.Length)];
                }
            }
        }
    }

    private void InitializeAccelerations()
    {
        reelAccelerations = new float[reels.Length];

        // Генерируем ускорение для каждого барабана
        for (int i = 0; i < reels.Length; i++)
        {
            float randomFactor = Random.Range(0.8f, 1.2f); // ±20%
            reelAccelerations[i] = deceleration * randomFactor;
        }
    }

    public void Spin()
    {
        if (isSpinning) return; // Не запускаем новый спин, пока предыдущий не завершился
        StartCoroutine(SpinReels());
    }

    private IEnumerator SpinReels()
    {
        isSpinning = true;

        // Устанавливаем начальную скорость для каждого барабана
        float[] currentSpeeds = new float[reels.Length];
        for (int i = 0; i < reels.Length; i++)
        {
            currentSpeeds[i] = spinSpeed;
        }

        bool spinning = true;

        while (spinning)
        {
            spinning = false;

            for (int i = 0; i < reels.Length; i++)
            {
                RectTransform reel = reels[i];

                // Если скорость этого барабана больше 0, он продолжает вращаться
                if (currentSpeeds[i] > 0)
                {
                    spinning = true;

                    // Перемещаем символы вниз
                    foreach (Transform symbol in reel)
                    {
                        symbol.localPosition -= new Vector3(0, currentSpeeds[i] * Time.deltaTime, 0);

                        // Если символ выходит за нижнюю границу, перемещаем его наверх
                        if (symbol.localPosition.y <= -symbolStep * (reel.childCount - 1))
                        {
                            symbol.localPosition += new Vector3(0, symbolStep * reel.childCount, 0);
                        }
                    }

                    // Уменьшаем скорость с учётом индивидуального ускорения
                    currentSpeeds[i] -= reelAccelerations[i] * Time.deltaTime;
                }
            }

            yield return null;
        }

        // Выравниваем позиции символов
        AlignReels();

        //isSpinning = false;

        // Проверяем результат
        CheckWin();
    }

    private void AlignReels()
    {
        foreach (RectTransform reel in reels)
        {
            foreach (Transform symbol in reel)
            {
                // Найдем ближайшую позицию, кратную symbolStep
                float closestY = Mathf.Round(symbol.localPosition.y / symbolStep) * symbolStep;
                symbol.localPosition = new Vector3(symbol.localPosition.x, closestY, symbol.localPosition.z);
            }
        }
    }


    private void CheckWin()
    {
        Sprite[] middleSprites = new Sprite[reels.Length];

        // Получаем центральные символы каждого барабана
        for (int i = 0; i < reels.Length; i++)
        {
            RectTransform reel = reels[i];

            // Находим символ, который находится в центре (позиция Y = 0)
            Transform middleSymbol = null;
            foreach (Transform symbol in reel)
            {
                if (Mathf.Abs(symbol.localPosition.y) < symbolStep / 2) // Центр ~0
                {
                    middleSymbol = symbol;
                    break;
                }
            }

            if (middleSymbol != null)
            {
                middleSprites[i] = middleSymbol.GetComponent<Image>().sprite;
            }
        }

        // Проверяем, совпадают ли центральные символы
        if (middleSprites[0] == middleSprites[1] && middleSprites[1] == middleSprites[2])
        {
            MoneyService.Default.AddMoney(10000);
        }
        else
        {
            MoneyService.Default.AddMoney(1000);
        }

        moneyLoot.gameObject.SetActive(true);
        moneyLoot.FlyIntoMoneyCounter(true);

        rouletteAnim.SetTrigger("Close");
    }
}