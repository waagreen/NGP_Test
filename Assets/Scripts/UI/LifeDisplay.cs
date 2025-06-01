using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class LifeDisplay : MonoBehaviour
{
    [SerializeField] private List<Transform> lifeHolders;

    private Sequence lifeSequence;

    public void UpdateLives(int currentAmount)
    {
        lifeSequence?.Kill();
        lifeSequence = DOTween.Sequence();
        for (int i = lifeHolders.Count; i >= 1; i--)
        {
            int scaleFactor = currentAmount < i ? 0 : 1;
            lifeSequence.Append(lifeHolders[i-1].DOScale(scaleFactor * Vector3.one, 0.2f).SetEase(Ease.OutBack));
        }

        lifeSequence.Play();
    }
}
