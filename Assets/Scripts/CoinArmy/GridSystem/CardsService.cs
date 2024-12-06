using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class CardsService : MonoBehaviour
{
    #region Singleton
    private static CardsService _default;
    public static CardsService Default => _default;
    #endregion

    [Serializable]
    private class State
    {
        [SerializeField]
        public List<int> Entries;
    }

    private void Awake()
    {
        _default = this;
    }

    public List<int> GetPlayerCards()
    {
        State savedSetup = JsonUtility.FromJson<State>(PlayerPrefs.GetString("PlayerCards"));

        if (savedSetup == null || savedSetup.Entries == null)
        {
            return new List<int>();
        }
        else
        {
            return savedSetup.Entries;
        }
    }

    public bool AddCard(int card)
    {
        if (card < 0)
        {
            return false;
        }

        var cards = GetPlayerCards();

        if (!cards.Contains(card))
        {
            cards.Add(card);

            State setupResult = new State();
            setupResult.Entries = cards;
            PlayerPrefs.SetString("PlayerCards", JsonUtility.ToJson(setupResult));

            return true;
        }

        return false;
    }
}
