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

        for (int i = 0; i < lifeHolders.Count; i++)
        {
            bool shouldBeVisible = i < currentAmount;
            bool isVisible = lifeHolders[i].localScale != Vector3.zero;

            if (isVisible && !shouldBeVisible)
            {
                lifeSequence.Append(lifeHolders[i].DOScale(Vector3.one * 1f, 0.2f).SetEase(Ease.InBack));
                lifeSequence.Append(lifeHolders[i].DOScale(Vector3.zero, 0.3f).SetEase(Ease.OutSine));
                break;
            }
            else if (!isVisible && shouldBeVisible)
            {
                lifeSequence.Append(lifeHolders[i].DOScale(Vector3.one, 0.15f).SetEase(Ease.OutBack));
                lifeSequence.AppendInterval(0.1f);
            }
        }

        lifeSequence.Play();
    }
}
