using System.Collections.Generic;
using CountdownAutoBattle.UI;
using UnityEngine;
using UnityEngine.UI;

namespace CountdownAutoBattle.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class CardDrawController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private Button drawButton;

        [SerializeField]
        private PointCardView pointCardPrefab;

        [SerializeField]
        private List<CardSlotView> poolSlots = new();

        private readonly List<PointCardInstance> drawPile = new();

        private int nextInstanceId = 1;
        private bool hasDrawn;

        private static readonly int[] InitialCardValues =
        {
            1, 2, 2, 2, 3,
            3, 4, 4, 5, 6
        };

        private void Awake()
        {
            ValidateReferences();
            CreateInitialDeck();
        }

        private void OnEnable()
        {
            if (drawButton != null)
            {
                drawButton.onClick.AddListener(HandleDrawClicked);
            }
        }

        private void OnDisable()
        {
            if (drawButton != null)
            {
                drawButton.onClick.RemoveListener(HandleDrawClicked);
            }
        }

        private void ValidateReferences()
        {
            if (drawButton == null)
            {
                Debug.LogError(
                    "Draw Button is not assigned.",
                    this);
            }

            if (pointCardPrefab == null)
            {
                Debug.LogError(
                    "Point Card Prefab is not assigned.",
                    this);
            }

            if (poolSlots.Count != 5)
            {
                Debug.LogError(
                    $"Pool Slots must contain exactly 5 entries. " +
                    $"Current count: {poolSlots.Count}",
                    this);
            }
        }

        private void CreateInitialDeck()
        {
            drawPile.Clear();
            nextInstanceId = 1;

            foreach (int value in InitialCardValues)
            {
                drawPile.Add(
                    new PointCardInstance(
                        nextInstanceId++,
                        value));
            }
        }

        private void HandleDrawClicked()
        {
            if (hasDrawn)
            {
                return;
            }

            int emptySlotCount = 0;

            foreach (CardSlotView slot in poolSlots)
            {
                if (slot != null && slot.IsEmpty)
                {
                    emptySlotCount++;
                }
            }

            int drawCount =
                Mathf.Min(
                    emptySlotCount,
                    drawPile.Count);

            for (int i = 0; i < drawCount; i++)
            {
                DrawOneCard();
            }

            hasDrawn = true;

            if (drawButton != null)
            {
                drawButton.interactable = false;
            }
        }

        private void DrawOneCard()
        {
            CardSlotView targetSlot =
                poolSlots.Find(slot =>
                    slot != null && slot.IsEmpty);

            if (targetSlot == null ||
                drawPile.Count == 0)
            {
                return;
            }

            int randomIndex =
                Random.Range(0, drawPile.Count);

            PointCardInstance cardData =
                drawPile[randomIndex];

            drawPile.RemoveAt(randomIndex);

            PointCardView cardView =
                Instantiate(
                    pointCardPrefab,
                    targetSlot.transform);

            cardView.Bind(cardData);
            targetSlot.SetCard(cardView);
        }
    }
}